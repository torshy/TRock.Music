using System;

namespace TRock.Music
{
    public class QueueEventArgs<T> : EventArgs
    {
        public QueueEventArgs(T item)
        {
            Item = item;
        }

        public T Item { get; private set; }
    }
}