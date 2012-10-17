using Torshify;

namespace TRock.Music.Torshify.Server
{
    public interface ILinkFactory
    {
        ILink<ITrackAndOffset> GetLink(string trackId);
    }
}