package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.FieldOrSelector;
import com.saxonica.ee.schema.IdentityField;
import com.saxonica.ee.schema.IdentitySelector;
import com.saxonica.ee.stream.StreamingPatternMaker;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.parser.Tokenizer;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ErrorType;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.value.Whitespace;

import java.util.ArrayList;
import java.util.List;

/**
 * An xs:field or xs:selector element in an XML Schema document
 */
public class XSDFieldOrSelector extends AnnotationParent {

    /**
     * The Field or Selector we are constructing
     */
    private FieldOrSelector fieldOrSelector = null;

    /**
     * Creates a new FieldOrSelector element
     */
    XSDFieldOrSelector() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {
        boolean isField = (getLocalPart().equals("field"));
        AttributeMap atts = attributes();
        String[] allowed = {"id", "xpath", "xpathDefaultNamespace"};
        allowAttributes(atts, allowed);

        NamespaceUri defaultNS;
        String defaultNSstring = atts.getValue(NamespaceUri.NULL, "xpathDefaultNamespace");
        if (defaultNSstring == null) {
            defaultNSstring = getXSDSchemaElement().getXPathDefaultNamespace();
        }
        if (defaultNSstring != null) {
            switch (defaultNSstring) {
                case "##defaultNamespace":
                    defaultNS = getURIForPrefix("", true);
                    break;
                case "##targetNamespace":
                    defaultNS = getXSDSchemaElement().getTargetNamespace();
                    break;
                case "##local":
                    defaultNS = NamespaceUri.NULL;
                    break;
                default:
                    defaultNS = NamespaceUri.of(defaultNSstring);
                    break;
            }
        } else {
            defaultNS = NamespaceUri.NULL;
        }

        String xpath = atts.getValue(NamespaceUri.NULL, "xpath");
        if (xpath == null) {
            missingAttribute("xpath");
            xpath = ".";  // for error recovery
        }

        XPathStaticContext env = new XPathStaticContext(this);
        env.setDefaultElementNamespace(defaultNS);
        env.setXPathLanguageLevel(20);
        Pattern selector;
        Expression exp;
        try {

            exp = new XPathParser(env).parse(xpath, 0, Tokenizer.END_OF_INPUT, env);
            boolean allowExtensions = getXSDSchemaElement().getAllowedExtensions().contains("id-xpath-syntax");
            if (allowExtensions) {
                List<String> reasonsForFailure = new ArrayList<>();
                selector = StreamingPatternMaker.makeStreamingPattern(exp, getConfiguration(), reasonsForFailure);
                if (!reasonsForFailure.isEmpty()) {
                    throw new XPathException(reasonsForFailure.get(0));
                }
            } else {
                SelectionParser selectionParser = new SelectionParser(env);
                selector = selectionParser.parseSelector(xpath, env, isField);
                exp = selectionParser.getOptimizedExpression();
            }
            selector.setLocation(env.getContainingLocation());
            selector.setRetainedStaticContext(env.makeRetainedStaticContext());
        } catch (XPathException err) {
            error(err.getMessage());
            // Error recovery path
            selector = new NodeTestPattern(ErrorType.getInstance());
            exp = Literal.makeEmptySequence();
        }

        //-- @id
        processId();

        //-- field
        if (isField) {
            //-- saxon:order
            String order = Whitespace.trim(getAttributeValue(NamespaceUri.SAXON, "order"));
            boolean ascending = true;
            if (order != null) {
                switch (order) {
                    case "ascending":
                        break;
                    case "descending":
                        ascending = false;
                        break;
                    default:
                        error("xs:field/@saxon:order must be 'ascending' or 'descending'");
                        break;
                }

            }
            fieldOrSelector = new IdentityField(xpath);
            fieldOrSelector.setLocator(this);
            fieldOrSelector.setSelection(selector);
            fieldOrSelector.setSelectExpression(exp);
            fieldOrSelector.setNamespaceContext(makeNamespaceContext());
            fieldOrSelector.setXPathDefaultNamespace(defaultNS);
            ((IdentityField)fieldOrSelector).setAscending(ascending);
        }

        //-- @selector
        else {
            fieldOrSelector = new IdentitySelector(xpath);
            fieldOrSelector.setLocator(this);
            fieldOrSelector.setSelection(selector);
            fieldOrSelector.setSelectExpression(exp);
            fieldOrSelector.setNamespaceContext(makeNamespaceContext());
            fieldOrSelector.setXPathDefaultNamespace(defaultNS);
        }

    }

    IdentityField getIdentityField() {
        if (fieldOrSelector instanceof IdentityField) {
            return (IdentityField) fieldOrSelector;
        } else {
            return null;
        }
    }

    /*@Nullable*/
    IdentitySelector getIdentitySelector() {
        if (fieldOrSelector instanceof IdentitySelector) {
            return (IdentitySelector) fieldOrSelector;
        } else {
            return null;
        }
    }


}

// Copyright (c) 2004-2023 Saxonica Limited
