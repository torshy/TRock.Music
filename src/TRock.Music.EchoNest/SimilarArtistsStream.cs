using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace TRock.Music.EchoNest
{
    public class SimilarArtistsStream : ISongStream
    {
        #region Fields

        private readonly ISongProvider _songProvider;
        private readonly string _artistName;
        private readonly string _echoNestApiKey;
        private string _sessionId;

        #endregion Fields

        #region Constructors

        public SimilarArtistsStream(
            ISongProvider songProvider,
            string artistName,
            string echoNestApiKey)
        {
            Name = "Similar";
            Description = "Artists similar to " + artistName;

            _songProvider = songProvider;
            _artistName = artistName;
            _echoNestApiKey = echoNestApiKey;
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public IEnumerable<Song> Current
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public bool MoveNext(CancellationToken token)
        {
            try
            {
                dynamic result;

                if (string.IsNullOrEmpty(_sessionId))
                {
                    dynamic options = new DynamicDictionary();
                    options.artist = _artistName;
                    options.type = "artist-radio";

                    result = EchoNestHelpers.Playlist.Create(_echoNestApiKey, options);

                    if (result != null && result.response.status.code == 0)
                    {
                        _sessionId = result.response.session_id;
                    }
                    else
                    {
                        return false;
                    }
                }

                result = EchoNestHelpers.Playlist.Next(_echoNestApiKey, _sessionId);

                if (result == null || result.response.status.code != 0)
                {
                    return false;
                }

                if (result.response.songs.HasValues)
                {
                    dynamic song = result.response.songs[0];
                    string query = song.artist_name + " " + song.title;

                    var task = _songProvider
                        .GetSongs(query, token)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                Trace.WriteLine(t.Exception);
                                Current = new Song[0];
                            }
                            else
                            {
                                Current = new[] { t.Result.FirstOrDefault() };
                            }
                        });

                    task.Wait(token);
                }

                return Current.Any();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return false;
        }

        #endregion Methods
    }
}