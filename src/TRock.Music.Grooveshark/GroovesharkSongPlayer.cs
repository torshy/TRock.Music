using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NAudio.Wave;

using TRock.Music.Grooveshark.IO;

namespace TRock.Music.Grooveshark
{
    public class GroovesharkSongPlayer : ISongPlayer
    {
        #region Fields

        private readonly Lazy<IGroovesharkClient> _groove;
        private readonly object _lockObject = new object();

        private bool _bufferingComplete;
        private SongData _currentSong;
        private bool _isMuted;
        private bool _isPlaying;
        private PlayerState _playerState;
        private float _volume;

        #endregion Fields

        #region Constructors

        public GroovesharkSongPlayer(Lazy<IGroovesharkClient> groove)
        {
            _groove = groove;
            _volume = 0.5f;
        }

        #endregion Constructors

        #region Enumerations

        public enum PlayerState
        {
            Stopped,
            Paused,
            Playing,
            Buffering
        }

        #endregion Enumerations

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

                    if (_currentSong != null && _currentSong.VolumeProvider != null)
                    {
                        _currentSong.VolumeProvider.Volume = Volume;
                    }

                    OnIsMutedChanged(new ValueChangedEventArgs<bool>(!_isMuted, _isMuted));
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
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
            get
            {
                if (IsMuted)
                {
                    return 0.0f;
                }

                return _volume;
            }
            set
            {
                value = value.Clamp(0.0f, 1.0f);

                if (_currentSong != null && _currentSong.VolumeProvider != null && !IsMuted)
                {
                    _currentSong.VolumeProvider.Volume = value;
                }

                var oldVolume = _volume;
                _volume = value;

                OnVolumeChanged(new ValueChangedEventArgs<float>(oldVolume, _volume));
            }
        }

        public Song CurrentSong
        {
            get
            {
                if (_currentSong != null)
                {
                    return _currentSong.Song;
                }

                return null;
            }
        }

        #endregion Properties

        #region Methods

        public bool CanPlay(Song song)
        {
            return song.Provider == GroovesharkSongProvider.ProviderName;
        }

        public void Start(Song song)
        {
            int songId;
            int artistId;

            if (!int.TryParse(song.Id, out songId))
            {
                throw new InvalidOperationException("Unable to parse song id");
            }

            if (!int.TryParse(song.Artist.Id, out artistId))
            {
                throw new InvalidOperationException("Unable to parse artist id");
            }

            Song oldSong = null;

            lock (_lockObject)
            {
                if (_currentSong != null)
                {
                    oldSong = _currentSong.Song;

                    if (!_currentSong.Cts.IsCancellationRequested)
                    {
                        _currentSong.Cts.Cancel();
                    }

                    Task.WaitAll(new[] { _currentSong.BufferTask, _currentSong.PlayTask }, TimeSpan.FromSeconds(5));
                }

                _currentSong = new SongData
                {
                    Song = song,
                    Stream = new Lazy<Tuple<Stream, int, TimeSpan>>(() =>
                    {
                        if (song.TotalSeconds == 0)
                        {
                            var streamKey = _groove.Value.GetStreamKey(songId);
                            double durationInMicroseconds;

                            if (double.TryParse(streamKey.uSecs, out durationInMicroseconds))
                            {
                                song.TotalSeconds = (int)(durationInMicroseconds / 1000000);
                            }
                        }

                        var bufferPeriod = TimeSpan.FromSeconds(song.TotalSeconds);
                        var stream = _groove.Value.GetMusicStream(songId, artistId);

                        return Tuple.Create(stream.Stream, stream.Length, bufferPeriod);
                    })
                };
            }

            OnCurrentSongChanged(new ValueChangedEventArgs<Song>(oldSong, _currentSong.Song));

            _currentSong.BufferTask =
                    Task.Factory
                        .StartNew(
                            d => BufferStream(((SongData)d)),
                            _currentSong,
                            _currentSong.Cts.Token,
                            TaskCreationOptions.LongRunning, TaskScheduler.Default)
                        .ContinueWith(task =>
                        {
                            Trace.WriteLineIf(task.IsFaulted, task.Exception);
                            Trace.WriteLineIf(task.IsCanceled, "Buffer-thread was cancelled");
                            Trace.WriteLineIf(task.IsCompleted, "Buffer-thread completed");

                            _bufferingComplete = true;
                        });

            _currentSong.PlayTask =
                Task.Factory
                    .StartNew(
                        d => PlayBuffer(((SongData)d)),
                        _currentSong,
                        _currentSong.Cts.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default)
                    .ContinueWith(task =>
                    {
                        Trace.WriteLineIf(task.IsFaulted, task.Exception);
                        Trace.WriteLineIf(task.IsCanceled, "Player-thread was cancelled");
                        Trace.WriteLineIf(task.IsCompleted, "Player-thread completed");

                        if (task.IsCanceled)
                        {
                            if (_currentSong.WaveOut != null)
                            {
                                _currentSong.WaveOut.Stop();
                                _currentSong.WaveOut.Dispose();
                            }

                            if (_currentSong.WaveProvider != null)
                            {
                                _currentSong.WaveProvider.ClearBuffer();
                            }
                        }

                        _currentSong = null;
                        _playerState = PlayerState.Stopped;
                        _isPlaying = false;

                        OnIsPlayingChanged(new ValueChangedEventArgs<bool>(false, true));

                        if (task.IsCompleted  && !task.IsCanceled)
                        {
                            OnCurrentSongCompleted(new SongEventArgs(((SongData) task.AsyncState).Song));
                        }
                    });
        }

        public void Play()
        {
            if (_playerState == PlayerState.Paused || _playerState == PlayerState.Buffering)
            {
                if (_currentSong.WaveOut != null)
                {
                    _playerState = PlayerState.Playing;
                    _currentSong.WaveOut.Play();
                    _isPlaying = true;

                    OnIsPlayingChanged(new ValueChangedEventArgs<bool>(false, true));
                }
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_currentSong != null)
                {
                    _currentSong.Cts.Cancel();
                }
            }
        }

        public void Pause()
        {
            if (_playerState == PlayerState.Playing)
            {

                if (_currentSong != null && _currentSong.WaveOut != null)
                {
                    _playerState = PlayerState.Paused;
                    _currentSong.WaveOut.Pause();
                    _isPlaying = false;

                    OnIsPlayingChanged(new ValueChangedEventArgs<bool>(true, false));
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

        private void BufferStream(SongData songData)
        {
            var stream = songData.Stream.Value.Item1;
            var bufferDuration = songData.Stream.Value.Item3;

            var buffer = new byte[16384 * 4];
            var waitTime = 0;
            var wait = new AutoResetEvent(false);

            using (var throttledStream = new ThrottledStream(stream))
            using (var readFullyStream = new ReadFullyStream(throttledStream))
            {
                IMp3FrameDecompressor decompressor = null;

                _bufferingComplete = false;

                do
                {
                    if (songData.Cts.Token.IsCancellationRequested)
                    {
                        songData.Cts.Token.ThrowIfCancellationRequested();
                    }

                    wait.WaitOne(waitTime);

                    try
                    {
                        var frame = Mp3Frame.LoadFromStream(readFullyStream);

                        var progress = (int)(readFullyStream.Position / (double)songData.Stream.Value.Item2 * 100);

                        if (progress % 5 == 0)
                        {
                            OnBuffering(new ValueProgressEventArgs<int>(progress, 100));
                        }

                        if (frame == null)
                        {
                            Trace.WriteLine("Mp3Frame is null");
                            break;
                        }

                        if (decompressor == null)
                        {
                            var waveFormat = new Mp3WaveFormat(
                                44100,
                                frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                                frame.FrameLength,
                                frame.BitRate);

                            double bufferTimeInSeconds = bufferDuration.TotalSeconds;

                            if (bufferTimeInSeconds.LessThan(60))
                            {
                                bufferTimeInSeconds = TimeSpan.FromMinutes(2).TotalSeconds;
                            }

                            decompressor = new AcmMp3FrameDecompressor(waveFormat);
                            songData.WaveProvider = new BufferedWaveProvider(decompressor.OutputFormat)
                            {
                                BufferDuration = TimeSpan.FromSeconds(bufferTimeInSeconds),
                                DiscardOnBufferOverflow = true
                            };

                            // Adjust the maximum bytes to stream per second
                            // This is done to avoid being blacklisted by Grooveshark for several hours
                            throttledStream.MaximumBytesPerSecond = songData.WaveProvider.WaveFormat.AverageBytesPerSecond / 4;
                        }

                        if (songData.WaveProvider != null)
                        {
                            try
                            {
                                int decompressed = decompressor.DecompressFrame(frame, buffer, 0);

                                songData.WaveProvider.AddSamples(buffer, 0, decompressed);

                                if (songData.WaveProvider.BufferLength - songData.WaveProvider.BufferedBytes <
                                    songData.WaveProvider.WaveFormat.AverageBytesPerSecond / 4)
                                {
                                    Trace.WriteLine("Buffer full. Waiting 500 ms");
                                    waitTime = 500;
                                }

                            }
                            catch (InvalidOperationException e)
                            {
                                Trace.WriteLine(e.Message + ". Wait time is 500ms");
                                waitTime = 500;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e);
                        break;
                    }

                } while (_playerState != PlayerState.Stopped);

                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
            }

            _bufferingComplete = true;

            Trace.WriteLine("Buffer-thread exiting");
        }

        private void PlayBuffer(SongData songData)
        {
            _playerState = PlayerState.Buffering;

            CancellationToken token = songData.Cts.Token;

            int currentTime = 0;

            while (_playerState != PlayerState.Stopped)
            {
                if (token.IsCancellationRequested)
                {
                    songData.Cts.Token.ThrowIfCancellationRequested();
                }

                if (songData.WaveOut == null && songData.WaveProvider != null)
                {
                    try
                    {
                        Trace.WriteLine("Initializing wave out");

                        songData.VolumeProvider = new VolumeWaveProvider16(songData.WaveProvider) { Volume = Volume };
                        songData.WaveOut = new WaveOutEvent();
                        songData.WaveOut.Init(songData.VolumeProvider);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                }
                else if (songData.WaveProvider != null && songData.WaveOut!= null)
                {
                    var bufferedSeconds = songData.WaveProvider.BufferedDuration.TotalSeconds;

                    // make it stutter less if we buffer up a decent amount before playing
                    if (bufferedSeconds < 0.5 && _playerState == PlayerState.Playing && !_bufferingComplete)
                    {
                        Trace.WriteLine("Buffer are less than limit. Starting buffering");

                        Pause();

                        _playerState = PlayerState.Buffering;
                    }
                    else if (bufferedSeconds > 4 && _playerState == PlayerState.Buffering)
                    {
                        Trace.WriteLine("Buffer are more than limit. Starting playback");
                        Play();
                    }
                    else if (bufferedSeconds.AreCloseTo(0) && _bufferingComplete)
                    {
                        break;
                    }
                }

                if (_playerState == PlayerState.Playing)
                {
                    currentTime += 250;

                    if (currentTime % 1000 == 0)
                    {
                        OnProgress(new ValueProgressEventArgs<int>(currentTime / 1000, (int)songData.Stream.Value.Item3.TotalSeconds));
                    }
                }

                Thread.Sleep(250);
            }

            if (songData.WaveOut != null)
            {
                songData.WaveOut.Stop();
                songData.WaveOut.Dispose();
            }

            if (songData.WaveProvider != null)
            {
                songData.WaveProvider.ClearBuffer();
            }

            _playerState = PlayerState.Stopped;

            Trace.WriteLine("Player-thread exiting");
        }

        #endregion Methods

        #region Nested Types

        public class SongData
        {
            #region Constructors

            public SongData()
            {
                Cts = new CancellationTokenSource();
            }

            #endregion Constructors

            #region Properties

            public Song Song
            {
                get;
                set;
            }

            public CancellationTokenSource Cts
            {
                get;
                set;
            }

            public Task BufferTask
            {
                get;
                set;
            }

            public Task PlayTask
            {
                get;
                set;
            }

            public Lazy<Tuple<Stream, int, TimeSpan>> Stream
            {
                get;
                set;
            }

            public VolumeWaveProvider16 VolumeProvider
            {
                get;
                set;
            }

            public WaveOutEvent WaveOut
            {
                get;
                set;
            }

            public BufferedWaveProvider WaveProvider
            {
                get;
                set;
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}