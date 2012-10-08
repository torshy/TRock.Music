namespace TRock.Music.Spotify
{
    public interface ISpotifyImageProvider
    {
        string GetCoverArtUri(string albumId);
    }
}