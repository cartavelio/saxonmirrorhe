using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ElementAvailable : Saxon.Hej.functions.SystemFunction {
        public static bool isXslt30Element(int fp) {
            switch (fp) {
              case Saxon.Hej.om.StandardNames.XSL_ACCEPT:
              case Saxon.Hej.om.StandardNames.XSL_ACCUMULATOR:
              case Saxon.Hej.om.StandardNames.XSL_ACCUMULATOR_RULE:
              case Saxon.Hej.om.StandardNames.XSL_ANALYZE_STRING:
              case Saxon.Hej.om.StandardNames.XSL_APPLY_IMPORTS:
              case Saxon.Hej.om.StandardNames.XSL_APPLY_TEMPLATES:
              case Saxon.Hej.om.StandardNames.XSL_ASSERT:
              case Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE:
              case Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET:
              case Saxon.Hej.om.StandardNames.XSL_BREAK:
              case Saxon.Hej.om.StandardNames.XSL_CALL_TEMPLATE:
              case Saxon.Hej.om.StandardNames.XSL_CATCH:
              case Saxon.Hej.om.StandardNames.XSL_CHARACTER_MAP:
              case Saxon.Hej.om.StandardNames.XSL_CHOOSE:
              case Saxon.Hej.om.StandardNames.XSL_COMMENT:
              case Saxon.Hej.om.StandardNames.XSL_CONTEXT_ITEM:
              case Saxon.Hej.om.StandardNames.XSL_COPY:
              case Saxon.Hej.om.StandardNames.XSL_COPY_OF:
              case Saxon.Hej.om.StandardNames.XSL_DECIMAL_FORMAT:
              case Saxon.Hej.om.StandardNames.XSL_DOCUMENT:
              case Saxon.Hej.om.StandardNames.XSL_ELEMENT:
              case Saxon.Hej.om.StandardNames.XSL_EVALUATE:
              case Saxon.Hej.om.StandardNames.XSL_EXPOSE:
              case Saxon.Hej.om.StandardNames.XSL_FALLBACK:
              case Saxon.Hej.om.StandardNames.XSL_FOR_EACH:
              case Saxon.Hej.om.StandardNames.XSL_FOR_EACH_GROUP:
              case Saxon.Hej.om.StandardNames.XSL_FORK:
              case Saxon.Hej.om.StandardNames.XSL_FUNCTION:
              case Saxon.Hej.om.StandardNames.XSL_GLOBAL_CONTEXT_ITEM:
              case Saxon.Hej.om.StandardNames.XSL_IF:
              case Saxon.Hej.om.StandardNames.XSL_IMPORT:
              case Saxon.Hej.om.StandardNames.XSL_IMPORT_SCHEMA:
              case Saxon.Hej.om.StandardNames.XSL_INCLUDE:
              case Saxon.Hej.om.StandardNames.XSL_ITEM_TYPE:
              case Saxon.Hej.om.StandardNames.XSL_ITERATE:
              case Saxon.Hej.om.StandardNames.XSL_KEY:
              case Saxon.Hej.om.StandardNames.XSL_MAP:
              case Saxon.Hej.om.StandardNames.XSL_MAP_ENTRY:
              case Saxon.Hej.om.StandardNames.XSL_MATCH:
              case Saxon.Hej.om.StandardNames.XSL_MATCHING_SUBSTRING:
              case Saxon.Hej.om.StandardNames.XSL_MERGE:
              case Saxon.Hej.om.StandardNames.XSL_MERGE_ACTION:
              case Saxon.Hej.om.StandardNames.XSL_MERGE_KEY:
              case Saxon.Hej.om.StandardNames.XSL_MERGE_SOURCE:
              case Saxon.Hej.om.StandardNames.XSL_MESSAGE:
              case Saxon.Hej.om.StandardNames.XSL_MODE:
              case Saxon.Hej.om.StandardNames.XSL_NAMESPACE:
              case Saxon.Hej.om.StandardNames.XSL_NAMESPACE_ALIAS:
              case Saxon.Hej.om.StandardNames.XSL_NEXT_ITERATION:
              case Saxon.Hej.om.StandardNames.XSL_NEXT_MATCH:
              case Saxon.Hej.om.StandardNames.XSL_NON_MATCHING_SUBSTRING:
              case Saxon.Hej.om.StandardNames.XSL_NUMBER:
              case Saxon.Hej.om.StandardNames.XSL_ON_COMPLETION:
              case Saxon.Hej.om.StandardNames.XSL_ON_EMPTY:
              case Saxon.Hej.om.StandardNames.XSL_ON_NON_EMPTY:
              case Saxon.Hej.om.StandardNames.XSL_OTHERWISE:
              case Saxon.Hej.om.StandardNames.XSL_OUTPUT:
              case Saxon.Hej.om.StandardNames.XSL_OUTPUT_CHARACTER:
              case Saxon.Hej.om.StandardNames.XSL_OVERRIDE:
              case Saxon.Hej.om.StandardNames.XSL_PACKAGE:
              case Saxon.Hej.om.StandardNames.XSL_PARAM:
              case Saxon.Hej.om.StandardNames.XSL_PERFORM_SORT:
              case Saxon.Hej.om.StandardNames.XSL_PRESERVE_SPACE:
              case Saxon.Hej.om.StandardNames.XSL_PROCESSING_INSTRUCTION:
              case Saxon.Hej.om.StandardNames.XSL_RESULT_DOCUMENT:
              case Saxon.Hej.om.StandardNames.XSL_SEQUENCE:
              case Saxon.Hej.om.StandardNames.XSL_SORT:
              case Saxon.Hej.om.StandardNames.XSL_SOURCE_DOCUMENT:
              case Saxon.Hej.om.StandardNames.XSL_STRIP_SPACE:
              case Saxon.Hej.om.StandardNames.XSL_STYLESHEET:
              case Saxon.Hej.om.StandardNames.XSL_SWITCH:
              case Saxon.Hej.om.StandardNames.XSL_TEMPLATE:
              case Saxon.Hej.om.StandardNames.XSL_TEXT:
              case Saxon.Hej.om.StandardNames.XSL_TRANSFORM:
              case Saxon.Hej.om.StandardNames.XSL_TRY:
              case Saxon.Hej.om.StandardNames.XSL_USE_PACKAGE:
              case Saxon.Hej.om.StandardNames.XSL_VALUE_OF:
              case Saxon.Hej.om.StandardNames.XSL_VARIABLE:
              case Saxon.Hej.om.StandardNames.XSL_WHEN:
              case Saxon.Hej.om.StandardNames.XSL_WHERE_POPULATED:
              case Saxon.Hej.om.StandardNames.XSL_WITH_PARAM:
                return true;
              default:
                return false;
            }
        }
        public static bool isXslt40Element(int fp) {
            return ((isXslt30Element(fp) || (fp == Saxon.Hej.om.StandardNames.XSL_ARRAY)) || (fp == Saxon.Hej.om.StandardNames.XSL_ARRAY_MEMBER));
        }
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            try {
                if ((arguments[0] is Saxon.Hej.expr.StringLiteral)) {
                    string arg = ((Saxon.Hej.expr.StringLiteral)(arguments[0])).stringify();
                    Saxon.Hej.om.StructuredQName elem = getElementName(arg);
                    if ((elem.hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT) && elem.getLocalPart().Equals("evaluate"))) {
                        return (base.getSpecialProperties(arguments)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT);
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
            }
            return base.getSpecialProperties(arguments);
        }
        private bool isElementAvailable(string lexicalName, string edition, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.StructuredQName qName = getElementName(lexicalName);
            if (qName.hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT)) {
                int fp = context.getConfiguration().getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.XSLT, qName.getLocalPart());
                int xsltVersion = getRetainedStaticContext().getPackageData().getHostLanguageVersion();
                bool known = ((xsltVersion == 40) ? isXslt40Element(fp) : isXslt30Element(fp));
                if ((fp == Saxon.Hej.om.StandardNames.XSL_EVALUATE)) {
                    known = (known && !(context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.DISABLE_XSL_EVALUATE)));
                }
                return known;
            } else {
                if ((qName.hasURI(Saxon.Hej.lib.NamespaceConstant.IXSL) && !(edition.Equals("JS")))) {
                    return false;
                }
            }
            return context.getConfiguration().isExtensionElementAvailable(qName);
        }
        private Saxon.Hej.om.StructuredQName getElementName(string lexicalName) {
            try {
                if (Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(lexicalName))) {
                    string uri = getRetainedStaticContext().getURIForPrefix("", true);
                    return new Saxon.Hej.om.StructuredQName("", uri, lexicalName);
                } else {
                    return Saxon.Hej.om.StructuredQName.fromLexicalQName(lexicalName, false, true, getRetainedStaticContext());
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Invalid element name passed to element-available(): "+e.Message));
                err.setErrorCode("XTDE1440");
                throw err;
            }
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string lexicalQName = arguments[0].head().getStringValue();
            bool b = isElementAvailable(lexicalQName, getRetainedStaticContext().getPackageData().getTargetEdition(), context);
            return Saxon.Hej.value.BooleanValue.get(b);
        }
    
    }
}