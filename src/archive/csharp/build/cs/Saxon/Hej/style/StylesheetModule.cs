using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class StylesheetModule {
        private readonly Saxon.Hej.style.StyleElement rootElement;
        private int precedence;
        private int minImportPrecedence;
        private Saxon.Hej.style.StylesheetModule importer;
        internal bool wasIncluded;
        private int inputTypeAnnotations = 0;
        protected internal System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> topLevel = new System.Collections.Generic.List<Saxon.Hej.style.ComponentDeclaration>();
        public StylesheetModule(Saxon.Hej.style.StyleElement rootElement, int precedence)  {
            this.rootElement = rootElement;
            this.precedence = precedence;
        }
        public static Saxon.Hej.tree.linked.DocumentImpl loadStylesheetModule(Saxon.Ejavax.xml.transform.Source styleSource, bool topLevelModule, Saxon.Hej.style.Compilation compilation, Saxon.Hej.value.NestedIntegerValue precedence) {
            string systemId = styleSource.getSystemId();
            Saxon.Hej.om.DocumentKey docURI = ((systemId == null) ? null : new Saxon.Hej.om.DocumentKey(systemId));
            if (((systemId != null) && compilation.getImportStack().Contains(docURI))) {
                throw new Saxon.Hej.trans.XPathException("The stylesheet module includes/imports itself directly or indirectly", "XTSE0180");
            }
            compilation.getImportStack().Push(docURI);
            Saxon.Hej.Configuration config = compilation.getConfiguration();
            Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setErrorReporter(compilation.getCompilerInfo().getErrorReporter());
            Saxon.Hej.tree.linked.LinkedTreeBuilder styleBuilder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(pipe);
            styleBuilder.setSystemId(styleSource.getSystemId());
            styleBuilder.setNodeFactory(compilation.getStyleNodeFactory(topLevelModule));
            styleBuilder.setLineNumbering(true);
            Saxon.Hej.style.UseWhenFilter useWhenFilter = new Saxon.Hej.style.UseWhenFilter(compilation, styleBuilder, precedence);
            useWhenFilter.setSystemId(styleSource.getSystemId());
            Saxon.Hej.om.StylesheetSpaceStrippingRule rule = new Saxon.Hej.om.StylesheetSpaceStrippingRule(config.getNamePool());
            Saxon.Hej.@event.Stripper styleStripper = new Saxon.Hej.@event.Stripper(rule, useWhenFilter);
            Saxon.Hej.@event.CommentStripper commentStripper = new Saxon.Hej.@event.CommentStripper(styleStripper);
            Saxon.Hej.tree.linked.DocumentImpl doc;
            Saxon.Hej.lib.ParseOptions options = makeStylesheetParseOptions(styleSource, pipe);
            try {
                sendStylesheetSource(styleSource, config, commentStripper, options);
                doc = (Saxon.Hej.tree.linked.DocumentImpl)(styleBuilder.getCurrentRoot());
                styleBuilder.reset();
                compilation.getImportStack().Pop();
                return doc;
            } catch (Saxon.Hej.trans.XPathException err) {
                if ((topLevelModule && !(err.hasBeenReported()))) {
                    compilation.reportError(err);
                }
                throw;
            } finally {
                if (options.isPleaseCloseAfterUse()) {
                    Saxon.Hej.lib.ParseOptions.close(styleSource);
                }
            }
        }
        private static Saxon.Hej.lib.ParseOptions makeStylesheetParseOptions(Saxon.Ejavax.xml.transform.Source styleSource, Saxon.Hej.@event.PipelineConfiguration pipe) {
            Saxon.Hej.lib.ParseOptions options;
            if ((styleSource is Saxon.Hej.lib.AugmentedSource)) {
                options = ((Saxon.Hej.lib.AugmentedSource)(styleSource)).getParseOptions();
            } else {
                options = new Saxon.Hej.lib.ParseOptions();
            }
            options.setSchemaValidationMode(Saxon.Hej.lib.Validation.STRIP);
            options.setDTDValidationMode(Saxon.Hej.lib.Validation.STRIP);
            options.setLineNumbering(true);
            options.setSpaceStrippingRule(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
            options.setErrorReporter(pipe.getErrorReporter());
            return options;
        }
        private static void sendStylesheetSource(Saxon.Ejavax.xml.transform.Source styleSource, Saxon.Hej.Configuration config, Saxon.Hej.@event.Receiver sourcePipeline, Saxon.Hej.lib.ParseOptions options) {
            Saxon.Hej.@event.Sender.send(styleSource, sourcePipeline, options);
        }
        public static Saxon.Hej.PreparedStylesheet loadStylesheet(Saxon.Ejavax.xml.transform.Source styleSource, Saxon.Hej.style.Compilation compilation) {
            string systemId = styleSource.getSystemId();
            Saxon.Hej.om.DocumentKey docURI = ((systemId == null) ? null : new Saxon.Hej.om.DocumentKey(systemId));
            if (((systemId != null) && compilation.getImportStack().Contains(docURI))) {
                throw new Saxon.Hej.trans.XPathException("The stylesheet module includes/imports itself directly or indirectly", "XTSE0180");
            }
            compilation.getImportStack().Push(docURI);
            compilation.setMinimalPackageData();
            Saxon.Hej.Configuration config = compilation.getConfiguration();
            Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setErrorReporter(compilation.getCompilerInfo().getErrorReporter());
            Saxon.Hej.tree.linked.LinkedTreeBuilder styleBuilder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(pipe);
            styleBuilder.setSystemId(styleSource.getSystemId());
            styleBuilder.setNodeFactory(compilation.getStyleNodeFactory(true));
            styleBuilder.setLineNumbering(true);
            Saxon.Hej.@event.Receiver sourcePipeline;
            Saxon.Hej.style.UseWhenFilter useWhenFilter = new Saxon.Hej.style.UseWhenFilter(compilation, styleBuilder, Saxon.Hej.value.NestedIntegerValue.TWO);
            useWhenFilter.setSystemId(styleSource.getSystemId());
            Saxon.Hej.om.StylesheetSpaceStrippingRule rule = new Saxon.Hej.om.StylesheetSpaceStrippingRule(config.getNamePool());
            Saxon.Hej.@event.Stripper styleStripper = new Saxon.Hej.@event.Stripper(rule, useWhenFilter);
            Saxon.Hej.@event.CommentStripper commentStripper = new Saxon.Hej.@event.CommentStripper(styleStripper);
            Saxon.Hej.tree.tiny.TinyBuilder packageBuilder = new Saxon.Hej.tree.tiny.TinyBuilder(pipe);
            packageBuilder.setSystemId(styleSource.getSystemId());
            Saxon.Hej.@event.CheckSumFilter checksummer = new Saxon.Hej.@event.CheckSumFilter(packageBuilder);
            checksummer.setCheckExistingChecksum(true);
            Saxon.Hej.@event.Valve valve = new Saxon.Hej.@event.Valve(Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, commentStripper, checksummer);
            sourcePipeline = valve;
            Saxon.Hej.lib.ParseOptions options = makeStylesheetParseOptions(styleSource, pipe);
            try {
                sendStylesheetSource(styleSource, config, sourcePipeline, options);
                Saxon.Hej.om.NodeInfo doc;
                if (valve.wasDiverted()) {
                    if (!(checksummer.isChecksumCorrect())) {
                        throw new Saxon.Hej.trans.XPathException("Compiled package cannot be loaded: incorrect checksum");
                    }
                    Saxon.Hej.trans.packages.IPackageLoader loader = config.makePackageLoader();
                    Saxon.Hej.style.StylesheetPackage pack = loader.loadPackageDoc(packageBuilder.getCurrentRoot());
                    compilation.setPackageData(pack);
                    Saxon.Hej.PreparedStylesheet pss = new Saxon.Hej.PreparedStylesheet(compilation);
                    pack.checkForAbstractComponents();
                    pack.updatePreparedStylesheet(pss);
                    return pss;
                } else {
                    doc = styleBuilder.getCurrentRoot();
                    styleBuilder.reset();
                    compilation.getImportStack().Pop();
                    Saxon.Hej.PreparedStylesheet pss = new Saxon.Hej.PreparedStylesheet(compilation);
                    Saxon.Hej.style.PrincipalStylesheetModule psm = compilation.compilePackage(doc.asActiveSource());
                    if ((compilation.getErrorCount()>0)) {
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Errors were reported during stylesheet compilation");
                        e.setHasBeenReported(true);
                        throw e;
                    }
                    psm.getStylesheetPackage().checkForAbstractComponents();
                    psm.getStylesheetPackage().updatePreparedStylesheet(pss);
                    pss.addPackage(compilation.getPackageData());
                    return pss;
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                if (!(err.hasBeenReported())) {
                    compilation.reportError(err);
                }
                throw;
            } finally {
                if (options.isPleaseCloseAfterUse()) {
                    Saxon.Hej.lib.ParseOptions.close(styleSource);
                }
            }
        }
        private static Saxon.Ejavax.xml.transform.Source compositeStylesheet(Saxon.Hej.Configuration config, string baseURI, Saxon.Ejavax.xml.transform.Source[] sources) {
            return sources[0]; // TODO: either implement this feature, or pull it completely
        }
        public void setImporter(Saxon.Hej.style.StylesheetModule importer) {
            this.importer = importer;
        }
        public Saxon.Hej.style.StylesheetModule getImporter() {
            return importer;
        }
        public virtual Saxon.Hej.style.PrincipalStylesheetModule getPrincipalStylesheetModule() {
            return importer.getPrincipalStylesheetModule();
        }
        public Saxon.Hej.style.StyleElement getRootElement() {
            return rootElement;
        }
        public Saxon.Hej.style.XSLModuleRoot getStylesheetElement() {
            return (Saxon.Hej.style.XSLModuleRoot)(rootElement);
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return rootElement.getConfiguration();
        }
        public int getPrecedence() {
            return (wasIncluded ? importer.getPrecedence() : precedence);
        }
        public void setWasIncluded() {
            wasIncluded = true;
        }
        public void setMinImportPrecedence(int min) {
            this.minImportPrecedence = min;
        }
        public int getMinImportPrecedence() {
            return this.minImportPrecedence;
        }
        public void spliceIncludes() {
            if (((topLevel == null) || (topLevel.Count == 0))) {
                topLevel = new System.Collections.Generic.List<Saxon.Hej.style.ComponentDeclaration>(50);
            }
            minImportPrecedence = precedence;
            Saxon.Hej.style.StyleElement previousElement = rootElement;
            foreach (Saxon.Hej.om.NodeInfo child in getStylesheetElement().children()) {
                if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                    if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                        previousElement.compileError("No character data is allowed between top-level elements", "XTSE0120");
                    }
                } else {
                    if ((child is Saxon.Hej.style.DataElement)) {
                        if (((Saxon.Hej.style.DataElement)(child)).getNodeName().getURI().Length==0) {
                            Saxon.Hej.expr.parser.Loc loc = new Saxon.Hej.expr.parser.Loc(child);
                            previousElement.compileError((("Top-level elements must be in a namespace: "+((Saxon.Hej.style.DataElement)(child)).getNodeName().getLocalPart())+" is not"), "XTSE0130", loc);
                        }
                    } else {
                        previousElement = (Saxon.Hej.style.StyleElement)(child);
                        if ((child is Saxon.Hej.style.XSLGeneralIncorporate)) {
                            Saxon.Hej.style.XSLGeneralIncorporate xslinc = (Saxon.Hej.style.XSLGeneralIncorporate)(child);
                            xslinc.processAttributes();
                            xslinc.validateInstruction();
                            int errors = ((Saxon.Hej.style.XSLGeneralIncorporate)(child)).getCompilation().getErrorCount();
                            Saxon.Hej.style.StylesheetModule inc = xslinc.getIncludedStylesheet(this, precedence);
                            if ((inc == null)) {
                                return ;
                            }
                            errors = (((Saxon.Hej.style.XSLGeneralIncorporate)(child)).getCompilation().getErrorCount()-errors);
                            if ((errors>0)) {
                                xslinc.compileError(((((("Reported "+errors)+(((errors == 1) ? " error" : " errors")))+" in ")+((xslinc.isImport() ? "imported" : "included")))+" stylesheet module"), "XTSE0165");
                            }
                            if (xslinc.isImport()) {
                                precedence = (inc.getPrecedence()+1);
                            } else {
                                precedence = inc.getPrecedence();
                                inc.setMinImportPrecedence(minImportPrecedence);
                                inc.setWasIncluded();
                            }
                            System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> incchildren = inc.topLevel;
                            foreach (Saxon.Hej.style.ComponentDeclaration decl in incchildren) {
                                int last = (topLevel.Count-1);
                                if (((last<0) || (decl.getPrecedence()>=topLevel[last].getPrecedence()))) {
                                    topLevel.Add(decl);
                                } else {
                                    while (((last>=0) && (decl.getPrecedence()<topLevel[last].getPrecedence()))) {
                                        last--;
                                    }
                                    topLevel.Insert((last+1), decl);
                                }
                            }
                        } else {
                            Saxon.Hej.style.ComponentDeclaration decl = new Saxon.Hej.style.ComponentDeclaration(this, (Saxon.Hej.style.StyleElement)(child));
                            topLevel.Add(decl);
                        }
                    }
                }
            }
        }
        public int getInputTypeAnnotations() {
            return inputTypeAnnotations;
        }
        public void setInputTypeAnnotations(int annotations) {
            inputTypeAnnotations |= annotations;
            if ((inputTypeAnnotations == ((Saxon.Hej.style.XSLModuleRoot.ANNOTATION_STRIP|Saxon.Hej.style.XSLModuleRoot.ANNOTATION_PRESERVE)))) {
                getPrincipalStylesheetModule().compileError(("One stylesheet module specifies input-type-annotations='strip', "+"another specifies input-type-annotations='preserve'"), "XTSE0265");
            }
            if ((annotations == Saxon.Hej.style.XSLModuleRoot.ANNOTATION_STRIP)) {
                getPrincipalStylesheetModule().getStylesheetPackage().setStripsTypeAnnotations(true);
            }
        }
    
    }
}