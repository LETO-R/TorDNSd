using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using TorDNSd.Logging;
using TorDNSd.Utils;

namespace TorDNSd
{
    public static class Program
    {
        public static bool InQuietMode;
        public static bool InVerboseMode;
        public static bool InInteractiveMode;

        /// <summary>
        /// Program name.
        /// </summary>
        public const string NAME = "tordnsd";

        /// <summary>
        /// Executable name.
        /// </summary>
        public const string EXE_NAME = "tordnsd.exe";

        /// <summary>
        /// Program version
        /// </summary>
        public static Version Version = typeof (Program).Assembly.GetName().Version;

        /// <summary>
        /// Default config file name. Will be looked for @ current working dir if no config file was specified.
        /// </summary>
        public static string DefaultConfigFile = "tordnsd.conf";

        /// <summary>
        /// After initialization it contains the list of valid configuration options.
        /// </summary>
        public static List<string> ValidSettings;

        /// <summary>
        /// After initialization it contains the list of valid program options.
        /// </summary>
        public static List<string> ValidOptions;

        /// <summary>
        /// TorDNS server.
        /// </summary>
        public static TorDNS TorDNS;

        /// <summary>
        /// Program point of entry.
        /// </summary>
        /// <param name="args">Supplied arguments.</param>
        private static void Main(string[] args)
        {
            // Initialize the program
            Initialize();

            // Process the arguments
            if (!ProcessArguments(args))
            {
                return;
            }

            if (!InQuietMode)
            {
                // Not in quiet mode, enable logging
                Logger.OnLog += OnLog;
            }

            // Ran from console
            TorDNS = new TorDNS();

            IEnumerable<string> configFiles =
                args.Where(a => a.StartsWith("--config=", StringComparison.InvariantCultureIgnoreCase)).Select(
                    a => a.Substring(9));

            // If no config files were specified, add 'tordnsd.conf' as the default
            if (configFiles.Count() == 0)
            {
                configFiles = new[] {Path.Combine(Directory.GetCurrentDirectory(), "tordnsd.conf")};
            }

            // First load the config files
            foreach (var configFile in configFiles)
            {
                try
                {
                    TorDNS.Configuration.Append(ConfigCollection.FromFile(configFile));
                }
                catch (Exception ex)
                {
                    Logger.Log(LogSeverity.Error, "An exception occured while loading the configuration from: {0}", configFile);

                    Logger.Log(LogSeverity.Debug, "{0}", ex.Message);
                    Logger.Log(LogSeverity.Debug, "{0}", ex.StackTrace);
                }
            }

            // Then add the other values
            foreach (var setting in args.Where(a => ValidSettings.Any(s => a.StartsWith("--" + s + "=", StringComparison.InvariantCultureIgnoreCase))))
            {
                TorDNS.Configuration.Append(setting);
            }

            // Launch the TorDNS server
            // Will in a seperate thread
            if (!InInteractiveMode)
            {
                try
                {
                    if (!TorDNS.Start())
                    {
                        Logger.Log(LogSeverity.Error,
                                   "Unable to launch the TorDNS server. Make sure that the 'server-enabled' setting is set to 'true'.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogSeverity.Error, "An exception occured while launching the TorDNS server. {0}", ex.Message);
                    Logger.Log(LogSeverity.Debug, ex.StackTrace);
                    return; // Failure :(
                }
                while (true)
                {
                    // Loop indefinitly (until killed)
                    Thread.Sleep(1000);
                }
            }
            
            
            // Interactive mode
            try
            {
                if (TorDNS.Start())
                {
                    Logger.Log(LogSeverity.Info, "The TorDNS server is now running.");
                }
                else
                {
                    Logger.Log(LogSeverity.Info, "The TorDNS server is currently disabled.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogSeverity.Error, "An exception occured while launching the TorDNS server. {0}", ex.Message);
                Logger.Log(LogSeverity.Debug, ex.StackTrace);
            }

            Console.WriteLine("Entering interactive mode...");
            Console.WriteLine();

            Logger.OnLog -= OnLog;

            (new TorDNSShell()).Run();
        }

        private static void OnLog(LogSeverity arg1, string arg2)
        {
            if (arg1 == LogSeverity.Error || arg1 == LogSeverity.Fatal)
            {
                Console.Error.WriteLine("{0}: {1}", arg1.ToString().ToUpper(), arg2);
            }
            else
            {
                Console.WriteLine("{0}: {1}", arg1.ToString().ToUpper(), arg2);
            }
        }

        /// <summary>
        /// Initialize TorDNSd basics
        /// </summary>
        /// <param name="args"></param>
        private static void Initialize()
        {
            // Catch unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Create the list of valid configuration options (will be prefixed with --)
            ValidSettings = new List<string>();

            ValidSettings.Add("server-bindip");
            ValidSettings.Add("server-enabled");

            ValidSettings.Add("socks-ip");
            ValidSettings.Add("socks-port");
            ValidSettings.Add("socks-enabled");

            ValidSettings.Add("dns-direct");
            ValidSettings.Add("dns-direct-timeout");
            ValidSettings.Add("dns-proxy");
            ValidSettings.Add("dns-proxy-timeout");
            ValidSettings.Add("dns-cache-enabled");
            ValidSettings.Add("dns-cache-ttl");
            ValidSettings.Add("dns-cache-size");

            ValidSettings.Add("filter-proxy");
            ValidSettings.Add("filter-skip-proxy");
            ValidSettings.Add("filter-reject");

            ValidSettings.Add("remap");
            ValidSettings.Add("remap-ttl");

            // Create the list of valid program options
            ValidOptions = new List<string>();
            ValidOptions.Add("-i");
            ValidOptions.Add("--interactive");

            ValidOptions.Add("-v");
            ValidOptions.Add("--verbose");

            ValidOptions.Add("-q");
            ValidOptions.Add("--quiet");

            ValidOptions.Add("-h");
            ValidOptions.Add("--help");
            ValidOptions.Add("--config=");
        }

        /// <summary>
        /// Get a list of invalid arguments.
        /// </summary>
        private static string[] GetInvalidArguments(string[] args)
        {
            // No arguments are considered valid
            if (args == null || args.Length == 0)
            {
                return new string[0];
            }

            List<string> invalidArguments = new List<string>();

            foreach (var arg in args)
            {
                if (!ValidOptions.Any(o => (o.EndsWith("=") && arg.StartsWith(o, StringComparison.InvariantCultureIgnoreCase)) || arg.Equals(o, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (!ValidSettings.Any(o => arg.StartsWith("--" + o, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        invalidArguments.Add(arg);
                    }
                }
            }

            return invalidArguments.ToArray();
        }


        private static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("USAGE: {0}{1} [options]", Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX ? "mono " : string.Empty, EXE_NAME);
            Console.WriteLine("Options:");
            Console.WriteLine();

            Console.WriteLine("-h, --help");
            Console.WriteLine("\tprint this usage message");

            Console.WriteLine("-v, --verbose");
            Console.WriteLine("\tenable verbose mode");

            Console.WriteLine("-q, --quiet");
            Console.WriteLine("\tenable quiet mode");

            Console.WriteLine("-i, --interactive");
            Console.WriteLine("\topen the interactive shell");

            Console.WriteLine("--config=<file>");
            Console.WriteLine("\tload tordnsd configuration from the specified file");
            Console.WriteLine("\tcan be specified multiple times");
            Console.WriteLine("\tby default it'll look for tordnsd.conf in the current working dir");
            Console.WriteLine();

            Console.WriteLine("Program settings can be specified or loaded from a config file.");
            Console.WriteLine("Program settings specified as an argument will be added last.");
            Console.WriteLine();

            foreach (var setting in ValidSettings)
            {
                Console.WriteLine("--{0}=<value>", setting);
            }
            Console.WriteLine();
        }

        private static bool ProcessArguments(string[] args)
        {
            // Get the list of invalid arguments (if any)
            string[] invalidArguments = GetInvalidArguments(args);

            // see if the user specified invalid arguments (if any)
            if (invalidArguments.Length > 0)
            {
                foreach (var arg in invalidArguments)
                {
                    Console.WriteLine("{0}: Invalid argument: {1}", NAME, arg);
                }

                ShowUsage();

                return false;
            }

            InInteractiveMode =
                args.Any(
                    a =>
                    a.Equals("--interactive", StringComparison.InvariantCultureIgnoreCase) ||
                    a.Equals("-i", StringComparison.InvariantCultureIgnoreCase));

            InQuietMode =
                args.Any(
                    a =>
                    a.Equals("--quiet", StringComparison.InvariantCultureIgnoreCase) ||
                    a.Equals("-q", StringComparison.InvariantCultureIgnoreCase));

            InVerboseMode =
                args.Any(
                    a =>
                    a.Equals("--verbose", StringComparison.InvariantCultureIgnoreCase) ||
                    a.Equals("-v", StringComparison.InvariantCultureIgnoreCase));

            Logger.MinLogSeverity = InVerboseMode ? LogSeverity.Trace : LogSeverity.Info;

            if (args.Any(
                    a =>
                    a.Equals("--help", StringComparison.InvariantCultureIgnoreCase) ||
                    a.Equals("-h", StringComparison.InvariantCultureIgnoreCase)))
            {
                ShowUsage();
                return false;
            }

            if (InInteractiveMode && InQuietMode)
            {
                Console.WriteLine("{0}: Cannot be both in interactive and quiet mode.", NAME);

                return false;
            }

            if (InVerboseMode && InQuietMode)
            {
                Console.WriteLine("{0}: Cannot be both in verbose and quiet mode.", NAME);

                return false;
            }

            return true;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
                Logger.Log(LogSeverity.Error, "Unhandled Exception: {0}", ex.Message);
                Logger.Log(LogSeverity.Error, ex.StackTrace);
            }
        }
    }
}
