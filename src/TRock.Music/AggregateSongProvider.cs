using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        #region Events

        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        #endregion Events

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
                    try
                    {
                        var result = p.GetSongs(query, cancellationToken).Result;

                        lock (lockObject)
                        {
                            songs.AddRange(result);
                        }
                    }
                    catch(Exception e)
                    {
                        var args = new UnhandledExceptionEventArgs(e);
                        OnUnhandledException(args);

                        if (!args.Handled)
                        {
                            throw;
                        }
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
                    albums.AddRange(task.Result);
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
                    try
                    {
                        var album = p.GetAlbum(albumId, cancellationToken).Result;

                        if (album != null)
                        {
                            result = album;
                        }
                    }
                    catch (Exception e)
                    {
                        var args = new UnhandledExceptionEventArgs(e);
                        OnUnhandledException(args);

                        if (!args.Handled)
                        {
                            throw;
                        }
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
                    try
                    {
                        var artist = p.GetArtist(artistId, cancellationToken).Result;

                        if (artist != null)
                        {
                            result = artist;
                        }
                    }
                    catch (Exception e)
                    {
                        var args = new UnhandledExceptionEventArgs(e);
                        OnUnhandledException(args);

                        if (!args.Handled)
                        {
                            throw;
                        }
                    }
                });

                return result;
            }, cancellationToken);
        }

        protected void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            EventHandler<UnhandledExceptionEventArgs> handler = UnhandledException;
            if (handler != null) handler(this, e);
        }

        #endregion Methods
    }

    public class UnhandledExceptionEventArgs : HandledEventArgs
    {
        #region Constructors

        public UnhandledExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        #endregion Constructors

        #region Properties

        public Exception Exception
        {
            get;
            private set;
        }

        #endregion Properties
    }
}