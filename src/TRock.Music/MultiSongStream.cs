using System.Collections.Generic;
using System.Threading;

namespace TRock.Music
{
    public class MultiSongStream : ISongStream
    {
        #region Fields

        private readonly IEnumerable<Song> _songs;
        private int _counter;

        #endregion Fields

        #region Constructors

        public MultiSongStream(IEnumerable<Song> songs)
        {
            _songs = songs;
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public IEnumerable<Song> Current
        {
            get
            {
                if (_counter == 1)
                {
                    return _songs;
                }

                return new Song[0];
            }
        }

        #endregion Properties

        #region Methods

        public bool MoveNext(CancellationToken token)
        {
            if (Interlocked.Increment(ref _counter) == 1)
            {
                return true;
            }

            return false;
        }

        #endregion Methods
    }
}