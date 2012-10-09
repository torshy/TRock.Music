using System.Diagnostics;
using SignalR;
using SignalR.Hubs;
namespace TRock.Music.Torshify.Server.Hubs
{
    public class TorshifyHub : Hub
    {
        #region Fields

        private readonly ISongPlayer _player;

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
            Trace.WriteLine("GetCurrentSong()");
            return _player.CurrentSong;
        }

        public void SetMuted(bool isMuted)
        {
            Trace.WriteLine("SetMuted(" + isMuted + ")");
            _player.IsMuted = isMuted;
        }

        public bool GetMuted()
        {
            Trace.WriteLine("GetMuted()");
            return _player.IsMuted;
        }

        public void SetVolume(float volume)
        {
            Trace.WriteLine("SetVolume(" + volume + ")");
            _player.Volume = volume;
        }

        public float GetVolume()
        {
            Trace.WriteLine("GetVolume()");
            return _player.Volume;
        }

        public bool GetIsPlaying()
        {
            Trace.WriteLine("GetIsPlaying()");
            return _player.IsPlaying;
        }

        public void SetIsPlaying(bool isPlaying)
        {
            Trace.WriteLine("SetIsPlaying(" + isPlaying + ")");
            _player.IsPlaying = isPlaying;
        }

        public void Start(Song song)
        {
            Trace.WriteLine("Start(" + song.Name + ", " + song.Id + ")");
            _player.Start(song);
        }

        public void Play()
        {
            Trace.WriteLine("Play()");
            _player.Play();
        }

        public void Pause()
        {
            Trace.WriteLine("Pause()");
            _player.Pause();
        }

        public void Stop()
        {
            Trace.WriteLine("Stop()");
            _player.Stop();
        }

        #endregion Methods
    }
}