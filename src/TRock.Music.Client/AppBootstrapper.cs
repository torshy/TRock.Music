using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using MahApps.Metro;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

using TRock.Music.Grooveshark;
using Unity.Extensions;

namespace TRock.Music.Client
{
    public class AppBootstrapper : UnityBootstrapper
    {
        #region Methods

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.AddNewExtension<LazySupportExtension>();
            Container.RegisterType<IVoteableQueue<ISongStream>, VoteableQueue<ISongStream>>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IGroovesharkClient, GroovesharkClientWrapper>(new ContainerControlledLifetimeManager(), new InjectionMethod("Connect"));
            Container.RegisterType<ISongProvider, GroovesharkSongProvider>(GroovesharkSongProvider.ProviderName, new ContainerControlledLifetimeManager());
            Container.RegisterType<ISongPlayer, GroovesharkSongPlayer>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISongStreamPlayer, SongStreamPlayer>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISongProvider>(new InjectionFactory(c => new CachedSongProvider(Container.Resolve<ISongProvider>(GroovesharkSongProvider.ProviderName))));
            Container.RegisterType<object, SearchView>(typeof(SearchView).Name);
            Container.RegisterType<SearchViewModel>();

            Container.RegisterType<object, SearchResultsView>(typeof(SearchResultsView).Name);
            Container.RegisterType<SearchResultsViewModel>();

            Task.Factory
                .StartNew(() => Container.Resolve<IGroovesharkClient>())
                .ContinueWith(grooveTask =>
                {
                    if (grooveTask.IsFaulted && grooveTask.Exception != null)
                    {
                        Logger.Log(grooveTask.Exception.Message, Category.Exception, Priority.High);
                    }
                });
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            ThemeManager.ChangeTheme(Application.Current, ThemeManager.DefaultAccents.First(a => a.Name == "Orange"), Theme.Light);

            Window window = (Window)Shell;
            Application.Current.MainWindow = window;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            ModuleCatalog.AddModule(new ModuleInfo(typeof(AppModule).Name, typeof(AppModule).AssemblyQualifiedName));
        }

        #endregion Methods
    }
}