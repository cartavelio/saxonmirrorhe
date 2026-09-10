package com.saxonica.functions.qt4.csv;

import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.IntegerValue;

import java.util.Map;
import java.util.function.Supplier;

/**
 * Handle validating option parameters that can't be handled using the <code>OptionsParameter</code> machinery.
 *
 * <p>Specifically, ensure that mutually exclusive options are not specified together, and that all integer value
 * options are positive (>0), which is required, but we can't use <code>xs:positiveInteger</code> to enforce it through
 * type checking.</p>
 */
class CsvOptionsParameterValidator {
    private final Map<String, GroundedValue> options;

    CsvOptionsParameterValidator(Map<String, GroundedValue> options) {
        this.options = options;
    }

    /**
     * Validate the supplied, processed, options map. Does not modify the map, simply returns nothing if they're okay,
     * or throws the appropriate XPathException if they are not.
     *
     * @param options the Map of options returned from processing with {@link OptionsParameter}
     * @throws XPathException thrown if the options are invalid
     */
    public static void validate(Map<String, GroundedValue> options) throws XPathException {
        (new CsvOptionsParameterValidator(options)).validate();
    }

    public boolean validate() throws XPathException {
        return ensureMutuallyExclusiveOptionsOk() && ensureFilterColumnsValid() && ensureNumberOfColumnsValid() && ensureColumnNamesValid();
    }

    private boolean nonEmpty(String key) {
        return (options.getOrDefault(key, EmptySequence.getInstance())).getLength() > 0;
    }

    private boolean ensureMutuallyExclusiveOptionsOk() throws XPathException {
        if (nonEmpty("filter-columns") && nonEmpty("number-of-columns")) {
            throw new XPathException("Cannot specify both number-of-columns and filter-columns as options for fn:csv-to-xdm", "FOCV0006");
        }
        return true;
    }

    private boolean ensureFilterColumnsValid() throws XPathException {
        if (!options.containsKey("filter-columns")) return true;
        GroundedValue filter = options.get("filter-columns").materialize();

        Supplier<XPathException> invalid = invalidPositiveIntegerOptionException(filter, "filter-columns");

        try {
            SequenceTool.supply(filter.iterate(), verifyPositiveInteger(invalid));
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }
        return true;
    }

    private boolean ensureNumberOfColumnsValid() throws XPathException {
        if (!options.containsKey("number-of-columns")) return true;

        AtomicValue value = options.get("number-of-columns").head().atomize().head();
        if (value.getPrimitiveType() == BuiltInAtomicType.INTEGER) {
            verifyPositiveInteger(invalidPositiveIntegerOptionException(value, "number-of-columns")).accept(value);
        }
        return true;
    }

    private boolean ensureColumnNamesValid() throws XPathException {
        if (!options.containsKey("column-names")) return true;
        Item value = options.get("column-names").head();
        if (value.getGenre() == Genre.MAP) {
            IterationUtils.forEach(((MapItem) value).keyValuePairs(), kvPair -> {
                verifyPositiveInteger(invalidPositiveIntegerOptionException(value, "column-names")).accept(kvPair.value.head());
            });
        }
        return true;
    }

    private ItemConsumer<Item> verifyPositiveInteger(Supplier<XPathException> exceptionSupplier) throws XPathException {
        return (maybeNumber) -> {
            if (maybeNumber == null) return;
            if (((AtomicValue) maybeNumber).getPrimitiveType() != BuiltInAtomicType.INTEGER)
                throw exceptionSupplier.get();
            if (maybeNumber instanceof IntegerValue && ((IntegerValue) maybeNumber).asSubscript() == -1)
                throw exceptionSupplier.get();
        };
    }

    private Supplier<XPathException> invalidPositiveIntegerOptionException(Sequence actual, String key) {
        return invalidOptionException(actual, key, "Must be a sequence of positive (>0) integers.", "FOCV0005");
    }

    private Supplier<XPathException> invalidOptionException(Sequence actual, String key, String message, String code) {
        return () -> {
            return new XPathException("Invalid option " + key + "=" + Err.depictSequence(actual) + ". " + message, code);
        };
    }
}
