using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class PackageData {
        protected internal Saxon.Hej.Configuration config;
        private Saxon.Api.HostLanguage hostLanguage;
        protected internal int hostLanguageVersion;
        private bool schemaAware;
        private Saxon.Hej.trans.DecimalFormatManager decimalFormatManager = null;
        protected internal Saxon.Hej.trans.KeyManager keyManager = null;
        private Saxon.Hej.expr.accum.AccumulatorRegistry accumulatorRegistry = null;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> globalVariables = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.GlobalVariable>();
        private Saxon.Hej.expr.instruct.SlotManager globalSlotManager;
        private int localLicenseId = -1;
        private string targetEdition;
        private bool relocatable;
        private Saxon.Hej.trans.TypeAliasManager typeAliasManager;
        public PackageData(Saxon.Hej.Configuration config)  {
            if ((config == null)) {
                throw new Saxon.Ejava.lang.NullPointerException();
            }
            this.config = config;
            targetEdition = config.getEditionCode();
            globalSlotManager = config.makeSlotManager();
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setConfiguration(Saxon.Hej.Configuration configuration) {
            this.config = configuration;
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            return hostLanguage;
        }
        public int getHostLanguageVersion() {
            return hostLanguageVersion;
        }
        public bool isXSLT() {
            return (hostLanguage == Saxon.Api.HostLanguage.XSLT);
        }
        public void setHostLanguage(Saxon.Api.HostLanguage hostLanguage, int version) {
            this.hostLanguage = hostLanguage;
            this.hostLanguageVersion = version;
        }
        public void setLocalLicenseId(int id) {
            localLicenseId = id;
        }
        public int getLocalLicenseId() {
            return localLicenseId;
        }
        public void setTargetEdition(string edition) {
            this.targetEdition = edition;
        }
        public string getTargetEdition() {
            return this.targetEdition;
        }
        public bool isRelocatable() {
            return relocatable;
        }
        public void setRelocatable(bool relocatable) {
            this.relocatable = relocatable;
        }
        public bool isSchemaAware() {
            return schemaAware;
        }
        public void setSchemaAware(bool schemaAware) {
            this.schemaAware = schemaAware;
        }
        public Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager() {
            if ((decimalFormatManager == null)) {
                decimalFormatManager = new Saxon.Hej.trans.DecimalFormatManager(hostLanguage, 31);
            }
            return decimalFormatManager;
        }
        public void setDecimalFormatManager(Saxon.Hej.trans.DecimalFormatManager manager) {
            decimalFormatManager = manager;
        }
        public Saxon.Hej.trans.KeyManager getKeyManager() {
            if ((keyManager == null)) {
                keyManager = new Saxon.Hej.trans.KeyManager(getConfiguration(), this);
            }
            return keyManager;
        }
        public void setKeyManager(Saxon.Hej.trans.KeyManager manager) {
            keyManager = manager;
        }
        public Saxon.Hej.expr.accum.AccumulatorRegistry getAccumulatorRegistry() {
            return accumulatorRegistry;
        }
        public void setAccumulatorRegistry(Saxon.Hej.expr.accum.AccumulatorRegistry accumulatorRegistry) {
            this.accumulatorRegistry = accumulatorRegistry;
        }
        public Saxon.Hej.expr.instruct.SlotManager getGlobalSlotManager() {
            return globalSlotManager;
        }
        public void setGlobalSlotManager(Saxon.Hej.expr.instruct.SlotManager manager) {
            this.globalSlotManager = manager;
        }
        public virtual void addGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable variable) {
            globalVariables.Add(variable);
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.instruct.GlobalVariable> getGlobalVariableList() {
            return globalVariables;
        }
        public void setTypeAliasManager(Saxon.Hej.trans.TypeAliasManager manager) {
            this.typeAliasManager = manager;
        }
        public Saxon.Hej.trans.TypeAliasManager obtainTypeAliasManager() {
            if ((typeAliasManager == null)) {
                typeAliasManager = config.makeTypeAliasManager();
            }
            return typeAliasManager;
        }
    
    }
}