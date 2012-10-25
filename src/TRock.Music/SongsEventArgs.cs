using System;

namespace TRock.Music
{
    public class SongsEventArgs : EventArgs
    {
        #region Constructors

        public SongsEventArgs(Song[] songs)
        {
            Songs = songs;
        }

        #endregion Constructors

        #region Properties

        public Song[] Songs
        {
            get;
            private set;
        }

        #endregion Properties
    }
}