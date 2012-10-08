namespace TRock.Music
{
    public class Song
    {
        #region Properties

        public string Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string Provider
        {
            get; set;
        }

        public Artist Artist
        {
            get; set;
        }

        public Album Album
        {
            get; set;
        }

        public int TotalSeconds
        {
            get; set;
        }

        #endregion Properties
    }
}