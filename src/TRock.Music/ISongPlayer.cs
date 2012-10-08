using System;

namespace TRock.Music
{
    public interface ISongPlayer : ISongPlayerReactive
    {
        #region Properties

        bool IsMuted { get; set; }

        bool IsPlaying { get; set; }

        float Volume { get; set; }

        Song CurrentSong { get; }

        #endregion Properties

        #region Methods

        bool CanPlay(Song song);

        void Start(Song song);

        void Play();

        void Stop();

        void Pause();

        #endregion Methods
    }

    public interface ISongPlayerEvents
    {
        event EventHandler<ValueChangedEventArgs<bool>> IsMutedChanged;

        event EventHandler<ValueChangedEventArgs<bool>> IsPlayingChanged;

        event EventHandler<ValueChangedEventArgs<float>> VolumeChanged;

        event EventHandler<ValueChangedEventArgs<Song>> CurrentSongChanged;

        event EventHandler<SongEventArgs> CurrentSongCompleted;

        event EventHandler<ValueProgressEventArgs<int>> Buffering;

        event EventHandler<ValueProgressEventArgs<int>> Progress;
    }

    public interface ISongPlayerReactive
    {
        IObservable<ValueChange<bool>> IsMutedChanged { get; }

        IObservable<ValueChange<bool>> IsPlayingChanged { get; }

        IObservable<ValueChange<float>> VolumeChanged { get; }

        IObservable<ValueChange<Song>> CurrentSongChanged { get; }

        IObservable<Song> CurrentSongCompleted { get; }

        IObservable<ValueProgress<int>> Buffering { get; }

        IObservable<ValueProgress<int>> Progress { get; }
    }
}