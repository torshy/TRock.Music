using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TRock.Music.Spotify;

using Xunit;

namespace TRock.Music.Tests
{
    public class SpotifySongProviderTest
    {
        #region Fields

        private readonly ISongProvider _provider;

        #endregion Fields

        #region Constructors

        public SpotifySongProviderTest()
        {
            _provider = new SpotifySongProvider(new DefaultSpotifyImageProvider());
        }

        #endregion Constructors

        [Fact]
        public async Task SongsGotCorrectProvider()
        {
            var songs = await _provider.GetSongs("NOFX", CancellationToken.None);

            foreach (var song in songs)
            {
                Assert.Equal(SpotifySongProvider.ProviderName, song.Provider);
            }
        }

        [Fact]
        public async Task SearchForSong()
        {
            var songs = await _provider.GetSongs("NOFX Linoleum", CancellationToken.None);

            Assert.NotEmpty(songs);
            Assert.True(songs.Any(s => s.Name.Equals("Linoleum", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task SearchForAlbums()
        {
            var songs = await _provider.GetSongs("NOFX", CancellationToken.None);
            var song = songs.First();
            var albums = _provider.GetAlbums(song.Artist.Id, CancellationToken.None).Result;

            Assert.NotEmpty(albums);
            Assert.True(albums.Any(a => a.Name.Equals("Punk in Drublic", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task SearchForAlbum()
        {
            var songs = await _provider.GetSongs("NOFX", CancellationToken.None);
            var song = songs.First();
            var result = _provider.GetAlbum(song.Album.Id, CancellationToken.None).Result;

            Assert.NotNull(result);
            Assert.NotEmpty(result.Songs);
        }
    }
}