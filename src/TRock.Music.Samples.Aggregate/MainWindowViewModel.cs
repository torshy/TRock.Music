using System.ComponentModel;

namespace TRock.Music.Samples.Aggregate
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly ISongPlayer _songPlayer;
        private readonly ISongProvider _songProvider;

        #endregion Fields

        #region Constructors

        public MainWindowViewModel(ISongProvider songProvider, ISongPlayer songPlayer)
        {
            _songProvider = songProvider;
            _songPlayer = songPlayer;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}