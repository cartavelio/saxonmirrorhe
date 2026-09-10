using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class StaticQueryContextEE : Saxon.Eej.expr.StaticQueryContextPE {
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.query.QueryLibrary> compiledLibraries = new System.Collections.Concurrent.ConcurrentDictionary<string, Saxon.Hej.query.QueryLibrary>();
        private bool schemaAware;
        private bool streaming;
        public StaticQueryContextEE(Saxon.Hej.Configuration config) : base (config) {
        }
        public StaticQueryContextEE(Saxon.Eej.config.EnterpriseConfiguration config, bool initialize) : base (config, initialize) {
        }
        public override void copyFrom(Saxon.Hej.query.StaticQueryContext c) {
            base.copyFrom(c);
            if ((c is Saxon.Eej.ee.optim.StaticQueryContextEE)) {
                schemaAware = ((Saxon.Eej.ee.optim.StaticQueryContextEE)(c)).schemaAware;
                streaming = ((Saxon.Eej.ee.optim.StaticQueryContextEE)(c)).streaming;
            }
        }
        public override void setSchemaAware(bool aware) {
            if (aware) {
                getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY, "schema-awareness", -1);
            }
            this.schemaAware = aware;
        }
        public override bool isSchemaAware() {
            return schemaAware;
        }
        public override void setStreaming(bool option) {
            streaming = option;
            if (option) {
                getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY, "streaming", -1);
                setRequiredContextItemType(Saxon.Hej.pattern.NodeKindTest.DOCUMENT);
            }
        }
        public override bool isStreaming() {
            return streaming;
        }
        public override void compileLibrary(string query) {
            try {
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Hej.expr.instruct.Executable executable = makeExecutable();
                Saxon.Hej.functions.FunctionLibraryList libList = new Saxon.Hej.functions.FunctionLibraryList();
                libList.addFunctionLibrary(new Saxon.Hej.functions.ExecutableFunctionLibrary(config));
                executable.setFunctionLibrary(libList);
                Saxon.Hej.query.XQueryParser qp = (Saxon.Hej.query.XQueryParser)(config.newExpressionParser("XQ", isUpdating(), 31));
                if ((getCodeInjector() != null)) {
                    qp.setCodeInjector(getCodeInjector());
                }
                Saxon.Hej.query.QueryLibrary topModule = new Saxon.Eej.expr.QueryLibraryImpl(this);
                executable.setTopLevelPackage(topModule.getPackageData());
                topModule.setExecutable(executable);
                topModule.setIsMainModule(false);
                qp.parseLibraryModule(query, topModule);
                topModule.bindUnboundFunctionCalls();
                topModule.bindUnboundVariables();
                Saxon.Impl.Helpers.MapUtils.Put(compiledLibraries, topModule.getModuleNamespace(), topModule);
                executable.fixupQueryModules(topModule);
                executable.addQueryLibraryModule(topModule);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public override void compileLibrary(System.IO.TextReader source) {
            char[] buffer = new char[4096];
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(4096);
            while (true) {
                int n = source.Read(buffer);
                if (isFinished(n)) {
                    break ;
                } else {
                    sb.append(buffer, 0, n);
                }
            }
            compileLibrary(sb.ToString());
        }
        private bool isFinished(int indicator) {
            return indicator == 0;
        }
        public override void compileLibrary(System.IO.Stream source, string encoding) {
            string query = Saxon.Hej.query.QueryReader.readInputStream(source, encoding, getConfiguration().getValidCharacterChecker());
            compileLibrary(query);
        }
        public override Saxon.Hej.query.QueryLibrary getCompiledLibrary(string @namespace) {
            return Saxon.Impl.Helpers.MapUtils.Get(compiledLibraries, @namespace);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.query.QueryLibrary> getCompiledLibraries() {
            return compiledLibraries.Values;
        }
    
    }
}