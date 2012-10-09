using System.Threading;

namespace TRock.Music
{
    public class AutoplaySongStreamPlayer : SongStreamPlayer
    {
        #region Fields

        private readonly ISongPlayer _songPlayer;

        #endregion Fields

        #region Constructors

        public AutoplaySongStreamPlayer(ISongPlayer songPlayer)
        {
            _songPlayer = songPlayer;
            _songPlayer.CurrentSongCompleted += OnCurrentSongCompleted;
        }

        #endregion Constructors

        #region Methods

        protected virtual void OnCurrentSongCompleted(object sender, SongEventArgs e)
        {
            if (!NextSongInBatch())
            {
                if (NextBatch(CancellationToken.None))
                {
                    NextSongInBatch();
                }
            }
        }

        protected override void OnCurrentStreamChanged(SongStreamEventArgs e)
        {
            base.OnCurrentStreamChanged(e);

            if (e.Stream != null)
            {
                if (NextBatch(CancellationToken.None))
                {
                    NextSongInBatch();
                }
            }
        }

        protected override void OnSongChanged(SongEventArgs e)
        {
            base.OnSongChanged(e);

            if (_songPlayer.CanPlay(e.Song))
            {
                _songPlayer.Start(e.Song);
            }
        }

        #endregion Methods
    }
}