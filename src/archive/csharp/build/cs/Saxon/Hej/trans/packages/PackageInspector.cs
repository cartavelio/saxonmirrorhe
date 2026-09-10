using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.packages {
    public class PackageInspector : Saxon.Hej.@event.ProxyReceiver {
        private bool isSefFile;
        private string packageName;
        private string packageVersion = "1";
        private int elementCount = 0;
        private string diagnostics;
        internal PackageInspector(Saxon.Hej.@event.PipelineConfiguration pipe) : base (new Saxon.Hej.@event.Sink(pipe)) {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((elementCount++>=1)) {
                throw new Saxon.Hej.trans.XPathException("#start#");
            }
            isSefFile = elemName.hasURI(Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT);
            if ((attributes.get("", "name") == null)) {
                diagnostics = (("Top level element "+elemName.getStructuredQName().getEQName())+" has no @name attribute");
            } else {
                packageName = attributes.get("", "name").getValue();
            }
            if ((attributes.get("", "package-version") != null)) {
                packageVersion = attributes.get("", "package-version").getValue();
            }
            if ((attributes.get("", "packageVersion") != null)) {
                packageVersion = attributes.get("", "packageVersion").getValue();
            }
            Saxon.Hej.om.AttributeInfo saxonVersion = attributes.get("", "saxonVersion");
            if ((saxonVersion != null)) {
                if (saxonVersion.getValue().StartsWith("9")) {
                    throw new Saxon.Hej.trans.XPathException(((("Saxon "+Saxon.Hej.Version.getProductVersion())+" cannot load a SEF file created using version ")+saxonVersion.getValue()));
                }
            }
        }
        private Saxon.Hej.trans.packages.VersionedPackageName getNameAndVersion() {
            if ((packageName == null)) {
                return null;
            }
            try {
                return new Saxon.Hej.trans.packages.VersionedPackageName(packageName, packageVersion);
            } catch (Saxon.Hej.trans.XPathException e) {
                return null;
            }
        }
        public Saxon.Hej.trans.packages.PackageDetails getPackageDetails(Saxon.Ejava.io.File top, Saxon.Hej.Configuration config) {
            try {
                Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions();
                options.setDTDValidationMode(Saxon.Hej.lib.Validation.SKIP);
                options.setSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP);
                Saxon.Hej.@event.Sender.send(new Saxon.Ejavax.xml.transform.stream.StreamSource(top), this, new Saxon.Hej.lib.ParseOptions());
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.Message.Equals("#start#"))) {
                    throw;
                }
            }
            Saxon.Hej.trans.packages.VersionedPackageName vp = getNameAndVersion();
            if ((vp == null)) {
                return null;
            } else {
                Saxon.Hej.trans.packages.PackageDetails details = new Saxon.Hej.trans.packages.PackageDetails();
                details.nameAndVersion = vp;
                if (isSefFile) {
                    details.exportLocation = new Saxon.Ejavax.xml.transform.stream.StreamSource(top);
                } else {
                    details.sourceLocation = new Saxon.Ejavax.xml.transform.stream.StreamSource(top);
                }
                return details;
            }
        }
        public string getDiagnostics() {
            return diagnostics;
        }
    
    }
}