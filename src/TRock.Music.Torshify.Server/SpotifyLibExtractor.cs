using System.IO;
using System.Reflection;

namespace TRock.Music.Torshify.Server
{
    internal class SpotifyLibExtractor
    {
        internal static void ExtractResourceToFile(string resourceName, string filename)
        {
            var baseDirectory = Path.GetDirectoryName(typeof(SpotifyLibExtractor).Assembly.Location);
            var libspotifyLocation = Path.Combine(baseDirectory, filename);

            if (!File.Exists(libspotifyLocation))
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    using (var fs = new FileStream(libspotifyLocation, FileMode.Create))
                    {
                        byte[] b = new byte[stream.Length];
                        stream.Read(b, 0, b.Length);
                        fs.Write(b, 0, b.Length);
                    }
                }
            }
        }
    }
}