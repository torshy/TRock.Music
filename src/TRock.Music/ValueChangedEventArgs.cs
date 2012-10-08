using System;

namespace TRock.Music
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        #region Constructors

        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion Constructors

        #region Properties

        public T OldValue
        {
            get; private set;
        }

        public T NewValue
        {
            get; private set;
        }

        #endregion Properties
    }
}