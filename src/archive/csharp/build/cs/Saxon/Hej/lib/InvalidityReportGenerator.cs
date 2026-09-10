using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class InvalidityReportGenerator : Saxon.Hej.lib.StandardInvalidityHandler {
        public static readonly Saxon.Hej.om.NamespaceUri REPORT_NS = Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/ns/validation");
        public InvalidityReportGenerator(Saxon.Hej.Configuration config) : base (config) {
        }
        public InvalidityReportGenerator(Saxon.Hej.Configuration config, Saxon.Hej.@event.Outputter receiver) : base (config) {
        }
        public void setReceiver(Saxon.Hej.@event.Outputter receiver) {
        }
        public virtual void setSystemId(string id) {
        }
        public virtual void setSchemaName(string name) {
        }
        public virtual int getErrorCount() {
            return 0;
        }
        public virtual int getWarningCount() {
            return 0;
        }
        public virtual void setXsdVersion(string version) {
        }
        public override void reportInvalidity(Saxon.Hej.lib.Invalidity failure) {
        }
        public override void startReporting(string systemId) {
        }
        public override Saxon.Hej.om.Sequence endReporting() {
            return null;
        }
        public virtual void createMetaData() {
        }
    
    }
}