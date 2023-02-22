namespace javax.xml.transform {
    public interface ErrorListener {
        void warning(net.sf.saxon.trans.XPathException err);
        void error(net.sf.saxon.trans.XPathException err);
        void fatalError(net.sf.saxon.trans.XPathException err);

    }
}