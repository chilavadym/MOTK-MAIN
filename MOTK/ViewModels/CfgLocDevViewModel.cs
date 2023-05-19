using MOTK.Models;
using MOTK.Services;
using ReactiveUI;
using Serial;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MOTK.ViewModels;

public class CfgLocDevViewModel : ViewModelBase
{
    private ObservableCollection<string> _cables;

    public ObservableCollection<string> Cables
    {
        get { return _cables; }
        set => this.RaiseAndSetIfChanged(ref _cables, value, nameof(Cables));

    }
    private Func<string, Serial.Device> DeviceFactory { get; }
    private Serial.Device selectedDevice = null;
    private int indexCurrentImage = 0;
    private CancellationTokenSource srcCancelIo = new();
    private readonly ManualResetEvent evtClosing = new(false);
    private readonly AutoResetEvent evtIoReady = new(true);
    private SynchronizationContext uiContext = null;
    private string _selectedCable;

    public string SelectedCable
    {
        get { return _selectedCable; }
        set
        {
            if (value != null)
            {
                IsDeviceListBoxVisible = true;
                CableEnabled = false;
            }
            else
            {
                IsDeviceListBoxVisible = false;
                CableEnabled = true;
            }

            this.RaiseAndSetIfChanged(ref _selectedCable, value, nameof(SelectedCable));
        }
    }
    private bool _cableEnabled;

    public bool CableEnabled
    {
        get { return _cableEnabled; }
        set => this.RaiseAndSetIfChanged(ref _cableEnabled, value);
    }

    private bool _isDeviceListBoxVisible;

    public bool IsDeviceListBoxVisible
    {
        get { return _isDeviceListBoxVisible; }
        set => this.RaiseAndSetIfChanged(ref _isDeviceListBoxVisible, value, nameof(IsDeviceListBoxVisible));
    }

    private ObservableCollection<Serial.Device> _devices;

    public ObservableCollection<Serial.Device> Devices
    {
        get { return _devices; }
        set => this.RaiseAndSetIfChanged(ref _devices, value, nameof(Devices));
    }
    private bool _deviceSelected;

    public bool DeviceSelected
    {
        get => _deviceSelected;
        set => this.RaiseAndSetIfChanged(ref _deviceSelected, value);
    }

    private Serial.Device _selectedDevice;

    public Serial.Device SelectedDevice
    {
        get { return _selectedDevice; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDevice, value, nameof(SelectedDevice));
            //This is sufficent as we are showint name differently
            // _oilTestModel.SensorSerialNumber = $"{SelectedDevice.SerialNumber.Actual:0000000} v{_selectedDevice.FwVersion.Actual:0.00} @{_selectedDevice.PortName}";
            _oilTestModel.SensorSerialNumber = $"{SelectedDevice.SerialNumber.Actual:0000000}";
            DeviceSelected = true;
            ReleasePortsAndDevices();
        }
    }

    GeneralSettingsDatabase db = new GeneralSettingsDatabase();
    public ReactiveCommand<object, System.Reactive.Unit> ChangeSelection { get; }

    private OilTest? _oilTestModel;
    public CfgLocDevViewModel(OilTest? oilTestModel)
    {
        _oilTestModel = oilTestModel;
        Cables = new ObservableCollection<string>();
        db.ReadFromDatabase();
        ChangeSelection = ReactiveCommand.Create<object>(OnChageSelection);
        InitializePorts();
        InitializeDevices();

    }
    public CfgLocDevViewModel(ConfigurationModel model)
    {
        InitializePorts();
        InitializeDevices();
        ChangeSelection = ReactiveCommand.Create<object>(OnChageSelection);
        Cables = new ObservableCollection<string>();
        //SelectedCable = model.Cable;
        SelectedDevice = model.Device;
    }
    private void OnChageSelection(object obj)
    {
        if (obj is string port)
        {
            SelectedCable = port;
        }
    }

    private void InitializeDevices()
    {
        Devices = new ObservableCollection<Serial.Device>();


    }
    private Serial.Sensor SensorFactory(string portName)
    {
        return new Serial.Sensor(portName);
    }
    private Serial.Display DisplayFactory(string portName)
    {
        return new Serial.Display(portName);
    }



    private void InitializePorts()
    {


        Task.Run(() => SearchForPortsAndDevicesAsync());

    }
    private void ReleasePortsAndDevices()
    {
        foreach (var device in Devices)
        {
            Cables.Remove(device.PortName);
            Devices.Remove(device);
            device.Dispose();
        }

        evtClosing.Close();
        evtClosing.Dispose();
    }

    /******************************
**     Private Functions     **
******************************/

    private async Task SearchForPortsAndDevicesAsync()
    {
        using CancelAndRelockIo ioLock = new(evtIoReady, ref srcCancelIo);

        const int loopDelayTime = 200; //ms

        // Load in our existing devices and check they're still there
        var tasks = new Dictionary<Serial.Device, Task>();
        lock (Devices)
        {
            foreach (var device in Devices)
            {
                tasks.Add(device, device.Initialize(ioLock.Token));
            }
        }

        var lastLoop = DateTime.Now;

        try
        {
            // Check for ports, then check for devices
            while (!evtClosing.WaitOne(0))
            {
                ioLock.Token.ThrowIfCancellationRequested();

                // Porks on current machine, convert to list for simpler code
                List<string> portsThisRound = new List<string>();

                if(db.GeneralSettings == null)
                {
                    portsThisRound = Serial.Common.GetPorts().ToList();
                }
                else if (db.GeneralSettings.AllowThirdPartySerialPorts)
                {
                    portsThisRound = SerialPort.GetPortNames().ToList();
                }
                else
                {
                    portsThisRound = Serial.Common.GetPorts().ToList();
                }

                // Select first found port on the list
                if(portsThisRound.Count > 0)
                {
                    SelectedCable = portsThisRound[0];
                }

                // Add new ports
                foreach (var portName in portsThisRound)
                {
                    // If already in our list, move to the next port name
                    if (tasks.Keys.Any(device => device.PortName == portName))
                        continue;

                    // Port doesn't exist, it's safe to see if we can open it
                    try
                    {
 
                        using var port = new System.IO.Ports.SerialPort(portName);
                        port.Open();
                        
                       

                        // Port is open
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        continue;   // We can't open this port, ignore the port
                    }
                    Cables.Add(portName);


                    var device = SensorFactory(portName);
                    tasks.Add(device, device.Initialize(ioLock.Token));
                }

                if (tasks.Count == 0)
                {
                    await Task.Delay(loopDelayTime);
                    continue;
                }

                { // Scoping brace
                    Task task = null;
                    Serial.Device device = null;

                    try
                    {
                        using var loopTimeout = CancellationTokenSource.CreateLinkedTokenSource(ioLock.Token);
                        loopTimeout.CancelAfter(loopDelayTime);

                        // Build a task which internally runs Task.WhenAny, using the task factory so that we can
                        // effictively add a timeout onto Task.WhenAny.
                        async Task<Task> Wait() => await Task.WhenAny(tasks.Values).ConfigureAwait(false);

                        // The await here is for Wait() / Task.WhenAny() to complete, which returns the
                        // task that completed, i.e. the we're interested in.
                        task = await Task.Run(Wait, loopTimeout.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // No sensors responsed yet, but go check for new ports
                        continue;
                    }

                    device = tasks.FirstOrDefault(kvp => kvp.Value == task).Key;
                    try
                    {
                        // Won't block because task is already completed
                        await task.ConfigureAwait(false);

                        // Refresh the retry
                        tasks[device] = device.Initialize(ioLock.Token);

                        // If we're here, the taks completed, sensor is alive
                        SelectedDevice = device;
                        lock (Devices)
                        {
                            if (!Devices.Contains(device))
                            {
                              
                                Devices.Add(device);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        switch (ex)
                        {
                            case FormatException:
                            case TimeoutException:
                                // Can only happen if port was open.
                                // TimeoutException means no device response, so remove this device from UI list
                                lock (Devices)
                                {
                                    Devices.Remove(device);

                                }

                                // Refresh the retry
                                tasks[device] = device.Initialize(ioLock.Token);
                                break;

                            default:
                                // On any other error, remove port and devices
                                foreach (var dev in tasks.Keys.ToArray())
                                {
                                    if (portsThisRound.Contains(dev.PortName))
                                        continue;

                                    if (tasks.Remove(dev))
                                    {
                                        lock (Devices)
                                        {
                                            Devices.Remove(dev);
                                        }

                                        uiContext.Post(obj =>
                                        {
                                            if (obj is Serial.Device cbDevice)
                                            {
                                                Cables.Remove(cbDevice.PortName);
                                                Devices.Remove(cbDevice);
                                                cbDevice.Dispose();
                                            }
                                        }, dev);
                                    }
                                }
                                break;
                        }
                    }
                }   // Scope
            } // While not cancelled
        }
        catch (OperationCanceledException)
        {
            if (!evtClosing.WaitOne(0))
            {

            }

            throw;
        }
        finally
        {
            lock (Devices)
            {
                foreach (var dev in tasks.Keys.ToArray())
                {
                    // Dispose our temporary devices to free the COM ports
                    if (!Devices.Contains(dev))
                        dev.Dispose();
                }
            }
        }
    }


    protected string GenerateDeviceKey(Serial.Device device)
    {
        if (Equals(device, null))
            throw new ArgumentNullException(nameof(device));
        return $"{device.SerialNumber.Actual:0000000} v{device.FwVersion.Actual:0.00} @{device.PortName}";
    }
    private class CancelAndRelockIo : CancellationTokenSource, IDisposable
    {
        private bool isDisposed;
        private readonly AutoResetEvent evtReady;

        public CancelAndRelockIo(AutoResetEvent evtIoReady, ref CancellationTokenSource srcCancelIo)
        {
            if (evtIoReady is null)
                throw new ArgumentNullException(nameof(evtIoReady));
            if (srcCancelIo is null)
                throw new ArgumentNullException(nameof(srcCancelIo));

            lock (srcCancelIo)
            {
                evtReady = evtIoReady;
                try
                {
                    srcCancelIo.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore
                }
#pragma warning disable CS0728 // Possibly incorrect assignment to local which is the argument to a using or lock statement
                srcCancelIo = this;
#pragma warning restore CS0728
            }
            evtIoReady.WaitOne();
        }

        protected override void Dispose(bool disposing)
        {
            // Just lock something so we don't set twice on race-condition
            lock (evtReady)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    evtReady.Set();
                }
            }
            base.Dispose(disposing);
        }
    }
}