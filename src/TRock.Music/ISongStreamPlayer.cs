using System;
using System.Collections.Generic;
using System.Threading;

namespace TRock.Music
{
    public interface ISongStreamPlayer
    {
        #region Events

        event EventHandler<SongStreamEventArgs> CurrentStreamChanged;

        event EventHandler<SongStreamEventArgs> CurrentStreamComplete;

        event EventHandler<SongEventArgs> SongChanged;

        #endregion Events

        #region Properties

        ISongStream CurrentStream
        {
            get;
            set;
        }

        IEnumerable<Song> CurrentStreamSongQueue
        {
            get;
        }

        #endregion Properties

        #region Methods

        bool NextBatch(CancellationToken token);

        bool NextSongInBatch();

        #endregion Methods
    }
}