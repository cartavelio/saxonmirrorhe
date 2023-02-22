using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class SystemProperty : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if (((arguments[0] is Saxon.Hej.expr.StringLiteral) && visitor.getTargetEdition().Equals(visitor.getConfiguration().getEditionCode()))) {
                try {
                    string name = ((Saxon.Hej.expr.StringLiteral)(arguments[0])).stringify();
                    Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(name, false, true, getRetainedStaticContext());
                    if (qName.hasURI(Saxon.Hej.om.NamespaceUri.XSLT)) {
                        string local = qName.getLocalPart();
                        if ((((((((local.Equals("version") || local.Equals("vendor")) || local.Equals("vendor-url")) || local.Equals("product-name")) || local.Equals("product-version")) || local.Equals("supports-backwards-compatibility")) || local.Equals("xpath-version")) || local.Equals("xsd-version"))) {
                            string result = getProperty(Saxon.Hej.lib.NamespaceConstant.XSLT, local, getRetainedStaticContext());
                            return new Saxon.Hej.expr.StringLiteral(result);
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                }
            }
            return null;
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string name = arguments[0].head().getStringValue();
            try {
                Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(name, false, true, getRetainedStaticContext());
                return new Saxon.Hej.value.StringValue(getProperty(qName.getNamespaceUri().ToString(), qName.getLocalPart(), getRetainedStaticContext()));
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.trans.XPathException(("Invalid system property name. "+err.Message), "XTDE1390", context);
            }
        }
        public static string yesOrNo(bool whatever) {
            return (whatever ? "yes" : "no");
        }
        public static string getProperty(string uri, string local, Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            Saxon.Hej.Configuration config = rsc.getConfiguration();
            string edition = rsc.getPackageData().getTargetEdition();
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.XSLT)) {
                switch (local) {
                  case "version":
                    return "3.0";
                  case "vendor":
                    return Saxon.Hej.Version.getProductVendor();
                  case "vendor-url":
                    return Saxon.Hej.Version.getWebSiteAddress();
                  case "product-name":
                    return Saxon.Hej.Version.getProductName();
                  case "product-version":
                    return Saxon.Hej.Version.getProductVariantAndVersion(edition);
                  case "is-schema-aware":
                    bool schemaAware = rsc.getPackageData().isSchemaAware();
                    return yesOrNo(schemaAware);
                  case "supports-serialization":
                    return "yes";
                  case "supports-backwards-compatibility":
                    return "yes";
                  case "supports-namespace-axis":
                    return "yes";
                  case "supports-streaming":
                    return yesOrNo((("EE".Equals(edition) && config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT)) && !(config.getConfigurationProperty(Saxon.Hej.lib.Feature<string>.STREAMABILITY).Equals("off"))));
                  case "supports-dynamic-evaluation":
                    return yesOrNo(!(config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.DISABLE_XSL_EVALUATE)));
                  case "supports-higher-order-functions":
                    return "yes";
                  case "xpath-version":
                    return "3.1";
                  case "xsd-version":
                    return ((rsc.getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10) ? "1.0" : "1.1");
                }
                return "";
            } else {
                if ((uri.Length==0 && config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS))) {
                    string val = Saxon.Ejava.lang.System.getProperty(local);
                    return ((val == null) ? "" : val);
                } else {
                    if ((uri.Equals(Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.NAMESPACE) && local.Equals("version"))) {
                        return (Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.VERSION).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    } else {
                        if ((uri.Equals(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.NAMESPACE) && local.Equals("version"))) {
                            return Saxon.Eej.functions.extfn.EXPathFileFunctionSet.VERSION.ToString();
                        } else {
                            return "";
                        }
                    }
                }
            }
        }
    
    }
}