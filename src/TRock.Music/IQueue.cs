using System;
using System.Collections.Generic;

namespace TRock.Music
{
    public interface IQueue<T, TK>
    {
        event EventHandler<QueueEventArgs<T>> ItemAdded;

        event EventHandler<QueueEventArgs<T>> ItemRemoved;

        IEnumerable<QueueItem<TK>> CurrentQueue { get; }

        T Enqueue(TK stream);

        bool TryDequeue(out T queueItem);

        bool TryPeek(out T queueItem);
    }
}