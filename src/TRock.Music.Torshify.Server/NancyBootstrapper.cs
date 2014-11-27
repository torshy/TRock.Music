using Nancy;
using Nancy.TinyIoc;

using Torshify;

namespace TRock.Music.Torshify.Server
{
    public class NancyBootstrapper : DefaultNancyBootstrapper
    {
        #region Fields

        private readonly ISession _session;

        #endregion Fields

        #region Constructors

        public NancyBootstrapper(ISession session)
        {
            _session = session;
        }

        #endregion Constructors

        #region Methods

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            
            container.Register(_session);
        }

        #endregion Methods
    }
}