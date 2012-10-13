using TRock.Music.Spotify;

namespace TRock.Music.Torshify
{
    public class TorshifyImageProvider : ISpotifyImageProvider
    {
        #region Constructors

        public TorshifyImageProvider()
        {
            TorshifyServerUrl = "http://localhost:8082";
        }

        #endregion Constructors

        #region Properties

        public string TorshifyServerUrl
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public string GetCoverArtUri(string albumId)
        {
            return TorshifyServerUrl + "/torshify/album/cover/" + albumId;
        }

        #endregion Methods
    }
}