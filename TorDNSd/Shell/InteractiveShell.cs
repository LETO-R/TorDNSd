using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveShell;
using TorDNSd.Logging;
using TorDNSd.Utils;
using Glob = InteractiveShell.Utils.Glob;

namespace TorDNSd.Shell
{
    public class InteractiveShell : InteractiveShellBase
    {
        private readonly List<string> _autocompleteList = new List<string>();

        public InteractiveShell()
        {
            InitializeAutoComplete();
            Logger.OnLog += OnLog;
        }

        private void OnLog(LogSeverity arg1, string arg2)
        {
            if (arg1 == LogSeverity.Error || arg1 == LogSeverity.Fatal)
            {
                WriteLine("{0}: {1}", arg1.ToString().ToUpper(), arg2);
            }
            else
            {
                WriteLine("{0}: {1}", arg1.ToString().ToUpper(), arg2);
            }
        }

        private void InitializeAutoComplete()
        {
            _autocompleteList.Add("quit");
            _autocompleteList.Add("start");
            _autocompleteList.Add("stop");
            _autocompleteList.Add("help");
            _autocompleteList.Add("config-add");
            _autocompleteList.Add("config-refresh");

            _autocompleteList.Sort();
        }

        public override string ShellPrefix
        {
            get { return "TorDNSd> "; }
        }


        /// <summary>
        /// Called when the user presses the 'tab' key. Return a not-null value to override the currently set input.
        /// </summary>
        protected override string OnAutoComplete(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            return _autocompleteList.FirstOrDefault(c => Glob.Match(c, input + "*"));
        }

        /// <summary>
        /// Called when the user presses the 'enter' key to accept the input.
        /// </summary>
        protected override void OnEnter(string input)
        {
            switch (input.Trim().ToLowerInvariant())
            {
                case "quit":
                    WriteLine("Goodbye!");
                    Halt();

                    break;
                case "help":
                    WriteLine("Available commands:");
                    WriteLine("");
                    WriteLine("quit");
                    WriteLine("\tquit the interactive shell and exit the application");
                    WriteLine("start");
                    WriteLine("\tenable and start the TorDNS server");
                    WriteLine("stop");
                    WriteLine("\tdisable and stop the TorDNS server");
                    WriteLine("config-add <key>=<value>");
                    WriteLine("\tadd the key/value pair to the active configuration");
                    WriteLine("config-refresh");
                    WriteLine("\tapply the added configuration entries");

                    break;
                case "start":
                    if (Program.TorDNS.IsRunning)
                    {
                        WriteLine("Already running!");
                        return;
                    }

                    Program.TorDNS.Configuration.Append("server-enabled=true");
                    Program.TorDNS.Start();
                    WriteLine("TorDNS server started.");

                    break;
                case "stop":
                    if (!Program.TorDNS.IsRunning)
                    {
                        WriteLine("Not running!");
                        return;
                    }

                    Program.TorDNS.Configuration.Append("server-enabled=false");
                    Program.TorDNS.Stop();
                    WriteLine("TorDNS server stopped.");

                    break;

                case "config-refresh":
                    WriteLine("Applying recent configuration changes.");

                    // Refresh to apply configuration changes
                    Program.TorDNS.RefreshConfiguration();
                    break;
                case "cache-clear":
                    WriteLine("Clearing the cache...");

                    // Refresh to apply configuration changes
                    Program.TorDNS.ClearCache();
                    break;
                default:
                    if (input.StartsWith("config-add ", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ConfigEntry entry = Program.TorDNS.Configuration.Append(input.Substring(11));

                        if (entry == null)
                        {
                            WriteLine("Invalid key/value pair.");
                        }
                        else
                        {
                            WriteLine("Added configuration. Key: {0} Value: {1}", entry.Key, entry.Value);
                        }
                    }
                    break;
            }
        }

        protected override void OnRun()
        {
            Console.WriteLine("Welcome to the TorDNS Interactive Shell");
            Console.WriteLine();
            Console.WriteLine("For the list of available commands type 'help'");
            Console.WriteLine();
        }
    }
}
