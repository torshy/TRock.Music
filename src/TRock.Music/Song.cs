using System;

namespace TRock.Music
{
    public class Song : IEquatable<Song>
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

        #region Methods

        public static bool operator ==(Song left, Song right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Song left, Song right)
        {
            return !Equals(left, right);
        }

        public bool Equals(Song other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Song) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        #endregion Methods
    }
}