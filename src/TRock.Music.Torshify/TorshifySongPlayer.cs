﻿using System;

using SignalR.Client;
using SignalR.Client.Hubs;

using TRock.Music.Spotify;

namespace TRock.Music.Torshify
{
    public class TorshifySongPlayer : ISongPlayer
    {
        #region Fields

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        #endregion Fields

        #region Constructors

        public TorshifySongPlayer(Uri server)
        {
            _connection = new HubConnection(server.AbsoluteUri);
            _proxy = _connection.CreateProxy("TorshifyHub");
            _proxy.On<Tuple<int, int>>("Progress", progress =>
            {
                Console.WriteLine("Progress=>" + progress.Item1 + "x" + progress.Item2);

                OnProgress(new ValueProgressEventArgs<int>(progress.Item1, progress.Item2));
            });
            _proxy.On<Tuple<int, int>>("Buffering", progress =>
            {
                Console.WriteLine("Buffering=>" + progress.Item1 + "x" + progress.Item2);

                OnBuffering(new ValueProgressEventArgs<int>(progress.Item1, progress.Item2));
            });
            _proxy.On<Tuple<int, int>>("VolumeChanged", volume =>
            {
                Console.WriteLine("VolumeChanged=>" + volume.Item1 + "x" + volume.Item2);

                OnVolumeChanged(new ValueChangedEventArgs<float>(volume.Item1, volume.Item2));
            });
            _proxy.On<bool>("IsMutedChanged", isMuted =>
            {
                Console.WriteLine("IsMutedChanged=>" + isMuted);

                OnIsMutedChanged(new ValueChangedEventArgs<bool>(!isMuted, isMuted));
            });
            _proxy.On<bool>("IsPlayingChanged", isPlaying =>
            {
                Console.WriteLine("IsPlayingChanged=>" + isPlaying);

                OnIsPlayingChanged(new ValueChangedEventArgs<bool>(!isPlaying, isPlaying));
            });
            _proxy.On<Tuple<Song, Song>>("CurrentSongChanged", song =>
            {
                Console.WriteLine("CurrentSongChanged=>" + song.Item1 + "x" + song.Item2);

                OnCurrentSongChanged(new ValueChangedEventArgs<Song>(song.Item1, song.Item2));
            });
            _proxy.On<Song>("CurrentSongCompleted", song =>
            {
                Console.WriteLine("CurrentSongCompleted=>" + song.Name);

                OnCurrentSongCompleted(new SongEventArgs(song));
            });

            _connection.Start();
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
                    return _proxy.Invoke<bool>("GetMuted").Result;
                }

                return false;
            }
            set
            {
                _proxy.Invoke("SetMuted", value);
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (IsConnected)
                {
                    return _proxy.Invoke<bool>("GetIsPlaying").Result;
                }

                return false;
            }
            set
            {
                _proxy.Invoke("SetIsPlaying", value);
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
                            Console.WriteLine("Unable to get volume. " + task.Exception);

                            return 0;
                        }

                        return task.Result;
                    }).Result;
                }

                return 0.0f;
            }
            set
            {
                _proxy.Invoke("SetVolume", value);
            }
        }

        public Song CurrentSong
        {
            get
            {
                if (IsConnected)
                {
                    return _proxy.Invoke<Song>("GetCurrentSong").Result;
                }

                return null;
            }
        }

        #endregion Properties

        #region Methods

        public bool CanPlay(Song song)
        {
            return song.Provider == SpotifySongProvider.ProviderName;
        }

        public void Start(Song song)
        {
            if (IsConnected)
            {
                _proxy.Invoke("Start", song);
            }
        }

        public void Play()
        {
            if (IsConnected)
            {
                _proxy.Invoke("Play");
            }
        }

        public void Stop()
        {
            if (IsConnected)
            {
                _proxy.Invoke("Stop");
            }
        }

        public void Pause()
        {
            if (IsConnected)
            {
                _proxy.Invoke("Pause");
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