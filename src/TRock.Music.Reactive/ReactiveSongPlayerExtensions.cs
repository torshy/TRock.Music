using System;
using System.Reactive;
using System.Reactive.Linq;

namespace TRock.Music.Reactive
{
    public static class ReactiveSongPlayerExtensions
    {
        public static IObservable<EventPattern<ValueChangedEventArgs<bool>>> ToIsMutedChangedObservable(this ISongPlayer player)
        {
            return Observable.FromEventPattern<ValueChangedEventArgs<bool>>(
                handler => player.IsMutedChanged += handler,
                handler => player.IsMutedChanged -= handler);
        }

        public static IObservable<EventPattern<ValueChangedEventArgs<bool>>> ToIsPlayingChangedObservable(this ISongPlayer player)
        {
            return Observable.FromEventPattern<ValueChangedEventArgs<bool>>(
                handler => player.IsPlayingChanged += handler,
                handler => player.IsPlayingChanged -= handler);
        }

        public static IObservable<EventPattern<ValueChangedEventArgs<float>>> ToVolumeChangedObservable(this ISongPlayer player)
        {
            return Observable.FromEventPattern<ValueChangedEventArgs<float>>(
                handler => player.VolumeChanged += handler,
                handler => player.VolumeChanged -= handler);
        }

        public static IObservable<EventPattern<ValueChangedEventArgs<Song>>> ToCurrentSongChangedObservable(this ISongPlayer player)
        {
            return Observable.FromEventPattern<ValueChangedEventArgs<Song>>(
                handler => player.CurrentSongChanged += handler,
                handler => player.CurrentSongChanged -= handler);
        }

        public static IObservable<EventPattern<SongEventArgs>> ToCurrentSongCompletedObservable(this ISongPlayer player)
        {
            return Observable.FromEventPattern<SongEventArgs>(
                handler => player.CurrentSongCompleted += handler,
                handler => player.CurrentSongCompleted -= handler);
        }

        public static IObservable<EventPattern<ValueProgressEventArgs<int>>> ToBufferingObservable(this ISongPlayer player)
        {
            return Observable.FromEventPattern<ValueProgressEventArgs<int>>(
                handler => player.Buffering += handler,
                handler => player.Buffering -= handler);
        }

        public static IObservable<EventPattern<ValueProgressEventArgs<int>>> ToProgressObservable(this ISongPlayer player)
        {
            return Observable.FromEventPattern<ValueProgressEventArgs<int>>(
                handler => player.Progress += handler,
                handler => player.Progress -= handler);
        }
    }
}