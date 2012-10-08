using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TRock.Music
{
    public interface ISongProvider
    {
        string Name { get; }

        Task<IEnumerable<Song>> GetSongs(string query, CancellationToken cancellationToken);

        Task<IEnumerable<Album>> GetAlbums(string artistId, CancellationToken cancellationToken);

        Task<ArtistAlbum> GetAlbum(string albumId, CancellationToken cancellationToken);
    }
}