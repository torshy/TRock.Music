using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;

namespace TRock.Music.Client
{
    public class SearchViewModel : NotificationObject, INavigationAware
    {
        #region Fields

        private IRegionNavigationService _navigationService;

        #endregion Fields

        #region Constructors

        public SearchViewModel()
        {
            SearchCommand = new DelegateCommand<string>(ExecuteQuery);
        }

        #endregion Constructors

        #region Properties

        public ICommand SearchCommand
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            _navigationService = navigationContext.NavigationService;
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private void ExecuteQuery(string query)
        {
            UriQuery q = new UriQuery();
            q.Add("Query", query);
            _navigationService.RequestNavigate(typeof(SearchResultsView).Name + "?" + q);
        }

        #endregion Methods
    }
}