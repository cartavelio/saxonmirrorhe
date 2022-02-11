using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class ExpressionPresenter {
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.@event.Receiver receiver;
        private Saxon.Hej.@event.ComplexContentOutputter cco;
        private int depth = 0;
        private bool inStartTag = false;
        private string nextRole = null;
        private readonly System.Collections.Generic.Stack<Saxon.Hej.expr.Expression> expressionStack = new System.Collections.Generic.Stack<Saxon.Hej.expr.Expression>();
        private readonly System.Collections.Generic.Stack<string> nameStack = new System.Collections.Generic.Stack<string>();
        private Saxon.Hej.om.NamespaceMap namespaceMap = Saxon.Hej.om.NamespaceMap.emptyMap();
        private string defaultNamespace;
        private Saxon.Hej.trace.ExpressionPresenter.Options options = new Saxon.Hej.trace.ExpressionPresenter.ExportOptions();
        private bool relocatable = false;
        public ExpressionPresenter()  {
        }
        public ExpressionPresenter(Saxon.Hej.Configuration config) : this (config, config.getLogger()) {
        }
        public ExpressionPresenter(Saxon.Hej.Configuration config, Saxon.Ejavax.xml.transform.stream.StreamResult @out) : this (config, @out, false) {
        }
        public ExpressionPresenter(Saxon.Hej.Configuration config, Saxon.Ejavax.xml.transform.stream.StreamResult @out, bool checksum)  {
            init(config, @out, checksum);
        }
        public void init(Saxon.Hej.Configuration config, Saxon.Ejavax.xml.transform.stream.StreamResult @out, bool checksum) {
            Saxon.Hej.serialize.SerializationProperties props = makeDefaultProperties(config);
            if ((config.getXMLVersion() == Saxon.Hej.Configuration.XML11)) {
                if ("JS".Equals(((Saxon.Hej.trace.ExpressionPresenter.ExportOptions)(getOptions())).target)) {
                    config.getLogger().warning("For target=JS, the SEF file will use XML 1.0, which disallows control characters");
                } else {
                    props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.VERSION, "1.1");
                }
            }
            try {
                receiver = config.getSerializerFactory().getReceiver(@out, props);
                receiver = new Saxon.Hej.@event.NamespaceReducer(receiver);
                if (checksum) {
                    receiver = new Saxon.Hej.@event.CheckSumFilter(receiver);
                }
                cco = new Saxon.Hej.@event.ComplexContentOutputter(receiver);
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.InternalError(err.Message);
            }
            this.config = config;
            try {
                cco.open();
                cco.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.InternalError(err.Message);
            }
        }
        public void init(Saxon.Hej.Configuration config, Saxon.Hej.@event.Receiver @out, bool checksum) {
            receiver = @out;
            receiver = new Saxon.Hej.@event.NamespaceReducer(receiver);
            if (checksum) {
                receiver = new Saxon.Hej.@event.CheckSumFilter(receiver);
            }
            cco = new Saxon.Hej.@event.ComplexContentOutputter(receiver);
            this.config = config;
            try {
                cco.open();
                cco.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.InternalError(err.Message);
            }
        }
        public ExpressionPresenter(Saxon.Hej.Configuration config, Saxon.Hej.lib.Logger @out) : this (config, @out.asStreamResult()) {
        }
        public ExpressionPresenter(Saxon.Hej.Configuration config, Saxon.Hej.@event.Receiver receiver)  {
            this.config = config;
            this.receiver = receiver;
            this.cco = new Saxon.Hej.@event.ComplexContentOutputter(receiver);
            try {
                cco.open();
                cco.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.InternalError(err.Message);
            }
        }
        public void setDefaultNamespace(string @namespace) {
            defaultNamespace = @namespace;
            namespaceMap = namespaceMap.put("", @namespace);
        }
        public void setOptions(Saxon.Hej.trace.ExpressionPresenter.Options options) {
            this.options = options;
        }
        public Saxon.Hej.trace.ExpressionPresenter.Options getOptions() {
            return options;
        }
        public bool isRelocatable() {
            return relocatable;
        }
        public void setRelocatable(bool relocatable) {
            this.relocatable = relocatable;
        }
        public static Saxon.Hej.@event.Receiver defaultDestination(Saxon.Hej.Configuration config, Saxon.Hej.lib.Logger @out) {
            Saxon.Hej.serialize.SerializationProperties props = makeDefaultProperties(config);
            return config.getSerializerFactory().getReceiver(@out.asStreamResult(), props);
        }
        public static Saxon.Hej.serialize.SerializationProperties makeDefaultProperties(Saxon.Hej.Configuration config) {
            Saxon.Hej.serialize.SerializationProperties props = new Saxon.Hej.serialize.SerializationProperties();
            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, "xml");
            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT, "yes");
            if (config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.INDENT_SPACES, "1");
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH, "4096");
            }
            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "no");
            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING, "utf-8");
            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.VERSION, "1.0");
            props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.SINGLE_QUOTES, "yes");
            return props;
        }
        public int startElement(string name, Saxon.Hej.expr.Expression expr) {
            Saxon.Hej.expr.Expression parent = ((expressionStack.Count==0) ? null : expressionStack.Peek());
            expressionStack.Push(expr);
            nameStack.Push(("*"+name));
            int n = _startElement(name);
            if (((parent == null) || (expr.getRetainedStaticContext() != parent.getRetainedStaticContext()))) {
                if ((expr.getRetainedStaticContext() == null)) {
                } else {
                    emitRetainedStaticContext(expr.getRetainedStaticContext(), ((parent == null) ? null : parent.getRetainedStaticContext()));
                }
            }
            string mod = expr.getLocation().getSystemId();
            if ((((mod != null) && (parent != null)) && (((parent.getLocation().getSystemId() == null) || !(parent.getLocation().getSystemId().Equals(mod)))))) {
                emitAttribute("module", truncatedModuleName(mod));
            }
            int lineNr = expr.getLocation().getLineNumber();
            if (((parent == null) || (((parent.getLocation().getLineNumber() != lineNr) && (lineNr != -1))))) {
                emitAttribute("line", (lineNr+""));
            }
            return n;
        }
        private string truncatedModuleName(string module) {
            if (!(relocatable)) {
                return module;
            } else {
                string[] parts = System.Text.RegularExpressions.Regex.Split(module, "/");
                for (int p = (parts.Length-1); (p>=0); p--) {
                    if (!(parts[p].Length==0)) {
                        return parts[p];
                    }
                }
                return module;
            }
        }
        public void emitRetainedStaticContext(Saxon.Hej.expr.parser.RetainedStaticContext sc, Saxon.Hej.expr.parser.RetainedStaticContext parentSC) {
            try {
                if ((((!(((Saxon.Hej.trace.ExpressionPresenter.ExportOptions)(options)).suppressStaticContext) && !(relocatable)) && (sc.getStaticBaseUri() != null)) && (((parentSC == null) || !(sc.getStaticBaseUri().Equals(parentSC.getStaticBaseUri())))))) {
                    emitAttribute("baseUri", sc.getStaticBaseUriString());
                }
                if ((!(sc.getDefaultCollationName().Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI)) && (((parentSC == null) || !(sc.getDefaultCollationName().Equals(parentSC.getDefaultCollationName())))))) {
                    emitAttribute("defaultCollation", sc.getDefaultCollationName());
                }
                if ((!(sc.getDefaultElementNamespace().Length==0) && (((parentSC == null) || !(sc.getDefaultElementNamespace().Equals(parentSC.getDefaultElementNamespace())))))) {
                    emitAttribute("defaultElementNS", sc.getDefaultElementNamespace());
                }
                if (!(Saxon.Hej.lib.NamespaceConstant.FN.Equals(sc.getDefaultFunctionNamespace()))) {
                    emitAttribute("defaultFunctionNS", sc.getDefaultFunctionNamespace());
                }
                if ((!(((Saxon.Hej.trace.ExpressionPresenter.ExportOptions)(options)).suppressStaticContext) && (((parentSC == null) || !(sc.declaresSameNamespaces(parentSC)))))) {
                    Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
                    for (System.Collections.Generic.IEnumerator<string> iter = sc.iteratePrefixes(); iter.MoveNext() /*UNSAFE CONVERSION*/; ) {
                        string p = iter.Current;
                        string uri = sc.getURIForPrefix(p, true);
                        ub.append(p);
                        ub.append("=");
                        if (Saxon.Hej.value.Whitespace.containsWhitespace(Saxon.Hej.str.StringTool.codePoints(uri))) {
                            throw new Saxon.Hej.trans.XPathException((("Cannot export a stylesheet if namespaces contain whitespace: '"+uri)+"'"));
                        }
                        if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.getUriForConventionalPrefix(p))) {
                            uri = "~";
                        }
                        ub.append(uri);
                        ub.append(" ");
                    }
                    emitAttribute("ns", Saxon.Hej.value.Whitespace.trim(ub.toUnicodeString()).ToString());
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
        public int startElement(string name) {
            nameStack.Push(name);
            return _startElement(name);
        }
        private int _startElement(string name) {
            try {
                if (inStartTag) {
                    cco.startContent();
                    inStartTag = false;
                }
                Saxon.Hej.om.NodeName nodeName;
                if ((defaultNamespace == null)) {
                    nodeName = new Saxon.Hej.om.NoNamespaceName(name);
                } else {
                    nodeName = new Saxon.Hej.om.FingerprintedQName("", defaultNamespace, name);
                }
                cco.startElement(nodeName, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                if ((nextRole != null)) {
                    emitAttribute("role", nextRole);
                    nextRole = null;
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.InternalError(err.Message);
            }
            inStartTag = true;
            return depth++;
        }
        public void setChildRole(string role) {
            nextRole = role;
        }
        public void emitAttribute(string name, string value) {
            if ((value != null)) {
                if (name.Equals("module")) {
                    value = truncatedModuleName(value);
                }
                try {
                    cco.attribute(new Saxon.Hej.om.NoNamespaceName(name), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, value, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                } catch (Saxon.Hej.trans.XPathException err) {
                    System.Console.Error.WriteLine(err);
                    throw new Saxon.Ejava.lang.InternalError(err.Message);
                }
            }
        }
        public void emitAttribute(string name, Saxon.Hej.om.StructuredQName value) {
            string attVal = value.getEQName();
            try {
                cco.attribute(new Saxon.Hej.om.NoNamespaceName(name), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, attVal, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.InternalError(err.Message);
            }
        }
        public void @namespace(string prefix, string uri) {
            try {
                cco.@namespace(prefix, uri, Saxon.Hej.@event.ReceiverOption.NONE);
            } catch (Saxon.Hej.trans.XPathException e) {
                System.Console.Error.WriteLine(e);
                throw new Saxon.Ejava.lang.InternalError(e.Message);
            }
        }
        public int endElement() {
            try {
                if (inStartTag) {
                    cco.startContent();
                    inStartTag = false;
                }
                cco.endElement();
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.InternalError(err.Message);
            }
            string name = nameStack.Pop();
            if (name.StartsWith("*")) {
                expressionStack.Pop();
            }
            return --depth;
        }
        public void startSubsidiaryElement(string name) {
            startElement(name);
        }
        public void endSubsidiaryElement() {
            endElement();
        }
        public void close() {
            try {
                if ((receiver is Saxon.Hej.@event.CheckSumFilter)) {
                    int c = ((Saxon.Hej.@event.CheckSumFilter)(receiver)).getChecksum();
                    cco.processingInstruction(Saxon.Hej.@event.CheckSumFilter.SIGMA, Saxon.Hej.str.BMPString.of(Saxon.Impl.Helpers.Utils.intToHexString(c)), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                }
                cco.endDocument();
                cco.close();
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.InternalError(err.Message);
            }
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return config.getNamePool();
        }
        public Saxon.Hej.type.TypeHierarchy getTypeHierarchy() {
            return config.getTypeHierarchy();
        }
        public static string jsEscape(string @in) {
            Saxon.Ejava.lang.StringBuilder @out = new Saxon.Ejava.lang.StringBuilder(@in.Length);
            for (int i = 0; (i<@in.Length); i++) {
                char c = @in[i];
                switch (c) {
                  case '\'':
                    @out.append("\\'");
                    break ;
                  case '"':
                    @out.append("\\\"");
                    break ;
                  case '\b':
                    @out.append("\\b");
                    break ;
                  case '\f':
                    @out.append("\\f");
                    break ;
                  case '\n':
                    @out.append("\\n");
                    break ;
                  case '\r':
                    @out.append("\\r");
                    break ;
                  case '\t':
                    @out.append("\\t");
                    break ;
                  case '\\':
                    @out.append("\\\\");
                    break ;
                  default:
                    if ((((c<32) || (((c>127) && (c<160)))) || (c>Saxon.Hej.serialize.charcode.UTF16CharacterSet.SURROGATE1_MIN))) {
                        @out.append("\\u");
                        Saxon.Ejava.lang.StringBuilder hex = new Saxon.Ejava.lang.StringBuilder(Saxon.Impl.Helpers.Utils.intToHexString(c).ToUpper());
                        while ((hex.length()<4)) {
                            hex.insert(0, "0");
                        }
                        @out.append(hex.ToString());
                    } else {
                        @out.append(c);
                    }
                    break ;
                }
            }
            return @out.ToString();
        }
        public interface Options {
        
        }
        public class ExportOptions : Saxon.Hej.trace.ExpressionPresenter.Options {
            public string target = "";
            public int targetVersion = 0;
            public Saxon.Hej.style.StylesheetPackage rootPackage;
            public System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentMap;
            public System.Collections.Generic.IDictionary<Saxon.Hej.style.StylesheetPackage,int> packageMap;
            public bool explaining;
            public bool suppressStaticContext;
            public bool addStaticType;
        
        }
    
    }
}