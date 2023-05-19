// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;
using MOTK.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MOTK.Statics;

internal class ApplicationStuff
{
    private static OilDatabase? _oilDatabase;

    internal ApplicationStuff(OilDatabase? oilDatabase)
    {
        _oilDatabase = oilDatabase;
    }

    internal static string? CommandLine { get; private set; }
    internal static Version? Version { get; private set; }


    internal static string GlobalSettingsSaveFile { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Tan Delta Systems",
        "UserData",
        "settings.json");
    internal static string AppSettingsSaveFile { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Tan Delta Systems",
        "UserData",
        "Mobile Oil Tester (MOT) Kit",
        "settings.json");


    internal static Settings GlobalSettings { get; set; } = new();
    internal static Settings AppSettings { get; set; } = new();


    /// <summary>The main entry point for the application.</summary>
    [STAThread]
    private static int Main(string[]? args)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version;

        // Register exception handle
        AppDomain.CurrentDomain.UnhandledException += This_UnhandledException;

        // Parse command line parameters (only used for double-click DB file update)
        if (args is not null && args.Length > 0)
        {
            CommandLine = args.Aggregate((a, b) => a + ' ' + b);

            try
            {
                ParseArgs(args);
                return 0;
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return -1;
            }
            catch
            {
                return -2;
            }
        }

        // Run primary application
        CommandLine = string.Empty;

        return 0;
    }

    private static void ParseArgs(IReadOnlyList<string>? args)
    {
        if (args == null) return;

        for (var i = 0; i < args.Count; ++i)
        {
            switch (args[i])
            {
                case "--updateOilDb":
                    UpdateOilDatabase(args[++i]);
                    break;
                default:
                    if (args[i].EndsWith(OilDatabase.DatabaseFileExtension))
                        UpdateOilDatabase(args[i]);
                    else
                        throw new KeyNotFoundException($"Unknown parameter '{args[i]}'");
                    break;
            }
        }
    }

    private static void UpdateOilDatabase(string databaseFile)
    {
        try
        {
            if (databaseFile == OilDatabase.DatabasePath)
            {
                return;
            }

            OilDatabase.Update(databaseFile);
        }
        catch (Exception ex)
        {
            Debug.DefaultLogger.Log(ex);
        }
    }

    private static void This_UnhandledException(object sender, UnhandledExceptionEventArgs arg)
    {
        Debug.DefaultLogger.Log($"UNHANDLED EXCEPTION OCCURED: {arg.ExceptionObject}");
            
        if (arg.IsTerminating)
        {
            var exObject = (Exception)arg.ExceptionObject;

            while (exObject is TargetInvocationException)
            {
                exObject = exObject.InnerException;
            }

            //await TryPostError(exObject);

            try
            {
                Debug.DefaultLogger.Log((Exception)arg.ExceptionObject);

                if (exObject != null)
                {
                    var errCode = GenerateErrorCode(exObject);
                    
                    if (arg.ExceptionObject is TargetException)
                    {
                        errCode = $"{CaseEncodeScope(arg.ExceptionObject.GetType().FullName)}/{errCode}";
                    }
                }

                try
                {
                    var filename = $@"{Directory.GetCurrentDirectory()}\error_log.txt";
                        
                    using (var file = new StreamWriter(filename, true))
                    {
                        file.WriteLine($"==========  {DateTime.Now:yyyy-MM-dd HH:mm:ss}  ==========\n{exObject}");
                        file.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        Debug.DefaultLogger.Flush();
    }

    private static string GenerateErrorCode(Exception ex)
    {
        var sourceErr = ex;

        while (sourceErr.InnerException is not null)
        {
            sourceErr = sourceErr.InnerException;
        }

        if (Equals(sourceErr, ex))
        {
            return GenerateSubErrorCode(ex);
        }

        return $"{GenerateSubErrorCode(sourceErr)}/{GenerateSubErrorCode(ex)}";
    }
        
    private static string GenerateSubErrorCode(Exception ex)
    {
        var exType = ex.GetType();
            
        var exNameStr = string.IsNullOrWhiteSpace(exType.FullName) ? "0" : CaseEncodeScope(exType.FullName);

        var stackFrames = ex.StackTrace?.Split('\n');
            
        var topFrame = string.Empty;

        if (stackFrames != null)
        {
            foreach (var frame in stackFrames)
            {
                if (frame.Contains(nameof(Statics)))
                {
                    topFrame = frame.Replace(" at ", string.Empty);
                }
            }
        }

        var exFrameStr = string.IsNullOrWhiteSpace(topFrame) ? "0" : CaseEncodeScope(topFrame);

        return $"{exNameStr}@{exFrameStr}";
    }
        
    private static string CaseEncodeScope(string? scope)
    {
        var output = string.Empty;
        ;
        var builder = new StringBuilder();
            
        scope = scope?.Trim(' ', '.');

        var firstLetter = true;

        if (scope == null) return output;
            
        foreach (var t in scope)
        {
            if (char.IsLetter(t))
            {
                if (firstLetter)
                {
                    builder.Append(char.ToUpperInvariant(t));
                    firstLetter = false;
                }
                else if (char.IsUpper(t))
                {
                    builder.Append(char.ToLowerInvariant(t));
                }
            }
            else if (t == '.')
            {
                firstLetter = true;
            }
            else if (!char.IsNumber(t) && t != '_')
            {
                break;
            }
        }

        output = builder.ToString();

        return output;
    }
}