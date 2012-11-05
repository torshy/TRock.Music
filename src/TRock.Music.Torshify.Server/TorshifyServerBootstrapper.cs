using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

using Nancy.Hosting.Self;

using NDesk.Options;

using SignalR;

using Torshify;

using TRock.Music.Torshify.Server.Hubs;

namespace TRock.Music.Torshify.Server
{
    public class TorshifyServerBootstrapper
    {
        #region Fields

        const int SW_HIDE = 0;

        #endregion Fields

        #region Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Run(string[] args)
        {
            InitializeLogging();

            var log = LogManager.GetLogger("Main");

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                var exception = (Exception) eventArgs.ExceptionObject;
                log.Fatal(exception);
            };

            string username = string.Empty;
            string password = string.Empty;
            string cacheFolder = Constants.CacheFolder;
            string settingsFolder = Constants.SettingsFolder;
            string userAgent = Constants.UserAgent;
            bool hidden = false;
            int port = 8081;
            int loginTimeout = 50000;

            new OptionSet
            {
                { "u|username=", v => username = v },
                { "p|password=", v => password = v },
                { "cf|cachefolder=", v => cacheFolder = v },
                { "sf|settingsfolder=", v => settingsFolder = v },
                { "ua|useragent=", v => userAgent = v },
                { "hidden", v => hidden = v != null },
                { "port=", v =>
                {
                    if(v != null)
                        int.TryParse(v, out port);
                }},
                { "logintimeout=", v =>
                {
                    if(v != null)
                        int.TryParse(v, out loginTimeout);
                }}
            }.Parse(args);

            if (hidden)
            {
                var hwnd = GetConsoleWindow();
                ShowWindow(hwnd, SW_HIDE);
            }

            var session =
               SessionFactory
                   .CreateSession(
                       Constants.ApplicationKey,
                       cacheFolder,
                       settingsFolder,
                       userAgent)
                   .SetPreferredBitrate(Bitrate.Bitrate320k);

            var wait = new ManualResetEvent(false);
            session.LoginComplete += (sender, eventArgs) =>
            {
                log.Info(eventArgs.Status == Error.OK ? "Logged in" : eventArgs.Message);
                wait.Set();
            };

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                log.Info("Please specify both username and password. -u=userName -p=password");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            session.Login(username, password);

            if (!wait.WaitOne(loginTimeout))
            {
                log.Error("Timed out [After " + loginTimeout + " ms]");
                Environment.Exit(-1);
            }

            var player = new TorshifySongPlayer(session, new SessionLinkFactory(session));

            GlobalHost.DependencyResolver.Register(typeof(ISongPlayer), () => player);

            var signalRUrl = "http://localhost:" + port + "/";
            var nancyFxUrl = "http://localhost:" + (port + 1) + "/torshify/";
            try
            {
                log.Info("SignalR URL: " + signalRUrl);
                log.Info("NancyFX URL: " + nancyFxUrl);

                var server = new SignalR.Hosting.Self.Server(signalRUrl);
                server.MapHubs();
                server.Start();

                var hub = GlobalHost.ConnectionManager.GetHubContext<TorshifyHub>();

                player.IsPlayingChanged += (sender, eventArgs) => hub.Clients.IsPlayingChanged(eventArgs);
                player.IsMutedChanged += (sender, eventArgs) => hub.Clients.IsMutedChanged(eventArgs);
                player.Buffering += (sender, eventArgs) => hub.Clients.Buffering(eventArgs);
                player.Progress += (sender, eventArgs) => hub.Clients.Progress(eventArgs);
                player.CurrentSongChanged += (sender, eventArgs) => hub.Clients.CurrentSongChanged(eventArgs);
                player.CurrentSongCompleted += (sender, eventArgs) => hub.Clients.CurrentSongCompleted(eventArgs);
                player.VolumeChanged += (sender, eventArgs) => hub.Clients.VolumeChanged(eventArgs);

                var bootstrapper = new NancyBootstrapper(session);

                var nancyHost = new NancyHost(new Uri(nancyFxUrl), bootstrapper);
                nancyHost.Start();
            }
            catch (Exception e)
            {
                log.Fatal(e);
            }

            Console.ReadLine();
            log.Info("Exiting");
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void InitializeLogging()
        {
            var fileAppender = new RollingFileAppender();
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            fileAppender.File = Path.Combine(Constants.LogFolder, assembly.GetName().Name + ".log");
            fileAppender.ImmediateFlush = true;
            fileAppender.AppendToFile = true;
            fileAppender.MaxSizeRollBackups = 10;
            fileAppender.MaxFileSize = 1024 * 1024;
            fileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            fileAppender.StaticLogFileName = true;
            fileAppender.Layout = new PatternLayout("%date{dd MMM yyyy HH:mm} [%thread] %-5level %logger - %message%newline");
            fileAppender.Threshold = Level.Info;
            fileAppender.ActivateOptions();

            var consoleAppender = new ColoredConsoleAppender();
            consoleAppender.AddMapping(
                new ColoredConsoleAppender.LevelColors
                {
                    ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity,
                    BackColor = ColoredConsoleAppender.Colors.Red | ColoredConsoleAppender.Colors.HighIntensity,
                    Level = Level.Fatal
                });
            consoleAppender.AddMapping(
                new ColoredConsoleAppender.LevelColors
                {
                    ForeColor = ColoredConsoleAppender.Colors.Red | ColoredConsoleAppender.Colors.HighIntensity,
                    Level = Level.Error
                });
            consoleAppender.AddMapping(
                new ColoredConsoleAppender.LevelColors
                {
                    ForeColor = ColoredConsoleAppender.Colors.Yellow | ColoredConsoleAppender.Colors.HighIntensity,
                    Level = Level.Warn
                });
            consoleAppender.AddMapping(
                new ColoredConsoleAppender.LevelColors
                {
                    ForeColor = ColoredConsoleAppender.Colors.Green | ColoredConsoleAppender.Colors.HighIntensity,
                    Level = Level.Info
                });
            consoleAppender.AddMapping(
                new ColoredConsoleAppender.LevelColors
                {
                    ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity,
                    Level = Level.Info
                });
            consoleAppender.Layout = new PatternLayout("%date{dd MM HH:mm} %-5level - %message%newline");
#if DEBUG
            consoleAppender.Threshold = Level.All;
#else
            consoleAppender.Threshold = Level.Info;
#endif
            consoleAppender.ActivateOptions();

            Logger root = ((Hierarchy)LogManager.GetRepository()).Root;
            root.AddAppender(consoleAppender);
            root.AddAppender(fileAppender);
            root.Repository.Configured = true;
        }

        #endregion Methods
    }
}