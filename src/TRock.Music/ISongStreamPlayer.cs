using System;
using System.Collections.Generic;
using System.Threading;

namespace TRock.Music
{
    public interface ISongStreamPlayer
    {
        #region Events

        event EventHandler<SongStreamEventArgs> CurrentStreamChanged;

        event EventHandler<SongStreamEventArgs> CurrentStreamCompleted;

        event EventHandler<SongEventArgs> CurrentSongsChanged;

        #endregion Events

        #region Properties

        ISongStream CurrentStream
        {
            get;
            set;
        }

        IEnumerable<Song> CurrentSongs
        {
            get;
        }

        #endregion Properties

        #region Methods

        bool Next(CancellationToken token);

        #endregion Methods
    }
}