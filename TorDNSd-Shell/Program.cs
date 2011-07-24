using System;
using System.Collections.Generic;
using System.Linq;

namespace TorDNSd.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> argsList = args.ToList();

            // Check if the interactive argument is passed, if not, force-add it
            if (!argsList.Contains("--interactive", StringComparer.InvariantCultureIgnoreCase) && !argsList.Contains("-i", StringComparer.InvariantCultureIgnoreCase))
            {
                argsList.Add("--interactive");
            }

            // Run the TorDNSd' main
            TorDNSd.Program.Main(argsList.ToArray());
        }
    }
}
