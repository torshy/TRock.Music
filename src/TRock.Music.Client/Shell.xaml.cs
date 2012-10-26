using System;

namespace TRock.Music.Client
{
    public partial class Shell
    {
        public Shell(IVoteableQueue<ISongStream> queue)
        {
            InitializeComponent();

            queue.ItemAdded += (sender, args) =>
            {
                _playlist.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _playlist.ItemsSource = queue.CurrentQueue;
                }));
            };

            queue.ItemRemoved += (sender, args) =>
            {
                _playlist.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _playlist.ItemsSource = queue.CurrentQueue;
                }));
            };
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            _playlistFlyout.IsOpen = !_playlistFlyout.IsOpen;
        }
    }
}
