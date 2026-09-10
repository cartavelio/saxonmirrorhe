package com.saxonica.ee.schema.sdoc;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceType;

/**
 * This object represents a saxon:param element in a schema document. The element may appear
 * only as a child of xs:schema. It permits a validation episode to be parameterized, and thus allows
 * a schema to define different sets of assertions (for example) for use under different circumstances,
 * for example at different stages of the life-cycle of the document being validated.
 * <p>The saxon:param element is similar to a stylesheet parameter in XSLT. The permitted
 * attributes are name (the name of the parameter), select (an XPath expression giving a default
 * value for the parameter), as (a SequenceType giving the required type of the parameter).</p>
 * <p>The element declares a variable that is in scope for all XPath expressions appearing within
 * the same schema document. It is an error for two schema documents to declare the same variable.</p>
 */
public class SaxonParam extends AnnotationParent {

    @Override
    protected void prepareAttributes() throws SchemaException {

        Configuration config = getConfiguration();
        NodeInfo parent = getParent();
        if (parent == null || parent.getFingerprint() != StandardNames.XS_APPINFO) {
            error("The saxon:param element is allowed only as a child of xs:appinfo");
        }

        AttributeMap atts = attributes();
        String[] allowed = {"as", "name", "select"};
        allowAttributes(atts, allowed);
        requireAttribute(atts, "name");

        String name = atts.getValue("", "name");
        StructuredQName qName = makeQName(name);
        if (qName.hasURI("") && qName.getLocalPart().equals("value")) {
            error("The variable name $value is reserved for system use");
        }

        String as = atts.getValue("", "as");
        if (as == null) {
            as = "item()*";
        }

        String select = atts.getValue("", "select");
        if (select == null) {
            select = "()";
        }
        XPathStaticContext env = new XPathStaticContext(this);
        env.setNamespaceResolver(makeNamespaceContext());
        env.setBaseURI(getBaseURI());
        env.setXPathLanguageLevel(31);
        SequenceType requiredType = makeSequenceType(as, env);
        GroundedValue defaultValue = EmptySequence.getInstance();
        try {
            Expression expr = ExpressionTool.make(select, env,
                0, Token.EOF, null);
            //expr.setContainer(env);
            RoleDiagnostic role = new RoleDiagnostic(RoleDiagnostic.PARAM, name, 0);
            ExpressionVisitor visitor = ExpressionVisitor.make(env);
            ItemType contextItemType = Type.ITEM_TYPE;
            expr = expr.typeCheck(visitor, config.makeContextItemStaticInfo(contextItemType, true));
            expr = config.getTypeChecker(false).staticTypeCheck(expr, requiredType, role, visitor);
            SlotManager stackFrameMap = config.makeSlotManager();
            ExpressionTool.allocateSlots(expr, stackFrameMap.getNumberOfVariables(), stackFrameMap);
            Controller controller = new Controller(config);
            XPathContext dynamicContext = controller.newXPathContext();
            dynamicContext = dynamicContext.newCleanContext();
            ((XPathContextMajor) dynamicContext).openStackFrame(stackFrameMap);
            defaultValue = SequenceTool.toGroundedValue(expr.iterate(dynamicContext));
        } catch (XPathException e) {
            error(e.getMessage());
        }

        PackageData pd = new PackageData(getConfiguration());
        pd.setHostLanguage(HostLanguage.XML_SCHEMA, getConfiguration().getXsdVersion());
        pd.setSchemaAware(true);

        GlobalParam param = new GlobalParam();
        param.setPackageData(pd);
        param.setVariableQName(qName);
        param.setRequiredType(requiredType);
        param.setBody(Literal.makeLiteral(defaultValue));
        getXSDSchema().addGlobalParam(param);


    }

    /**
     * Process an attribute whose value is a SequenceType
     *
     * @param sequenceType the source text of the attribute
     * @param env          the static context for evaluating the sequence type
     * @return the processed sequence type
     * @throws SchemaException if the syntax is invalid or for example if it refers to a type
     *                         that is not in the static context
     */

    public SequenceType makeSequenceType(String sequenceType, StaticContext env)
            throws SchemaException {
        try {
            XPathParser parser = new XPathParser();
            return parser.parseSequenceType(sequenceType, env);
        } catch (XPathException err) {
            error(err.getMessage());
            // recovery path after reporting an error, e.g. undeclared namespace prefix
            return SequenceType.ANY_SEQUENCE;
        }
    }

    /**
     * Make a structured QName, using this Element as the context for namespace resolution, and
     * registering the code in the namepool. If the name is unprefixed, the
     * default namespace is <b>not</b> used.
     *
     * @param lexicalQName The lexical QName as written, in the form "[prefix:]localname". The name must have
     *                     already been validated as a syntactically-correct QName. Leading and trailing whitespace
     *                     will be trimmed
     * @return the StructuredQName representation of this lexical QName
     * @throws SchemaException if the qname is not a lexically-valid QName, or if the name
     *                         is in a reserved namespace.
     */

    public final StructuredQName makeQName(String lexicalQName)
            throws SchemaException {

        StructuredQName qName;
        try {
            qName = StructuredQName.fromLexicalQName(lexicalQName, false, true, this);
        } catch (XPathException e) {
            e.setIsStaticError(true);
            String code = e.getErrorCodeLocalPart();
            if ("FONS0004".equals(code)) {
                e.setErrorCode("XTSE0280");
            } else if ("FOCA0002".equals(code)) {
                e.setErrorCode("XTSE0020");
            } else if (code == null) {
                e.setErrorCode("XTSE0020");
            }
            throw new SchemaException(e);
        }
        return qName;
    }


}

//
// Copyright (c) 2010-2022 Saxonica Limited