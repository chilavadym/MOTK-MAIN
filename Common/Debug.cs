using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Common;

public class Debug : IDisposable
{
    public enum ETokens
    {
        LogStart = 0,
        Except,
        ExceptUnhandled,
        ScopeEnter,
        ScopeExit
    };


    static Debug()
    {
        TokenStrings = ((ETokens[])Enum.GetValues(typeof(ETokens))).ToDictionary(k => k, k => $"[{k:g}]");
        DefaultLogger = new("log.dat");
    }

    public Debug()
    {
        _logStream = null;
    }
    public Debug(string? logFile = null)
    {
        if (logFile != null) _logStream = File.OpenWrite(logFile);
        Log(TokenStrings[ETokens.LogStart], null, null, 0);
    }


    public void Log(Exception ex, [CallerFilePath] string? file = "", [CallerMemberName] string? member = "", [CallerLineNumber] int lineNum = 0)
    {
        Log(JsonConvert.SerializeObject(ex), file, member, lineNum);
    }
    public void Log(string? message = null, [CallerFilePath] string? file = "", [CallerMemberName] string? member = "", [CallerLineNumber] int lineNum = 0)
    {
        // Date is used multiple times, keep the same timestamp throughout
        var now = DateTime.Now;
        var thread = Thread.CurrentThread.ManagedThreadId;

        // Parameter sanitisation
        if (message is null)
            message = string.Empty;
        if (string.IsNullOrWhiteSpace(file))
            file = string.Empty;
        if (string.IsNullOrWhiteSpace(member))
            member = string.Empty;

        // No useful data
        if (message.Length + file.Length + member.Length == 0)
            return;
#if DEBUG
        foreach (var token in TokenStrings.Values)
        {
            if (message == token || file == token || member == token)
                return;
        }

        System.Diagnostics.Debug.WriteLine($"{now:u}\t{thread}\t{file}\t{lineNum}\t{member}");
        System.Diagnostics.Debug.Indent();
        System.Diagnostics.Debug.WriteLine(message);
        System.Diagnostics.Debug.Unindent();
#endif
        if (!DebugLogEnabled) return;
        if (_logStream == null) return;
                
        _logStream.Write(BitConverter.GetBytes(now.Ticks), 0, sizeof(long));
        _logStream.Write(BitConverter.GetBytes(thread), 0, sizeof(int));
        _logStream.Write(BitConverter.GetBytes(file.Length), 0, sizeof(int));
        _logStream.Write(file.Select(c => (byte)c).ToArray(), 0, file.Length);
        _logStream.Write(BitConverter.GetBytes(lineNum), 0, sizeof(int));
        _logStream.Write(BitConverter.GetBytes(member.Length), 0, sizeof(int));
        _logStream.Write(member.Select(c => (byte)c).ToArray(), 0, member.Length);
        _logStream.Write(BitConverter.GetBytes(message.Length), 0, sizeof(int));
        _logStream.Write(message.Select(c => (byte)c).ToArray(), 0, message.Length);
    }

    public void Flush()
    {
#if DEBUG
        System.Diagnostics.Debug.Flush();
#endif
        _logStream?.Flush();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
            
        if (_logStream is null) return;
            
        _logStream.Flush();
        _logStream.Dispose();
        _logStream = null;
    }


    public static Debug DefaultLogger { get; }
    private static IReadOnlyDictionary<ETokens, string> TokenStrings { get; }


    public bool DebugLogEnabled
    {
        get => _logStream is not null && _logEnabled;
        set
        {
            if (_logStream is null)
            {
                throw new InvalidOperationException("No output file specified on initialise");
            }

            _logEnabled = value;
        }
    }


    private bool _logEnabled;
    private FileStream? _logStream;
}