using System.Threading;
using System.Threading.Tasks;

using TRock.Music.Grooveshark;
using TRock.Music.Spotify;

using Xunit;

namespace TRock.Music.Tests
{
    public class AggregateSongProviderTest
    {
        [Fact]
        public async Task DoIt()
        {
            var provider = new AggregateSongProvider();
            provider.Providers.Add(new SpotifySongProvider(new DefaultSpotifyImageProvider()));
            provider.Providers.Add(new GroovesharkSongProvider(new GroovesharkClientWrapper()));

            var songs = await provider.GetSongs("NOFX", CancellationToken.None);
            Assert.NotEmpty(songs);
        }
    }
}