package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.SingleNamespaceSchema;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.tree.linked.ElementImpl;
import net.sf.saxon.tree.linked.NodeFactory;
import net.sf.saxon.tree.linked.TextImpl;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SchemaType;

import java.util.HashMap;

/**
 * Class SchemaNodeFactory. <br>
 * A Factory for nodes in the schema document tree. <br>
 * Currently only allows Element nodes to be user-constructed.
 *
 */

public class SchemaNodeFactory implements NodeFactory {

    private final PipelineConfiguration pipe;
    private final SchemaCompiler compiler;
    private final EnterpriseConfiguration config;
    private final HashMap<String, SchemaElement> idTable = new HashMap<String, SchemaElement>(32);
    private final SchemaElement referrer;

    /**
     * Create a SchemaNodeFactory, used for building the tree representation of a schema document
     *
     * @param compiler used for error reporting
     * @param pipe     the PipelineConfiguration
     * @param referrer the element, typically an xs:include, xs:import, or xs:redefine, that caused this
     *                 schema document to be built
     */
    public SchemaNodeFactory(SchemaCompiler compiler, PipelineConfiguration pipe, SchemaElement referrer) {
        this.compiler = compiler;
        this.pipe = pipe;
        config = (EnterpriseConfiguration) pipe.getConfiguration();
        this.referrer = referrer;
    }

    /**
     * Get the pipeline configuration
     *
     * @return the pipeline configuration
     */

    public PipelineConfiguration getPipelineConfiguration() {
        return pipe;
    }

    /**
     * Get the SchemaCompiler associated with this node factory
     *
     * @return the SchemaCompiler associated with this node factory
     */

    public SchemaCompiler getSchemaCompiler() {
        return compiler;
    }


    /**
     * Create an Element node.
     */

    @Override
    public ElementImpl makeElementNode(
            NodeInfo parent, NodeName elemName, SchemaType elemType, boolean isNilled,
            AttributeMap attlist, NamespaceMap namespaces,
            PipelineConfiguration pipe, Location locationId, int sequence) {
        String baseURI = null;
        int lineNumber = -1;
        int columnNumber = -1;

        baseURI = locationId.getSystemId();
        lineNumber = locationId.getLineNumber();
        columnNumber = locationId.getColumnNumber();

        int f = elemName.obtainFingerprint(pipe.getConfiguration().getNamePool());

        SchemaElement e = makeXSDElement(f, parent.getFingerprint());

        if (e instanceof XSDSchemaElement) {
            config.checkLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
        }

        e.setSchemaNodeFactory(this);
        e.setNamespaceMap(namespaces);
        e.initialise(elemName, elemType, attlist, parent, sequence);
        e.setLocation(baseURI, lineNumber, columnNumber);
        return e;
    }

    /**
     * Make an element node in the schema document tree
     *
     * @param f      the fingerprint of the element name
     * @param parent the fingerprint of the name of the element's parent
     * @return the new element node
     */

    private SchemaElement makeXSDElement(int f, int parent) {
        switch (f) {
            case StandardNames.XS_ALTERNATIVE:
                return new XSDAlternative();
            case StandardNames.XS_ANNOTATION:
                return new XSDAnnotation();
            case StandardNames.XS_ASSERT:
                return new XSDAssert();
            case StandardNames.XS_ASSERTION:
                return new XSDAssert();
            case StandardNames.XS_ATTRIBUTE:
                return new XSDAttribute();
            case StandardNames.XS_ALL:
                return new XSDCompositor();
            case StandardNames.XS_ANY:
                return new XSDAny();
            case StandardNames.XS_ANY_ATTRIBUTE:
                return new XSDAnyAttribute();
            case StandardNames.XS_ATTRIBUTE_GROUP:
                return new XSDAttributeGroup();
            case StandardNames.XS_CHOICE:
                return new XSDCompositor();
            case StandardNames.XS_COMPLEX_CONTENT:
                return new XSDComplexContent();
            case StandardNames.XS_COMPLEX_TYPE:
                return new XSDComplexType();
            case StandardNames.XS_DEFAULT_OPEN_CONTENT:
                return new XSDDefaultOpenContent();
            case StandardNames.XS_ELEMENT:
                return new XSDElement();
            case StandardNames.XS_EXTENSION:
                return new XSDExtension();
            case StandardNames.XS_GROUP:
                return new XSDGroup();
            case StandardNames.XS_IMPORT:
                return new XSDImport();
            case StandardNames.XS_INCLUDE:
                return new XSDInclude();
            case StandardNames.XS_KEY:
                return new XSDIdentityConstraint();
            case StandardNames.XS_KEYREF:
                return new XSDIdentityConstraint();
            case StandardNames.XS_LIST:
                return new XSDList();
            case StandardNames.XS_notation:
                return new XSDNotation();
            case StandardNames.XS_OPEN_CONTENT:
                return new XSDOpenContent();
            case StandardNames.XS_OVERRIDE:
                return new XSDOverride();
            case StandardNames.XS_REDEFINE:
                return new XSDRedefine();
            case StandardNames.XS_RESTRICTION:
                if (parent == StandardNames.XS_SIMPLE_TYPE) {
                    return new XSDSimpleTypeRestriction();
                } else if (parent == StandardNames.XS_SIMPLE_CONTENT) {
                    return new XSDSimpleContentRestriction();
                } else {
                    return new XSDComplexContentRestriction();
                }
            case StandardNames.XS_SCHEMA:
                XSDSchemaElement s = new XSDSchemaElement();
                SingleNamespaceSchema preparedSchema = new SingleNamespaceSchema(config, NamespaceUri.NULL);
                s.setSchema(preparedSchema);
                s.setSchemaCompiler(compiler);
                s.setReferrer(referrer);
                return s;
            case StandardNames.XS_SEQUENCE:
                return new XSDCompositor();
            case StandardNames.XS_SIMPLE_CONTENT:
                return new XSDSimpleContent();
            case StandardNames.XS_SIMPLE_TYPE:
                return new XSDSimpleType();
            case StandardNames.XS_UNIQUE:
                return new XSDIdentityConstraint();
            case StandardNames.XS_UNION:
                return new XSDUnion();

            case StandardNames.XS_APPINFO:
            case StandardNames.XS_DOCUMENTATION:
                return new XSDDocumentation();

            case StandardNames.XS_FIELD:
            case StandardNames.XS_SELECTOR:
                return new XSDFieldOrSelector();

            case StandardNames.XS_ENUMERATION:
            case StandardNames.XS_FRACTION_DIGITS:
            case StandardNames.XS_LENGTH:
            case StandardNames.XS_MAX_EXCLUSIVE:
            case StandardNames.XS_MAX_INCLUSIVE:
            case StandardNames.XS_MAX_LENGTH:
            case StandardNames.XS_MAX_SCALE:
            case StandardNames.XS_MIN_EXCLUSIVE:
            case StandardNames.XS_MIN_INCLUSIVE:
            case StandardNames.XS_MIN_LENGTH:
            case StandardNames.XS_MIN_SCALE:
            case StandardNames.XS_NOTATION:
            case StandardNames.XS_PATTERN:
            case StandardNames.XS_EXPLICIT_TIMEZONE:
            case StandardNames.XS_TOTAL_DIGITS:
            case StandardNames.XS_WHITE_SPACE:
                return new XSDFacet();

            case StandardNames.SAXON_PARAM:
                return new SaxonParam();
            case StandardNames.SAXON_PREPROCESS:
                return new SaxonPreprocess();
            case StandardNames.SAXON_DISTINCT:
            case StandardNames.SAXON_ORDER:
                return new XSDFacet();

            default:

                return new UnknownElement();
        }
    }

    /**
     * Make a text node
     *
     * @param parent  the parent element
     * @param content the content of the text node
     * @return the constructed text node
     */
    @Override
    public TextImpl makeTextNode(NodeInfo parent, UnicodeString content) {
        return new TextImpl(content);
    }

    /**
     * Register an ID value found on an element in the schema document
     *
     * @param id      the ID value
     * @param element the element on which the ID value appears
     * @return a SchemaException if the ID value is invalid or non-unique: note this is returned,
     *         not thrown.
     */

    /*@Nullable*/
    public SchemaException registerId(String id, SchemaElement element) {
        if (!NameChecker.isValidNCName(id)) {
            return new SchemaException("Invalid id value " + Err.wrap(id));
        }
        SchemaElement e = idTable.get(id);
        if (e != null) {
            return new SchemaException("Non-unique id value " + Err.wrap(id));
        }
        idTable.put(id, element);
        return null;
    }

}

// Copyright (c) 2004-2023 Saxonica Limited