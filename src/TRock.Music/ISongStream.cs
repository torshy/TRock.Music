using System.Collections.Generic;
using System.Threading;

namespace TRock.Music
{
    public interface ISongStream
    {
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

        bool MoveNext(CancellationToken token);
    }
}