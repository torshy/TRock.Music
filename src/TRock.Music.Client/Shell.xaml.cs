using System;

namespace TRock.Music.Client
{
    public partial class Shell
    {
        public Shell(ISongStreamPlayer player)
        {
            InitializeComponent();

            player.CurrentSongsChanged += (sender, args) =>
            {
                _playlist.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _playlist.ItemsSource = args.Songs;
                }));
            };
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            _playlistFlyout.IsOpen = !_playlistFlyout.IsOpen;
        }
    }
}
