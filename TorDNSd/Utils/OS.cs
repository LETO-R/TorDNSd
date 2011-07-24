using System;
using System.Security.Principal;
using Mono.Unix.Native;

namespace TorDNSd.Utils
{
    /// <summary>
    /// Operating System utility class
    /// </summary>
    public static class OS
    {
        /// <summary>
        /// Get the current platform.
        /// </summary>
        public static PlatformID Platform
        {
            get
            {
                return Environment.OSVersion.Platform;
            }
        }


        /// <summary>
        /// True if ran on under Windows (or related ie XBox)
        /// </summary>
        public static readonly bool IsWindows;

        /// <summary>
        /// True if ran on linux / unix.
        /// </summary>
        public static readonly bool IsUnix;

        /// <summary>
        /// True if ran on MacOSX
        /// </summary>
        public static readonly bool IsMacOSX;

        /// <summary>
        /// Is the current application run in elevated mode (root on non-windows systems)
        /// </summary>
        public static bool IsElevated
        {
            get
            {
                // Non-windows system, check the process uid
                if (!IsWindows)
                {
                    return Syscall.getuid() == 0;
                }

                WindowsIdentity currentIdendity;

                if ((currentIdendity = WindowsIdentity.GetCurrent()) == null)
                {
                    // Couldn't get the current identity, assume not elevated
                    return false;
                }

                // Check the role
                return (new WindowsPrincipal(currentIdendity)).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Initialize the OS class.
        /// </summary>
        static OS()
        {
            // Ran on Windows?
            IsWindows = (Platform == PlatformID.Win32NT || Platform == PlatformID.Win32S ||
                         Platform == PlatformID.Win32Windows || Platform == PlatformID.WinCE ||
                         Platform == PlatformID.Xbox);

            // Ran on MacOSX?
            IsMacOSX = !IsWindows && Platform == PlatformID.MacOSX;

            // Ran on UNIX (Linux, ...)?
            IsUnix = !IsMacOSX && Platform == PlatformID.Unix;
        }
    }
}
