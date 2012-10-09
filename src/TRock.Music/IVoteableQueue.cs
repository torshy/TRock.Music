using System;

namespace TRock.Music
{
    public interface IVoteableQueue<T, TK> : IQueue<T, TK>
    {
        #region Events

        event EventHandler<QueueEventArgs<VoteableQueueItem<TK>>> ItemUpvoted;

        event EventHandler<QueueEventArgs<VoteableQueueItem<TK>>> ItemDownvoted;

        #endregion Events

        #region Methods

        bool Upvote(long id);

        bool Downvote(long id);

        #endregion Methods
    }
}