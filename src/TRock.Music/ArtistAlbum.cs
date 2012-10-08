namespace TRock.Music
{
    public class ArtistAlbum
    {
        #region Properties

        public Artist Artist
        {
            get; set;
        }

        public Album Album
        {
            get; set;
        }

        public Song[] Songs
        {
            get; set;
        }

        #endregion Properties
    }
}