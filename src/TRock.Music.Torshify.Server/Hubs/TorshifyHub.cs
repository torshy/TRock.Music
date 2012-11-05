using SignalR;
using SignalR.Hubs;

using log4net;

namespace TRock.Music.Torshify.Server.Hubs
{
    public class TorshifyHub : Hub
    {
        #region Fields

        private readonly ISongPlayer _player;
        private readonly ILog _log = LogManager.GetLogger("TorshifyHub");

        #endregion Fields

        #region Constructors

        public TorshifyHub()
        {
            _player = GlobalHost.DependencyResolver.Resolve<ISongPlayer>();
        }

        #endregion Constructors

        #region Methods

        public Song GetCurrentSong()
        {
            _log.Debug("GetCurrentSong()");
            return _player.CurrentSong;
        }

        public void SetMuted(bool isMuted)
        {
            _log.Debug("SetMuted(" + isMuted + ")");
            _player.IsMuted = isMuted;
        }

        public bool GetMuted()
        {
            _log.Debug("GetMuted()");
            return _player.IsMuted;
        }

        public void SetVolume(float volume)
        {
            _log.Debug("SetVolume(" + volume + ")");
            _player.Volume = volume;
        }

        public float GetVolume()
        {
            _log.Debug("GetVolume()");
            return _player.Volume;
        }

        public bool GetIsPlaying()
        {
            _log.Debug("GetIsPlaying()");
            return _player.IsPlaying;
        }

        public void SetIsPlaying(bool isPlaying)
        {
            _log.Debug("SetIsPlaying(" + isPlaying + ")");
            _player.IsPlaying = isPlaying;
        }

        public void Start(Song song)
        {
            _log.Info("Start(" + song.Name + ", " + song.Id + ")");
            _player.Start(song);
        }

        public void Play()
        {
            _log.Info("Play()");
            _player.Play();
        }

        public void Pause()
        {
            _log.Info("Pause()");
            _player.Pause();
        }

        public void Stop()
        {
            _log.Info("Stop()");
            _player.Stop();
        }

        #endregion Methods
    }
}