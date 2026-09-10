package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.List;

/**
 * This class implements the function <code>fn:csv-fetch-field-by-column()</code>, which is a proposed function for
 * XPath 4.0.
 *
 * <p>Calling this function with a <code>csv-columns-record</code>, sequence of strings (fields), and key performs a
 * lookup of the value of the key in the fields sequence. Effectively the lookup function returned as part of
 * <code>csv-row-record</code> is a curried version of this function where the first two arguments have already been
 * bound.
 * </p>
 */
public class CsvFetchFieldByColumn extends SystemFunction {
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        MapItem columns = (MapItem) arguments[0].head();
        Sequence fields = arguments[1];
        AtomicValue key = arguments[2].head().atomize().head();

        UnboundFieldGetter fieldGetter = new UnboundFieldGetter(columns);
        List<GroundedValue> fieldsList = new ArrayList<>();
        fields.materialize().asIterable().forEach(fieldsList::add);

        return fieldGetter.get(key, fieldsList);
    }
}
