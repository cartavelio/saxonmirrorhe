namespace javax.xml.transform {
    public interface URIResolver {
        Source resolve(string baseUri, string relativeUri);
    }
}