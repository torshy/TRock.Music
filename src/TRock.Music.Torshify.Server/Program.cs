using System;
using System.Diagnostics;
using System.Threading;

using SignalR;

using Torshify;

using TRock.Music.Torshify.Server.Hubs;

namespace TRock.Music.Torshify.Server
{
    class Program
    {
        #region Methods

        static void Main(string[] args)
        {
            EnsureProcessIsViableSpotifyHost();

            SpotifyLibExtractor.ExtractResourceToFile("TRock.Music.Torshify.Server.libspotify.dll", "libspotify.dll");

            Trace.Listeners.Add(new ConsoleTraceListener());

            var session =
               SessionFactory
                   .CreateSession(
                       Constants.ApplicationKey,
                       Constants.CacheFolder,
                       Constants.SettingsFolder,
                       Constants.UserAgent)
                   .SetPreferredBitrate(Bitrate.Bitrate320k);

            var wait = new ManualResetEvent(false);
            session.LoginComplete += (sender, eventArgs) =>
            {
                Trace.WriteLine(eventArgs.Status == Error.OK ? "Logged in" : eventArgs.Message);
                wait.Set();
            };

            session.Login(args[0], args[1]);

            if (!wait.WaitOne(5000))
            {
                Trace.WriteLine("Timed out");
                Environment.Exit(-1);
            }

            var player = new TorshifySongPlayer(session);

            GlobalHost.DependencyResolver.Register(typeof(ISongPlayer), () => player);

            var server = new SignalR.Hosting.Self.Server("http://localhost:8081/");
            server.MapHubs();
            server.Start();

            var hub = GlobalHost.ConnectionManager.GetHubContext<TorshifyHub>();

            player.IsPlayingChanged += (sender, eventArgs) => hub.Clients.IsPlayingChanged(eventArgs.NewValue);
            player.IsMutedChanged += (sender, eventArgs) => hub.Clients.IsMutedChanged(eventArgs.NewValue);
            player.Buffering += (sender, eventArgs) => hub.Clients.Buffering(eventArgs);
            player.Progress += (sender, eventArgs) => hub.Clients.Progress(eventArgs);
            player.CurrentSongChanged += (sender, eventArgs) => hub.Clients.CurrentSongChanged(eventArgs);
            player.CurrentSongCompleted += (sender, eventArgs) => hub.Clients.CurrentSongCompleted(eventArgs.Song);
            player.VolumeChanged += (sender, eventArgs) => hub.Clients.VolumeChanged(eventArgs);

            Console.ReadLine();
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