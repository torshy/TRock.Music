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

        public event EventHandler<SongsEventArgs> CurrentSongsChanged;

        public event EventHandler<SongStreamEventArgs> CurrentStreamChanged;

        public event EventHandler<SongStreamEventArgs> CurrentStreamCompleted;

        public event EventHandler<SongEventArgs> NextSong;

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

        public IEnumerable<Song> CurrentSongs
        {
            get
            {
                return _currentSongQueue;
            }
        }

        #endregion Properties

        #region Methods

        public bool Next(CancellationToken token)
        {
            if (_currentStream == null)
            {
                return false;
            }

            if (_currentSongQueue == null)
            {
                if (CurrentStream.MoveNext(token))
                {
                    _currentSongQueue = new ConcurrentQueue<Song>(CurrentStream.Current);
                    OnCurrentSongsChanged(new SongsEventArgs(_currentSongQueue.ToArray()));
                }
                else
                {
                    return false;
                }
            }

            Song song;
            if (_currentSongQueue.TryDequeue(out song))
            {
                OnNextSong(new SongEventArgs(song));
                OnCurrentSongsChanged(new SongsEventArgs(_currentSongQueue.ToArray()));
                return true;
            }

            if (_currentStream.MoveNext(token))
            {
                _currentSongQueue = new ConcurrentQueue<Song>(_currentStream.Current);
                OnCurrentSongsChanged(new SongsEventArgs(_currentSongQueue.ToArray()));

                if (_currentSongQueue.TryDequeue(out song))
                {
                    OnNextSong(new SongEventArgs(song));
                    OnCurrentSongsChanged(new SongsEventArgs(_currentSongQueue.ToArray()));
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

        protected void OnNextSong(SongEventArgs e)
        {
            EventHandler<SongEventArgs> handler = NextSong;
            if (handler != null) handler(this, e);
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

        protected void OnCurrentSongsChanged(SongsEventArgs e)
        {
            EventHandler<SongsEventArgs> handler = CurrentSongsChanged;
            if (handler != null) handler(this, e);
        }

        #endregion Methods
    }
}