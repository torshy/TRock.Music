using System;

using Microsoft.Expression.Interactivity.Core;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace TRock.Music.Client
{
    public class AppModule : IModule
    {
        #region Fields

        private readonly IRegionManager _regionManager;
        private readonly Lazy<ISongStreamPlayer> _songStreamPlayer;

        #endregion Fields

        #region Constructors

        public AppModule(IRegionManager regionManager, Lazy<ISongStreamPlayer> songStreamPlayer)
        {
            _regionManager = regionManager;
            _songStreamPlayer = songStreamPlayer;
        }

        #endregion Constructors

        #region Methods

        public void Initialize()
        {
            _regionManager.RequestNavigate("MainRegion", typeof(SearchView).Name);

            AppCommands.NavigateBackCommand.RegisterCommand(new DelegateCommand(ExecuteNavigateBack));
            AppCommands.PlayCommand.RegisterCommand(new ActionCommand(ExecutePlay));
        }

        private void ExecuteNavigateBack()
        {
            _regionManager.Regions["MainRegion"].NavigationService.Journal.GoBack();
        }

        private void ExecutePlay(object argument)
        {
            if (argument is Song)
            {
                _songStreamPlayer.Value.CurrentStream = new SingleSongStream(((Song) argument));
            }
        }

        #endregion Methods
    }
}