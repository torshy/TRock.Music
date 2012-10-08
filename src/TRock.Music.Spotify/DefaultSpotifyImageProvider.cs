namespace TRock.Music.Spotify
{
    public class DefaultSpotifyImageProvider : ISpotifyImageProvider
    {
        #region Properties

        public string DefaultCoverArt
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public string GetCoverArtUri(string albumId)
        {
            return DefaultCoverArt;
        }

        #endregion Methods
    }
}