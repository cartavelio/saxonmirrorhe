using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.sxpath {
    public class IndependentContext : Saxon.Hej.sxpath.AbstractStaticContext, Saxon.Hej.sxpath.XPathStaticContext, Saxon.Hej.om.NamespaceResolver {
        protected internal System.Collections.Generic.Dictionary<string,string> namespaces = new System.Collections.Generic.Dictionary<string, string>(10);
        protected internal System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> variables = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.sxpath.XPathVariable>(20);
        protected internal Saxon.Hej.om.NamespaceResolver externalResolver = null;
        protected internal Saxon.Hej.type.ItemType requiredContextItemType = Saxon.Hej.type.AnyItemType.getInstance();
        protected internal System.Collections.Generic.ISet<string> importedSchemaNamespaces = new System.Collections.Generic.HashSet<string>();
        protected internal bool autoDeclare = false;
        protected internal Saxon.Hej.expr.instruct.Executable executable;
        protected internal Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext;
        protected internal Saxon.Hej.expr.parser.OptimizerOptions optimizerOptions;
        protected internal bool parentlessContextItem;
        public IndependentContext() : this (new Saxon.Hej.Configuration()) {
        }
        public IndependentContext(Saxon.Hej.Configuration config)  {
            setConfiguration(config);
            clearNamespaces();
            setDefaultFunctionLibrary(31);
            usingDefaultFunctionLibrary = true;
            setDefaultCollationName(config.getDefaultCollationName());
            setOptimizerOptions(config.getOptimizerOptions());
            Saxon.Hej.expr.PackageData pd = new Saxon.Hej.expr.PackageData(config);
            pd.setHostLanguage(Saxon.Api.HostLanguage.XPATH, 31);
            pd.setSchemaAware(false);
            setPackageData(pd);
        }
        public IndependentContext(Saxon.Hej.sxpath.IndependentContext ic) : this (ic.getConfiguration()) {
            setPackageData(ic.getPackageData());
            setBaseURI(ic.getStaticBaseURI());
            setContainingLocation(ic.getContainingLocation());
            setDefaultElementNamespace(ic.getDefaultElementNamespace());
            setDefaultFunctionNamespace(ic.getDefaultFunctionNamespace());
            setBackwardsCompatibilityMode(ic.isInBackwardsCompatibleMode());
            namespaces = new System.Collections.Generic.Dictionary<string, string>(ic.namespaces);
            variables = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.sxpath.XPathVariable>(10);
            Saxon.Hej.functions.FunctionLibraryList libList = (Saxon.Hej.functions.FunctionLibraryList)(ic.getFunctionLibrary());
            if ((libList != null)) {
                setFunctionLibrary((Saxon.Hej.functions.FunctionLibraryList)(libList.copy()));
            }
            setImportedSchemaNamespaces(ic.importedSchemaNamespaces);
            externalResolver = ic.externalResolver;
            autoDeclare = ic.autoDeclare;
            setXPathLanguageLevel(ic.getXPathVersion());
            requiredContextItemType = ic.requiredContextItemType;
            setExecutable(ic.getExecutable());
        }
        public override Saxon.Hej.expr.parser.RetainedStaticContext makeRetainedStaticContext() {
            if ((retainedStaticContext == null)) {
                retainedStaticContext = new Saxon.Hej.expr.parser.RetainedStaticContext(this);
            }
            return retainedStaticContext;
        }
        public void declareNamespace(string prefix, string uri) {
            if ((prefix == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Null prefix supplied to declareNamespace()");
            }
            if ((uri == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("Null namespace URI supplied to declareNamespace()");
            }
            if ("".Equals(prefix)) {
                setDefaultElementNamespace(uri);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(namespaces, prefix, uri);
            }
        }
        public override void setDefaultElementNamespace(string uri) {
            if ((uri == null)) {
                uri = "";
            }
            base.setDefaultElementNamespace(uri);
            Saxon.Impl.Helpers.MapUtils.Put(namespaces, "", uri);
        }
        public void clearNamespaces() {
            namespaces.Clear();
            declareNamespace("xml", Saxon.Hej.lib.NamespaceConstant.XML);
            declareNamespace("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT);
            declareNamespace("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON);
            declareNamespace("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA);
            declareNamespace("", "");
        }
        public void clearAllNamespaces() {
            namespaces.Clear();
            declareNamespace("xml", Saxon.Hej.lib.NamespaceConstant.XML);
            declareNamespace("", "");
        }
        public void setNamespaces(Saxon.Hej.om.NodeInfo node) {
            namespaces.Clear();
            int kind = node.getNodeKind();
            if ((((((kind == Saxon.Hej.type.Type.ATTRIBUTE) || (kind == Saxon.Hej.type.Type.TEXT)) || (kind == Saxon.Hej.type.Type.COMMENT)) || (kind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) || (kind == Saxon.Hej.type.Type.NAMESPACE))) {
                node = node.getParent();
            }
            if ((node == null)) {
                return ;
            }
            Saxon.Hej.tree.iter.AxisIterator iter = node.iterateAxis(Saxon.Hej.om.AxisInfo.NAMESPACE);
            while (true) {
                Saxon.Hej.om.NodeInfo ns = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
                if ((ns == null)) {
                    return ;
                }
                string prefix = ns.getLocalPart();
                if ("".Equals(prefix)) {
                    setDefaultElementNamespace(ns.getStringValue());
                } else {
                    declareNamespace(ns.getLocalPart(), ns.getStringValue());
                }
            }
        }
        public void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            externalResolver = resolver;
        }
        public void setAllowUndeclaredVariables(bool allow) {
            autoDeclare = allow;
        }
        public bool isAllowUndeclaredVariables() {
            return autoDeclare;
        }
        public Saxon.Hej.sxpath.XPathVariable declareVariable(Saxon.Hej.value.QNameValue qname) {
            return declareVariable(qname.getStructuredQName());
        }
        public Saxon.Hej.sxpath.XPathVariable declareVariable(string namespaceURI, string localName) {
            Saxon.Hej.om.StructuredQName qName = new Saxon.Hej.om.StructuredQName("", namespaceURI, localName);
            return declareVariable(qName);
        }
        public Saxon.Hej.sxpath.XPathVariable declareVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.sxpath.XPathVariable var = Saxon.Impl.Helpers.MapUtils.Get(variables, qName);
            if ((var != null)) {
                return var;
            } else {
                var = Saxon.Hej.sxpath.XPathVariable.make(qName);
                int slot = variables.Count;
                var.setSlotNumber(slot);
                Saxon.Impl.Helpers.MapUtils.Put(variables, qName, var);
                return var;
            }
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.sxpath.XPathVariable> getExternalVariables() {
            return variables.Values;
        }
        public Saxon.Hej.sxpath.XPathVariable getExternalVariable(Saxon.Hej.om.StructuredQName qName) {
            return Saxon.Impl.Helpers.MapUtils.Get(variables, qName);
        }
        public int getSlotNumber(Saxon.Hej.value.QNameValue qname) {
            Saxon.Hej.om.StructuredQName sq = qname.getStructuredQName();
            Saxon.Hej.sxpath.XPathVariable var = Saxon.Impl.Helpers.MapUtils.Get(variables, sq);
            if ((var == null)) {
                return -1;
            }
            return var.getLocalSlotNumber();
        }
        public override Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            if ((externalResolver != null)) {
                return externalResolver;
            } else {
                return this;
            }
        }
        public string getURIForPrefix(string prefix, bool useDefault) {
            if ((externalResolver != null)) {
                return externalResolver.getURIForPrefix(prefix, useDefault);
            }
            if (prefix.Length==0) {
                return (useDefault ? getDefaultElementNamespace() : "");
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(namespaces, prefix);
            }
        }
        public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
            if ((externalResolver != null)) {
                return externalResolver.iteratePrefixes();
            } else {
                return Saxon.Impl.Helpers.MapUtils.KeySet(namespaces).GetEnumerator();
            }
        }
        public override Saxon.Hej.expr.Expression bindVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.sxpath.XPathVariable var = Saxon.Impl.Helpers.MapUtils.Get(variables, qName);
            if ((var == null)) {
                if (autoDeclare) {
                    return new Saxon.Hej.expr.LocalVariableReference(declareVariable(qName));
                } else {
                    throw new Saxon.Hej.trans.XPathException(("Undeclared variable in XPath expression: $"+qName.getClarkName()), "XPST0008");
                }
            } else {
                return new Saxon.Hej.expr.LocalVariableReference(var);
            }
        }
        public Saxon.Hej.expr.instruct.SlotManager getStackFrameMap() {
            Saxon.Hej.expr.instruct.SlotManager map = getConfiguration().makeSlotManager();
            Saxon.Hej.sxpath.XPathVariable[] va = new Saxon.Hej.sxpath.XPathVariable[variables.Count];
            foreach (Saxon.Hej.sxpath.XPathVariable var in variables.Values) {
                va[var.getLocalSlotNumber()] = var;
            }
            foreach (Saxon.Hej.sxpath.XPathVariable v in va) {
                map.allocateSlotNumber(v.getVariableQName());
            }
            return map;
        }
        public System.Collections.Generic.ICollection<Saxon.Hej.sxpath.XPathVariable> getDeclaredVariables() {
            return variables.Values;
        }
        public override bool isImportedSchema(string @namespace) {
            return importedSchemaNamespaces.Contains(@namespace);
        }
        public override System.Collections.Generic.ISet<string> getImportedSchemaNamespaces() {
            return importedSchemaNamespaces;
        }
        public void setImportedSchemaNamespaces(System.Collections.Generic.ISet<string> namespaces) {
            importedSchemaNamespaces = namespaces;
            if (!(namespaces.Count==0)) {
                setSchemaAware(true);
            }
        }
        public void setRequiredContextItemType(Saxon.Hej.type.ItemType type) {
            requiredContextItemType = type;
        }
        public override Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return requiredContextItemType;
        }
        public void setOptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions options) {
            this.optimizerOptions = options;
        }
        public override Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return this.optimizerOptions;
        }
        public void setExecutable(Saxon.Hej.expr.instruct.Executable exec) {
            executable = exec;
        }
        public virtual Saxon.Hej.expr.instruct.Executable getExecutable() {
            return executable;
        }
        public int getColumnNumber() {
            return -1;
        }
        public string getPublicId() {
            return null;
        }
        public int getLineNumber() {
            return -1;
        }
        public bool isContextItemParentless() {
            return parentlessContextItem;
        }
        public void setContextItemParentless(bool parentless) {
            parentlessContextItem = parentless;
        }
    
    }
}