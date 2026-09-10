using System.Text;
using J2N.Text;

namespace Saxon.Ejava.text
{
    public class CollationKeyUsingCSharp : Saxon.Ejava.text.CollationKey
    {
        private readonly string key;

        internal CollationKeyUsingCSharp(string source) : base(source)
        {
            this.key = source;
        }

        public string getKey()
        {
            return key;
        }

        public override /* REPLACING public */ int compareTo(Saxon.Ejava.text.CollationKey target)
        {
            return key.CompareTo(((CollationKeyUsingCSharp)(target)).getKey());
        }

        public override /* REPLACING public */ byte[] toByteArray()
        {
            return key.GetBytes(Encoding.Unicode);
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        public override bool Equals(System.Object obj)
        {
            return ((obj is CollationKeyUsingCSharp) &&
                    key.Equals(((CollationKeyUsingCSharp)(obj)).key));
        }

    }
}
