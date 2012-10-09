using System;
using System.Timers;

using NAudio.Wave;

using Torshify;

namespace TRock.Music.Torshify.Server
{
    public class TorshifySongPlayer : ISongPlayer
    {
        #region Fields

        private readonly ISession _session;
        private readonly Timer _timer;
        private readonly WaveOut _waveOut;
        private readonly object _lockObject = new object();

        private Song _currentSong;
        private TimeSpan _currentSongElapsed;
        private bool _isMuted;
        private bool _isPlaying;
        private float _volume;
        private BufferedWaveProvider _waveProvider;

        #endregion Fields

        #region Constructors

        public TorshifySongPlayer(ISession session)
        {
            _session = session;
            _session.MusicDeliver += OnMusicDeliver;
            _session.EndOfTrack += OnEndOfTrack;
            _session.PlayTokenLost += OnPlayerTokenLost;

            _volume = 0.5f;

            _waveOut = new WaveOut();
            _waveOut.Volume = Volume;

            _timer = new Timer(1000);
            _timer.Elapsed += OnTrackElapsedTimerTick;
            _currentSongElapsed = TimeSpan.Zero;
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
                if (_isMuted != value)
                {
                    _isMuted = value;

                    if (_waveOut != null)
                    {
                        _waveOut.Volume = Volume;
                    }

                    OnIsMutedChanged(new ValueChangedEventArgs<bool>(!_isMuted, _isMuted));
                }
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (value)
                {
                    Play();
                }
                else
                {
                    Pause();
                }
            }
        }

        public float Volume
        {
            get { return _volume; }
            set
            {
                value = Clamp(value, 0.0f, 1.0f);

                if (_volume != value)
                {
                    if (_waveOut != null && !IsMuted)
                    {
                        _waveOut.Volume = value;
                    }

                    var oldVolume = _volume;
                    _volume = value;

                    OnVolumeChanged(new ValueChangedEventArgs<float>(oldVolume, _volume));
                }
            }
        }

        public Song CurrentSong
        {
            get
            {
                return _currentSong;
            }
        }
        #endregion Properties

        #region Methods

        public bool CanPlay(Song song)
        {
            return song.Provider == "Spotify";
        }

        public void Start(Song song)
        {
            if (IsPlaying)
            {
                if (_waveOut != null)
                {
                    _waveOut.Dispose();
                }
            }

            lock (_lockObject)
            {
                _waveProvider = null;
                _currentSongElapsed = TimeSpan.Zero;
            }

            using (var link = _session.FromLink<ITrackAndOffset>(song.Id))
            {
                using (var track = link.Object.Track)
                {
                    if (track.WaitUntilLoaded(2000))
                    {
                        var oldSong = _currentSong;
                        _session.PlayerLoad(track);
                        _session.PlayerPlay();
                        _currentSong = song;
                        _currentSong.TotalSeconds = (int)track.Duration.TotalSeconds;

                        OnCurrentSongChanged(new ValueChangedEventArgs<Song>(oldSong, song));
                        OnBuffering(new ValueProgressEventArgs<int>(100, 100));
                    }
                    else
                    {
                        OnCurrentSongCompleted(new SongEventArgs(song));
                    }
                }
            }
        }

        public void Play()
        {
            if (_waveOut != null && _waveOut.PlaybackState != PlaybackState.Playing)
            {
                _session.PlayerPlay();

                if (_waveOut.PlaybackState == PlaybackState.Paused)
                {
                    _waveOut.Resume();
                }
                else
                {
                    _waveOut.Play();
                }

                _isPlaying = true;

                OnIsPlayingChanged(new ValueChangedEventArgs<bool>(false, true));

                _timer.Start();
            }
        }

        public void Stop()
        {
            if (_waveOut != null && _waveOut.PlaybackState != PlaybackState.Stopped)
            {
                _session.PlayerPause();
                _session.PlayerUnload();
                _waveOut.Stop();
                _isPlaying = false;

                OnIsPlayingChanged(new ValueChangedEventArgs<bool>(true, false));
                
                _timer.Stop();
            }
        }

        public void Pause()
        {
            if (_waveOut != null && _waveOut.PlaybackState != PlaybackState.Paused)
            {
                _session.PlayerPause();
                _waveOut.Pause();
                _isPlaying = false;
                
                OnIsPlayingChanged(new ValueChangedEventArgs<bool>(true, false));
                
                _timer.Stop();
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

        private void OnPlayerTokenLost(object sender, SessionEventArgs e)
        {
            Pause();
        }

        private void OnTrackElapsedTimerTick(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                _currentSongElapsed = _currentSongElapsed.Add(TimeSpan.FromMilliseconds(_timer.Interval));
             
                OnProgress(new ValueProgressEventArgs<int>((int)_currentSongElapsed.TotalSeconds, _currentSong.TotalSeconds));
            }
        }

        private void OnEndOfTrack(object sender, SessionEventArgs e)
        {
            if (e.Status == Error.OK)
            {
                OnCurrentSongCompleted(new SongEventArgs(_currentSong));
                Stop();
            }
        }

        private void OnMusicDeliver(object sender, MusicDeliveryEventArgs e)
        {
            int consumed = 0;

            lock (_lockObject)
            {
                if (_waveProvider == null || e.Frames == 0)
                {
                    _isPlaying = true;
                    _waveProvider = new BufferedWaveProvider(new WaveFormat(e.Rate, e.Channels));
                    _waveProvider.BufferDuration = TimeSpan.FromSeconds(1);
                    _waveOut.Init(_waveProvider);
                    _waveOut.Play();
                    _timer.Start();
                }

                if ((_waveProvider.BufferLength - _waveProvider.BufferedBytes) > e.Samples.Length)
                {
                    _waveProvider.AddSamples(e.Samples, 0, e.Samples.Length);
                    consumed = e.Frames;
                }
            }

            e.ConsumedFrames = consumed;
        }

        private T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;
            return val;
        }

        #endregion Methods
    }
}