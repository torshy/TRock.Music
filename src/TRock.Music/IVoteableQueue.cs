using System;

namespace TRock.Music
{
    public interface IVoteableQueue<T, TK> : IQueue<T, TK>
    {
        event EventHandler<QueueEventArgs<VoteableQueueItem<TK>>> ItemUpvoted;

        event EventHandler<QueueEventArgs<VoteableQueueItem<TK>>> ItemDownvoted;

        bool Upvote(long id);

        bool Downvote(long id);
    }
}