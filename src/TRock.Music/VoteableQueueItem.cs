using System;

namespace TRock.Music
{
    public class VoteableQueueItem<T> : QueueItem<T>
    {
        #region Fields

        private int _downvotes;
        private int _upvotes;

        #endregion Fields

        #region Properties

        public int Upvotes
        {
            get { return _upvotes; }
            set
            {
                _upvotes = Math.Max(0, value);
                VoteTick = Environment.TickCount;
            }
        }

        public int Downvotes
        {
            get { return _downvotes; }
            set
            {
                _downvotes = Math.Max(0, value);
                VoteTick = Environment.TickCount;
            }
        }

        public int Votes
        {
            get { return Upvotes + Downvotes; }
        }

        public double Score
        {
            get
            {
                if (Votes == 0)
                    return 0.5;

                double z = 1.0;// #1.0 = 85%, 1.6 = 95%
                double phat = (double)Upvotes / (double)Votes;
                return Math.Sqrt(phat + z * z / (2 * Votes) - z * ((phat * (1 - phat) + z * z / (4 * Votes)) / Votes)) / (1 + z * z / Votes);
            }
        }

        public long VoteTick
        {
            get; private set;
        }

        #endregion Properties


    }
}