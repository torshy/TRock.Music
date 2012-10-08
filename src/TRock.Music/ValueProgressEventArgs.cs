using System;

namespace TRock.Music
{
    public class ValueProgressEventArgs<T> : EventArgs
    {
        #region Constructors

        public ValueProgressEventArgs(T current, T total)
        {
            Current = current;
            Total = total;
        }

        #endregion Constructors

        #region Properties

        public T Current
        {
            get;
            private set;
        }

        public T Total
        {
            get;
            private set;
        }

        #endregion Properties
    }
}