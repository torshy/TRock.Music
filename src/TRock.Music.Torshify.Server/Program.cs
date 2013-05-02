using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TRock.Music.Torshify.Server
{
    class Program
    {
        #region Enumerations

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }

        #endregion Enumerations

        #region Methods

        [DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);

        static void Main(string[] args)
        {
            EnsureProcessIsViableSpotifyHost();
            var errorMode = SetErrorMode(ErrorModes.SYSTEM_DEFAULT);
            SetErrorMode(errorMode | ErrorModes.SEM_NOGPFAULTERRORBOX);

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                String resourceName = "TRock.Music.Torshify.Server.Libs." + new AssemblyName(e.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            var bootstrapper = new TorshifyServerBootstrapper();
            bootstrapper.Run(args);
        }

        private static void EnsureProcessIsViableSpotifyHost()
        {
            var proc = Process.GetCurrentProcess();

            if (proc.MainModule.FileName.EndsWith("vshost.exe"))
            {
                throw new InvalidOperationException("Turn off vshost to get this thing going");
            }

            if (Environment.Is64BitProcess)
            {
                throw new InvalidOperationException("Need to be run under x86-mode");
            }
        }

        #endregion Methods
    }
}