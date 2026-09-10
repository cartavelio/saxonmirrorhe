package com.saxonica.functions.qt4.csv;

import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.PrependSequenceIterator;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

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
     * @throws XPathException if tree building fails
     */
    @Override
    public Item fromRowSequence(SequenceIterator rows) throws XPathException {
        generator = new Generator(context, staticBaseUri);
        generator.startDocument();

        ArrayItem firstRow = (ArrayItem) rows.next();
        if (firstRow == null) {
            generateRows(EmptyIterator.getInstance(), explicitColumnNamesAsStrings());
            generator.endDocument();
            return generator.getResult();
        }
        if (rowAdjuster == null) {
            if (trimRows) {
                rowAdjuster = makeRowTrimmer(firstRow.arrayLength());
            } else {
                rowAdjuster = row -> row;
            }
        }
        if (extractHeaderRow) {
            firstRow = (ArrayItem) rowAdjuster.map(firstRow);
            List<StringValue> columnNames = generateColumnNames(firstRow);
            ItemMappingIterator adjustedRows = ItemMappingIterator.map(rows, rowAdjuster);
            generateRows(adjustedRows, columnNames);
        } else {
            rows = new PrependSequenceIterator(firstRow, rows);
            ItemMappingIterator adjustedRows = ItemMappingIterator.map(rows, rowAdjuster);
            List<StringValue> columnNames = explicitColumnNamesAsStrings();
            generateRows(adjustedRows, columnNames);
        }
        generator.endDocument();
        return generator.getResult();
    }

    private List<StringValue> explicitColumnNamesAsStrings() {
        List<StringValue> columnNames = new ArrayList<>();
        if (explicitColumnNames != null) {
            for (Item name : explicitColumnNames.asIterable()) {
                columnNames.add((StringValue)name);
            }
        }
        return columnNames;
    }

    /**
     * Generate the {@code columns} and {@code rows} elements in the XML result
     * @param rows iterator over the rows delivered by the CSV parser
     * @param columnNames the list of column names (in the order they are output),
     *                    base either on the contents of the header row, or
     *                    the explicitly supplied header option. May be an empty list.
     * @throws XPathException if tree building fails
     */
    private void generateRows(SequenceIterator rows, List<StringValue> columnNames) throws XPathException {
        if (!columnNames.isEmpty()) {
            generator.columns(columnNames);
        }
        generator.startRows();
        SequenceTool.supply(rows, row -> {
            generator.row((ArrayItem) row, columnNames);
        });
        generator.endRows();
    }


    /**
     * Handles the actual output tree building of the CSV's XML representation. Instances of this class CANNOT be
     * reused.
     */
    static class Generator {
        private final Outputter out;
        private final Builder builder;
        private final NodeName columnAttrName;
        private final NodeName csvElementName;
        private final NodeName columnsElementName;
        private final NodeName columnElementName;
        private final NodeName rowsElementName;
        private final NodeName rowElementName;
        private final NodeName fieldElementName;

        /**
         * @param context the XPath context of the fn:csv-to-xml's invocation
         */
        Generator(XPathContext context, String staticBaseUri) {
            builder = context.getController().makeBuilder();
            builder.setSystemId(staticBaseUri);
            builder.setTiming(false);
            builder.setDurability(Durability.TEMPORARY);
            out = new ComplexContentOutputter(builder);
            csvElementName = makeNodeName("csv", context);
            columnsElementName = makeNodeName("columns", context);
            columnElementName = makeNodeName("column", context);
            rowsElementName = makeNodeName("rows", context);
            rowElementName = makeNodeName("row", context);
            fieldElementName = makeNodeName("field", context);
            columnAttrName = new NoNamespaceName("column");
        }

        private NodeName makeNodeName(String name, XPathContext context) {
            int fp = context.getConfiguration().getNamePool().allocateFingerprint(NamespaceUri.FN, name);
            return new FingerprintedQName("", NamespaceUri.FN, name, fp);
        }

        /**
         * Start the <code>&lt;rows&gt;</code> element.
         *
         * @throws XPathException if tree building fails
         */
        public void startRows() throws XPathException {
            out.startElement(rowsElementName, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            out.startContent();
        }

        /**
         * Close the <code>&lt;rows&gt;</code> element.
         *
         * @throws XPathException if tree building fails
         */
        public void endRows() throws XPathException {
            out.endElement();
        }

        /**
         * Opens the outputter, starts document generation and adds the root element. Do not call more than once...
         *
         * @throws XPathException if tree building fails
         */
        public void startDocument() throws XPathException {
            out.open();
            out.startDocument(0);
            out.startElement(csvElementName, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            out.startContent();
        }

        /**
         * Closes the root element, ends the document, and closes the outputter. Do not call more than once...
         *
         * @throws XPathException if tree building fails
         */
        public void endDocument() throws XPathException {
            out.endElement();
            out.endDocument();
            out.close();
        }

        /**
         * Retrieves the outermost element of the built document. Should not be called before <code>endDocument()</code>
         *
         * @return the built document node.
         */
        public Item getResult() {
            return builder.getCurrentRoot();
        }
        
        public void columns(List<StringValue> columnNames) throws XPathException {
            out.startElement(columnsElementName, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            out.startContent();
            for (StringValue name : columnNames) {
                out.startElement(columnElementName, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
                out.startContent();
                out.characters(name.getUnicodeStringValue(), Loc.NONE, ReceiverOption.NONE);
                out.endElement();
            }
            out.endElement();
        }

        /**
         * Creates a <code>&lt;row&gt;</code> element and populates it with <code>&lt;field&gt;</code> elements
         *
         * @param row a row of the parsed CSV, represented as an XDM array.
         * @throws XPathException if tree building fails
         */
        public void row(ArrayItem row, List<StringValue> columnNames) throws XPathException {
            out.startElement(rowElementName, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
            out.startContent();
            int pos = 0;
            for (GroundedValue member : row.members()) {
                out.startElement(fieldElementName, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
                if (pos < columnNames.size() && !columnNames.get(pos).isEmpty()) {
                    out.attribute(columnAttrName, BuiltInAtomicType.UNTYPED_ATOMIC,
                                  columnNames.get(pos).getStringValue(), Loc.NONE, ReceiverOption.NONE);
                }
                out.startContent();
                out.characters(((StringValue)member).getUnicodeStringValue(), Loc.NONE, ReceiverOption.NONE);
                out.endElement();
                pos++;
            }
            out.endElement();
        }
    }
}
