using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TRock.Music.Spotify
{
    public class SpotifySongProvider : ISongProvider
    {
        #region Fields

        private readonly ISpotifyImageProvider _imageProvider;

        public const string ProviderName = "Spotify";

        #endregion Fields

        #region Constructors

        public SpotifySongProvider(ISpotifyImageProvider imageProvider)
        {
            _imageProvider = imageProvider;
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get
            {
                return ProviderName;
            }
        }

        #endregion Properties

        #region Methods

        public async Task<IEnumerable<Song>> GetSongs(string query, CancellationToken cancellationToken)
        {
            var songs = new List<Song>();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(new Uri("http://ws.spotify.com/search/1/track.json?q=" + query), cancellationToken);
                dynamic result = response.Content
                    .ReadAsStringAsync()
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Trace.WriteLine(task.Exception);
                            return null;
                        }

                        return JsonConvert.DeserializeObject<dynamic>(task.Result);
                    }).Result;

                if (result != null)
                {
                    var tracks = (IEnumerable<dynamic>)result["tracks"];
                    foreach (dynamic song in tracks)
                    {
                        songs.Add(new Song
                        {
                            Id = song["href"].Value,
                            Name = song["name"].Value,
                            Provider = ProviderName,
                            TotalSeconds = (int)song["length"].Value,
                            Album = new Album
                            {
                                Id = song["album"]["href"].Value,
                                Name = song["album"]["name"].Value,
                                CoverArt = _imageProvider.GetCoverArtUri(song["album"]["href"].Value) ?? string.Empty
                            },
                            Artist = new Artist
                            {
                                Id = song["artists"][0]["href"].Value,
                                Name = song["artists"][0]["name"].Value
                            }
                        });
                    }
                }
            }

            return songs;
        }

        public async Task<IEnumerable<Album>> GetAlbums(string artistId, CancellationToken cancellationToken)
        {
            var albums = new List<Album>();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(new Uri("http://ws.spotify.com/lookup/1/.json?uri=" + artistId + "&extras=album"), cancellationToken);
                var result = response.Content.ReadAsStringAsync()
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Trace.WriteLine(task.Exception);
                            return null;
                        }

                        return JsonConvert.DeserializeObject<dynamic>(task.Result);
                    }).Result;


                if (result != null)
                {
                    foreach (var item in result["artist"]["albums"])
                    {
                        albums.Add(new Album
                        {
                            Id = item["album"]["href"].Value,
                            Name = item["album"]["name"].Value,
                            CoverArt = _imageProvider.GetCoverArtUri(item["album"]["href"].Value) ?? string.Empty
                        });
                    }
                }
            }

            return albums;
        }

        public async Task<ArtistAlbum> GetAlbum(string albumId, CancellationToken cancellationToken)
        {
            ArtistAlbum artistAlbum = null;

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(new Uri("http://ws.spotify.com/lookup/1/.json?uri=" + albumId + "&extras=trackdetail"), cancellationToken);
                var result = response.Content.ReadAsStringAsync()
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Trace.WriteLine(task.Exception);
                            return null;
                        }

                        return JsonConvert.DeserializeObject<dynamic>(task.Result);
                    }).Result;


                if (result != null)
                {
                    string artistId = result["album"]["artist-id"].Value;
                    string artistName = result["album"]["artist"].Value;
                    string albumName = result["album"]["name"].Value;

                    var songs = new List<Song>();

                    foreach (var song in result["album"]["tracks"])
                    {
                        songs.Add(new Song
                        {
                            Id = song["href"].Value,
                            Name = song["name"].Value,
                            Provider = ProviderName,
                            TotalSeconds = (int)song["length"].Value,
                            Album = new Album
                            {
                                Id = result["album"]["href"].Value,
                                Name = albumName,
                                CoverArt = _imageProvider.GetCoverArtUri(result["album"]["href"].Value) ?? string.Empty
                            },
                            Artist = new Artist
                            {
                                Id = artistId,
                                Name = artistName
                            }
                        });
                    }

                    artistAlbum = new ArtistAlbum
                    {
                        Album = new Album
                        {
                            Id = result["album"]["href"].Value,
                            Name = albumName,
                            CoverArt = _imageProvider.GetCoverArtUri(result["album"]["href"].Value) ?? string.Empty
                        },
                        Artist = new Artist
                        {
                            Id = artistId,
                            Name = artistName
                        },
                        Songs = songs.ToArray()
                    };
                }
            }

            return artistAlbum;
        }

        #endregion Methods
    }
}