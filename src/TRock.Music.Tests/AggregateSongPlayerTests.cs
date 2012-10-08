using System.Threading;

using NSubstitute;

using System;
using System.Reactive.Subjects;

using TRock.Music.Aggregate;
using TRock.Music.Grooveshark;
using TRock.Music.Spotify;

using Xunit;

using System.Linq;
using System.Reactive.Linq;

namespace TRock.Music.Tests
{
    public class AggregateSongPlayerTests
    {
        #region Methods

        [Fact]
        public void DoIt()
        {
            var subPlayer1 = Substitute.For<ISongPlayer>();
            var subPlayer2 = Substitute.For<ISongPlayer>();
            var subPlayer3 = Substitute.For<ISongPlayer>();

            var song = new Song
            {
                Provider = "SubProvider#1"
            };

            var buffering = new Subject<ValueProgress<int>>();
            subPlayer1.Buffering.Returns(buffering);
            subPlayer1.CanPlay(Arg.Is(song)).Returns(true);

            var player = new AggregateSongPlayer();
            player.Players.Add(subPlayer1);
            player.Players.Add(subPlayer2);
            player.Players.Add(subPlayer3);

            var receivedBuffering = false;
            player.Buffering.Subscribe(item =>
            {
                receivedBuffering = true;
            });

            buffering.OnNext(new ValueProgress<int>());

            Assert.True(receivedBuffering);
            Assert.True(player.CanPlay(song));
        }

        [Fact]
        public void TestIt()
        {
            var groove = new GroovesharkClientWrapper();
            groove.Connect();

            var provider = new AggregateSongProvider();
            provider.Providers.Add(new GroovesharkSongProvider(groove));
            provider.Providers.Add(new SpotifySongProvider(new DefaultSpotifyImageProvider()));

            var player = new AggregateSongPlayer();
            player.Players.Add(new GroovesharkSongPlayer(groove));

            var song = provider.GetSongs("NOFX", CancellationToken.None).Result.FirstOrDefault();

            if (player.CanPlay(song))
            {
                player.Progress.Buffer(TimeSpan.FromSeconds(5)).Subscribe(p =>
                {
                    player.Stop();
                    Assert.NotEmpty(p);
                });

                player.Start(song);
            }
        }

        #endregion Methods
    }
}