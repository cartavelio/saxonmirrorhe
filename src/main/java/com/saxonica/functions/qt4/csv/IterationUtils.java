package com.saxonica.functions.qt4.csv;

import net.sf.saxon.trans.XPathException;

class IterationUtils {
    @FunctionalInterface
    public interface XPEConsumer<T> {
        void accept(T value) throws XPathException;
    }

    static public <T> void forEach(Iterable<T> values, XPEConsumer<T> action) throws XPathException {
        for (T value : values) {
            action.accept(value);
        }
    }

}
