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

    public class SongStreamEventArgs : EventArgs
    {
        #region Constructors

        public SongStreamEventArgs(ISongStream stream)
        {
            Stream = stream;
        }

        #endregion Constructors

        #region Properties

        public ISongStream Stream
        {
            get;
            private set;
        }

        #endregion Properties
    }
}