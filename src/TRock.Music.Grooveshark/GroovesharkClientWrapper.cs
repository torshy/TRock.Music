using System.Net;

using SciLorsGroovesharkAPI.Groove;
using SciLorsGroovesharkAPI.Groove.Functions;
using SciLorsGroovesharkAPI.Groove.Music;

namespace TRock.Music.Grooveshark
{
    public class GroovesharkClientWrapper : IGroovesharkClient
    {
        #region Fields

        private GroovesharkClient _client;

        #endregion Fields

        #region Constructors

        public GroovesharkClientWrapper()
        {
            GrooveFixExtractor.ExtractResourceToFile("TRock.Music.Grooveshark.GrooveFix.xml", "GrooveFix.xml");
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
        }

        #endregion Constructors

        #region Properties

        public bool IsConnected
        {
            get
            {
                return _client != null && _client.isConnected;
            }
        }

        #endregion Properties

        #region Methods

        public void Connect()
        {
            if (_client == null || !_client.isConnected)
            {
                _client = new GroovesharkClient();
            }
        }

        public SearchArtist.getSearchArtistResultsResponse SearchArtist(string query)
        {
            if (!IsConnected)
            {
                Connect();
            }

            return _client.SearchArtist(query);
        }

        public artistGetSongs.artistGetSongsResponse GetSongsByArtist(int artistId)
        {
            if (!IsConnected)
            {
                Connect();
            }

            return _client.GetSongsByArtist(new SearchArtist.SearchArtistResult
            {
                ArtistID = artistId,
                IsVerified = 1
            });
        }

        public albumGetSongs.albumGetSongsResponse GetSongsByAlbum(int albumId)
        {
            if (!IsConnected)
            {
                Connect();
            }

            return _client.GetSongsByAlbum(new SearchArtist.SearchArtistResult
            {
                AlbumID = albumId,
                IsVerified = 1
            });
        }

        public GroovesharkAudioStream GetMusicStream(int songId, int artistId)
        {
            if (!IsConnected)
            {
                Connect();
            }

            return _client.GetMusicStream(songId, artistId);
        }

        public GetStreamKeyFromSongIDEx.GetStreamKeyFromSongIDExResult GetStreamKey(int songId)
        {
            if (!IsConnected)
            {
                Connect();
            }

            return _client.GetStreamKey(songId);
        }

        #endregion Methods
    }
}