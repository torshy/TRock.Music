using System;
using System.Linq;
using System.Threading;
using TRock.Music.Grooveshark;

using Xunit;

namespace TRock.Music.Tests
{
    public class GroovesharkSongProviderTests
    {
        #region Fields

        private readonly ISongProvider _provider;

        #endregion Fields

        #region Constructors

        public GroovesharkSongProviderTests()
        {
            var client = new GroovesharkClientWrapper();
            client.Connect();
            _provider = new GroovesharkSongProvider(client);
        }

        #endregion Constructors

        #region Methods

        [Fact]
        public void SongsGotCorrectProvider()
        {
            var songs = _provider.GetSongs("NOFX", CancellationToken.None).Result;

            foreach (var song in songs)
            {
                Assert.Equal(GroovesharkSongProvider.ProviderName, song.Provider);
            }
        }

        [Fact]
        public void SearchForSong()
        {
            var songs = _provider.GetSongs("NOFX Linoleum", CancellationToken.None).Result;

            Assert.NotEmpty(songs);
            Assert.True(songs.Any(s => s.Name.Equals("Linoleum", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void SearchForAlbums()
        {
            var song = _provider.GetSongs("NOFX", CancellationToken.None).Result.FirstOrDefault();
            var albums = _provider.GetAlbums(song.Artist.Id, CancellationToken.None).Result;

            Assert.NotEmpty(albums);
            Assert.True(albums.Any(a => a.Name.Equals("Punk in Drublic", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void SearchForAlbum()
        {
            var song = _provider.GetSongs("NOFX", CancellationToken.None).Result.FirstOrDefault();
            var result = _provider.GetAlbum(song.Album.Id, CancellationToken.None).Result;
            
            Assert.NotNull(result);
            Assert.NotEmpty(result.Songs);
        }

        #endregion Methods
    }
}