using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace TRock.Music
{
    public class AggregateSongPlayer : ISongPlayer
    {
        #region Fields

        private readonly object _lockObject = new object();

        private ISongPlayer _currentSongPlayer;
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

        #region Events

        public event EventHandler<ValueProgressEventArgs<int>> Buffering;

        public event EventHandler<ValueChangedEventArgs<Song>> CurrentSongChanged;

        public event EventHandler<SongEventArgs> CurrentSongCompleted;

        public event EventHandler<ValueChangedEventArgs<bool>> IsMutedChanged;

        public event EventHandler<ValueChangedEventArgs<bool>> IsPlayingChanged;

        public event EventHandler<ValueProgressEventArgs<int>> Progress;

        public event EventHandler<ValueChangedEventArgs<float>> VolumeChanged;

        #endregion Events

        #region Properties

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

        public static T Clamp<T>(T val, T min, T max)
            where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;
            return val;
        }

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

        protected void OnIsMutedChanged(ValueChangedEventArgs<bool> e)
        {
            EventHandler<ValueChangedEventArgs<bool>> handler = IsMutedChanged;
            if (handler != null) handler(this, e);
        }

        protected void OnIsPlayingChanged(ValueChangedEventArgs<bool> e)
        {
            EventHandler<ValueChangedEventArgs<bool>> handler = IsPlayingChanged;
            if (handler != null) handler(this, e);
        }

        protected void OnVolumeChanged(ValueChangedEventArgs<float> e)
        {
            EventHandler<ValueChangedEventArgs<float>> handler = VolumeChanged;
            if (handler != null) handler(this, e);
        }

        protected void OnCurrentSongChanged(ValueChangedEventArgs<Song> e)
        {
            EventHandler<ValueChangedEventArgs<Song>> handler = CurrentSongChanged;
            if (handler != null) handler(this, e);
        }

        protected void OnCurrentSongCompleted(SongEventArgs e)
        {
            EventHandler<SongEventArgs> handler = CurrentSongCompleted;
            if (handler != null) handler(this, e);
        }

        protected void OnBuffering(ValueProgressEventArgs<int> e)
        {
            EventHandler<ValueProgressEventArgs<int>> handler = Buffering;
            if (handler != null) handler(this, e);
        }

        protected void OnProgress(ValueProgressEventArgs<int> e)
        {
            EventHandler<ValueProgressEventArgs<int>> handler = Progress;
            if (handler != null) handler(this, e);
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
                foreach (ISongPlayer player in e.OldItems)
                {
                    player.Buffering -= PlayerOnBuffering;
                    player.CurrentSongChanged -= PlayerOnCurrentSongChanged;
                    player.CurrentSongCompleted -= PlayerOnCurrentSongCompleted;
                    player.IsMutedChanged -= PlayerOnIsMutedChanged;
                    player.IsPlayingChanged -= PlayerOnIsPlayingChanged;
                    player.Progress -= PlayerOnProgress;
                    player.VolumeChanged -= PlayerOnVolumeChanged;
                }
            }
        }

        private void InitializePlayer(ISongPlayer player)
        {
            player.Buffering += PlayerOnBuffering;
            player.CurrentSongChanged += PlayerOnCurrentSongChanged;
            player.CurrentSongCompleted += PlayerOnCurrentSongCompleted;
            player.IsMutedChanged += PlayerOnIsMutedChanged;
            player.IsPlayingChanged += PlayerOnIsPlayingChanged;
            player.Progress += PlayerOnProgress;
            player.VolumeChanged += PlayerOnVolumeChanged;
        }

        private void PlayerOnVolumeChanged(object sender, ValueChangedEventArgs<float> e)
        {
            OnVolumeChanged(e);
        }

        private void PlayerOnProgress(object sender, ValueProgressEventArgs<int> e)
        {
            OnProgress(e);
        }

        private void PlayerOnIsPlayingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            OnIsPlayingChanged(e);
        }

        private void PlayerOnIsMutedChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            OnIsMutedChanged(e);
        }

        private void PlayerOnCurrentSongCompleted(object sender, SongEventArgs e)
        {
            OnCurrentSongCompleted(e);
        }

        private void PlayerOnCurrentSongChanged(object sender, ValueChangedEventArgs<Song> e)
        {
            OnCurrentSongChanged(e);
        }

        private void PlayerOnBuffering(object sender, ValueProgressEventArgs<int> e)
        {
            OnBuffering(e);
        }

        #endregion Methods
    }
}