namespace java.io {
    public interface Reader {

        int read(char[] buffer);
        
        int read(char[] buffer, int start, int len);

        void close();

    }
}