using System.IO;

namespace SaxonCS.Saxon.Helpers
{
    public class StreamLineReader
    {
        private Stream istream;
    
        public StreamLineReader(Stream stream)
        {
            istream = stream;
        }

        public string ReadLines()
        {
            using (StreamReader reader = new StreamReader(istream))
            {
                string result = reader.ReadToEnd();
                result = result.Replace("\r\n", "\n");
                return result;
            }
        }        
    }
}