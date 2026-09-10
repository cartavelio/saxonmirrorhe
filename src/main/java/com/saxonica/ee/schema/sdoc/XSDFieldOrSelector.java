package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.FieldOrSelector;
import com.saxonica.ee.schema.IdentityField;
import com.saxonica.ee.schema.IdentitySelector;
import com.saxonica.ee.stream.StreamingPatternMaker;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ErrorType;
import net.sf.saxon.type.SchemaException;

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
    public XSDFieldOrSelector() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    public void prepareAttributes() throws SchemaException {
        boolean isField = (getLocalPart().equals("field"));
        AttributeCollection atts = getAttributeList();
        String[] allowed = {"id", "xpath", "xpathDefaultNamespace"};
        allowAttributes(atts, allowed);


        String defaultNS = atts.getValue("", "xpathDefaultNamespace");
        if (defaultNS == null) {
            defaultNS = getXSDSchema().getXPathDefaultNamespace();
        }
        if (defaultNS != null) {
            if (defaultNS.equals("##defaultNamespace")) {
                defaultNS = getURIForPrefix("", true);
            } else if (defaultNS.equals("##targetNamespace")) {
                defaultNS = getXSDSchema().getTargetNamespace();
            } else if (defaultNS.equals("##local")) {
                defaultNS = "";
            }
        } else {
            defaultNS = "";
        }

        String xpath = atts.getValue("", "xpath");
        if (xpath == null) {
            missingAttribute("xpath");
            xpath = ".";  // for error recovery
        }

        XPathStaticContext env = new XPathStaticContext(getConfiguration());
        env.setDefaultElementNamespace(defaultNS);
        Pattern selector;
        Expression exp;
        try {
            exp = new XPathParser().parse(xpath, 0, Token.EOF, env);
            boolean allowExtensions = getXSDSchema().getAllowedExtensions().contains("id-xpath-syntax");
            if (allowExtensions) {
                List<String> reasonsForFailure = new ArrayList<String>();
                selector = StreamingPatternMaker.makeStreamingPattern(exp, getConfiguration(), reasonsForFailure);
                if (!reasonsForFailure.isEmpty()) {
                    throw new XPathException(reasonsForFailure.get(0));
                }
            } else {
                SelectionParser selectionParser = new SelectionParser();
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
            fieldOrSelector = new IdentityField(xpath);
            fieldOrSelector.setLocator(this);
            fieldOrSelector.setSelection(selector);
            fieldOrSelector.setSelectExpression(exp);
            fieldOrSelector.setNamespaceContext(makeNamespaceContext());
            fieldOrSelector.setXPathDefaultNamespace(defaultNS);
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

    public IdentityField getIdentityField() {
        if (fieldOrSelector instanceof IdentityField) {
            return (IdentityField) fieldOrSelector;
        } else {
            return null;
        }
    }

    /*@Nullable*/
    public IdentitySelector getIdentitySelector() {
        if (fieldOrSelector instanceof IdentitySelector) {
            return (IdentitySelector) fieldOrSelector;
        } else {
            return null;
        }
    }


}

// Copyright (c) 2004-2006 Saxonica Limited
