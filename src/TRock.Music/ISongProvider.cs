using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TRock.Music
{
    public interface ISongProvider
    {
        #region Properties

        string Name
        {
            get;
        }

        #endregion Properties

        #region Methods

        Task<IEnumerable<Song>> GetSongs(string query, CancellationToken cancellationToken);

        Task<IEnumerable<Album>> GetAlbums(string artistId, CancellationToken cancellationToken);

        Task<ArtistAlbum> GetAlbum(string albumId, CancellationToken cancellationToken);

        Task<Artist> GetArtist(string artistId, CancellationToken cancellationToken);

        #endregion Methods
    }
}