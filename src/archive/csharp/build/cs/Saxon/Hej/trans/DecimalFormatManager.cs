using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class DecimalFormatManager {
        private readonly Saxon.Hej.trans.DecimalSymbols defaultDFS;
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.DecimalSymbols> formatTable;
        private readonly Saxon.Api.HostLanguage language;
        private readonly int languageLevel;
        public DecimalFormatManager(Saxon.Api.HostLanguage language, int languageLevel)  {
            formatTable = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.trans.DecimalSymbols>(10);
            defaultDFS = new Saxon.Hej.trans.DecimalSymbols(language, languageLevel);
            this.language = language;
            this.languageLevel = languageLevel;
        }
        public Saxon.Hej.trans.DecimalSymbols getDefaultDecimalFormat() {
            return defaultDFS;
        }
        public Saxon.Hej.trans.DecimalSymbols getNamedDecimalFormat(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.trans.DecimalSymbols ds = Saxon.Impl.Helpers.MapUtils.Get(formatTable, qName);
            if ((ds == null)) {
                return null;
            }
            return ds;
        }
        public Saxon.Hej.trans.DecimalSymbols obtainNamedDecimalFormat(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.trans.DecimalSymbols ds = Saxon.Impl.Helpers.MapUtils.Get(formatTable, qName);
            if ((ds == null)) {
                ds = new Saxon.Hej.trans.DecimalSymbols(language, languageLevel);
                Saxon.Impl.Helpers.MapUtils.Put(formatTable, qName, ds);
            }
            return ds;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.om.StructuredQName> getDecimalFormatNames() {
            return Saxon.Impl.Helpers.MapUtils.KeySet(formatTable);
        }
        public void checkConsistency() {
            defaultDFS.checkConsistency(null);
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.DecimalSymbols> entry in formatTable) {
                entry.Value.checkConsistency(entry.Key);
            }
        }
    
    }
}