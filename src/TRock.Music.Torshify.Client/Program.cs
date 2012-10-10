using System;
using System.Linq;
using System.Threading;

using TRock.Music.Spotify;

namespace TRock.Music.Torshify.Client
{
    class Program
    {
        #region Methods

        static void Main(string[] args)
        {
            var songPlayer = new TorshifySongPlayerClient(new Uri("http://localhost:8081"));

            Console.WriteLine("Connecting");

            songPlayer.Connect().ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(t.Exception.GetBaseException().Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.WriteLine("Connected ;)");

                    var songProvider = new SpotifySongProvider(new DefaultSpotifyImageProvider());

                    //SongQueueSample(songPlayer, songProvider);
                    SongStreamSample(songPlayer, songProvider);        
                }
            });

            Console.ReadLine();
        }

        static void SongQueueSample(ISongPlayer player, ISongProvider songProvider)
        {
            var queue = new VoteableQueue<Song>();
            queue.ItemAdded += (sender, eventArgs) =>
            {
                Console.WriteLine("Added song " + eventArgs.Item.Item.Name + " to queue");

                if (queue.CurrentQueue.Count() == 1)
                {
                    VoteableQueueItem<Song> head;

                    if (queue.TryPeek(out head))
                    {
                        player.Start(head.Item);
                    }
                }
            };
            player.CurrentSongChanged += (sender, eventArgs) => Console.WriteLine("Current song is " + eventArgs.NewValue.Name);
            player.CurrentSongCompleted += (sender, eventArgs) =>
            {
                VoteableQueueItem<Song> head;

                if (queue.TryDequeue(out head))
                {
                    if (queue.TryPeek(out head))
                    {
                        player.Start(head.Item);
                    }
                }
            };

            string query;

            do
            {
                Console.WriteLine("Enter query >> ");
                query = Console.ReadLine();

                if (!string.IsNullOrEmpty(query))
                {
                    songProvider
                        .GetSongs(query, CancellationToken.None)
                        .ContinueWith(resultTask =>
                        {
                            var song = resultTask.Result.FirstOrDefault();

                            if (song != null)
                            {
                                queue.Enqueue(song);
                            }
                        });
                }
            } while (string.IsNullOrEmpty(query));
        }

        static void SongStreamSample(ISongPlayer player, ISongProvider songProvider)
        {
            var queue = new VoteableQueue<ISongStream>();
            var streamPlayer = new AutoplaySongStreamPlayer(player);
            
            queue.ItemAdded += (sender, eventArgs) =>
            {
                Console.WriteLine("Added song " + eventArgs.Item.Item.Name + " to queue");

                if (queue.CurrentQueue.Count() == 1)
                {
                    VoteableQueueItem<ISongStream> head;

                    if (queue.TryPeek(out head))
                    {
                        streamPlayer.CurrentStream = head.Item;
                    }
                }
            };
            player.CurrentSongChanged += (sender, eventArgs) => Console.WriteLine("Current song is " + eventArgs.NewValue.Name);
            streamPlayer.StreamComplete += (sender, args) =>
            {
                VoteableQueueItem<ISongStream> head;

                if (queue.TryDequeue(out head))
                {
                    if (queue.TryPeek(out head))
                    {
                        streamPlayer.CurrentStream = head.Item;
                    }
                }
            };        

            string query;
            do
            {
                Console.WriteLine("Enter query >> ");
                query = Console.ReadLine();

                if (!string.IsNullOrEmpty(query))
                {
                    songProvider
                        .GetSongs(query, CancellationToken.None)
                        .ContinueWith(resultTask =>
                        {
                            var songs = resultTask.Result.Take(3).ToArray();

                            if (songs.Any())
                            {
                                Console.WriteLine("Enqueueing " + songs.Count() + " songs");
                                queue.Enqueue(new MultiSongStream(songs));
                            }
                        });
                }
            } while (string.IsNullOrEmpty(query));
        }

        #endregion Methods
    }
}