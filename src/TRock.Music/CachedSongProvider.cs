using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace TRock.Music
{
    public class CachedSongProvider : ISongProvider
    {
        #region Fields

        private readonly ISongProvider _provider;

        #endregion Fields

        #region Constructors

        public CachedSongProvider(ISongProvider provider)
        {
            _provider = provider;

            Name = "CachedSongProvider";
            SlidingExpiration = TimeSpan.FromMinutes(2);
        }

        #endregion Constructors

        #region Properties

        public TimeSpan SlidingExpiration
        {
            get; 
            set;
        }

        public string Name
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public Task<IEnumerable<Song>> GetSongs(string query, CancellationToken cancellationToken)
        {
            var result = MemoryCache.Default.Get(query) as IEnumerable<Song>;

            if (result != null)
            {
                return Task.Factory.StartNew(() => result);
            }

            var tcs = new TaskCompletionSource<IEnumerable<Song>>();

            _provider
                .GetSongs(query, cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetException(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        MemoryCache.Default.Set(query, t.Result, new CacheItemPolicy { SlidingExpiration = SlidingExpiration });
                        tcs.SetResult(t.Result);
                    }
                });

            return tcs.Task;
        }

        public Task<IEnumerable<Album>> GetAlbums(string artistId, CancellationToken cancellationToken)
        {
            var result = MemoryCache.Default.Get(artistId) as IEnumerable<Album>;

            if (result != null)
            {
                return Task.Factory.StartNew(() => result);
            }

            var tcs = new TaskCompletionSource<IEnumerable<Album>>();

            _provider
                .GetAlbums(artistId, cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetException(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        MemoryCache.Default.Set(artistId, t.Result, new CacheItemPolicy { SlidingExpiration = SlidingExpiration });
                        tcs.SetResult(t.Result);
                    }
                });

            return tcs.Task;
        }

        public Task<ArtistAlbum> GetAlbum(string albumId, CancellationToken cancellationToken)
        {
            var result = MemoryCache.Default.Get(albumId) as ArtistAlbum;

            if (result != null)
            {
                return Task.Factory.StartNew(() => result);
            }

            var tcs = new TaskCompletionSource<ArtistAlbum>();

            _provider
                .GetAlbum(albumId, cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetException(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        MemoryCache.Default.Set(albumId, t.Result, new CacheItemPolicy { SlidingExpiration = SlidingExpiration });
                        tcs.SetResult(t.Result);
                    }
                });

            return tcs.Task;
        }

        #endregion Methods
    }
}