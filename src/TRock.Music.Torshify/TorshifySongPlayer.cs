using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using SignalR.Client;
using SignalR.Client.Hubs;

using TRock.Music.Spotify;

namespace TRock.Music.Torshify
{
    public class TorshifySongPlayer : ISongPlayer
    {
        #region Fields

        private readonly Subject<ValueProgress<int>> _buffering;
        private readonly Subject<ValueChange<Song>> _currentSongChanged;
        private readonly Subject<Song> _currentSongCompleted;
        private readonly Subject<ValueChange<bool>> _isMutedChanged;
        private readonly Subject<ValueChange<bool>> _isPlayingChanged;
        private readonly Subject<ValueProgress<int>> _progress;
        private readonly Subject<ValueChange<float>> _volumeChanged;
        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        #endregion Fields

        #region Constructors

        public TorshifySongPlayer(Uri server)
        {
            _isMutedChanged = new Subject<ValueChange<bool>>();
            _isPlayingChanged = new Subject<ValueChange<bool>>();
            _volumeChanged = new Subject<ValueChange<float>>();
            _currentSongChanged = new Subject<ValueChange<Song>>();
            _currentSongCompleted = new Subject<Song>();
            _buffering = new Subject<ValueProgress<int>>();
            _progress = new Subject<ValueProgress<int>>();

            _connection = new HubConnection(server.AbsoluteUri);
            _proxy = _connection.CreateProxy("TorshifyHub");
            _proxy.On<Tuple<int, int>>("Progress", progress =>
            {
                Console.WriteLine("Progress=>" + progress.Item1 + "x" + progress.Item2);

                _progress
                    .NotifyOn(Scheduler.Default)
                    .OnNext(new ValueProgress<int>
                    {
                        Current = progress.Item1,
                        Total = progress.Item2
                    });
            });
            _proxy.On<Tuple<int, int>>("Buffering", progress =>
            {
                Console.WriteLine("Buffering=>" + progress.Item1 + "x" + progress.Item2);

                _buffering
                    .NotifyOn(Scheduler.Default)
                    .OnNext(new ValueProgress<int>
                    {
                        Current = progress.Item1,
                        Total = progress.Item2
                    });
            });
            _proxy.On<Tuple<int, int>>("VolumeChanged", volume =>
            {
                Console.WriteLine("VolumeChanged=>" + volume.Item1 + "x" + volume.Item2);

                _volumeChanged
                    .NotifyOn(Scheduler.Default)
                    .OnNext(new ValueChange<float>
                    {
                        OldValue = volume.Item1,
                        NewValue = volume.Item2
                    });
            });
            _proxy.On<bool>("IsMutedChanged", isMuted =>
            {
                Console.WriteLine("IsMutedChanged=>" + isMuted);

                _isMutedChanged
                    .NotifyOn(Scheduler.Default)
                    .OnNext(new ValueChange<bool>
                    {
                        OldValue = !isMuted,
                        NewValue = isMuted
                    });
            });
            _proxy.On<bool>("IsPlayingChanged", isPlaying =>
            {
                Console.WriteLine("IsPlayingChanged=>" + isPlaying);

                _isPlayingChanged
                    .NotifyOn(Scheduler.Default)
                    .OnNext(new ValueChange<bool>
                    {
                        OldValue = !isPlaying,
                        NewValue = isPlaying
                    });
            });
            _proxy.On<Tuple<Song, Song>>("CurrentSongChanged", song =>
            {
                Console.WriteLine("CurrentSongChanged=>" + song.Item1 + "x" + song.Item2);

                _currentSongChanged
                    .NotifyOn(Scheduler.Default)
                    .OnNext(new ValueChange<Song>
                    {
                        OldValue = song.Item1,
                        NewValue = song.Item2
                    });
            });
            _proxy.On<Song>("CurrentSongCompleted", song =>
            {
                Console.WriteLine("CurrentSongCompleted=>" + song.Name);

                _currentSongCompleted
                    .NotifyOn(Scheduler.Default)
                    .OnNext(song);
            });
            
            _connection.Start();
        }

        #endregion Constructors

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

        #endregion Methods
    }
}