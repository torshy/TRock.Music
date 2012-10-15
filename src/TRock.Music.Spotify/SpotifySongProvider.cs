using System;
using System.Collections.Generic;
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
        private readonly HttpClient _client;

        public const string ProviderName = "Spotify";

        #endregion Fields

        #region Constructors

        public SpotifySongProvider(ISpotifyImageProvider imageProvider)
        {
            _imageProvider = imageProvider;
            _client = new HttpClient();
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

        public Task<IEnumerable<Song>> GetSongs(string query, CancellationToken cancellationToken)
        {
            return _client
                .GetAsync(new Uri("http://ws.spotify.com/search/1/track.json?q=" + query), cancellationToken)
                .ContinueWith(requestTask =>
                {
                    var response = requestTask.Result;
                    response.EnsureSuccessStatusCode();

                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<dynamic>(content);
                    var tracks = (IEnumerable<dynamic>)result["tracks"];

                    var songs = new List<Song>();
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
                                Provider = ProviderName,
                                Name = song["album"]["name"].Value,
                                CoverArt =
                                    _imageProvider.GetCoverArtUri(song["album"]["href"].Value) ?? string.Empty
                            },
                            Artist = new Artist
                            {
                                Id = song["artists"][0]["href"].Value,
                                Name = song["artists"][0]["name"].Value
                            }
                        });
                    }

                    return (IEnumerable<Song>)songs;
                });
        }

        public Task<IEnumerable<Album>> GetAlbums(string artistId, CancellationToken cancellationToken)
        {
            return _client
                .GetAsync(new Uri("http://ws.spotify.com/lookup/1/.json?uri=" + artistId + "&extras=album"), cancellationToken)
                .ContinueWith(requestTask =>
                {
                    var response = requestTask.Result;
                    response.EnsureSuccessStatusCode();

                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<dynamic>(content);

                    var albums = new List<Album>();
                    foreach (var item in result["artist"]["albums"])
                    {
                        albums.Add(new Album
                        {
                            Id = item["album"]["href"].Value,
                            Provider = ProviderName,
                            Name = item["album"]["name"].Value,
                            CoverArt = _imageProvider.GetCoverArtUri(item["album"]["href"].Value) ?? string.Empty
                        });
                    }

                    return (IEnumerable<Album>)albums;
                });
        }

        public Task<ArtistAlbum> GetAlbum(string albumId, CancellationToken cancellationToken)
        {
            return _client
                .GetAsync(new Uri("http://ws.spotify.com/lookup/1/.json?uri=" + albumId + "&extras=trackdetail"), cancellationToken)
                .ContinueWith(requestTask =>
                {
                    var response = requestTask.Result;
                    response.EnsureSuccessStatusCode();

                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<dynamic>(content);

                    dynamic artistIdValue = result["album"]["artist-id"];
                    string artistId = artistIdValue != null ? artistIdValue.Value : null;
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
                                Provider = ProviderName,
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

                    return new ArtistAlbum
                    {
                        Album = new Album
                        {
                            Id = result["album"]["href"].Value,
                            Provider = ProviderName,
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
                });
        }

        public Task<Artist> GetArtist(string artistId, CancellationToken cancellationToken)
        {
            return _client
                .GetAsync(new Uri("http://ws.spotify.com/lookup/1/.json?uri=" + artistId), cancellationToken)
                .ContinueWith(requestTask =>
                {
                    var response = requestTask.Result;
                    response.EnsureSuccessStatusCode();
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<dynamic>(content);
                    
                    return new Artist
                    {
                        Id = artistId, 
                        Name = result["artist"]["name"].Value
                    };
                });
        }

        #endregion Methods
    }
}