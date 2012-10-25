using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace TRock.Music
{
    public class SongStreamPlayer : ISongStreamPlayer
    {
        #region Fields

        private ConcurrentQueue<Song> _currentSongQueue;
        private ISongStream _currentStream;

        #endregion Fields

        #region Events

        public event EventHandler<SongStreamEventArgs> CurrentStreamChanged;

        public event EventHandler<SongStreamEventArgs> CurrentStreamCompleted;

        public event EventHandler<SongEventArgs> CurrentSongsChanged;

        #endregion Events

        #region Properties

        public ISongStream CurrentStream
        {
            get { return _currentStream; }
            set
            {
                if (_currentStream != value)
                {
                    _currentSongQueue = null;
                    _currentStream = value;
                    OnCurrentStreamChanged(new SongStreamEventArgs(value));
                }
            }
        }

        #endregion Properties

        #region Methods

        public IEnumerable<Song> CurrentSongs
        {
            get
            {
                return _currentSongQueue;
            }
        }

        public bool Next(CancellationToken token)
        {
            if (_currentStream == null)
            {
                throw new InvalidOperationException("Please initialize the stream player with a song stream");
            }

            if (_currentSongQueue == null)
            {
                if (CurrentStream.MoveNext(token))
                {
                    _currentSongQueue = new ConcurrentQueue<Song>(CurrentStream.Current);
                }
                else
                {
                    return false;
                }
            }

            Song song;
            if (_currentSongQueue.TryDequeue(out song))
            {
                OnCurrentSongsChanged(new SongEventArgs(song));
                return true;
            }

            if (_currentStream.MoveNext(token))
            {
                _currentSongQueue = new ConcurrentQueue<Song>(_currentStream.Current);

                if (_currentSongQueue.TryDequeue(out song))
                {
                    OnCurrentSongsChanged(new SongEventArgs(song));
                    return true;
                }

                OnCurrentStreamCompleted(new SongStreamEventArgs(CurrentStream));
            }
            else
            {
                OnCurrentStreamCompleted(new SongStreamEventArgs(CurrentStream));
            }

            return false;
        }

        protected virtual void OnCurrentStreamChanged(SongStreamEventArgs e)
        {
            EventHandler<SongStreamEventArgs> handler = CurrentStreamChanged;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnCurrentStreamCompleted(SongStreamEventArgs e)
        {
            EventHandler<SongStreamEventArgs> handler = CurrentStreamCompleted;
            if (handler != null) handler(this, e);
        }

        protected void OnCurrentSongsChanged(SongEventArgs e)
        {
            EventHandler<SongEventArgs> handler = CurrentSongsChanged;
            if (handler != null) handler(this, e);
        }

        #endregion Methods
    }
}