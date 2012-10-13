using System.IO;
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
                using (var link = session.FromLink<IAlbum>((string)o.id))
                {
                    using (var album = link.Object)
                    {
                        album.WaitUntilLoaded(2000);

                        using (var image = session.GetImage(album.CoverId))
                        {
                            image.WaitUntilLoaded(2000);
                            var memoryStream = new MemoryStream(image.Data);
                            return new StreamResponse(() => memoryStream, "image/jpeg");
                        }
                    }
                }
            };
        }
    }
}