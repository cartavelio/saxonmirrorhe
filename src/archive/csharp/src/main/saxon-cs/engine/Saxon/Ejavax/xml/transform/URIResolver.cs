namespace Saxon.Ejavax.xml.transform {
    public interface URIResolver {
        Source resolve(string baseUri, string relativeUri);
    }
}