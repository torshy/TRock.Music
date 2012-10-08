using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Subjects;
using System.Linq;

namespace TRock.Music.Aggregate
{
    public class AggregateSongPlayer : ISongPlayer
    {
        #region Fields

        private ISongPlayer _currentSongPlayer;

        private readonly Subject<ValueProgress<int>> _buffering;
        private readonly Subject<ValueChange<Song>> _currentSongChanged;
        private readonly Subject<Song> _currentSongCompleted;
        private readonly Subject<ValueChange<bool>> _isMutedChanged;
        private readonly Subject<ValueChange<bool>> _isPlayingChanged;
        private readonly Subject<ValueProgress<int>> _progress;
        private readonly Subject<ValueChange<float>> _volumeChanged;
        private readonly Dictionary<ISongPlayer, IEnumerable<IDisposable>> _eventHooks;
        private readonly object _lockObject = new object();

        private bool _isMuted;
        private float _volume;

        #endregion Fields

        #region Constructors

        public AggregateSongPlayer()
            : this(new ISongPlayer[0])
        {

        }

        public AggregateSongPlayer(params ISongPlayer[] players)
            : this((IEnumerable<ISongPlayer>)players)
        {
            
        }

        public AggregateSongPlayer(IEnumerable<ISongPlayer> players)
        {
            _volume = 0.5f;
            _eventHooks = new Dictionary<ISongPlayer, IEnumerable<IDisposable>>();
            _isMutedChanged = new Subject<ValueChange<bool>>();
            _isPlayingChanged = new Subject<ValueChange<bool>>();
            _volumeChanged = new Subject<ValueChange<float>>();
            _currentSongChanged = new Subject<ValueChange<Song>>();
            _currentSongCompleted = new Subject<Song>();
            _buffering = new Subject<ValueProgress<int>>();
            _progress = new Subject<ValueProgress<int>>();

            Players = new ObservableCollection<ISongPlayer>(players);

            foreach (var songPlayer in Players)
            {
                InitializePlayer(songPlayer);
            }

            // ReSharper disable SuspiciousTypeConversion.Global
            // ReSharper disable ExpressionIsAlwaysNull
            // ReSharper disable PossibleNullReferenceException
            var notify = Players as INotifyCollectionChanged;
            notify.CollectionChanged += OnCollectionChanged;
            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore ExpressionIsAlwaysNull
            // ReSharper restore SuspiciousTypeConversion.Global
        }

        #endregion Constructors

        #region Properties

        public IObservable<ValueChange<bool>> IsMutedChanged
        {
            get { return _isMutedChanged; }
        }

        public IObservable<ValueChange<bool>> IsPlayingChanged
        {
            get { return _isPlayingChanged; }
        }

        public IObservable<ValueChange<float>> VolumeChanged
        {
            get { return _volumeChanged; }
        }

        public IObservable<ValueChange<Song>> CurrentSongChanged
        {
            get { return _currentSongChanged; }
        }

        public IObservable<Song> CurrentSongCompleted
        {
            get { return _currentSongCompleted; }
        }

        public IObservable<ValueProgress<int>> Buffering
        {
            get { return _buffering; }
        }

        public IObservable<ValueProgress<int>> Progress
        {
            get { return _progress; }
        }

        public bool IsMuted
        {
            get
            {
                return _isMuted;
            }
            set
            {
                _isMuted = value;

                lock (_lockObject)
                {
                    if (_currentSongPlayer != null)
                    {
                        _currentSongPlayer.IsMuted = value;
                    }
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                lock (_lockObject)
                {
                    if (_currentSongPlayer != null)
                    {
                        return _currentSongPlayer.IsPlaying;
                    }
                }

                return false;
            }
            set
            {
                lock (_lockObject)
                {
                    if (_currentSongPlayer != null)
                    {
                        _currentSongPlayer.IsPlaying = value;
                    }    
                }
            }
        }

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = Clamp(value, 0.0f, 1.0f); ;

                lock (_lockObject)
                {
                    if (_currentSongPlayer != null)
                    {
                        _currentSongPlayer.Volume = value;
                    }
                }
            }
        }

        public Song CurrentSong
        {
            get
            {
                if (_currentSongPlayer != null)
                {
                    return _currentSongPlayer.CurrentSong;
                }

                return null;
            }
        }

        public ICollection<ISongPlayer> Players
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public bool CanPlay(Song song)
        {
            return Players.Any(player => player.CanPlay(song));
        }

        public void Start(Song song)
        {
            lock (_lockObject)
            {
                if (_currentSongPlayer != null)
                {
                    _currentSongPlayer.Stop();
                }

                _currentSongPlayer = Players.FirstOrDefault(player => player.CanPlay(song));

                if (_currentSongPlayer != null)
                {
                    _currentSongPlayer.Start(song);
                }
            }
        }

        public void Play()
        {
            lock (_lockObject)
            {
                if (_currentSongPlayer != null)
                {
                    _currentSongPlayer.Play();
                }
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_currentSongPlayer != null)
                {
                    _currentSongPlayer.Stop();
                }
            }
        }

        public void Pause()
        {
            lock (_lockObject)
            {
                if (_currentSongPlayer != null)
                {
                    _currentSongPlayer.Pause();
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ISongPlayer newItem in e.NewItems)
                {
                    InitializePlayer(newItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ISongPlayer oldItem in e.OldItems)
                {
                    IEnumerable<IDisposable> disposables;

                    if (_eventHooks.TryGetValue(oldItem, out disposables))
                    {
                        foreach (var disposable in disposables)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
        }

        private void InitializePlayer(ISongPlayer player)
        {
            var disposables = new[]
            {
                player.Buffering.Subscribe(_buffering.OnNext),
                player.CurrentSongChanged.Subscribe(_currentSongChanged.OnNext),
                player.CurrentSongCompleted.Subscribe(song =>
                {
                    _currentSongCompleted.OnNext(song);
                }),
                player.IsMutedChanged.Subscribe(_isMutedChanged.OnNext),
                player.IsPlayingChanged.Subscribe(_isPlayingChanged.OnNext),
                player.Progress.Subscribe(_progress.OnNext),
                player.VolumeChanged.Subscribe(_volumeChanged.OnNext)
            };

            _eventHooks[player] = disposables;
        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;
            return val;
        }

        #endregion Methods
    }
}