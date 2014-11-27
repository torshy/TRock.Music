using System.IO;
using System.Reflection;
using SciLorsGroovesharkAPI.Groove;

namespace TRock.Music.Grooveshark
{
    internal class GrooveFixExtractor
    {
        internal static void ExtractResourceToFile(string resourceName, string filename)
        {
            var baseDirectory = Path.GetDirectoryName(typeof(GrooveFix).Assembly.GetName().CodeBase);

            if (baseDirectory.StartsWith(@"file:\"))
            {
                baseDirectory = baseDirectory.Substring(6);
            }

            var outputFile = Path.Combine(baseDirectory, filename);
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (FileStream fs = new FileStream(outputFile, FileMode.Create))
                {
                    byte[] b = new byte[s.Length];
                    s.Read(b, 0, b.Length);
                    fs.Write(b, 0, b.Length);
                }
            }
        }
    }
}