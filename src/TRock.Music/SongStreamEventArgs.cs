using System;

namespace TRock.Music
{
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