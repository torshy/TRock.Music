namespace TRock.Music
{
    public static class QueueExtensions
    {
        public static bool TryGetNext<T, TK>(this IQueue<T, TK> queue, out T item)
        {
            if (queue.TryDequeue(out item))
            {
                return queue.TryPeek(out item);
            }

            item = default(T);
            return false;
        }
    }
}