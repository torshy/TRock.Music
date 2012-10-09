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
        }

        #endregion Constructors

        #region Methods

        protected override void OnStreamAdded(SongStreamEventArgs e)
        {
            base.OnStreamAdded(e);

            lock (_lockObject)
            {
                if (_streams.Count == 1)
                {
                    if (NextBatch(CancellationToken.None))
                    {
                        NextSongInBatch();
                    }
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