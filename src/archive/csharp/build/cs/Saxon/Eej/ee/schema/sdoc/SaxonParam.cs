using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class SaxonParam : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        protected internal override void prepareAttributes() {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            if (((parent == null) || (parent.getFingerprint() != Saxon.Hej.om.StandardNames.XS_APPINFO))) {
                error("The saxon:param element is allowed only as a child of xs:appinfo");
            }
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"as", "name", "select"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "name");
            string name = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "name");
            Saxon.Hej.om.StructuredQName qName = makeQName(name);
            if ((qName.hasURI(Saxon.Hej.om.NamespaceUri.NULL) && qName.getLocalPart().Equals("value"))) {
                error("The variable name $value is reserved for system use");
            }
            string @as = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "as");
            if ((@as == null)) {
                @as = "item()*";
            }
            string select = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "select");
            if ((select == null)) {
                select = "()";
            }
            Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext env = new Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext(this);
            env.setNamespaceResolver(makeNamespaceContext());
            env.setBaseURI(getBaseURI());
            env.setXPathLanguageLevel(31);
            Saxon.Hej.value.SequenceType requiredType = makeSequenceType(@as, env);
            Saxon.Hej.om.GroundedValue defaultValue = Saxon.Hej.value.EmptySequence.getInstance();
            try {
                Saxon.Hej.expr.Expression expr = Saxon.Hej.expr.parser.ExpressionTool.make(select, env, 0, Saxon.Hej.expr.parser.Token.EOF, null);
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
                Saxon.Hej.type.ItemType contextItemType = Saxon.Hej.type.Type.ITEM_TYPE;
                expr = expr.typeCheck(visitor, config.makeContextItemStaticInfo(contextItemType, true));
                expr = config.getTypeChecker(false).staticTypeCheck(expr, requiredType, () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.PARAM, name, 0), visitor);
                Saxon.Hej.expr.instruct.SlotManager stackFrameMap = config.makeSlotManager();
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(expr, stackFrameMap.getNumberOfVariables(), stackFrameMap);
                Saxon.Hej.Controller controller = new Saxon.Hej.Controller(config);
                Saxon.Hej.expr.XPathContext dynamicContext = controller.newXPathContext();
                dynamicContext = dynamicContext.newCleanContext();
                ((Saxon.Hej.expr.XPathContextMajor)(dynamicContext)).openStackFrame(stackFrameMap);
                defaultValue = Saxon.Hej.om.SequenceTool.toGroundedValue(expr.iterate(dynamicContext));
            } catch (Saxon.Hej.trans.XPathException e) {
                error(e.Message);
            }
            Saxon.Hej.expr.PackageData pd = new Saxon.Hej.expr.PackageData(getConfiguration());
            pd.setHostLanguage(Saxon.Api.HostLanguage.XML_SCHEMA, getConfiguration().getXsdVersion());
            pd.setSchemaAware(true);
            Saxon.Hej.expr.instruct.GlobalParam param = new Saxon.Hej.expr.instruct.GlobalParam();
            param.setPackageData(pd);
            param.setVariableQName(qName);
            param.setRequiredType(requiredType);
            param.setBody(Saxon.Hej.expr.Literal.makeLiteral(defaultValue));
            getXSDSchema().addGlobalParam(param);
        }
        public Saxon.Hej.value.SequenceType makeSequenceType(string sequenceType, Saxon.Hej.expr.StaticContext env) {
            try {
                Saxon.Hej.expr.parser.XPathParser parser = new Saxon.Hej.expr.parser.XPathParser();
                return parser.parseSequenceType(sequenceType, env);
            } catch (Saxon.Hej.trans.XPathException err) {
                error(err.Message);
                return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            }
        }
        public Saxon.Hej.om.StructuredQName makeQName(string lexicalQName) {
            Saxon.Hej.om.StructuredQName qName;
            try {
                qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(lexicalQName, false, true, this);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.setIsStaticError(true);
                string code = e.getErrorCodeLocalPart();
                if ("FONS0004".Equals(code)) {
                    e.setErrorCode("XTSE0280");
                } else {
                    if ("FOCA0002".Equals(code)) {
                        e.setErrorCode("XTSE0020");
                    } else {
                        if ((code == null)) {
                            e.setErrorCode("XTSE0020");
                        }
                    }
                }
                throw new Saxon.Hej.type.SchemaException(e);
            }
            return qName;
        }
    
    }
}