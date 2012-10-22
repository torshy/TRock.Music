namespace TRock.Music.Client
{
    public partial class Shell
    {
        public Shell()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            _playlistFlyout.IsOpen = !_playlistFlyout.IsOpen;
        }
    }
}
