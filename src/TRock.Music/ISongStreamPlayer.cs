using System;
using System.Threading;

namespace TRock.Music
{
    public interface ISongStreamPlayer
    {
        #region Events

        event EventHandler<SongStreamEventArgs> CurrentStreamChanged;

        event EventHandler<SongStreamEventArgs> StreamComplete;

        event EventHandler<SongEventArgs> SongChanged;

        #endregion Events

        #region Properties

        ISongStream CurrentStream
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        bool NextBatch(CancellationToken token);

        bool NextSongInBatch();

        #endregion Methods
    }
}