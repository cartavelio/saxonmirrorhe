namespace Saxon.Ejava.io {
    public interface FilenameFilter {
        bool accept(File dir, string name);
    }
}