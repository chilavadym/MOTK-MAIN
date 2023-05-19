// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Serial;

public static class Common
{
    public enum EOperation : byte
    {
        Read = (byte)'R',
        Write = (byte)'W',
    }
    public enum EWakeCode : byte
    {
        Sensor = (byte)'?',
        Display = (byte)'!',
        ReplyAck = (byte)'A',
        ReplyErr = (byte)'E',
    }

    public const int WriteSleepPerByte = 20;
    public const int WriteTimeout = 50;

    public static byte[] DecodePacket(byte[] data)
    {
        if (data.Length == 0)
        {
            throw new TimeoutException("No data received");
        }

        var startIndex = 0;
            
        while (data[startIndex] != (byte)EWakeCode.ReplyAck && ++startIndex < data.Length)
        {
            // Nothing to do, loop condition does all the work
        }

        var index = startIndex + 1;

        if (index >= data.Length)
        {
            throw new FormatException("Wake character not found");
        }

        var packetLength = data[index++];

        if (packetLength > (data.Length) - index)
        {
            throw new TimeoutException("Read timed out before message was completed");
        }

        index += packetLength - sizeof(ushort);

        var checksum = (ushort)((data[index] << 8) | data[index + 1]);

        while (--index >= startIndex)
        {
            checksum += data[index];
        }

        if (checksum != 0xFFFF)
        {
            throw new FormatException($"Checksum mismatch, difference = {(checksum - 0xFFFF):X4}");
        }

        var result = new byte[packetLength - sizeof(ushort)];
        
        for (var i = 0; i < packetLength - sizeof(ushort); ++i)
        {
            result[i] = data[startIndex + 2 + i];
        }
        
        return result;
    }

    public static async Task CommsWrite(CancellationToken cancelToken, CommsInfo comms, byte[] data)
    {
        //This copy of the CommsInfo is used to prevent CloseIfOnlyOneInstance() from closing port
        using var commsCopy = new CommsInfo(comms.PortName);
            
        comms.SetConfig(Properties.Settings.Default.ReadTimeout, WriteTimeout);

        if (Equals(comms.BaudRate, 9600))
        {
            for (var i = 0; i < data.Length; ++i)
            {
                cancelToken.ThrowIfCancellationRequested();
                    
                if (i > 0)
                {
                    await Task.Delay(WriteSleepPerByte, cancelToken).ConfigureAwait(false);
                }
                    
                await comms.BaseStream.WriteAsync(data, i, 1, cancelToken).ConfigureAwait(false);
            }
        }
        else
        {
            await comms.BaseStream.WriteAsync(data, 0, data.Length, cancelToken).ConfigureAwait(false);
        }
    }
    public static async Task<byte[]> CommsRead(CancellationToken cancelToken, CommsInfo comms, int expectedReadLength)
    {
        //This copy of the CommsInfo is used to prevent CloseIfOnlyOneInstance() from closing port
        using var commsCopy = new CommsInfo(comms.PortName);

        var buffer = new byte[expectedReadLength];
            
        var readTimeout = Properties.Settings.Default.ReadTimeout;

        comms.SetConfig(readTimeout, WriteTimeout);

        var readCount = 0;

        // Token to pass to lower level APIs that don't support normal timeouts
        using var timeoutSource = new CancellationTokenSource(readTimeout);
        var timeoutToken = timeoutSource.Token;

        // Combining the tokens will allow the user to cancel still, but we can internally use a timeout
        using var combinedSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, timeoutToken);
        var combinedToken = combinedSource.Token;

        try
        {
            while (!combinedToken.IsCancellationRequested && readCount < expectedReadLength)
            {
                readCount += await Task.Factory.StartNew(
                    () => comms.BaseStream.Read(buffer, readCount, expectedReadLength - readCount),
                    combinedToken,
                    TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously,
                    TaskScheduler.Current
                ).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            if (timeoutToken.IsCancellationRequested)
                throw new TimeoutException();
            throw;
        }
        catch (Exception)
        {
            throw new TimeoutException();
        }

        return buffer.Take(readCount).ToArray();
    }

    public static async IAsyncEnumerable<byte> CommsReadStream([EnumeratorCancellation] CancellationToken cancelToken, CommsInfo comms)
    {
        using var commsCopy = new CommsInfo(comms.PortName);
            
        var buffer = new byte[1024];
            
        var readTimeout = Properties.Settings.Default.ReadTimeout;

        comms.SetConfig(readTimeout, WriteTimeout);

        // Token to pass to lower level APIs that don't support normal timeouts
        // 20s As first frame can take a while, when we get the 1st byte we'll shorten the timeout to normal
        using var timeoutSource = new CancellationTokenSource(20_000);
            
        var timeoutToken = timeoutSource.Token;

        // Combining the tokens will allow the user to cancel still, but we can internally use a timeout
        using var combinedSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, timeoutToken);
            
        var combinedToken = combinedSource.Token;

        while (!combinedToken.IsCancellationRequested)
        {
            Exception? error = null;
            var readCount = 0;

            try
            {
                readCount = await Task.Factory.StartNew(
                    () => comms.BaseStream.Read(buffer, 0, buffer.Length),
                    combinedToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Current
                ).ConfigureAwait(false);

                timeoutSource.CancelAfter(readTimeout); // This resets when not cancelled yet
            }
            catch(OperationCanceledException? ex)
            {
                // For APIs that don't work properly with Timeout
                if (ex.CancellationToken == timeoutToken)
                {
                    error = new TimeoutException();
                }
                else if (ex.CancellationToken == cancelToken)
                {
                    error = ex;
                }
                else
                {
                    throw;
                }
            }
            catch(TimeoutException? ex)
            {
                error = ex;
            }

            for (var i = 0; i < readCount; ++i)
            {
                yield return buffer[i];
            }

            if (error is not null)
            {
                throw error;
            }
        }
    }

    public static IEnumerable<string> GetPorts()
    {
        // We're going to use the LocalMachine registry to find our serial ports
        var regexFindValidDevice = new Regex(@"USB\\VID_0403&PID_6015\\(?<sn>[A-Z\d]+)");               // Filter used for finding devices and serial numbers in the registry

#pragma warning disable CA1416 // Validate platform compatibility
        using var hklm = Microsoft.Win32.RegistryKey.OpenBaseKey(
            Microsoft.Win32.RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? Microsoft.Win32.RegistryView.Registry64 : Microsoft.Win32.RegistryView.Registry32
        );
#pragma warning restore CA1416 // Validate platform compatibility

#pragma warning disable CA1416 // Validate platform compatibility
        using var regActiveEnum = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\FTDIBUS\Enum");
#pragma warning restore CA1416 // Validate platform compatibility

        if (regActiveEnum is null) yield break;

        // FTDIBUS tells us how many items devices the driver can see
#pragma warning disable CA1416 // Validate platform compatibility
        var count = (int?)regActiveEnum.GetValue("Count") ?? 0;
#pragma warning restore CA1416 // Validate platform compatibility

        if (count == 0) yield break;

        var ftdiDevices = new List<string>(count);

        // Get serial number for each device
        // Loop is safe if array changes as long as order doesn't (TODO: Check if order can change)
        for (var i = 0; i < count; ++i)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var device = regActiveEnum.GetValue(i.ToString()) as string;
#pragma warning restore CA1416 // Validate platform compatibility

            var match = regexFindValidDevice.Match(device ?? string.Empty);

            if (match.Success)
            {
                ftdiDevices.Add(match.Groups["sn"].Value);
            }
        }

        // Use the device serial numbers to discover which ports they're attached to
        // Return a list of port names
        foreach (var device in ftdiDevices)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using var subKey = hklm.OpenSubKey($@"SYSTEM\CurrentControlSet\Enum\FTDIBUS\VID_0403+PID_6015+{device}A\0000\Device Parameters");
#pragma warning restore CA1416 // Validate platform compatibility

#pragma warning disable CA1416 // Validate platform compatibility
            var portName = subKey?.GetValue("PortName") as string;
#pragma warning restore CA1416 // Validate platform compatibility

            if (string.IsNullOrWhiteSpace(portName))
                continue;

            yield return portName;
        }
    }
}