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

            var wait = new ManualResetEvent(false);

            songPlayer.Connect().ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(t.Exception.GetBaseException().Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Environment.Exit(-1);
                }

                wait.Set();
            });

            if (wait.WaitOne(5000))
            {
                Console.WriteLine("Connected ;)");
                var songProvider = new SpotifySongProvider(new DefaultSpotifyImageProvider());
                SongQueueSample(songPlayer, songProvider);
                Console.ReadLine();
            }
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

        #endregion Methods
    }
}