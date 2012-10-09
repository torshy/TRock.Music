using System.Threading;

using NSubstitute;

using TRock.Music.Grooveshark;
using TRock.Music.Spotify;

using Xunit;

using System.Linq;

namespace TRock.Music.Tests
{
    public class AggregateSongPlayerTest
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

            subPlayer1.CanPlay(Arg.Is(song)).Returns(true);

            var player = new AggregateSongPlayer();
            player.Players.Add(subPlayer1);
            player.Players.Add(subPlayer2);
            player.Players.Add(subPlayer3);
            
            var receivedBuffering = false;
            player.Buffering += (sender, args) => receivedBuffering = true;

            subPlayer1.Buffering += Raise.EventWith(player, new ValueProgressEventArgs<int>(1, 100));

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
                player.Start(song);
            }
        }

        #endregion Methods
    }
}