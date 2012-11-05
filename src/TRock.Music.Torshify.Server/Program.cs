using System;
using System.Diagnostics;
using System.Reflection;

namespace TRock.Music.Torshify.Server
{
    class Program
    {
        #region Methods

        static void Main(string[] args)
        {
            EnsureProcessIsViableSpotifyHost();

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                String resourceName = "TRock.Music.Torshify.Server.Libs." + new AssemblyName(e.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
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