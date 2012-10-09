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

            Console.Write("Query >> ");
            while ((search = Console.ReadLine()) != null)
            {
                songProvider.GetSongs(search, CancellationToken.None)
                    .ContinueWith(t =>
                    {
                        var song = t.Result.FirstOrDefault();

                        if (song != null)
                        {
                            Console.WriteLine("Starting to play " + song.Name + " by " + song.Artist.Name);
                            client.Start(song);
                        }
                    });

                Console.Write("Query >> ");
            }

            Console.ReadLine();
        }

        #endregion Methods
    }
}