using System.Windows;

namespace TRock.Music.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var bootstrapper = new AppBootstrapper();
            bootstrapper.Run();
        }
    }
}
