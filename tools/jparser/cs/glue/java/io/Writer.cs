namespace java.io {
    public interface Writer {
        void write(string str);
        void write(char ch);
        void write(char[] chars);
        void write(char[] chars, int start, int length);
        void flush();
        void close();
    }
}