using System;
using System.Collections.Generic;
using System.Linq;

namespace TRock.Music
{
    public class VoteableQueue<T> : IVoteableQueue<VoteableQueueItem<T>, T>
    {
        #region Fields

        private static long _unique;
        private readonly List<VoteableQueueItem<T>> _items;
        private readonly object _lockObject = new object();
        private VoteableQueueItem<T> _head;

        #endregion Fields

        #region Constructors

        public VoteableQueue()
        {
            _items = new List<VoteableQueueItem<T>>();
        }

        #endregion Constructors

        #region Events

        public event EventHandler<QueueEventArgs<VoteableQueueItem<T>>> ItemAdded;

        public event EventHandler<QueueEventArgs<VoteableQueueItem<T>>> ItemDownvoted;

        public event EventHandler<QueueEventArgs<VoteableQueueItem<T>>> ItemRemoved;

        public event EventHandler<QueueEventArgs<VoteableQueueItem<T>>> ItemUpvoted;

        #endregion Events

        #region Properties

        public IEnumerable<QueueItem<T>> CurrentQueue
        {
            get
            {
                lock (_lockObject)
                {
                    return _items
                        .OrderByDescending(x => x == _head)
                        .ThenByDescending(s => s.Score)
                        .ThenBy(s => s.Id)
                        .ThenBy(s => s.VoteTick).ToArray();
                }
            }
        }

        #endregion Properties

        #region Methods

        public VoteableQueueItem<T> Enqueue(T stream)
        {
            var item = new VoteableQueueItem<T>
            {
                Id = _unique++,
                Item = stream
            };

            lock (_lockObject)
            {
                _items.Add(item);

                if (_items.Count == 1)
                {
                    _head = item;
                }

                OnItemAdded(new QueueEventArgs<VoteableQueueItem<T>>(item));
            }

            return item;
        }

        public bool TryDequeue(out VoteableQueueItem<T> queueItem)
        {
            lock (_lockObject)
            {
                if (_items.Count > 0)
                {
                    queueItem = _items[0];
                    _items.RemoveAt(0);
                    TryPeek(out _head);
                    OnItemRemoved(new QueueEventArgs<VoteableQueueItem<T>>(queueItem));
                    return true;
                }
            }

            queueItem = null;
            return false;
        }

        public bool TryPeek(out VoteableQueueItem<T> queueItem)
        {
            lock (_lockObject)
            {
                if (_items.Count > 0)
                {
                    queueItem = _items[0];
                    return true;
                }
            }

            queueItem = null;
            return false;
        }

        public bool Upvote(long id)
        {
            lock (_lockObject)
            {
                var result = _items.Find(item => item.Id == id);

                if (result != null)
                {
                    result.Upvotes++;
                    OnItemUpvoted(new QueueEventArgs<VoteableQueueItem<T>>(result));
                    return true;
                }
            }

            return false;
        }

        public bool Downvote(long id)
        {
            lock (_lockObject)
            {
                var result = _items.Find(item => item.Id == id);

                if (result != null)
                {
                    result.Downvotes++;
                    OnItemDownvoted(new QueueEventArgs<VoteableQueueItem<T>>(result));
                    return true;
                }
            }

            return false;
        }

        protected void OnItemAdded(QueueEventArgs<VoteableQueueItem<T>> e)
        {
            EventHandler<QueueEventArgs<VoteableQueueItem<T>>> handler = ItemAdded;
            if (handler != null) handler(this, e);
        }

        protected void OnItemDownvoted(QueueEventArgs<VoteableQueueItem<T>> e)
        {
            EventHandler<QueueEventArgs<VoteableQueueItem<T>>> handler = ItemDownvoted;
            if (handler != null) handler(this, e);
        }

        protected void OnItemRemoved(QueueEventArgs<VoteableQueueItem<T>> e)
        {
            EventHandler<QueueEventArgs<VoteableQueueItem<T>>> handler = ItemRemoved;
            if (handler != null) handler(this, e);
        }

        protected void OnItemUpvoted(QueueEventArgs<VoteableQueueItem<T>> e)
        {
            EventHandler<QueueEventArgs<VoteableQueueItem<T>>> handler = ItemUpvoted;
            if (handler != null) handler(this, e);
        }

        #endregion Methods
    }
}