using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;

using Xunit;

namespace TRock.Music.Tests
{
    public class AggregateSongProviderTest
    {
        #region Methods

        [Fact]
        public void GetSongs_ExceptionBubblesUp()
        {
            var provider = new AggregateSongProvider();

            var provider1 = Substitute.For<ISongProvider>();
            var provider2 = Substitute.For<ISongProvider>();
            provider.Providers.Add(provider1);
            provider.Providers.Add(provider2);

            provider1
                .GetSongs(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(x => { throw new Exception(); });

            provider2
                .GetSongs(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    return Task<IEnumerable<Song>>.Factory.StartNew(() => new[] { new Song(), new Song() });
                });

            Assert.Throws<AggregateException>(() => provider.GetSongs("NOFX", CancellationToken.None).Wait());
        }

        [Fact]
        public void GetSongs_AllSongsFromEveryProviderReturned()
        {
            var provider = new AggregateSongProvider();

            var provider1 = Substitute.For<ISongProvider>();
            var provider2 = Substitute.For<ISongProvider>();
            provider.Providers.Add(provider1);
            provider.Providers.Add(provider2);

            provider1
                .GetSongs(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    return Task<IEnumerable<Song>>.Factory.StartNew(() => new[] { new Song(), new Song() });
                });

            provider2
                .GetSongs(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    return Task<IEnumerable<Song>>.Factory.StartNew(() => new[] { new Song(), new Song() });
                });

            var result = provider.GetSongs("NOFX", CancellationToken.None).Result;

            Assert.Equal(4, result.Count());
        }

        #endregion Methods
    }
}