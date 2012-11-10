using System;
using System.Diagnostics;
using System.Threading.Tasks;

using SignalR.Client;
using SignalR.Client.Hubs;

using TRock.Music.Spotify;

namespace TRock.Music.Torshify
{
    public class TorshifySongPlayerClient : ISongPlayer
    {
        #region Fields

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        #endregion Fields

        #region Constructors

        public TorshifySongPlayerClient(Uri serverUri)
        {
            _connection = new HubConnection(serverUri.AbsoluteUri);
            _proxy = _connection.CreateProxy("TorshifyHub");
            _proxy.On<ValueProgressEventArgs<int>>("Progress", OnProgress);
            _proxy.On<ValueProgressEventArgs<int>>("Buffering", OnBuffering);
            _proxy.On<ValueChangedEventArgs<float>>("VolumeChanged", OnVolumeChanged);
            _proxy.On<ValueChangedEventArgs<bool>>("IsMutedChanged", OnIsMutedChanged);
            _proxy.On<ValueChangedEventArgs<bool>>("IsPlayingChanged", OnIsPlayingChanged);
            _proxy.On<ValueChangedEventArgs<Song>>("CurrentSongChanged", OnCurrentSongChanged);
            _proxy.On<SongEventArgs>("CurrentSongCompleted", OnCurrentSongCompleted);
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

        public bool IsConnected
        {
            get
            {
                if (_connection.State == ConnectionState.Disconnected)
                {
                    _connection.Start().Wait(1000);
                }

                return _connection.State == ConnectionState.Connected;
            }
        }

        public bool IsMuted
        {
            get
            {
                if (IsConnected)
                {
                    return _proxy
                        .Invoke<bool>("GetMuted")
                        .ContinueWith(t =>
                        {
                            if (t.Exception != null)
                            {
                                Trace.WriteLine(t.Exception);
                                return false;
                            }

                            return t.Result;
                        }).Result;
                }

                return false;
            }
            set
            {
                _proxy
                    .Invoke("SetMuted", value)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Trace.WriteLine(t.Exception);
                        }
                    });
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (IsConnected)
                {
                    return _proxy
                        .Invoke<bool>("GetIsPlaying")
                        .ContinueWith(t =>
                        {
                            if (t.Exception != null)
                            {
                                Trace.WriteLine(t.Exception);
                                return false;
                            }

                            return t.Result;
                        }).Result;
                }

                return false;
            }
            set
            {
                _proxy
                    .Invoke("SetIsPlaying", value)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Trace.WriteLine(t.Exception);
                        }
                    });
            }
        }

        public float Volume
        {
            get
            {
                if (IsConnected)
                {
                    return _proxy.Invoke<float>("GetVolume").ContinueWith(task =>
                    {
                        if (task.Exception != null)
                        {
                            Trace.WriteLine("Unable to get volume. " + task.Exception);

                            return 0;
                        }

                        return task.Result;
                    }).Result;
                }

                return 0.0f;
            }
            set
            {
                _proxy
                    .Invoke("SetVolume", value)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Trace.WriteLine(t.Exception);
                        }
                    });
            }
        }

        public Song CurrentSong
        {
            get
            {
                if (IsConnected)
                {
                    return _proxy
                        .Invoke<Song>("GetCurrentSong")
                        .ContinueWith(t =>
                        {
                            if (t.Exception != null)
                            {
                                Trace.WriteLine(t.Exception);
                                return null;
                            }

                            return t.Result;
                        }).Result;
                }

                return null;
            }
        }

        #endregion Properties

        #region Methods

        public Task Connect()
        {
            if (_connection.State == ConnectionState.Disconnected)
            {
                return _connection.Start();
            }

            return Task.Factory.StartNew(() => { });
        }

        public bool CanPlay(Song song)
        {
            return song.Provider == SpotifySongProvider.ProviderName;
        }

        public void Start(Song song)
        {
            if (IsConnected)
            {
                _proxy.Invoke("Start", song)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Trace.WriteLine(t.Exception);
                        }
                    });
            }
        }

        public void Play()
        {
            if (IsConnected)
            {
                _proxy
                    .Invoke("Play")
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Trace.WriteLine(t.Exception);
                        }
                    });
            }
        }

        public void Stop()
        {
            if (IsConnected)
            {
                _proxy
                    .Invoke("Stop")
                    .ContinueWith(t=>
                    {
                        if (t.Exception != null)
                        {
                            Trace.WriteLine(t.Exception);
                        }
                    });
            }
        }

        public void Pause()
        {
            if (IsConnected)
            {
                _proxy
                    .Invoke("Pause")
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Trace.WriteLine(t.Exception);
                        }
                    });
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

        #endregion Methods
    }
}