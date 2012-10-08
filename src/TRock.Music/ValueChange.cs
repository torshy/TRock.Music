namespace TRock.Music
{
    public class ValueChange<T>
    {
        #region Properties

        public T OldValue
        {
            get; set;
        }

        public T NewValue
        {
            get; set;
        }

        #endregion Properties
    }
}