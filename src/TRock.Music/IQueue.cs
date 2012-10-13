using System;
using System.Collections.Generic;

namespace TRock.Music
{
    public interface IQueue<T, in TK>
    {
        #region Events

        event EventHandler<QueueEventArgs<T>> ItemAdded;

        event EventHandler<QueueEventArgs<T>> ItemRemoved;

        #endregion Events

        #region Properties

        IEnumerable<T> CurrentQueue
        {
            get;
        }

        #endregion Properties

        #region Methods

        T Enqueue(TK stream, Action<T> setup = null);

        bool TryDequeue(out T queueItem);

        bool TryPeek(out T queueItem);

        bool IsInFront(T queueItem);

        #endregion Methods
    }
}