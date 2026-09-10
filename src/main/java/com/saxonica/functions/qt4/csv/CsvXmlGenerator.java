package com.saxonica.functions.qt4.csv;

import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;

import java.util.*;

/**
 * Handles generating a simple XML representation of a CSV from a sequence of array items
 * (<code>array(xs:string)*</code>).
 */
public class CsvXmlGenerator extends CsvRepresentationGenerator {
    private final XPathContext context;
    private final String staticBaseUri;
    private Generator generator;

    /**
     * Construct an instance using the passed in options Map.
     *
     * @param options options for the conversion as a map of String : Sequence pairs
     * @param context the current XPath context
     * @throws XPathException if there's a problem getting values from the options
     */
    public CsvXmlGenerator(Map<String, GroundedValue> options, XPathContext context, String staticBaseUri) throws XPathException {
        super(options);
        this.context = context;
        this.staticBaseUri = staticBaseUri;
    }

    /**
     * Generate an XML representation of a CSV from the provided sequence of rows (<code>array(xs:string)*</code>.
     *
     * @param rows the sequence of rows (ArrayItems)
     * @return the XML document
     * @throws XPathException
     */
    @Override
    public Item fromRowSequence(Sequence rows) throws XPathException {
        generator = new Generator(context, staticBaseUri);
        generator.startDocument();
        if (_shouldExtractColumns) {
            SequenceIterator iter = rows.iterate();
            List<GroundedValue> columnNames = generateColumns(iter);
            generateRows(iter, columnNames);
        } else {
            generateRows(rows.iterate());
        }
        generator.endDocument();
        return generator.getResult();
    }

    private List<GroundedValue> generateColumns(SequenceIterator rowsIter) throws XPathException {
        Item columnNamesOpt = getColumnNamesOpt();
        if (columnNamesOpt.getGenre() == Genre.MAP) {
            return generateColumns((MapItem) columnNamesOpt);
        } else {
            ArrayItem headerRow = (ArrayItem) rowsIter.next();
            return generateColumns(headerRow);
        }
    }

    private List<GroundedValue> generateColumns(MapItem columnNamesMap) throws XPathException {
        List<GroundedValue> fields = generateColumnsFields(columnNamesMap);
        if (numberOfColumnsMode() == NumberOfColumns.FIRST_ROW) numberOfColumns = fields.size();

        generator.columnsRow(rowAsSequence(fields));
        return fields;
    }

    private List<GroundedValue> generateColumns(ArrayItem rowArray) throws XPathException {
        if (rowArray == null) {
            generator.columnsRow(EmptySequence.getInstance());
            return new ArrayList<>();
        }

        List<GroundedValue> wantedFields = generateColumnsFields(rowArray);

        generator.columnsRow(rowAsSequence(wantedFields));
        return wantedFields;
    }

    private List<GroundedValue> generateColumnsFields(MapItem columnNamesMap) throws XPathException {
        List<GroundedValue> fields = new ArrayList<>();
        Map<Integer, GroundedValue> fieldsMap = new HashMap<>();
        int fieldsLength = 0;
        for (KeyValuePair kvPair : columnNamesMap.keyValuePairs()) {
            int idx = (int) ((IntegerValue) kvPair.value.head()).longValue();
            if (idx > fieldsLength) {
                fieldsLength = idx;
            }
            fieldsMap.put(idx, kvPair.key);
        }

        for (int i = 1; i <= fieldsLength; i++) {
            fields.add(fieldsMap.getOrDefault(i, StringValue.EMPTY_STRING));
        }
        return fields;
    }

    private List<GroundedValue> generateColumnsFields(ArrayItem rowArray) throws XPathException {
        List<GroundedValue> wantedFields = new ArrayList<>();
        for (int oneBasedIndex : getColumnIndexes(rowArray)) {
            wantedFields.add(rowArray.get(oneBasedIndex - 1));
        }
        return wantedFields;
    }

    private void generateRows(SequenceIterator rows) throws XPathException {
        generator.startRows();
        SequenceTool.supply(rows, row -> {
            generator.row(plainRow(filteredRow((ArrayItem) row)));
        });
        generator.endRows();
    }

    private void generateRows(SequenceIterator rows, List<GroundedValue> columnFields) throws XPathException {
        generator.startRows();
        SequenceTool.supply(rows, row -> {
            generator.row(namedRow(filteredRow((ArrayItem) row), columnFields));
        });
        generator.endRows();
    }

    private Iterable<IterationUtils.XPEConsumer<Outputter>> plainRow(Iterable<GroundedValue> fields) {
        List<IterationUtils.XPEConsumer<Outputter>> plainFields = new ArrayList<>();
        for (GroundedValue field : fields) {
            plainFields.add(generator.plainField((StringValue) field));
        }
        return plainFields;
    }

    private Iterable<IterationUtils.XPEConsumer<Outputter>> namedRow(Iterable<GroundedValue> fields, Iterable<GroundedValue> columnNames) throws XPathException {
        List<IterationUtils.XPEConsumer<Outputter>> namedFields = new ArrayList<>();
        Iterator<GroundedValue> columns = columnNames.iterator();
        GroundedValue column = null;
        for (GroundedValue value : fields) {
            if (columns.hasNext()) {
                column = columns.next();
            } else {
                column = null;
            }
            IterationUtils.XPEConsumer<Outputter> fieldGenerator;
            if (column == null) {
                fieldGenerator = generator.plainField((StringValue) value);
            } else {
                fieldGenerator = generator.namedField((StringValue) value, column.getStringValue());
            }
            namedFields.add(fieldGenerator);
        }
        return namedFields;
    }

    /**
     * Handles the actual output tree building of the CSV's XML representation. Instances of this class CANNOT be
     * reused.
     */
    static class Generator {
        private final Outputter out;
        private final Builder builder;
        private final Map<String, NodeName> names = new HashMap<>();
        private final NamespaceUri nsUri = NamespaceUri.of("http://www.w3.org/2005/xpath-functions");
        private final NodeName columnAttrName = FingerprintedQName.fromEQName("column");

        /**
         * @param context the XPath context of the fn:csv-to-xml's invocation
         */
        Generator(XPathContext context, String staticBaseUri) {
            builder = context.getController().makeBuilder();
            builder.setSystemId(staticBaseUri);
            builder.setTiming(false);
            builder.setDurability(Durability.TEMPORARY);
            out = new ComplexContentOutputter(builder);
            setNames("csv", "columns", "column", "rows", "row", "field");
        }

        /**
         * Start the <code>&lt;rows&gt;</code> element.
         *
         * @throws XPathException
         */
        public void startRows() throws XPathException {
            out.startElement(names.get("rows"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            out.startContent();
        }

        /**
         * Close the <code>&lt;rows&gt;</code> element.
         *
         * @throws XPathException
         */
        public void endRows() throws XPathException {
            out.endElement();
        }

        /**
         * Opens the outputter, starts document generation and adds the root element. Do not call more than once...
         *
         * @throws XPathException
         */
        public void startDocument() throws XPathException {
            out.open();
            out.startDocument(ReceiverOption.NONE);
            out.startElement(names.get("csv"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            out.startContent();
        }

        /**
         * Closes the root element, ends the document, and closes the outputter. Do not call more than once...
         *
         * @throws XPathException
         */
        public void endDocument() throws XPathException {
            out.endElement();
            out.endDocument();
            out.close();
        }

        /**
         * Retrieves the built document. Should not be called before <code>endDocument()</code>
         *
         * @return the built document node.
         * @throws XPathException
         */
        public Item getResult() throws XPathException {
            return builder.getCurrentRoot();
        }

        /**
         * Generates the needed NodeNames - there's a very limited set, so they're generated once for reuse.
         *
         * @param locals
         */
        private void setNames(String... locals) {
            for (String local : locals) {
                names.put(local, new FingerprintedQName(null, nsUri, local));
            }
        }

        /**
         * Creates the <code>&lt;columns&gt;</code> element and its children from the supplied sequence of fields.
         *
         * @param row the sequence of fields representing the columns row
         * @throws XPathException
         */
        public void columnsRow(GroundedValue row) throws XPathException {
            out.startElement(names.get("columns"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            out.startContent();
            SequenceTool.supply(row.iterate(), item -> {
                out.startElement(names.get("column"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
                out.startContent();
                out.characters(item.getUnicodeStringValue(), Loc.NONE, ReceiverOption.NONE);
                out.endElement();
            });
            out.endElement();
        }

        /**
         * Returns a lambda that will generate a plain field element (no column name attribute).
         *
         * @param value the string value of the field
         * @return a Consumer-style lambda that returns nothing but can throw XPathException
         */
        public IterationUtils.XPEConsumer<Outputter> plainField(StringValue value) {
            return out -> {
                out.startElement(names.get("field"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
                out.startContent();
                out.characters(value.getUnicodeStringValue(), Loc.NONE, ReceiverOption.NONE);
                out.endElement();
            };
        }

        /**
         * Returns a lambda that will generate a field element with a column name attribute.
         *
         * @param value  the string value of the field.
         * @param column the String attribute value.
         * @return a Consumer-style lambda that returns nothing but can throw XPathException.
         */
        public IterationUtils.XPEConsumer<Outputter> namedField(StringValue value, String column) {
            return out -> {
                out.startElement(names.get("field"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
                out.attribute(columnAttrName, BuiltInAtomicType.STRING, column, Loc.NONE, ReceiverOption.NONE);
                out.startContent();
                out.characters(value.getUnicodeStringValue(), Loc.NONE, ReceiverOption.NONE);
                out.endElement();
            };
        }

        /**
         * Creates a <code>&lt;row&gt;</code> element and populates it with <code>&lt;field&gt;</code> elements by
         * iterating over the supplied field lambdas and invoking them with the outputter.
         *
         * @param fields an Iterable of field-generating lambdas.
         * @throws XPathException
         */
        public void row(Iterable<IterationUtils.XPEConsumer<Outputter>> fields) throws XPathException {
            out.startElement(names.get("row"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            out.startContent();
            for (IterationUtils.XPEConsumer<Outputter> field : fields) {
                field.accept(out);
            }
            out.endElement();
        }
    }
}
