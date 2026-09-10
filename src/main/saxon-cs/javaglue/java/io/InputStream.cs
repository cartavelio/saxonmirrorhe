namespace java.io {
    public interface InputStream : System.IDisposable {

        bool markSupported();

        void mark(int pos);

        int read(byte[] buffer, int start, int max);

        void reset();
        void close();

    }
}