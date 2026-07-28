package net.sf.saxon.sapling;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.Serializer;
import net.sf.saxon.s9api.XdmNode;

import java.io.StringWriter;

public interface BaseSaplingTest {
    final Processor processor = new Processor(false);

    default String serializeSaplingToString(SaplingDocument doc) throws SaxonApiException {
        return serializeXdmNodeToString(doc.toXdmNode(processor));
    }

    default String serializeSaplingToString(SaplingElement element) throws SaxonApiException {
        return serializeXdmNodeToString(element.toXdmNode(processor));
    }

    default String serializeXdmNodeToString(XdmNode node) throws SaxonApiException {
        StringWriter sw = new StringWriter();
        Serializer serializer = processor.newSerializer(sw);
        serializer.setOutputProperty(Serializer.Property.INDENT, "no");
        serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        return serializer.serializeNodeToString(node);
    }
}
