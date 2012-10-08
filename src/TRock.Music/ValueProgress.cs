namespace TRock.Music
{
    public class ValueProgress<T>
    {
        #region Properties

        public T Current
        {
            get; set;
        }

        public T Total
        {
            get; set;
        }

        #endregion Properties
    }
}