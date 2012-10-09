using System.Collections.Generic;
using System.Threading;

namespace TRock.Music
{
    public interface ISongStream
    {
        #region Properties

        string Name
        {
            get;
        }

        string Description
        {
            get;
        }

        IEnumerable<Song> Current
        {
            get;
        }

        #endregion Properties

        #region Methods

        bool MoveNext(CancellationToken token);

        #endregion Methods
    }
}