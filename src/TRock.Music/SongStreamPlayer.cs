using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace TRock.Music
{
    public class SongStreamPlayer : ISongStreamPlayer
    {
        #region Fields

        protected readonly object _lockObject = new object();
        protected readonly ConcurrentQueue<Song> _songQueue;
        protected readonly List<ISongStream> _streams;

        #endregion Fields

        #region Constructors

        public SongStreamPlayer()
        {
            _streams = new List<ISongStream>();
            _songQueue = new ConcurrentQueue<Song>();
        }

        #endregion Constructors

        #region Events

        public event EventHandler<SongStreamEventArgs> CurrentStreamChanged;

        public event EventHandler<SongEventArgs> SongChanged;

        public event EventHandler<SongStreamEventArgs> StreamAdded;

        public event EventHandler<SongStreamEventArgs> StreamComplete;

        public event EventHandler<SongStreamEventArgs> StreamRemoved;

        #endregion Events

        #region Properties

        public ISongStream CurrentStream
        {
            get
            {
                lock (_lockObject)
                {
                    if (_streams.Count > 0)
                    {
                        return _streams[0];
                    }

                    return null;
                }
            }
        }

        public IEnumerable<ISongStream> Streams
        {
            get
            {
                lock (_lockObject)
                {
                    return _streams.ToArray();
                }
            }
        }

        #endregion Properties

        #region Methods

        public void Add(ISongStream stream)
        {
            lock (_lockObject)
            {
                _streams.Add(stream);
                OnStreamAdded(new SongStreamEventArgs(stream));
            }
        }

        public void Remove(ISongStream stream)
        {
            lock (_lockObject)
            {
                if (_streams.Remove(stream))
                {
                    OnStreamRemoved(new SongStreamEventArgs(stream));
                }
            }
        }

        public bool NextBatch(CancellationToken token)
        {
            ISongStream stream = CurrentStream;

            if (stream == null)
            {
                return false;
            }

            if (stream.MoveNext(token))
            {
                Song _;
                while (_songQueue.TryDequeue(out _)) { }

                foreach (var song in stream.Current)
                {
                    _songQueue.Enqueue(song);
                }

                return !_songQueue.IsEmpty;
            }

            return false;
        }

        public bool NextSongInBatch()
        {
            ISongStream stream = CurrentStream;

            if (stream != null && _songQueue.IsEmpty && !NextBatch(CancellationToken.None))
            {
                OnStreamComplete(new SongStreamEventArgs(stream));
                return false;
            }

            Song upNext;
            if (_songQueue.TryDequeue(out upNext))
            {
                OnSongChanged(new SongEventArgs(upNext));
                return true;
            }

            return false;
        }

        protected virtual void OnSongChanged(SongEventArgs e)
        {
            EventHandler<SongEventArgs> handler = SongChanged;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnCurrentStreamChanged(SongStreamEventArgs e)
        {
            EventHandler<SongStreamEventArgs> handler = CurrentStreamChanged;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnStreamAdded(SongStreamEventArgs e)
        {
            EventHandler<SongStreamEventArgs> handler = StreamAdded;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnStreamRemoved(SongStreamEventArgs e)
        {
            EventHandler<SongStreamEventArgs> handler = StreamRemoved;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnStreamComplete(SongStreamEventArgs e)
        {
            EventHandler<SongStreamEventArgs> handler = StreamComplete;
            if (handler != null) handler(this, e);
        }

        #endregion Methods
    }
}