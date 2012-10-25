using System;
using System.Reactive;
using System.Reactive.Linq;

namespace TRock.Music.Reactive
{
    public static class ReactiveSongStreamPlayerExtensions
    {
        public static IObservable<EventPattern<SongStreamEventArgs>> ToCurrentStreamChangedObservable(this ISongStreamPlayer player)
        {
            return Observable.FromEventPattern<SongStreamEventArgs>(
                handler => player.CurrentStreamChanged += handler,
                handler => player.CurrentStreamChanged -= handler);
        }
    }
}