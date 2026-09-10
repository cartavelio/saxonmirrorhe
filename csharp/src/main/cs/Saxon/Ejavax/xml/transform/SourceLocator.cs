namespace Saxon.Ejavax.xml.transform
{
    public interface SourceLocator {
        int getLineNumber();
        int getColumnNumber();
        string getSystemId();
        string getPublicId();
    }
}