using System.Threading;
using Xunit;

namespace TRock.Music.Tests
{
    public class SongStreamTest
    {
        [Fact]
        public void SingleSongStreamTest()
        {
            SingleSongStream stream = new SingleSongStream(new Song());

            Assert.Empty(stream.Current);
            Assert.True(stream.MoveNext(CancellationToken.None));
            Assert.NotEmpty(stream.Current);
            Assert.False(stream.MoveNext(CancellationToken.None));
        }

        [Fact]
        public void MultiSongStreamTest()
        {
            MultiSongStream stream = new MultiSongStream(new[] { new Song(), new Song()});

            Assert.Empty(stream.Current);
            Assert.True(stream.MoveNext(CancellationToken.None));
            Assert.NotEmpty(stream.Current);
            Assert.False(stream.MoveNext(CancellationToken.None));
        }
    }
}