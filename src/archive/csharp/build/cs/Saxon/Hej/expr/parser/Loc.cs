using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class Loc : Saxon.Hej.s9api.Location {
        private readonly string systemId;
        private readonly int lineNumber;
        private int columnNumber = -1;
        public static Saxon.Hej.expr.parser.Loc NONE = new Saxon.Hej.expr.parser.Loc(null, -1, -1);
        public Loc(Saxon.Ejavax.xml.transform.SourceLocator loc)  {
            systemId = loc.getSystemId();
            lineNumber = loc.getLineNumber();
            columnNumber = loc.getColumnNumber();
        }
        public Loc(string systemId, int lineNumber, int columnNumber)  {
            this.systemId = systemId;
            this.lineNumber = lineNumber;
            this.columnNumber = columnNumber;
        }
        public string getSystemId() {
            return systemId;
        }
        public string getPublicId() {
            return null;
        }
        public int getLineNumber() {
            return lineNumber;
        }
        public int getColumnNumber() {
            return columnNumber;
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public static bool isUnknown(Saxon.Hej.s9api.Location location) {
            return ((location == null) || ((((location.getSystemId() == null) || location.getSystemId().Length==0)) && (location.getLineNumber() == -1)));
        }
    
    }
}