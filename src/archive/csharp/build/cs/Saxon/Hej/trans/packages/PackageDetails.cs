using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.packages {
    public class PackageDetails {
        public Saxon.Hej.trans.packages.VersionedPackageName nameAndVersion;
        public string baseName;
        public string shortName;
        public Saxon.Hej.style.StylesheetPackage loadedPackage;
        public Saxon.Ejavax.xml.transform.Source sourceLocation;
        public Saxon.Ejavax.xml.transform.Source exportLocation;
        public int priority = System.Int32.MinValue;
        public System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.GroundedValue> staticParams;
        public Saxon.Ejava.lang.Thread beingProcessed;
    
    }
}