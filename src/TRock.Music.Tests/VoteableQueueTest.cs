using Xunit;

namespace TRock.Music.Tests
{
    public class VoteableQueueTest
    {
        [Fact]
        public void Upvote_ItemMovedUpInList()
        {
            var queue = new VoteableQueue<int>();

            var item1 = queue.Enqueue(1);
            var item2 = queue.Enqueue(2);
            var item3 = queue.Enqueue(3);

            queue.Upvote(item3.Id);

            Assert.Equal(new[] {item1, item3, item2 }, queue.CurrentQueue);
        }

        [Fact]
        public void Downvote_ItemMovesDownInList()
        {
            var queue = new VoteableQueue<int>();

            var item1 = queue.Enqueue(1);
            var item2 = queue.Enqueue(2);
            var item3 = queue.Enqueue(3);

            queue.Downvote(item2.Id);

            Assert.Equal(new[] { item1, item3, item2 }, queue.CurrentQueue);
        }
    }
}