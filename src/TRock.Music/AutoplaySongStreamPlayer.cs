using System.Threading;

namespace TRock.Music
{
    public class AutoplaySongStreamPlayer : SongStreamPlayer
    {
        #region Fields

        private readonly ISongPlayer _songPlayer;
        private readonly IVoteableQueue<ISongStream> _streamQueue;

        #endregion Fields

        #region Constructors

        public AutoplaySongStreamPlayer(ISongPlayer songPlayer, IVoteableQueue<ISongStream> streamQueue)
        {
            _songPlayer = songPlayer;
            _streamQueue = streamQueue;
            _streamQueue.ItemAdded += StreamQueueOnItemAdded;
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

        protected override void OnCurrentStreamComplete(SongStreamEventArgs e)
        {
            base.OnCurrentStreamComplete(e);

            VoteableQueueItem<ISongStream> item;

            if (_streamQueue.TryGetNext(out item))
            {
                CurrentStream = item.Item;
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

        private void StreamQueueOnItemAdded(object sender, QueueEventArgs<VoteableQueueItem<ISongStream>> e)
        {
            if (_streamQueue.IsInFront(e.Item))
            {
                CurrentStream = e.Item.Item;
            }
        }

        #endregion Methods
    }
}