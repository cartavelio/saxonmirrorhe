package com.saxonica.functions.qt4.csv;


import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.Map;

public class CsvToXdm extends CsvFunction {
    public static OptionsParameter OPTION_DETAILS;
    static {
        OPTION_DETAILS = coreOptions();
        OPTION_DETAILS.addAllowedOption("column-names", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        OPTION_DETAILS.addAllowedOption("filter-columns", SequenceType.INTEGER_SEQUENCE, EmptyAtomicSequence.getInstance());
    }

    private Map<String, GroundedValue> options;

    /**
     * Execute a dynamic call to the function
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences.
     * @return the result of the evaluation, as a Sequence. It is the responsibility
     * of the callee to ensure that the type of result conforms to the expected result type.
     * @throws XPathException (should not happen)
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item arg0 = arguments[0].head();
        if (arg0 == null) {
            return CsvXdmGenerator.EMPTY_CSV;
        }
        StringValue input = (StringValue) arg0;
        MapItem unparsedOptions  = new DictionaryMap();
        if (arguments.length >= 2) {
            unparsedOptions = (MapItem) arguments[1].head();
        }

        options = getDetails().optionDetails.processSuppliedOptions(unparsedOptions, context);

        CsvXdmGenerator generator = new CsvXdmGenerator(options);
        return generator.fromRowSequence(CsvParser.parse(input, options));
    }

    //
//    @FunctionalInterface
//    public interface RowMapper<T extends Item, R extends Item> {
//        /**
//         * Map one item to another
//         * @param item the item to be processed
//         * @throws XPathException if the processing fails
//         */
//        Item apply(T item) throws XPathException;
//    }
//
//    @FunctionalInterface
//    public interface ItemSupplier<T extends Item> {
//        /**
//         * Supply Items from a sequence to a stream
//         * @return An item from the sequence
//         * @throws XPathException if the processing fails
//         */
//        T get() throws XPathException;
//    }
//
//    public class SequenceSupplier implements ItemSupplier<Item> {
//        private final SequenceIterator iter;
//
//        public SequenceSupplier(SequenceIterator iter) {
//            this.iter = iter;
//        }
//
//        @Override
//        public Item get() throws XPathException {
//            Item item = iter.next();
//            return item;
//        }
//    }
//
//    public static void supply(SequenceIterator iter, ItemConsumer<? super Item> consumer) {
//        try {
//            for (Item item; (item = iter.next()) != null; ) {
//                consumer.accept(item);
//            }
//        } catch (XPathException e) {
//            throw new UncheckedXPathException(e);
//        }
//    }

}