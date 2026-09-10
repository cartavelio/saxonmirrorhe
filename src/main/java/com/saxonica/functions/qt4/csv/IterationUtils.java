package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.ItemConsumer;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

import java.util.stream.Collectors;
import java.util.stream.StreamSupport;

class IterationUtils {
    @FunctionalInterface
    public interface XPEConsumer<T> {
        void accept(T value) throws XPathException;
    }

    @FunctionalInterface
    public interface XPEFunction<T, R> {
        R apply(T value) throws XPathException;
    }

    static public <T> void forEach(Iterable<T> values, XPEConsumer<T> action) throws XPathException {
        for (T value : values) {
            action.accept(value);
        }
    }

    static public <T> void forEach(SequenceIterator values, XPEConsumer<T> action) throws XPathException {
        T current = (T) values.next();
        while (current != null) {
            action.accept(current);
            current = (T) values.next();
        }
    }


    static public <T, R> Iterable<R> map(Iterable<T> values, XPEFunction<T, R> mapper) throws XPathException {
        try {
            return StreamSupport.stream(values.spliterator(), false).map(item -> {
                try {
                    return mapper.apply(item);
                } catch (XPathException ex) {
                    throw new UncheckedXPathException(ex);
                }
            }).collect(Collectors.toList());
        } catch (UncheckedXPathException ex) {
            throw ex.getXPathException();
        }
    }

    static public SequenceIterator map(SequenceIterator values, ItemMapper.Lambda mapper) {
        return ItemMappingIterator.map(values, mapper);
    }
}
