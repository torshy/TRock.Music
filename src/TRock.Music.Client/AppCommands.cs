using Microsoft.Practices.Prism.Commands;

namespace TRock.Music.Client
{
    public class AppCommands
    {
        public static CompositeCommand PlayCommand = new CompositeCommand();
        public static CompositeCommand PauseCommand = new CompositeCommand();
        public static CompositeCommand NextCommand = new CompositeCommand();
        public static CompositeCommand VolumeUpCommand = new CompositeCommand();
        public static CompositeCommand VolumeDownCommand = new CompositeCommand();
        public static CompositeCommand EnqueueCommand = new CompositeCommand();
        public static CompositeCommand NavigateBackCommand = new CompositeCommand();
    }
}