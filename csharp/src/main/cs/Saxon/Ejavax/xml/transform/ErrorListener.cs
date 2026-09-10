namespace Saxon.Ejavax.xml.transform {
    public interface ErrorListener {
        void warning(Saxon.Hej.trans.XPathException err);
        void error(Saxon.Hej.trans.XPathException err);
        void fatalError(Saxon.Hej.trans.XPathException err);

    }
}