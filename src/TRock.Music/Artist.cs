using System;

namespace TRock.Music
{
    public class Artist : IEquatable<Artist>
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

        #endregion Properties

        #region Methods

        public static bool operator ==(Artist left, Artist right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Artist left, Artist right)
        {
            return !Equals(left, right);
        }

        public bool Equals(Artist other)
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
            return Equals((Artist) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        #endregion Methods
    }
}