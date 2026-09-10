package net.sf.saxon.type;

import com.saxonica.ee.schema.ElementDecl;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NodeTest;

/**
 * A schema is a collection of schema components. It may be derived from a single schema
 * document (an xs:schema element), in which case it contains those components defined in that
 * schema document, the components defined in the included documents (expanded
 * transitively), and the components defined in imported schema documents (not
 * expanded transitively). Alternatively, it may be a collection of schema components
 * derived from multiple independent schema documents.
 */

public class MinimalSchema implements Schema {

    private Configuration config;

    public Configuration getConfiguration() {
        return config;
    }

    /**
     * Get a global attribute declaration by fingerprint
     *
     * @param fingerprint the NamePool fingerprint of the element name
     * @return the attribute declaration whose name matches the given
     * fingerprint, or null if no element declaration with this name has
     * been registered.
     */

    public SchemaDeclaration getAttributeDecl(int fingerprint) {
        return null;
    }

    /**
     * Get a global attribute declaration by QName
     *
     * @param name the the element name
     * @return the attribute declaration whose name matches the given
     * fingerprint, or null if no element declaration with this name has
     * been registered.
     */

    public SchemaDeclaration getAttributeDecl(StructuredQName name) {
        return null;
    }

    /**
     * Get the global element declaration with a given fingerprint
     *
     * @param fingerprint the NamePool fingerprint of the name of the element
     * @return the element declaration, or null if not found.
     */

    public ElementDecl getElementDecl(int fingerprint) {
        return null;
    }


    /**
     * Get the global element declaration with a given  name
     *
     * @param name the name of the element
     * @return the element declaration, or null if not found.
     */

    public SchemaDeclaration getElementDecl(StructuredQName name) {
        return null;
    }

    /**
     * Get the top-level schema type definition with a given QName.
     *
     * @param name the name of the required schema type
     * @return the schema type , or null if there is none
     * with this name.
     * @since 9.7
     */

    /*@Nullable*/
    public SchemaType getSchemaType(StructuredQName name) {
        if (name.hasURI(NamespaceUri.SCHEMA)) {
            return BuiltInType.getSchemaTypeByLocalName(name.getLocalPart());
        }
        return null;
    }

    /**
     * Make a {@code schema-element} node test for a given element name
     *
     * @param fp the integer fingerprint of the element name
     * @return a node test corresponding to the XPath item type syntax schema-element(E)
     * @throws SchemaException if there is no element declaration with this name in the schema
     */
    @Override
    public NodeTest makeSchemaElementTest(int fp) throws SchemaException {
        return null;
    }

    /**
     * Make a {@code schema-attribute} node test for a given attribute name
     *
     * @param fp the integer fingerprint of the attribute name
     * @return a node test corresponding to the XPath item type syntax schema-attribute(E)
     * @throws SchemaException if there is no attribute declaration with this name in the schema
     */
    @Override
    public NodeTest makeSchemaAttributeTest(int fp) throws SchemaException {
        return null;
    }
}

