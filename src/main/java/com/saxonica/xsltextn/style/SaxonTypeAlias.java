package com.saxonica.xsltextn.style;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.QNameParser;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.style.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.SequenceType;


/**
 * The class implements a saxon:import-query declaration in a stylesheet. This
 * declaration imports an XQuery library module and adds the functions defined
 * in that module to the static context, making them available for calling from
 * XPath expressions in the stylesheet.
 */

public class SaxonTypeAlias extends StyleElement {

    private StructuredQName typeAlias;
    private boolean resolved = false;

    public void prepareAttributes() throws XPathException {

        // Avoid reporting errors twice
        if (typeAlias != null) {
            return;
        }

        String typeAtt = null;

        AttributeCollection atts = getAttributeList();

        for (int a = 0; a < atts.getLength(); a++) {
            String f = atts.getQName(a);
            switch (f) {
                case "name":
                    typeAlias = makeQName(atts.getValue(a));
                    break;
                case "type":
                    typeAtt = atts.getValue(a);
                    break;
                default:
                    checkUnknownAttribute(atts.getNodeName(a));
                    break;
            }
        }

        if (typeAlias == null) {
            reportAbsence("name");
        }

        if (typeAtt == null) {
            reportAbsence("type");
        }


    }

    public StructuredQName getObjectName() {
        if (typeAlias == null) {
            try {
                prepareAttributes();
            } catch (XPathException e) {
                return new StructuredQName("saxon", NamespaceConstant.SAXON, "error-name");
            }
        }
        return typeAlias;
    }

    public void indexTypeAlias(ComponentDeclaration decl, PrincipalStylesheetModule top) throws XPathException {
        prepareAttributes();
        top.getTypeAliasManager().processDeclaration(decl);
    }

    public void validate(ComponentDeclaration decl) throws XPathException {
        checkEmpty();
        checkTopLevel("XTSE0010", false);
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:type-alias",
                                                getPackageData().getLocalLicenseId());
    }

    public ItemType tryToResolve() throws XPathException {
        StaticContext env = new TypeAliasContext();
        resolved = true;
        ItemType type = makeItemType(env);
        return resolved ? type : null;
    }

    private void markUnresolved() {
        resolved = false;
    }

    private ItemType makeItemType(StaticContext env) throws XPathException {
        try {
            XPathParser parser =
                    getConfiguration().newExpressionParser("XP", false, 31);
            parser.setLanguage(XPathParser.XPATH, 31);
            QNameParser qp = new QNameParser(env.getNamespaceResolver());
            qp.setAcceptEQName(true);
            qp.setDefaultNamespace("");
            qp.setErrorOnBadSyntax("XPST0003");
            qp.setErrorOnUnresolvedPrefix("XPST0081");
            parser.setQNameParser(qp);
            String typeAtt = getAttributeValue("type");
            if (typeAtt == null) {
                reportAbsence("type");
                typeAtt = "item()";
            }
            SequenceType st = parser.parseExtendedSequenceType(typeAtt, env);
            if (st.getCardinality() != StaticProperty.ALLOWS_ONE) {
                compileError("Item type must not include an occurrence indicator");
            }
            return st.getPrimaryType();
        } catch (XPathException err) {
            compileError(err);
            // recovery path after reporting an error, e.g. undeclared namespace prefix
            return AnyItemType.getInstance();
        }
    }


    /**
     * Compile this XSLT declaration. This always fails for this class.
     *
     * @param exec the Executable
     * @param decl the containing top-level declaration, for example xsl:function or xsl:template
     * @return a compiled expression or null.
     * @throws XPathException (always)
     */

    /*@Nullable*/
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return null;
    }

    private class TypeAliasContext extends ExpressionContext {

        public TypeAliasContext() {
            super(SaxonTypeAlias.this, new StructuredQName("", "", "type"));
        }

        /**
         * Get type alias. This is a Saxon extension. A type alias is a QName which can
         * be used as a shorthand for an itemtype, using the syntax ~typename anywhere that
         * an item type is permitted.
         *
         * @param typeName the name of the type alias
         * @return the corresponding item type, if the name is recognised; otherwise null.
         */
        @Override
        public ItemType resolveTypeAlias(StructuredQName typeName) {
            ItemType resolved = super.resolveTypeAlias(typeName);
            if (resolved == null) {
                markUnresolved();
                return AnyItemType.getInstance();
            } else {
                return resolved;
            }
        }
    }
}

// Copyright (c) 2018 Saxonica Limited.