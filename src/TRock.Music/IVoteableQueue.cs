using System;

namespace TRock.Music
{
    public interface IVoteableQueue<T> : IQueue<VoteableQueueItem<T>, T>
    {
        #region Events

        event EventHandler<QueueEventArgs<VoteableQueueItem<T>>> ItemUpvoted;

        event EventHandler<QueueEventArgs<VoteableQueueItem<T>>> ItemDownvoted;

        event EventHandler<QueueEventArgs<VoteableQueueItem<T>>> ItemMoved;

        #endregion Events

        #region Methods

        bool Upvote(long id);

        bool Downvote(long id);

        #endregion Methods
    }
}