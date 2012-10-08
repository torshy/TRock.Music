using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SciLorsGroovesharkAPI.Groove.Functions;

namespace TRock.Music.Grooveshark
{
    public class GroovesharkSongProvider : ISongProvider
    {
        #region Fields

        public const string ProviderName = "Grooveshark";

        private readonly IGroovesharkClient _client;

        #endregion Fields

        #region Constructors

        public GroovesharkSongProvider(IGroovesharkClient client)
        {
            _client = client;
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get
            {
                return ProviderName;
            }
        }

        #endregion Properties

        #region Methods

        public Task<IEnumerable<Song>> GetSongs(string query, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var response = _client.SearchArtist(query);
                return ConvertToSongs(response.result.result);
            }, cancellationToken);
        }

        public Task<IEnumerable<Album>> GetAlbums(string artistId, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                int artistIdAsInt;

                if (int.TryParse(artistId, out artistIdAsInt))
                {
                    var response = _client.GetSongsByArtist(artistIdAsInt);
                    var songs = response.result.songs.Select(ConvertToSong);
                    var albums = songs.GroupBy(s => new
                    {
                        s.Album.Id,
                        s.Album.Name
                    })
                    .Select(x => new Album
                    {
                        Id = x.Key.Id,
                        Name = x.Key.Name,
                        CoverArt = "http://images.grooveshark.com/static/albums/90_" + x.Key + ".jpg",
                    });
                    return albums;
                }

                return new Album[0];
            }, cancellationToken);
        }

        public Task<ArtistAlbum> GetAlbum(string albumId, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                int albumIdAsInt;

                if (int.TryParse(albumId, out albumIdAsInt))
                {
                    var response = _client.GetSongsByAlbum(albumIdAsInt);
                    var songs = ConvertToSongs(response.result.songs).ToArray();
                    var firstSong = songs.FirstOrDefault();

                    var album = new Album
                    {
                        Id = firstSong.Album.Id,
                        Name = firstSong.Album.Name,
                        CoverArt = firstSong.Album.CoverArt,
                    };

                    var artist = new Artist
                    {
                        Id = firstSong.Artist.Id,
                        Name = firstSong.Artist.Name
                    };

                    return new ArtistAlbum
                    {
                        Album = album,
                        Artist = artist,
                        Songs = songs
                    };
                }

                return null;
            }, cancellationToken);
        }

        private static IEnumerable<Song> ConvertToSongs(IEnumerable<SearchArtist.SearchArtistResult> songs)
        {
            return songs.Select(ConvertToSong);
        }

        internal static Song ConvertToSong(SearchArtist.SearchArtistResult song)
        {
            return new Song
            {
                Id = song.SongID.ToString(),
                Name = song.Name,
                Provider = ProviderName,
                Artist = new Artist
                {
                    Id = song.ArtistID.ToString(),
                    Name = song.ArtistName
                },
                Album = new Album
                {
                    Id = song.AlbumID.ToString(),
                    Name = song.AlbumName,
                    CoverArt = "http://images.grooveshark.com/static/albums/90_" + song.AlbumID + ".jpg"
                },
                TotalSeconds = (int)song.EstimateDuration
            };
        }

        #endregion Methods
    }
}