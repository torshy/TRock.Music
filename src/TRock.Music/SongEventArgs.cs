using System;

namespace TRock.Music
{
    public class SongEventArgs : EventArgs
    {
        #region Constructors

        public SongEventArgs(Song song)
        {
            Song = song;
        }

        #endregion Constructors

        #region Properties

        public Song Song
        {
            get; private set;
        }

        #endregion Properties
    }
}