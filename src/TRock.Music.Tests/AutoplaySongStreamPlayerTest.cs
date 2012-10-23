using System.Threading;

using NSubstitute;

using Xunit;

namespace TRock.Music.Tests
{
    public class AutoplaySongStreamPlayerTest
    {
        #region Fields

        private readonly ISongPlayer _songPlayer;
        private readonly ISongStreamPlayer _streamPlayer;
        private readonly IVoteableQueue<ISongStream> _streamQueue;

        #endregion Fields

        #region Constructors

        public AutoplaySongStreamPlayerTest()
        {
            _streamQueue = Substitute.For<IVoteableQueue<ISongStream>>();
            _songPlayer = Substitute.For<ISongPlayer>();
            _songPlayer.CanPlay(Arg.Any<Song>()).Returns(true);
            _songPlayer
                .When(x => x.Start(Arg.Any<Song>()))
                .Do(c => _songPlayer.CurrentSong.Returns(c[0] as Song));


            _streamPlayer = new AutoplaySongStreamPlayer(_songPlayer, _streamQueue);
        }

        #endregion Constructors

        #region Methods

        [Fact]
        public void CurrentStream_PlayBatchAutomatically()
        {
            ISongStream stream = Substitute.For<ISongStream>();
            stream.MoveNext(Arg.Any<CancellationToken>()).Returns(true);
            stream.Current.Returns(new[] { new Song() });

            _streamPlayer.CurrentStream = stream;

            // Expect the song player to have been automatically called when adding the first song stream
            _songPlayer.Received().CanPlay(Arg.Any<Song>());
            _songPlayer.Received().Start(Arg.Any<Song>());
        }

        [Fact]
        public void NextSongInBatch_PlaySongAutomatically()
        {
            Song song1 = new Song();
            Song song2 = new Song();

            ISongStream stream = Substitute.For<ISongStream>();
            stream.MoveNext(Arg.Any<CancellationToken>()).Returns(true);
            stream.Current.Returns(new[] { song1, song2 });

            _streamPlayer.CurrentStream = stream;
            Assert.Equal(song1, _songPlayer.CurrentSong);

            _streamPlayer.NextSongInBatch();
            Assert.Equal(song2, _songPlayer.CurrentSong);

            stream.MoveNext(Arg.Any<CancellationToken>()).Returns(false);

            Assert.False(_streamPlayer.NextSongInBatch());
        }

        [Fact]
        public void CurrentSongCompleted_PlayNextSongAutomatically()
        {
            Song song1 = new Song();
            Song song2 = new Song();

            ISongStream stream = Substitute.For<ISongStream>();
            stream.MoveNext(Arg.Any<CancellationToken>()).Returns(true);
            stream.Current.Returns(new[] { song1, song2 });

            _streamPlayer.CurrentStream = stream;
            Assert.Equal(song1, _songPlayer.CurrentSong);

            _songPlayer.CurrentSongCompleted += Raise.EventWith(_songPlayer, new SongEventArgs(song1));

            Assert.Equal(song2, _songPlayer.CurrentSong);
        }

        #endregion Methods
    }
}