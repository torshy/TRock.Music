using System;

namespace TRock.Music
{
    public interface ISongPlayer
    {
        #region Events

        event EventHandler<ValueChangedEventArgs<bool>> IsMutedChanged;

        event EventHandler<ValueChangedEventArgs<bool>> IsPlayingChanged;

        event EventHandler<ValueChangedEventArgs<float>> VolumeChanged;

        event EventHandler<ValueChangedEventArgs<Song>> CurrentSongChanged;

        event EventHandler<SongEventArgs> CurrentSongCompleted;

        event EventHandler<ValueProgressEventArgs<int>> Buffering;

        event EventHandler<ValueProgressEventArgs<int>> Progress;

        #endregion Events

        #region Properties

        bool IsMuted
        {
            get; set;
        }

        bool IsPlaying
        {
            get; set;
        }

        float Volume
        {
            get; set;
        }

        Song CurrentSong
        {
            get;
        }

        #endregion Properties

        #region Methods

        bool CanPlay(Song song);

        void Start(Song song);

        void Play();

        void Stop();

        void Pause();

        #endregion Methods
    }
}