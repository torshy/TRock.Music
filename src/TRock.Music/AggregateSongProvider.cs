using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TRock.Music
{
    public class AggregateSongProvider : ISongProvider
    {
        #region Constructors

        public AggregateSongProvider()
        {
            Providers = new List<ISongProvider>();
        }

        public AggregateSongProvider(params ISongProvider[] providers)
        {
            Providers = new List<ISongProvider>(providers);
        }

        public AggregateSongProvider(IEnumerable<ISongProvider> providers)
        {
            Providers = new List<ISongProvider>(providers);
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get
            {
                return "Aggregate";
            }
        }

        public ICollection<ISongProvider> Providers
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public Task<IEnumerable<Song>> GetSongs(string query, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                var lockObject = new object();
                var songs = new List<Song>(200);

                Parallel.ForEach(Providers, p =>
                {
                    var result = p.GetSongs(query, cancellationToken).Result;

                    lock (lockObject)
                    {
                        songs.AddRange(result);
                    }
                });

                return (IEnumerable<Song>)songs.ToArray();
            });
        }

        public Task<IEnumerable<Album>> GetAlbums(string artistId, CancellationToken cancellationToken)
        {
            return Task.Factory.ContinueWhenAll(Providers.Select(p => p.GetAlbums(artistId, cancellationToken)).ToArray(), tasks =>
            {
                var albums = new List<Album>();

                foreach (Task<IEnumerable<Album>> task in tasks)
                {
                    foreach (Album album in task.Result)
                    {
                        albums.Add(album);
                    }
                }

                return (IEnumerable<Album>)albums;
            });
        }

        public Task<ArtistAlbum> GetAlbum(string albumId, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                ArtistAlbum result = null;

                Parallel.ForEach(Providers, p =>
                {
                    var album = p.GetAlbum(albumId, cancellationToken).Result;

                    if (album != null)
                    {
                        result = album;
                    }
                });

                return result;
            }, cancellationToken);
        }

        public Task<Artist> GetArtist(string artistId, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                Artist result = null;

                Parallel.ForEach(Providers, p =>
                {
                    var artist = p.GetArtist(artistId, cancellationToken).Result;

                    if (artist != null)
                    {
                        result = artist;
                    }
                });

                return result;
            }, cancellationToken);
        }

        #endregion Methods
    }
}