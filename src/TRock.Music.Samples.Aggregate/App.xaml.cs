using System;
using System.IO;
using System.Windows;
using Microsoft.Practices.Unity;
using TRock.Music.Grooveshark;
using TRock.Music.Spotify;
using TRock.Music.Torshify;
using Unity.Extensions;

namespace TRock.Music.Samples.Aggregate
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<LazySupportExtension>();

            // Register grooveshark related stuff
            container.RegisterType<IGroovesharkClient, GroovesharkClientWrapper>(
                new ContainerControlledLifetimeManager(), 
                new InjectionMethod("Connect"));
            container.RegisterType<ISongProvider, GroovesharkSongProvider>(
                GroovesharkSongProvider.ProviderName, 
                new ContainerControlledLifetimeManager());
            container.RegisterType<ISongPlayer, GroovesharkSongPlayer>(
                GroovesharkSongProvider.ProviderName,
                new ContainerControlledLifetimeManager());

            // Register spotify/torshify related stuff
            container.RegisterType<ISongProvider, SpotifySongProvider>(
                SpotifySongProvider.ProviderName,
                new ContainerControlledLifetimeManager());
            container.RegisterType<ISongPlayer, TorshifySongPlayerClient>(
                SpotifySongProvider.ProviderName,
                new ContainerControlledLifetimeManager());
            container.RegisterType<ISpotifyImageProvider, TorshifyImageProvider>();

            // Aggregate provider that combines Grooveshark and Spotify players and providers
            container.RegisterType<ISongProvider, AggregateSongProvider>(new InjectionFactory(c =>
            {
                return new AggregateSongProvider(
                    c.Resolve<ISongProvider>(GroovesharkSongProvider.ProviderName),
                    c.Resolve<ISongProvider>(SpotifySongProvider.ProviderName));
            }));
            container.RegisterType<ISongPlayer, AggregateSongPlayer>(new InjectionFactory(c =>
            {
                return new AggregateSongPlayer(
                    c.Resolve<ISongPlayer>(GroovesharkSongProvider.ProviderName),
                    c.Resolve<ISongPlayer>(SpotifySongProvider.ProviderName));
            }));

            TorshifyServerProcessHandler torshifyServerProcess = new TorshifyServerProcessHandler();
            torshifyServerProcess.CloseServerTogetherWithClient = true;
            //torshifyServerProcess.Hidden = true;
            torshifyServerProcess.TorshifyServerLocation = Path.Combine(Environment.CurrentDirectory, "TRock.Music.Torshify.Server.exe");
            torshifyServerProcess.UserName = "<insert username>";
            torshifyServerProcess.Password = "<insert password>";
            torshifyServerProcess.Start();

            var provider = container.Resolve<ISongProvider>();

            MainWindow = container.Resolve<MainWindow>();
            MainWindow.Show();
        }
    }
}
