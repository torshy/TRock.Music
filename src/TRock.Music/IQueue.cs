using System;
using System.Collections.Generic;

namespace TRock.Music
{
    public interface IQueue<T, TK>
    {
        #region Events

        event EventHandler<QueueEventArgs<T>> ItemAdded;

        event EventHandler<QueueEventArgs<T>> ItemRemoved;

        #endregion Events

        #region Properties

        IEnumerable<QueueItem<TK>> CurrentQueue
        {
            get;
        }

        #endregion Properties

        #region Methods

        T Enqueue(TK stream);

        bool TryDequeue(out T queueItem);

        bool TryPeek(out T queueItem);

        #endregion Methods
    }
}