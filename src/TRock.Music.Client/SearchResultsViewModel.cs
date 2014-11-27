using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;

namespace TRock.Music.Client
{
    public class SearchResultsViewModel : NotificationObject, INavigationAware
    {
        #region Fields

        private readonly ObservableCollection<ArtistAlbum> _albums;
        private readonly ObservableCollection<ArtistAlbum> _artists;
        private readonly CancellationTokenSource _cts;
        private readonly ISongProvider _songProvider;
        private readonly ObservableCollection<Song> _songs;

        private bool _isSearching;

        #endregion Fields

        #region Constructors

        public SearchResultsViewModel(ISongProvider songProvider)
        {
            _songProvider = songProvider;
            _songs = new ObservableCollection<Song>();
            _albums = new ObservableCollection<ArtistAlbum>();
            _artists = new ObservableCollection<ArtistAlbum>();
            _cts = new CancellationTokenSource();
            CancelSearchCommand = new DelegateCommand(() => _cts.Cancel());
        }

        #endregion Constructors

        #region Properties

        public bool IsSearching
        {
            get { return _isSearching; }
            set
            {
                if (value.Equals(_isSearching)) return;
                _isSearching = value;
                RaisePropertyChanged("IsSearching");
            }
        }

        public ICollectionView Songs
        {
            get
            {
                return new ListCollectionView(_songs);
            }
        }

        public ICollectionView Albums
        {
            get
            {
                return new ListCollectionView(_albums);
            }
        }

        public ICollectionView Artists
        {
            get
            {
                return new ListCollectionView(_artists);
            }
        }

        public ICommand CancelSearchCommand
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var query = navigationContext.Parameters["Query"].ToString();
            var ui = TaskScheduler.FromCurrentSynchronizationContext();

            _songProvider
                .GetSongs(query, _cts.Token)
                .ContinueWith(queryTask =>
                {
                    if (queryTask.IsCanceled)
                    {
                        navigationContext.NavigationService.Journal.GoBack();
                    }
                    else if (queryTask.IsCompleted)
                    {
                        _albums.Clear();
                        _artists.Clear();
                        _songs.Clear();

                        foreach (var song in queryTask.Result)
                        {
                            _songs.Add(song);
                        }

                        var artists = queryTask.Result.GroupBy(q => q.Artist);
                        var albums = queryTask.Result.GroupBy(q => q.Album);

                        foreach (var album in albums)
                        {
                            _albums.Add(new ArtistAlbum
                            {
                                Album = album.Key,
                                Artist = album.First().Artist,
                                Songs = album.ToArray()
                            });
                        }

                        foreach (var artist in artists)
                        {
                            _artists.Add(new ArtistAlbum
                            {
                                Album = artist.First().Album,
                                Artist = artist.Key,
                                Songs = artist.ToArray()
                            });
                        }
                    }
                    else if (queryTask.IsFaulted)
                    {
                        Trace.WriteLine(queryTask.Exception);
                    }

                    IsSearching = false;
                }, ui);

            IsSearching = true;
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        #endregion Methods
    }
}