package com.saxonica.ptree;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.NamespaceBindingSet;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.serialize.Emitter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;

import javax.xml.transform.stream.StreamResult;
import java.io.DataOutputStream;
import java.io.IOException;

/**
 * An Emitter (serializer) to output a result tree in Saxon PTree format
 */

public class PTreeEmitter extends Emitter {

    /*@NotNull*/
    private TinyBuilder builder;

    public PTreeEmitter(PipelineConfiguration pipe) {
        builder = new TinyBuilder(pipe);
        builder.setStatistics(pipe.getConfiguration().getTreeStatistics().RESULT_TREE_STATISTICS);
        setPipelineConfiguration(pipe);
    }

    /**
     * Set the namePool in which all name codes can be found, and other configuration details
     */

    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration config) {
        builder.setPipelineConfiguration(config);
        super.setPipelineConfiguration(config);
    }

    /**
     * Set the StreamResult acting as the output destination of the Emitter
     */

    public void setStreamResult(StreamResult result) throws XPathException {
        super.setStreamResult(result);
        makeWriter();
    }

    /**
     * Notify an attribute. Attributes are notified after the startElement event, and before any
     * children. Namespaces and attributes may be intermingled.
     *
     * @param nameCode   The name of the attribute
     * @param typeCode   The type of the attribute
     * @param locationId an integer which can be interpreted using a LocationMap to return
     *                   information such as line number and system ID. If no location information is available,
     *                   the value zero is supplied.
     * @param properties Bit significant value. The following bits are defined:
     *                   <dl>
     *                   <dt>DISABLE_ESCAPING</dt>    <dd>Disable escaping for this attribute</dd>
     *                   <dt>NO_SPECIAL_CHARACTERS</dt>      <dd>Attribute value contains no special characters</dd>
     *                   </dl>
     * @throws IllegalStateException: attempt to output an attribute when there is no open element
     *                                start tag
     */

    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) throws XPathException {
        builder.attribute(nameCode, typeCode, value, locationId, properties);
    }

    /**
     * Notify character data. Note that some receivers may require the character data to be
     * sent in a single event, but in general this is not a requirement.
     *  @param chars      The characters
     * @param locationId an integer which can be interpreted using a LocationMap to return
     *                   information such as line number and system ID. If no location information is available,
     *                   the value zero is supplied.
     * @param properties Bit significant value. The following bits are defined:
     *                   <dl>
     *                   <dt>DISABLE_ESCAPING</dt>           <dd>Disable escaping for this text node</dd>
     *                   </dl>
     */

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        builder.characters(chars, locationId, properties);
    }

    /**
     * Notify the end of the event stream
     */

    public void close() throws XPathException {
        TinyDocumentImpl doc = (TinyDocumentImpl) builder.getCurrentRoot();
        if (doc != null) {
            PTreeWriter out = new PTreeWriter((ProfessionalConfiguration) getPipelineConfiguration().getConfiguration());
            try {
                out.writeTree(doc.getTree(), new DataOutputStream(getOutputStream()));
            } catch (IOException e) {
                throw new XPathException(e);
            }
        }
        //noinspection ConstantConditions
        builder = null;
        super.close();
    }

    /**
     * Notify a comment. Comments are only notified if they are outside the DTD.
     *
     * @param content    The content of the comment
     * @param locationId an integer which can be interpreted using a LocationMap to return
     *                   information such as line number and system ID. If no location information is available,
     *                   the value zero is supplied.
     * @param properties Additional information about the comment. The following bits are
     *                   defined:
     *                   <dl>
     *                   <dt>CHECKED</dt>    <dd>Comment is known to be legal (e.g. doesn't contain "--")</dd>
     *                   </dl>
     *
     * @throws IllegalArgumentException: the content is invalid for an XML comment
     */

    public void comment(CharSequence content, Location locationId, int properties) throws XPathException {
        builder.comment(content, locationId, properties);
    }

    /**
     * Notify the end of a document node
     */

    public void endDocument() throws XPathException {
        builder.endDocument();
    }

    /**
     * Notify the end of an element. The receiver must maintain a stack if it needs to know which
     * element is ending.
     */

    public void endElement() throws XPathException {
        builder.endElement();
    }

    /**
     * Notify a namespace. Namespaces are notified <b>after</b> the startElement event, and before
     * any children for the element. The namespaces that are reported are only required
     * to include those that are different from the parent element. The events represent namespace
     * declarations and undeclarations rather than in-scope namespace nodes: an undeclaration is represented
     * by a namespace code of zero. If the sequence of namespace events contains two
     * A namespace must not conflict with any namespaces already used for element or attribute names.
     *  @param namespaceBindings the (prefix, uri) pair representing a namespace binding
     * @param properties       The most important property is REJECT_DUPLICATES. If this property is set, the
     *                         namespace declaration will be rejected if it conflicts with a previous declaration of the same
     *                         prefix. If the property is not set, the namespace declaration will be ignored if it conflicts
     *                         with a previous declaration. This reflects the fact that when copying a tree, namespaces for child
     *                         elements are emitted before the namespaces of their parent element. Unfortunately this conflicts
     *                         with the XSLT rule for complex content construction, where the recovery action in the event of
     */

    public void namespace(NamespaceBindingSet namespaceBindings, int properties) throws XPathException {
        builder.namespace(namespaceBindings, properties);
    }

    /**
     * Notify the start of the event stream
     */

    public void open() throws XPathException {
        builder.open();
    }

    /**
     * Output a processing instruction
     *
     * @param name       The PI name. This must be a legal name (it will not be checked).
     * @param data       The data portion of the processing instruction
     * @param locationId an integer which can be interpreted using a LocationMap to return
     *                   information such as line number and system ID. If no location information is available,
     *                   the value zero is supplied.
     * @param properties Additional information about the PI. The following bits are
     *                   defined:
     *                   <dl>
     *                   <dt>CHECKED</dt>    <dd>Data is known to be legal (e.g. doesn't contain "?&gt;")</dd>
     *                   </dl>
     * @throws IllegalArgumentException: the content is invalid for an XML processing instruction
     */

    public void processingInstruction(String name, CharSequence data, Location locationId, int properties) throws XPathException {
        builder.processingInstruction(name, data, locationId, properties);
    }

    /**
     * Notify the start of the content, that is, the completion of all attributes and namespaces.
     * Note that the initial receiver of output from XSLT instructions will not receive this event,
     * it has to detect it itself. Note that this event is reported for every element even if it has
     * no attributes, no namespaces, and no content.
     */

    public void startContent() throws XPathException {
        builder.startContent();
    }

    /**
     * Notify the start of a document node
     */

    public void startDocument(int properties) throws XPathException {
        open();
        builder.startDocument(properties);
    }

    /**
     * Notify the start of an element
     * @param nameCode   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool. The value -1
 *                   indicates the default type, xs:untyped.
     * @param location information such as line number and system ID. 
     * @param properties bit-significant properties of the element node. If there are no revelant
     */

    public void startElement(NodeName nameCode, SchemaType typeCode, Location location, int properties) throws XPathException {
        builder.startElement(nameCode, typeCode, location, properties);
    }

    /**
     * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
     * supplied on element and attribute events
     *
     * @return true if the Receiver makes any use of this information. If false, the caller
     *         may supply untyped nodes instead of supplying the type annotation
     */

    public boolean usesTypeAnnotations() {
        return true;
    }
}

// Copyright (c) 2005 Saxonica Limited
