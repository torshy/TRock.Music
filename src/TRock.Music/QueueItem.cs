namespace TRock.Music
{
    public class QueueItem<T>
    {
        #region Properties

        public long Id
        {
            get; set;
        }

        public T Item
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return string.Format("Id: {0}", Id);
        }

        #endregion Methods
    }
}