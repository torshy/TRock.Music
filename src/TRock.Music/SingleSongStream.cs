using System.Collections.Generic;
using System.Threading;

namespace TRock.Music
{
    public class SingleSongStream : ISongStream
    {
        #region Fields

        private readonly Song _song;
        private int _counter;

        #endregion Fields

        #region Constructors

        public SingleSongStream(Song song)
        {
            _song = song;
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
                    return new[] { _song };
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