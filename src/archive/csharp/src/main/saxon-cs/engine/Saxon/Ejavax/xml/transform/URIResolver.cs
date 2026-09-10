namespace Saxon.Ejavax.xml.transform {
    public interface URIResolver {
        Source resolve(string relativeUri, string baseUri);
    }
}