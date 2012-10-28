using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Nancy;
using Nancy.Responses;
using Torshify;

namespace TRock.Music.Torshify.Server
{
    public class ImageModule : NancyModule
    {
        public ImageModule(ISession session)
        {
            Get["/album/cover/{id}"] = o =>
            {
                try
                {
                    using (var link = session.FromLink<IAlbum>((string)o.id))
                    {
                        using (var album = link.Object)
                        {
                            album.WaitUntilLoaded(2000);

                            if (album.IsLoaded)
                            {
                                using (var image = session.GetImage(album.CoverId))
                                {
                                    image.WaitUntilLoaded(2000);
                                    var memoryStream = new MemoryStream(image.Data);
                                    return new StreamResponse(() => memoryStream, "image/jpeg");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TRock.Music.Torshify.Server.cover.jpg"))
                {
                    var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    return new StreamResponse(() => memoryStream, "image/jpeg");
                }
            };
        }
    }
}