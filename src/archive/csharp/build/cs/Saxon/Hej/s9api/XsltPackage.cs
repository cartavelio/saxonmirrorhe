using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XsltPackage {
        private readonly Saxon.Hej.s9api.XsltCompiler compiler;
        private readonly Saxon.Hej.style.StylesheetPackage stylesheetPackage;
        protected internal XsltPackage(Saxon.Hej.s9api.XsltCompiler compiler, Saxon.Hej.style.StylesheetPackage pp)  {
            this.compiler = compiler;
            this.stylesheetPackage = pp;
        }
        public Saxon.Hej.s9api.Processor getProcessor() {
            return compiler.getProcessor();
        }
        public string getName() {
            return stylesheetPackage.getPackageName();
        }
        public string getVersion() {
            return stylesheetPackage.getPackageVersion().ToString();
        }
        public Saxon.Hej.style.PackageVersion getPackageVersion() {
            return stylesheetPackage.getPackageVersion();
        }
        public Saxon.Hej.s9api.XsltExecutable link() {
            try {
                Saxon.Hej.Configuration config = getProcessor().getUnderlyingConfiguration();
                Saxon.Hej.trans.CompilerInfo info = compiler.getUnderlyingCompilerInfo();
                Saxon.Hej.style.Compilation compilation = new Saxon.Hej.style.Compilation(config, info);
                compilation.setPackageData(stylesheetPackage);
                stylesheetPackage.checkForAbstractComponents();
                Saxon.Hej.PreparedStylesheet pss = new Saxon.Hej.PreparedStylesheet(compilation);
                stylesheetPackage.updatePreparedStylesheet(pss);
                pss.addPackage(stylesheetPackage);
                return new Saxon.Hej.s9api.XsltExecutable(getProcessor(), pss);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void save(Saxon.Ejava.io.File file) {
            string target = stylesheetPackage.getTargetEdition();
            if ((target == null)) {
                target = getProcessor().getSaxonEdition();
            }
            save(file, target);
        }
        public void save(Saxon.Ejava.io.File file, string target) {
            try {
                Saxon.Hej.Query.createFileIfNecessary(file);
                Saxon.Hej.trace.ExpressionPresenter presenter = getProcessor().getUnderlyingConfiguration().newExpressionExporter(target, new System.IO.FileStream(file.getPath(), System.IO.FileMode.Create), stylesheetPackage);
                presenter.getOptions().relocatable = stylesheetPackage.isRelocatable();
                stylesheetPackage.export(presenter);
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is System.IO.IOException) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.style.StylesheetPackage getUnderlyingPreparedPackage() {
            return stylesheetPackage;
        }
    
    }
}