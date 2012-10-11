using System.Dynamic;

namespace TRock.Music
{
    public class QueueItem<T>
    {
        #region Constructors

        public QueueItem()
        {
            Bag = new ExpandoObject();
        }

        #endregion Constructors

        #region Properties

        public long Id
        {
            get; set;
        }

        public T Item
        {
            get; set;
        }

        public dynamic Bag
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