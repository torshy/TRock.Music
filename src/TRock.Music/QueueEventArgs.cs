using System;

namespace TRock.Music
{
    public class QueueEventArgs<T> : EventArgs
    {
        #region Constructors

        public QueueEventArgs(T item)
        {
            Item = item;
        }

        #endregion Constructors

        #region Properties

        public T Item
        {
            get; private set;
        }

        #endregion Properties
    }
}