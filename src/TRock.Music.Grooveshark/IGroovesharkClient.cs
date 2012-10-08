using SciLorsGroovesharkAPI.Groove.Functions;
using SciLorsGroovesharkAPI.Groove.Music;

namespace TRock.Music.Grooveshark
{
    public interface IGroovesharkClient
    {
        #region Methods

        void Connect();

        SearchArtist.getSearchArtistResultsResponse SearchArtist(string query);

        artistGetSongs.artistGetSongsResponse GetSongsByArtist(int artistId);

        albumGetSongs.albumGetSongsResponse GetSongsByAlbum(int albumId);

        GroovesharkAudioStream GetMusicStream(int songId, int artistId);

        GetStreamKeyFromSongIDEx.GetStreamKeyFromSongIDExResult GetStreamKey(int songId);

        #endregion Methods
    }
}