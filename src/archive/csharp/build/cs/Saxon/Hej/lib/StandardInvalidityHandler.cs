using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardInvalidityHandler : Saxon.Hej.lib.StandardDiagnostics, Saxon.Hej.lib.InvalidityHandler {
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Hej.lib.Logger logger;
        public StandardInvalidityHandler(Saxon.Hej.Configuration config)  {
            this.config = config;
            this.logger = config.getLogger();
        }
        public void setLogger(Saxon.Hej.lib.Logger logger) {
            this.logger = logger;
        }
        public Saxon.Hej.lib.Logger getLogger() {
            return logger;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public virtual void startReporting(string systemId) {
        }
        public virtual void reportInvalidity(Saxon.Hej.lib.Invalidity failure) {
            Saxon.Hej.lib.Logger localLogger = logger;
            if ((localLogger == null)) {
                localLogger = config.getLogger();
            }
            string explanation = getExpandedMessage(failure);
            string constraintReference = getConstraintReferenceMessage(failure);
            string contextLocation = ((Saxon.Hej.type.ValidationFailure)(failure)).getContextLocationText();
            string finalMessage = (((((("Validation error "+getLocationMessage(failure))+"\n  ")+wordWrap(explanation))+wordWrap((contextLocation.Length==0 ? "" : ("\n  "+contextLocation))))+wordWrap(((constraintReference == null) ? "" : ("\n  "+constraintReference))))+formatListOfOffendingNodes((Saxon.Hej.type.ValidationFailure)(failure)));
            localLogger.error(finalMessage);
        }
        public string getLocationMessage(Saxon.Hej.lib.Invalidity err) {
            string locMessage = "";
            string systemId;
            Saxon.Hej.om.NodeInfo node = err.getInvalidNode();
            Saxon.Hej.om.AbsolutePath path;
            string nodeMessage = null;
            int lineNumber = err.getLineNumber();
            if ((nodeMessage == null)) {
                if (((lineNumber == -1) && ((path = err.getPath()) != null))) {
                    nodeMessage = (("at "+path)+' ');
                } else {
                    if ((node != null)) {
                        nodeMessage = (("at "+Saxon.Hej.tree.util.Navigator.getPath(node))+' ');
                    }
                }
            }
            bool containsLineNumber = (lineNumber != -1);
            if ((nodeMessage != null)) {
                locMessage += nodeMessage;
            }
            if (containsLineNumber) {
                locMessage += (("on line "+lineNumber)+' ');
                if ((err.getColumnNumber() != -1)) {
                    locMessage += (("column "+err.getColumnNumber())+' ');
                }
            }
            systemId = err.getSystemId();
            if (((systemId != null) && (systemId.Length != 0))) {
                locMessage += ((((containsLineNumber ? "of " : "in "))+abbreviateLocationURI(systemId))+':');
            }
            return locMessage;
        }
        public string getExpandedMessage(Saxon.Hej.lib.Invalidity err) {
            string code = err.getErrorCode();
            return ((((code == null) ? "" : (code+": ")))+err.getMessage());
        }
        public string getConstraintReferenceMessage(Saxon.Hej.lib.Invalidity err) {
            if ((err.getSchemaPart() == -1)) {
                return null;
            }
            return ((((("See https://www.w3.org/TR/xmlschema11-"+err.getSchemaPart())+"/#")+err.getConstraintName())+" clause ")+err.getConstraintClauseNumber());
        }
        public virtual Saxon.Hej.om.Sequence endReporting() {
            return Saxon.Hej.value.EmptySequence.getInstance();
        }
    
    }
}