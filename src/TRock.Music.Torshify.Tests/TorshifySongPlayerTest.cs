using NSubstitute;

using Torshify;

using TRock.Music.Torshify.Server;

using Xunit;

namespace TRock.Music.Torshify.Tests
{
    public class TorshifySongPlayerTest
    {
        #region Fields

        private readonly ILink<ITrackAndOffset> _link;
        private readonly ISession _session;
        private readonly TorshifySongPlayer _songPlayer;

        #endregion Fields

        #region Constructors

        public TorshifySongPlayerTest()
        {
            _session = Substitute.For<ISession>();
            _link = Substitute.For<ILink<ITrackAndOffset>>();

            var linkFactory = Substitute.For<ILinkFactory>();
            linkFactory.GetLink(Arg.Any<string>()).Returns(_link);

            _songPlayer = new TorshifySongPlayer(_session, linkFactory);
        }

        #endregion Constructors

        #region Methods

        [Fact]
        public void CurrentSongChanged_Is_Raised_When_Start()
        {
            var song = new Song();

            _link.Object.Track.IsLoaded.Returns(true);

            bool wasCalled = false;
            _songPlayer.CurrentSongChanged += (sender, args) => wasCalled = true;
            _songPlayer.Start(song);
            
            Assert.True(wasCalled);
        }
        
        [Fact]
        public void CurrentSongCompleted_Is_Raised_When_SongCantLoad()
        {
            var song = new Song();

            _link.Object.Track.IsLoaded.Returns(false);
            _link.Object.Track.Error.Returns(Error.IsLoading);

            bool wasCalled = false;
            
            _songPlayer.CurrentSongCompleted += (sender, args) => wasCalled = true;
            _songPlayer.Start(song);

            Assert.True(wasCalled);
        }

        [Fact]
        public void CurrentSongCompleted_Is_Raised_When_Stop()
        {
            var song = new Song();

            bool wasCalled = false;
            _songPlayer.CurrentSongCompleted += (sender, args) => wasCalled = true;
            _songPlayer.Start(song);

            _session.MusicDeliver += Raise.EventWith(new MusicDeliveryEventArgs(2, 44100, new byte[1024], 1024));

            _songPlayer.Stop();

            Assert.True(wasCalled);
        }

        #endregion Methods
    }
}