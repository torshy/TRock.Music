#Example#

##Using Grooveshark
Quick example how to find a song and play it using Grooveshark

	var groove = new Lazy<IGroovesharkClient>(() => new GroovesharkClientWrapper());
	var songProvider = new GroovesharkSongProvider(groove);
	var songPlayer = new GroovesharkSongPlayer(groove);

	songProvider
		.GetSongs("Your Favorite Artist", CancellationToken.None)
		.ContinueWith(queryTask => songPlayer.Start(queryTask.Result.First()));
		
##Using Spotify
Spotify is a bit more complex. The song player is in a separate process to make it possible to run the main app as x64 and to isolate possible crashes. 

	var spotifyPlayerHost = new TorshifyServerProcessHandler();
	spotifyPlayerHost.TorshifyServerLocation = "TRock.Music.Torshify.Server.exe";
	spotifyPlayerHost.UserName = "YourSpotifyUserName";
	spotifyPlayerHost.Password = "YourSpotifyPassword";
	spotifyPlayerHost.Start();

	var songProvider = new SpotifySongProvider(new TorshifyImageProvider());
	var songPlayer = new TorshifySongPlayerClient(new Uri("http://localhost:8081"));
	songPlayer.Connect().ContinueWith(connectTask =>
	{
		songProvider
			.GetSongs("Your Favorite Artist", CancellationToken.None)
			.ContinueWith(queryTask =>
			{
				songPlayer.Start(queryTask.Result.First());
			});
	});

##Using both at the same time
If you want to use both Grooveshark and Spotify at the same time, you can place them in the AggregateSongProvider and AggregateSongPlayer

	var aggregateSongProvider = new AggregateSongProvider(groovesharkSongProvider, spotifySongProvider);
	var aggregateSongPlayer = new AggregateSongPlayer(groovesharkSongPlayer, spotifySongPlayer);

	aggregateSongProvider
		.GetSongs("Your Favorite Artist", CancellationToken.None)
		.ContinueWith(queryTask =>
		{
			aggregateSongPlayer.Start(queryTask.Result.First());
		});
		
##Caching search results
By wrapping a song provider in the CachedSongProvider, recent search results will be cached to provide quick response times when navigating backwards in the UI

	var cachedSongProvider = new CachedSongProvider(aggregateSongProvider);
	cachedSongProvider.SlidingExpiration = TimeSpan.FromMinutes(5);

	cachedSongProvider
		.GetSongs("Your Favorite Artist", CancellationToken.None)
		.ContinueWith(queryTask =>
		{
			aggregateSongPlayer.Start(queryTask.Result.First());
		});