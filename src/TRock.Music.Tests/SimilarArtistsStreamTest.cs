using System.Threading;

using TRock.Music.EchoNest;
using TRock.Music.Spotify;

using Xunit;

namespace TRock.Music.Tests
{
    public class SimilarArtistsStreamTest
    {
        [Fact]
        public void MoveNext_CurrentIsNotEmpty()
        {
            var provider = new SpotifySongProvider(new DefaultSpotifyImageProvider());
            var stream = new SimilarArtistsStream(provider, "NOFX", "RJOXXESVUVZ07WY1T");
            
            if (stream.MoveNext(CancellationToken.None))
            {
                Assert.NotEmpty(stream.Current);
            }
        }
    }
}