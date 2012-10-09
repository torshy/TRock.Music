namespace TRock.Music
{
    public class QueueItem<T>
    {
        public long Id { get; set; }
        public T Item { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}", Id);
        }
    }
}