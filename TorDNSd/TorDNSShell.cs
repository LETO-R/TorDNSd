using System;
using TorDNSd.Logging;
using TorDNSd.Utils;

namespace TorDNSd
{
    public class TorDNSShell
    {
        public InteractiveShell Shell { get; private set; }

        /// <summary>
        /// Create a new TorDNS shell with the default input prefix ('TORDNS> ')
        /// </summary>
        public TorDNSShell()
            : this(string.Empty)
        {
            Logger.OnLog += OnLog;
        }

        private void OnLog(LogSeverity arg1, string arg2)
        {
            if (arg1 == LogSeverity.Error || arg1 == LogSeverity.Fatal)
            {
                Shell.WriteLine("{0}: {1}", arg1.ToString().ToUpper(), arg2);
            }
            else
            {
                Shell.WriteLine("{0}: {1}", arg1.ToString().ToUpper(), arg2);
            }
        }

        /// <summary>
        /// Create a new TorDNS shell with the specified input prefix.
        /// </summary>
        /// <param name="inputPrefix"></param>
        public TorDNSShell(string inputPrefix)
        {
            // Create the shell
            Shell = new InteractiveShell(string.IsNullOrEmpty(inputPrefix) ? "TORDNS> " : inputPrefix);

            // Register the handlers
            Shell.OnEnter += OnEnter;
            Shell.OnTab += OnTab;
        }

        private void OnTab(string input)
        {
            if (input.Contains("*") || input.Contains(" ") || string.IsNullOrEmpty(input.Trim()))
            {
                return;
            }

            if (Glob.Match("quit", input + "*"))
            {
                Shell.ForceInput("quit");
                return;
            }

            if (Glob.Match("start", input + "*"))
            {
                Shell.ForceInput("start");
                return;
            }

            if (Glob.Match("stop", input + "*"))
            {
                Shell.ForceInput("stop");
                return;
            }

            if (Glob.Match("help", input + "*"))
            {
                Shell.ForceInput("help");
                return;
            }

            if (Glob.Match("config-reload", input + "*"))
            {
                Shell.ForceInput("config-reload");
                return;
            }
            if (Glob.Match("config-add", input + "*"))
            {
                Shell.ForceInput("config-add ");
                return;
            }
        }

        private void OnEnter(string input)
        {
            switch (input.Trim().ToLowerInvariant())
            {
                case "quit":
                    Shell.WriteLine("Goodbye!");
                    Shell.Halt();

                    break;
                case "help":
                    Shell.WriteLine("Available commands:");
                    Shell.WriteLine("");
                    Shell.WriteLine("quit");
                    Shell.WriteLine("\tquit the interactive shell and exit the application");
                    Shell.WriteLine("start");
                    Shell.WriteLine("\tenable and start the TorDNS server");
                    Shell.WriteLine("stop");
                    Shell.WriteLine("\tdisable and stop the TorDNS server");
                    Shell.WriteLine("config-add <key>=<value>");
                    Shell.WriteLine("\tadd the key/value pair to the active configuration");
                    Shell.WriteLine("config-reload");
                    Shell.WriteLine("\tapply the added configuration entries");

                    break;
                case "start":
                    if (Program.TorDNS.IsRunning)
                    {
                        Shell.WriteLine("Already running!");
                        return;
                    }

                    Program.TorDNS.Configuration.Append("server-enabled=true");
                    Program.TorDNS.Start();
                    Shell.WriteLine("TorDNS server started.");

                    break;
                case "stop":
                    if (!Program.TorDNS.IsRunning)
                    {
                        Shell.WriteLine("Not running!");
                        return;
                    }

                    Program.TorDNS.Configuration.Append("server-enabled=false");
                    Program.TorDNS.Stop();
                    Shell.WriteLine("TorDNS server stopped.");

                    break;

                case "config-reload":
                    Shell.WriteLine("Reloading the server using the active configuration.");

                     // Refresh to apply configuration changes
                    Program.TorDNS.Refresh();
                    break;
                default:
                    if (input.StartsWith("config-add ", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ConfigEntry entry = Program.TorDNS.Configuration.Append(input.Substring(11));

                        if (entry == null)
                        {
                            Shell.WriteLine("Invalid key/value pair.");
                        }
                        else
                        {
                            Shell.WriteLine("Added configuration. Key: {0} Value: {1}", entry.Key, entry.Value);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Start running the interactive shell. Will not return before the shell is closed.
        /// </summary>
        public void Run()
        {
            Console.WriteLine("Welcome to the TorDNS Interactive Shell");
            Console.WriteLine();
            Console.WriteLine("For the list of available commands type 'help'");
            Console.WriteLine();

            Shell.Run();
        }
    }
}
