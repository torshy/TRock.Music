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
            var client = new TorshifySongPlayerClient(new Uri("http://localhost:8081"));

            Console.WriteLine("Connecting");
            
            if (!client.Connect().Wait(TimeSpan.FromSeconds(5)))
            {
                Console.WriteLine("Unable to connect...");
                return;
            }
            
            Console.WriteLine("Connected ;)");

            var songProvider = new SpotifySongProvider(new DefaultSpotifyImageProvider());

            string search;

            var queue = new VoteableQueue<Song>();
            queue.ItemAdded += (sender, eventArgs) =>
            {
                Console.WriteLine("Added song " + eventArgs.Item.Item.Name + " to queue");

                if (queue.CurrentQueue.Count() == 1)
                {
                    VoteableQueueItem<Song> head;

                    if (queue.TryPeek(out head))
                    {
                        client.Start(head.Item);
                    }
                }
            };

            client.CurrentSongChanged += (sender, eventArgs) =>
            {
                Console.WriteLine("Current song is " + eventArgs.NewValue.Name);
            };

            client.CurrentSongCompleted += (sender, eventArgs) =>
            {
                VoteableQueueItem<Song> head;

                if (queue.TryDequeue(out head))
                {
                    if (queue.TryPeek(out head))
                    {
                        client.Start(head.Item);
                    }
                }
            };

            Console.Write("Query >> ");
            while ((search = Console.ReadLine()) != null)
            {
                songProvider
                    .GetSongs(search, CancellationToken.None)
                    .ContinueWith(t =>
                    {
                        var song = t.Result.FirstOrDefault();

                        if (song != null)
                        {
                            queue.Enqueue(song);
                        }
                        else
                        {
                            Console.WriteLine("Unable to find " + search);
                        }
                    })
                    .Wait();

                Console.Write("Query >> ");
            }

            Console.ReadLine();
        }

        #endregion Methods
    }
}