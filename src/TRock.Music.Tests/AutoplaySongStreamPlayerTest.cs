using System.Threading;
using NSubstitute;
using Xunit;

namespace TRock.Music.Tests
{
    public class AutoplaySongStreamPlayerTest
    {
        private readonly ISongStreamPlayer _streamPlayer;
        private readonly ISongPlayer _songPlayer;

        public AutoplaySongStreamPlayerTest()
        {
            _songPlayer = Substitute.For<ISongPlayer>();
            _songPlayer.CanPlay(Arg.Any<Song>()).Returns(true);
            _streamPlayer = new AutoplaySongStreamPlayer(_songPlayer);
        }

        [Fact]
        public void Add_Stream_To_Empty_Queue_Song_Automatically_Started()
        {
            ISongStream stream = Substitute.For<ISongStream>();
            stream.MoveNext(Arg.Any<CancellationToken>()).Returns(true);
            stream.Current.Returns(new[]
            {
                new Song(),
                new Song()
            });
            
            // Add stream to empty list
            _streamPlayer.Add(stream);

            // Expect the song player to have been automatically called when adding the first song stream
            _songPlayer.Received().CanPlay(Arg.Any<Song>());
            _songPlayer.Received().Start(Arg.Any<Song>());
        }
    }
}