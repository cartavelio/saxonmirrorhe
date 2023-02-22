using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class StyleNodeFactory : Saxon.Hej.tree.linked.NodeFactory {
        protected internal Saxon.Hej.Configuration config;
        protected internal Saxon.Hej.om.NamePool namePool;
        private readonly Saxon.Hej.style.Compilation compilation;
        private bool topLevelModule;
        public StyleNodeFactory(Saxon.Hej.Configuration config, Saxon.Hej.style.Compilation compilation)  {
            this.config = config;
            this.compilation = compilation;
            namePool = config.getNamePool();
        }
        public void setTopLevelModule(bool topLevelModule) {
            this.topLevelModule = topLevelModule;
        }
        public bool isTopLevelModule() {
            return topLevelModule;
        }
        public Saxon.Hej.style.Compilation getCompilation() {
            return compilation;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public virtual Saxon.Hej.tree.linked.ElementImpl makeElementNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType elemType, bool isNilled, Saxon.Hej.om.AttributeMap attlist, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.s9api.Location location, int sequence) {
            int f = elemName.obtainFingerprint(pipe.getConfiguration().getNamePool());
            bool toplevel = (parent is Saxon.Hej.style.XSLModuleRoot);
            string baseURI = location.getSystemId();
            int lineNumber = location.getLineNumber();
            int columnNumber = location.getColumnNumber();
            if ((parent is Saxon.Hej.style.DataElement)) {
                Saxon.Hej.style.DataElement d = new Saxon.Hej.style.DataElement();
                d.setNamespaceMap(namespaces);
                d.initialise(elemName, elemType, attlist, parent, sequence);
                d.setLocation(baseURI, lineNumber, columnNumber);
                return d;
            }
            Saxon.Hej.style.StyleElement e = makeXSLElement(f, (Saxon.Hej.tree.linked.NodeImpl)(parent));
            if (((((e is Saxon.Hej.style.XSLStylesheet) || (e is Saxon.Hej.style.XSLPackage))) && (parent.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT))) {
                e = new Saxon.Hej.style.AbsentExtensionElement();
                Saxon.Hej.trans.XmlProcessingIncident reason = new Saxon.Hej.trans.XmlProcessingIncident((elemName.getDisplayName()+" can only appear at the outermost level"), "XTSE0010");
                e.setValidationError(reason, Saxon.Hej.style.StyleElement.OnFailure.REPORT_ALWAYS);
            }
            if ((e != null)) {
                e.setCompilation(compilation);
                e.setNamespaceMap(namespaces);
                e.initialise(elemName, elemType, attlist, parent, sequence);
                e.setLocation(baseURI, lineNumber, columnNumber);
                e.processExtensionElementAttribute(Saxon.Hej.om.NamespaceUri.NULL);
                e.processExcludedNamespaces(Saxon.Hej.om.NamespaceUri.NULL);
                e.processVersionAttribute(Saxon.Hej.om.NamespaceUri.NULL);
                e.processDefaultXPathNamespaceAttribute(Saxon.Hej.om.NamespaceUri.NULL);
                e.processExpandTextAttribute(Saxon.Hej.om.NamespaceUri.NULL);
                e.processDefaultValidationAttribute(Saxon.Hej.om.NamespaceUri.NULL);
                if ((((toplevel && !(e.isDeclaration())) && !(((e is Saxon.Hej.style.XSLExpose)))) && e.forwardsCompatibleModeIsEnabled())) {
                    Saxon.Hej.style.DataElement d = new Saxon.Hej.style.DataElement();
                    d.setNamespaceMap(namespaces);
                    d.initialise(elemName, elemType, attlist, parent, sequence);
                    d.setLocation(baseURI, lineNumber, columnNumber);
                    return d;
                }
                if (((((parent is Saxon.Hej.style.AbsentExtensionElement) && ((Saxon.Hej.style.AbsentExtensionElement)(parent)).forwardsCompatibleModeIsEnabled()) && ((Saxon.Hej.style.AbsentExtensionElement)(parent)).isInXsltNamespace()) && !(((e is Saxon.Hej.style.XSLFallback))))) {
                    Saxon.Hej.style.AbsentExtensionElement temp = new Saxon.Hej.style.AbsentExtensionElement();
                    temp.initialise(elemName, elemType, attlist, parent, sequence);
                    temp.setLocation(baseURI, lineNumber, columnNumber);
                    temp.setCompilation(compilation);
                    temp.setIgnoreInstruction();
                    return temp;
                }
                return e;
            }
            Saxon.Hej.om.NamespaceUri uri = elemName.getNamespaceUri();
            if ((toplevel && !(uri.Equals(Saxon.Hej.om.NamespaceUri.XSLT)))) {
                Saxon.Hej.style.DataElement d = new Saxon.Hej.style.DataElement();
                d.setNamespaceMap(namespaces);
                d.initialise(elemName, elemType, attlist, parent, sequence);
                d.setLocation(baseURI, lineNumber, columnNumber);
                return d;
            } else {
                string localname = elemName.getLocalPart();
                Saxon.Hej.style.StyleElement temp = null;
                if (uri.Equals(Saxon.Hej.om.NamespaceUri.XSLT)) {
                    if ((parent is Saxon.Hej.style.XSLStylesheet)) {
                        if ((((Saxon.Hej.style.XSLStylesheet)(parent)).getEffectiveVersion()<=20)) {
                            temp = new Saxon.Hej.style.AbsentExtensionElement();
                            temp.setCompilation(compilation);
                            temp.setValidationError(new Saxon.Hej.trans.XmlProcessingIncident("Unknown top-level XSLT declaration"), Saxon.Hej.style.StyleElement.OnFailure.REPORT_UNLESS_FORWARDS_COMPATIBLE);
                        }
                    } else {
                        temp = new Saxon.Hej.style.AbsentExtensionElement();
                        temp.initialise(elemName, elemType, attlist, parent, sequence);
                        temp.setLocation(baseURI, lineNumber, columnNumber);
                        temp.setCompilation(compilation);
                        temp.processStandardAttributes(Saxon.Hej.om.NamespaceUri.NULL);
                        int effectiveVersion = temp.getEffectiveVersion();
                        Saxon.Hej.trans.XmlProcessingIncident incident = new Saxon.Hej.trans.XmlProcessingIncident(("Unknown XSLT instruction xsl:"+localname), "XTSE0010", location);
                        if ((effectiveVersion>compilation.getCompilerInfo().getXsltVersion())) {
                            temp.setValidationError(incident, Saxon.Hej.style.StyleElement.OnFailure.REPORT_STATICALLY_UNLESS_FALLBACK_AVAILABLE);
                        } else {
                            if ((effectiveVersion == 10)) {
                                temp.setValidationError(incident, Saxon.Hej.style.StyleElement.OnFailure.REPORT_IF_INSTANTIATED);
                            } else {
                                temp.setValidationError(incident, Saxon.Hej.style.StyleElement.OnFailure.REPORT_ALWAYS);
                            }
                        }
                    }
                }
                if (uri.Equals(Saxon.Hej.om.NamespaceUri.SAXON)) {
                    string message = (elemName.getDisplayName()+" is not recognized as a Saxon instruction");
                    if (config.getEditionCode().Equals("HE")) {
                        message += ". Saxon extensions require Saxon-PE or higher";
                    } else {
                        if (!(config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION))) {
                            message += ". No Saxon-PE or -EE license was found";
                        }
                    }
                    Saxon.Hej.trans.XmlProcessingIncident err = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(message, Saxon.Hej.trans.SaxonErrorCode.SXWN9008, location.saveLocation()).asWarning());
                    pipe.getErrorReporter().report(err);
                }
                bool extensionElement = isExtensionNamespace(uri, parent, namespaces, attlist);
                if ((temp == null)) {
                    if (extensionElement) {
                        temp = new Saxon.Hej.style.AbsentExtensionElement();
                    } else {
                        temp = new Saxon.Hej.style.LiteralResultElement();
                    }
                }
                temp.setNamespaceMap(namespaces);
                temp.setCompilation(compilation);
                temp.initialise(elemName, elemType, attlist, parent, sequence);
                temp.setLocation(baseURI, lineNumber, columnNumber);
                temp.processStandardAttributes(Saxon.Hej.om.NamespaceUri.XSLT);
                Saxon.Hej.trans.XmlProcessingIncident reason;
                if (uri.Equals(Saxon.Hej.om.NamespaceUri.XSLT)) {
                } else {
                    if (extensionElement) {
                        if (Saxon.Hej.om.NamespaceUri.isReserved(uri)) {
                            reason = new Saxon.Hej.trans.XmlProcessingIncident("Cannot use a reserved namespace for extension instructions", "XTSE0800");
                            temp.setValidationError(reason, Saxon.Hej.style.StyleElement.OnFailure.REPORT_ALWAYS);
                        } else {
                            reason = new Saxon.Hej.trans.XmlProcessingIncident(("Unknown extension instruction "+Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)), "XTDE1450");
                            temp.setValidationError(reason, Saxon.Hej.style.StyleElement.OnFailure.REPORT_DYNAMICALLY_UNLESS_FALLBACK_AVAILABLE);
                        }
                    }
                }
                return temp;
            }
        }
        private static bool isExtensionNamespace(Saxon.Hej.om.NamespaceUri uri, Saxon.Hej.om.NodeInfo parent, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.om.AttributeMap attlist) {
            string attValue = attlist.getValue(Saxon.Hej.om.NamespaceUri.XSLT, "extension-element-prefixes");
            if ((attValue != null)) {
                Saxon.Ejava.util.StringTokenizer st2 = new Saxon.Ejava.util.StringTokenizer(attValue, " \t\n\r", false);
                while (st2.hasMoreTokens()) {
                    string s = st2.nextToken();
                    if ("#default".Equals(s)) {
                        s = "";
                    }
                    Saxon.Hej.om.NamespaceUri ns = namespaces.getURIForPrefix(s, false);
                    if (uri.Equals(ns)) {
                        return true;
                    }
                }
            }
            return ((parent is Saxon.Hej.style.StyleElement) && ((Saxon.Hej.style.StyleElement)(parent)).isExtensionNamespace(uri));
        }
        protected internal Saxon.Hej.style.StyleElement makeXSLElement(int f, Saxon.Hej.tree.linked.NodeImpl parent) {
            switch (f) {
              case Saxon.Hej.om.StandardNames.XSL_ACCEPT:
                return new Saxon.Hej.style.XSLAccept();
              case Saxon.Hej.om.StandardNames.XSL_ACCUMULATOR:
                return new Saxon.Hej.style.XSLAccumulator();
              case Saxon.Hej.om.StandardNames.XSL_ACCUMULATOR_RULE:
                return new Saxon.Hej.style.XSLAccumulatorRule();
              case Saxon.Hej.om.StandardNames.XSL_ANALYZE_STRING:
                return new Saxon.Hej.style.XSLAnalyzeString();
              case Saxon.Hej.om.StandardNames.XSL_APPLY_IMPORTS:
                return new Saxon.Hej.style.XSLApplyImports();
              case Saxon.Hej.om.StandardNames.XSL_APPLY_TEMPLATES:
                return new Saxon.Hej.style.XSLApplyTemplates();
              case Saxon.Hej.om.StandardNames.XSL_ARRAY:
                return new Saxon.Hej.style.XSLArray();
              case Saxon.Hej.om.StandardNames.XSL_ARRAY_MEMBER:
                return new Saxon.Hej.style.XSLArrayMember();
              case Saxon.Hej.om.StandardNames.XSL_ASSERT:
                return new Saxon.Hej.style.XSLAssert();
              case Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE:
                return new Saxon.Hej.style.XSLAttribute();
              case Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET:
                return new Saxon.Hej.style.XSLAttributeSet();
              case Saxon.Hej.om.StandardNames.XSL_BREAK:
                return new Saxon.Hej.style.XSLBreak();
              case Saxon.Hej.om.StandardNames.XSL_CALL_TEMPLATE:
                return new Saxon.Hej.style.XSLCallTemplate();
              case Saxon.Hej.om.StandardNames.XSL_CATCH:
                return new Saxon.Hej.style.XSLCatch();
              case Saxon.Hej.om.StandardNames.XSL_CONTEXT_ITEM:
                return new Saxon.Hej.style.XSLContextItem();
              case Saxon.Hej.om.StandardNames.XSL_CHARACTER_MAP:
                return new Saxon.Hej.style.XSLCharacterMap();
              case Saxon.Hej.om.StandardNames.XSL_CHOOSE:
                return new Saxon.Hej.style.XSLChoose();
              case Saxon.Hej.om.StandardNames.XSL_COMMENT:
                return new Saxon.Hej.style.XSLComment();
              case Saxon.Hej.om.StandardNames.XSL_COPY:
                return new Saxon.Hej.style.XSLCopy();
              case Saxon.Hej.om.StandardNames.XSL_COPY_OF:
                return new Saxon.Hej.style.XSLCopyOf();
              case Saxon.Hej.om.StandardNames.XSL_DECIMAL_FORMAT:
                return new Saxon.Hej.style.XSLDecimalFormat();
              case Saxon.Hej.om.StandardNames.XSL_DOCUMENT:
                return new Saxon.Hej.style.XSLDocument();
              case Saxon.Hej.om.StandardNames.XSL_ELEMENT:
                return new Saxon.Hej.style.XSLElement();
              case Saxon.Hej.om.StandardNames.XSL_EVALUATE:
                return new Saxon.Hej.style.XSLEvaluate();
              case Saxon.Hej.om.StandardNames.XSL_EXPOSE:
                return new Saxon.Hej.style.XSLExpose();
              case Saxon.Hej.om.StandardNames.XSL_FALLBACK:
                return new Saxon.Hej.style.XSLFallback();
              case Saxon.Hej.om.StandardNames.XSL_FOR_EACH:
                return new Saxon.Hej.style.XSLForEach();
              case Saxon.Hej.om.StandardNames.XSL_FOR_EACH_GROUP:
                return new Saxon.Hej.style.XSLForEachGroup();
              case Saxon.Hej.om.StandardNames.XSL_FORK:
                return new Saxon.Hej.style.XSLFork();
              case Saxon.Hej.om.StandardNames.XSL_FUNCTION:
                return new Saxon.Hej.style.XSLFunction();
              case Saxon.Hej.om.StandardNames.XSL_GLOBAL_CONTEXT_ITEM:
                return new Saxon.Hej.style.XSLGlobalContextItem();
              case Saxon.Hej.om.StandardNames.XSL_IF:
                return new Saxon.Hej.style.XSLIf();
              case Saxon.Hej.om.StandardNames.XSL_IMPORT:
                return new Saxon.Hej.style.XSLImport();
              case Saxon.Hej.om.StandardNames.XSL_IMPORT_SCHEMA:
                return new Saxon.Hej.style.XSLImportSchema();
              case Saxon.Hej.om.StandardNames.XSL_INCLUDE:
                return new Saxon.Hej.style.XSLInclude();
              case Saxon.Hej.om.StandardNames.XSL_ITEM_TYPE:
                return new Saxon.Hej.style.XSLItemType();
              case Saxon.Hej.om.StandardNames.XSL_ITERATE:
                return new Saxon.Hej.style.XSLIterate();
              case Saxon.Hej.om.StandardNames.XSL_KEY:
                return new Saxon.Hej.style.XSLKey();
              case Saxon.Hej.om.StandardNames.XSL_MAP:
                return new Saxon.Hej.style.XSLMap();
              case Saxon.Hej.om.StandardNames.XSL_MAP_ENTRY:
                return new Saxon.Hej.style.XSLMapEntry();
              case Saxon.Hej.om.StandardNames.XSL_MATCHING_SUBSTRING:
                return new Saxon.Hej.style.XSLMatchingSubstring();
              case Saxon.Hej.om.StandardNames.XSL_MERGE:
                return new Saxon.Hej.style.XSLMerge();
              case Saxon.Hej.om.StandardNames.XSL_MERGE_ACTION:
                return new Saxon.Hej.style.XSLMergeAction();
              case Saxon.Hej.om.StandardNames.XSL_MERGE_KEY:
                return new Saxon.Hej.style.XSLMergeKey();
              case Saxon.Hej.om.StandardNames.XSL_MERGE_SOURCE:
                return new Saxon.Hej.style.XSLMergeSource();
              case Saxon.Hej.om.StandardNames.XSL_MESSAGE:
                return new Saxon.Hej.style.XSLMessage();
              case Saxon.Hej.om.StandardNames.XSL_MODE:
                return new Saxon.Hej.style.XSLMode();
              case Saxon.Hej.om.StandardNames.XSL_NEXT_ITERATION:
                return new Saxon.Hej.style.XSLNextIteration();
              case Saxon.Hej.om.StandardNames.XSL_NEXT_MATCH:
                return new Saxon.Hej.style.XSLNextMatch();
              case Saxon.Hej.om.StandardNames.XSL_NON_MATCHING_SUBSTRING:
                return new Saxon.Hej.style.XSLMatchingSubstring();
              case Saxon.Hej.om.StandardNames.XSL_NUMBER:
                return new Saxon.Hej.style.XSLNumber();
              case Saxon.Hej.om.StandardNames.XSL_NAMESPACE:
                return new Saxon.Hej.style.XSLNamespace();
              case Saxon.Hej.om.StandardNames.XSL_NAMESPACE_ALIAS:
                return new Saxon.Hej.style.XSLNamespaceAlias();
              case Saxon.Hej.om.StandardNames.XSL_ON_COMPLETION:
                return new Saxon.Hej.style.XSLOnCompletion();
              case Saxon.Hej.om.StandardNames.XSL_ON_EMPTY:
                return new Saxon.Hej.style.XSLOnEmpty();
              case Saxon.Hej.om.StandardNames.XSL_ON_NON_EMPTY:
                return new Saxon.Hej.style.XSLOnNonEmpty();
              case Saxon.Hej.om.StandardNames.XSL_OTHERWISE:
                return new Saxon.Hej.style.XSLOtherwise();
              case Saxon.Hej.om.StandardNames.XSL_OUTPUT:
                return new Saxon.Hej.style.XSLOutput();
              case Saxon.Hej.om.StandardNames.XSL_OUTPUT_CHARACTER:
                return new Saxon.Hej.style.XSLOutputCharacter();
              case Saxon.Hej.om.StandardNames.XSL_OVERRIDE:
                return new Saxon.Hej.style.XSLOverride();
              case Saxon.Hej.om.StandardNames.XSL_PACKAGE:
                return new Saxon.Hej.style.XSLPackage();
              case Saxon.Hej.om.StandardNames.XSL_PARAM:
                return (((parent is Saxon.Hej.style.XSLModuleRoot) || (parent is Saxon.Hej.style.XSLOverride)) ? (Saxon.Hej.style.StyleElement)(new Saxon.Hej.style.XSLGlobalParam()) : (Saxon.Hej.style.StyleElement)(new Saxon.Hej.style.XSLLocalParam()));
              case Saxon.Hej.om.StandardNames.XSL_PERFORM_SORT:
                return new Saxon.Hej.style.XSLPerformSort();
              case Saxon.Hej.om.StandardNames.XSL_PRESERVE_SPACE:
                return new Saxon.Hej.style.XSLPreserveSpace();
              case Saxon.Hej.om.StandardNames.XSL_PROCESSING_INSTRUCTION:
                return new Saxon.Hej.style.XSLProcessingInstruction();
              case Saxon.Hej.om.StandardNames.XSL_RESULT_DOCUMENT:
                compilation.setCreatesSecondaryResultDocuments(true);
                return new Saxon.Hej.style.XSLResultDocument();
              case Saxon.Hej.om.StandardNames.XSL_SEQUENCE:
                return new Saxon.Hej.style.XSLSequence();
              case Saxon.Hej.om.StandardNames.XSL_SORT:
                return new Saxon.Hej.style.XSLSort();
              case Saxon.Hej.om.StandardNames.XSL_SOURCE_DOCUMENT:
                return new Saxon.Hej.style.XSLSourceDocument();
              case Saxon.Hej.om.StandardNames.XSL_STRIP_SPACE:
                return new Saxon.Hej.style.XSLPreserveSpace();
              case Saxon.Hej.om.StandardNames.XSL_STYLESHEET:
              case Saxon.Hej.om.StandardNames.XSL_TRANSFORM:
                return (topLevelModule ? (Saxon.Hej.style.StyleElement)(new Saxon.Hej.style.XSLPackage()) : (Saxon.Hej.style.StyleElement)(new Saxon.Hej.style.XSLStylesheet()));
              case Saxon.Hej.om.StandardNames.XSL_SWITCH:
                return new Saxon.Hej.style.XSLSwitch();
              case Saxon.Hej.om.StandardNames.XSL_TEMPLATE:
                return new Saxon.Hej.style.XSLTemplate();
              case Saxon.Hej.om.StandardNames.XSL_TEXT:
                return new Saxon.Hej.style.XSLText();
              case Saxon.Hej.om.StandardNames.XSL_TRY:
                return new Saxon.Hej.style.XSLTry();
              case Saxon.Hej.om.StandardNames.XSL_USE_PACKAGE:
                return new Saxon.Hej.style.XSLUsePackage();
              case Saxon.Hej.om.StandardNames.XSL_VALUE_OF:
                return new Saxon.Hej.style.XSLValueOf();
              case Saxon.Hej.om.StandardNames.XSL_VARIABLE:
                return (((parent is Saxon.Hej.style.XSLModuleRoot) || (parent is Saxon.Hej.style.XSLOverride)) ? (Saxon.Hej.style.StyleElement)(new Saxon.Hej.style.XSLGlobalVariable()) : (Saxon.Hej.style.StyleElement)(new Saxon.Hej.style.XSLLocalVariable()));
              case Saxon.Hej.om.StandardNames.XSL_WITH_PARAM:
                return new Saxon.Hej.style.XSLWithParam();
              case Saxon.Hej.om.StandardNames.XSL_WHEN:
                return new Saxon.Hej.style.XSLWhen();
              case Saxon.Hej.om.StandardNames.XSL_WHERE_POPULATED:
                return new Saxon.Hej.style.XSLWherePopulated();
              default:
                return null;
            }
        }
        public Saxon.Hej.tree.linked.TextImpl makeTextNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.str.UnicodeString content) {
            if (((parent is Saxon.Hej.style.StyleElement) && ((Saxon.Hej.style.StyleElement)(parent)).isExpandingText())) {
                return new Saxon.Hej.style.TextValueTemplateNode(content);
            } else {
                return new Saxon.Hej.tree.linked.TextImpl(content);
            }
        }
        public virtual bool isElementAvailable(Saxon.Hej.om.NamespaceUri uri, string localName, bool instructionsOnly) {
            int fingerprint = namePool.getFingerprint(uri, localName);
            if (uri.Equals(Saxon.Hej.om.NamespaceUri.XSLT)) {
                if ((fingerprint == -1)) {
                    return false;
                }
                Saxon.Hej.style.StyleElement e = makeXSLElement(fingerprint, null);
                if ((e != null)) {
                    return (!(instructionsOnly) || e.isInstruction());
                }
            }
            return false;
        }
        public virtual Saxon.Hej.expr.accum.AccumulatorRegistry makeAccumulatorManager() {
            return new Saxon.Hej.expr.accum.AccumulatorRegistry();
        }
        public virtual Saxon.Hej.style.PrincipalStylesheetModule newPrincipalModule(Saxon.Hej.style.XSLPackage node) {
            return new Saxon.Hej.style.PrincipalStylesheetModule(node);
        }
    
    }
}