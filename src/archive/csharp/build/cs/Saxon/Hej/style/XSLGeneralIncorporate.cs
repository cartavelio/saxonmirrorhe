using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class XSLGeneralIncorporate : Saxon.Hej.style.StyleElement {
        private string href;
        private Saxon.Hej.tree.linked.DocumentImpl targetDoc;
        public override bool isDeclaration() {
            return true;
        }
        public abstract bool isImport();
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("href")) {
                    href = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
            if ((href == null)) {
                reportAbsence("href");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            validateInstruction();
        }
        public void validateInstruction() {
            checkEmpty();
            checkTopLevel((isImport() ? "XTSE0190" : "XTSE0170"), false);
        }
        public Saxon.Hej.style.StylesheetModule getIncludedStylesheet(Saxon.Hej.style.StylesheetModule importer, int precedence) {
            if ((href == null)) {
                return null;
            }
            try {
                Saxon.Hej.style.PrincipalStylesheetModule psm = importer.getPrincipalStylesheetModule();
                Saxon.Ejavax.xml.transform.URIResolver resolver = getCompilation().getCompilerInfo().getURIResolver();
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Hej.style.XSLStylesheet includedSheet;
                Saxon.Hej.style.StylesheetModule incModule;
                Saxon.Hej.om.DocumentKey key = Saxon.Hej.functions.DocumentFn.computeDocumentKey(href, getBaseURI(), getCompilation().getPackageData(), resolver, false);
                includedSheet = (Saxon.Hej.style.XSLStylesheet)(psm.getStylesheetDocument(key));
                if ((includedSheet != null)) {
                    incModule = new Saxon.Hej.style.StylesheetModule(includedSheet, precedence);
                    incModule.setImporter(importer);
                    if (checkForRecursion(importer, incModule.getRootElement().asActiveSource())) {
                        return null;
                    }
                } else {
                    Saxon.Hej.tree.linked.DocumentImpl includedDoc = targetDoc;
                    System.Diagnostics.Debug.Assert((includedDoc != null));
                    Saxon.Hej.tree.linked.ElementImpl outermost = includedDoc.getDocumentElement();
                    if ((outermost is Saxon.Hej.style.LiteralResultElement)) {
                        includedDoc = ((Saxon.Hej.style.LiteralResultElement)(outermost)).makeStylesheet(false);
                        outermost = includedDoc.getDocumentElement();
                    }
                    if (!(((outermost is Saxon.Hej.style.XSLStylesheet)))) {
                        string verb = ((this is Saxon.Hej.style.XSLImport) ? "Imported" : "Included");
                        compileError((((verb+" document ")+href)+" is not a stylesheet"), "XTSE0165");
                        return null;
                    }
                    includedSheet = (Saxon.Hej.style.XSLStylesheet)(outermost);
                    psm.putStylesheetDocument(key, includedSheet);
                    incModule = new Saxon.Hej.style.StylesheetModule(includedSheet, precedence);
                    incModule.setImporter(importer);
                    Saxon.Hej.style.ComponentDeclaration decl = new Saxon.Hej.style.ComponentDeclaration(incModule, includedSheet);
                    includedSheet.validate(decl);
                    if ((includedSheet.validationError != null)) {
                        if ((reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.REPORT_ALWAYS)) {
                            includedSheet.compileError(includedSheet.validationError);
                        } else {
                            if ((includedSheet.reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.REPORT_UNLESS_FORWARDS_COMPATIBLE)) {
                                includedSheet.compileError(includedSheet.validationError);
                            }
                        }
                    }
                }
                incModule.spliceIncludes();
                importer.setInputTypeAnnotations((includedSheet.getInputTypeAnnotationsAttribute()|incModule.getInputTypeAnnotations()));
                return incModule;
            } catch (Saxon.Hej.trans.XPathException err) {
                err.setErrorCode("XTSE0165");
                err.setIsStaticError(true);
                compileError(err);
                return null;
            }
        }
        public void setTargetDocument(Saxon.Hej.tree.linked.DocumentImpl doc) {
            this.targetDoc = doc;
        }
        private bool checkForRecursion(Saxon.Hej.style.StylesheetModule importer, Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Hej.style.StylesheetModule anc = importer;
            if ((source.getSystemId() != null)) {
                while ((anc != null)) {
                    if (Saxon.Hej.om.DocumentKey.normalizeURI(source.getSystemId()).Equals(Saxon.Hej.om.DocumentKey.normalizeURI(anc.getRootElement().getSystemId()))) {
                        compileError((("A stylesheet cannot "+getLocalPart())+" itself"), ((this is Saxon.Hej.style.XSLInclude) ? "XTSE0180" : "XTSE0210"));
                        return true;
                    }
                    anc = anc.getImporter();
                }
            }
            return false;
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
        }
    
    }
}