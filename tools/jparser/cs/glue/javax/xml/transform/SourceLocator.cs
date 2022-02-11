namespace javax.xml.transform
{
    public interface SourceLocator {
        int getLineNumber();
        int getColumnNumber();
        string getSystemId();
    }
}