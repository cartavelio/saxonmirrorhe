using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class RetainedStaticContext : Saxon.Hej.om.NamespaceResolver {
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Hej.expr.PackageData packageData;
        private System.Uri staticBaseUri;
        private string staticBaseUriString;
        private string defaultCollationName;
        private Saxon.Hej.om.NamespaceResolver namespaces;
        private string defaultFunctionNamespace = Saxon.Hej.lib.NamespaceConstant.FN;
        private string defaultElementNamespace;
        private Saxon.Hej.trans.DecimalFormatManager decimalFormatManager;
        private bool backwardsCompatibility;
        public RetainedStaticContext(Saxon.Hej.Configuration config)  {
            this.config = config;
            packageData = new Saxon.Hej.expr.PackageData(config);
            namespaces = Saxon.Hej.om.NamespaceMap.emptyMap();
            defaultCollationName = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
        }
        public RetainedStaticContext(Saxon.Hej.expr.StaticContext sc)  {
            this.config = sc.getConfiguration();
            this.packageData = sc.getPackageData();
            if ((sc.getStaticBaseURI() != null)) {
                staticBaseUriString = sc.getStaticBaseURI();
                try {
                    this.staticBaseUri = Saxon.Hej.expr.parser.ExpressionTool.getBaseURI(sc, null, true);
                } catch (Saxon.Hej.trans.XPathException e) {
                    staticBaseUri = null;
                }
            }
            this.defaultCollationName = sc.getDefaultCollationName();
            this.decimalFormatManager = sc.getDecimalFormatManager();
            this.defaultElementNamespace = sc.getDefaultElementNamespace();
            defaultFunctionNamespace = sc.getDefaultFunctionNamespace();
            backwardsCompatibility = sc.isInBackwardsCompatibleMode();
            if (Saxon.Hej.Version.platform.JAXPStaticContextCheck(this, sc)) {
            } else {
                Saxon.Hej.om.NamespaceResolver resolver = sc.getNamespaceResolver();
                if ((resolver is Saxon.Hej.om.NamespaceMap)) {
                    namespaces = resolver;
                } else {
                    Saxon.Hej.om.NamespaceMap map = Saxon.Hej.om.NamespaceMap.emptyMap();
                    for (System.Collections.Generic.IEnumerator<string> it = resolver.iteratePrefixes(); it.MoveNext() /*UNSAFE CONVERSION*/; ) {
                        string prefix = it.Current;
                        if (!(prefix.Equals("xml"))) {
                            map = map.put(prefix, resolver.getURIForPrefix(prefix, true));
                        }
                    }
                    namespaces = map;
                }
            }
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setPackageData(Saxon.Hej.expr.PackageData packageData) {
            this.packageData = packageData;
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return packageData;
        }
        public void setStaticBaseUriString(string baseUri) {
            if ((baseUri != null)) {
                staticBaseUriString = baseUri;
                try {
                    this.staticBaseUri = new System.Uri(baseUri, System.UriKind.RelativeOrAbsolute);
                } catch (System.UriFormatException e) {
                    staticBaseUri = null;
                }
            }
        }
        public System.Uri getStaticBaseUri() {
            if ((staticBaseUri == null)) {
                if (((staticBaseUriString == null) || staticBaseUriString.Length==0)) {
                    return null;
                } else {
                    throw new Saxon.Hej.trans.XPathException((("Supplied static base URI "+staticBaseUriString)+" is not a valid URI"));
                }
            }
            return staticBaseUri;
        }
        public string getStaticBaseUriString() {
            return staticBaseUriString;
        }
        public string getDefaultCollationName() {
            return defaultCollationName;
        }
        public void setDefaultCollationName(string defaultCollationName) {
            this.defaultCollationName = defaultCollationName;
        }
        public string getDefaultFunctionNamespace() {
            return defaultFunctionNamespace;
        }
        public void setDefaultFunctionNamespace(string defaultFunctionNamespace) {
            this.defaultFunctionNamespace = defaultFunctionNamespace;
        }
        public string getDefaultElementNamespace() {
            return ((defaultElementNamespace == null) ? "" : defaultElementNamespace);
        }
        public void setDefaultElementNamespace(string ns) {
            defaultElementNamespace = ns;
        }
        public Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager() {
            return decimalFormatManager;
        }
        public void setDecimalFormatManager(Saxon.Hej.trans.DecimalFormatManager decimalFormatManager) {
            this.decimalFormatManager = decimalFormatManager;
        }
        public bool isBackwardsCompatibility() {
            return backwardsCompatibility;
        }
        public void setBackwardsCompatibility(bool backwardsCompatibility) {
            this.backwardsCompatibility = backwardsCompatibility;
        }
        public void declareNamespace(string prefix, string uri) {
            if ((namespaces is Saxon.Hej.om.NamespaceMap)) {
                namespaces = ((Saxon.Hej.om.NamespaceMap)(namespaces)).put(prefix, uri);
            } else {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
        }
        public string getURIForPrefix(string prefix, bool useDefault) {
            return namespaces.getURIForPrefix(prefix, useDefault);
        }
        public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
            return namespaces.iteratePrefixes();
        }
        public bool declaresSameNamespaces(Saxon.Hej.expr.parser.RetainedStaticContext other) {
            return namespaces.Equals(other.namespaces);
        }
        public override int GetHashCode() {
            int h = 0x2457cbce;
            if ((staticBaseUriString != null)) {
                h ^= staticBaseUriString.GetHashCode();
            }
            h ^= defaultCollationName.GetHashCode();
            h ^= defaultFunctionNamespace.GetHashCode();
            h ^= namespaces.GetHashCode();
            return h;
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.expr.parser.RetainedStaticContext)))) {
                return false;
            }
            Saxon.Hej.expr.parser.RetainedStaticContext r = (Saxon.Hej.expr.parser.RetainedStaticContext)(other);
            return (((Saxon.Hej.expr.parser.ExpressionTool.equalOrNull(staticBaseUriString, r.staticBaseUriString) && defaultCollationName.Equals(r.defaultCollationName)) && defaultFunctionNamespace.Equals(r.defaultFunctionNamespace)) && namespaces.Equals(r.namespaces));
        }
        public void setNamespaces(Saxon.Hej.om.NamespaceResolver namespaces) {
            this.namespaces = namespaces;
        }
    
    }
}