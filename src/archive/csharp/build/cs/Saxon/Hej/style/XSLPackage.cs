using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLPackage : Saxon.Hej.style.XSLModuleRoot {
        private string nameAtt = null;
        private Saxon.Hej.style.PackageVersion packageVersion = null;
        private bool declaredModes = true;
        private bool prepared = false;
        public override void initialise(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType elementType, Saxon.Hej.om.AttributeMap atts, Saxon.Hej.om.NodeInfo parent, int sequenceNumber) {
            base.initialise(elemName, elementType, atts, parent, sequenceNumber);
            declaredModes = getLocalPart().Equals("package");
        }
        public string getName() {
            if ((nameAtt == null)) {
                prepareAttributes();
            }
            return nameAtt;
        }
        public int getVersion() {
            if ((version == -1)) {
                prepareAttributes();
            }
            return version;
        }
        public Saxon.Hej.trans.packages.VersionedPackageName getNameAndVersion() {
            return new Saxon.Hej.trans.packages.VersionedPackageName(getName(), getPackageVersion());
        }
        public Saxon.Hej.style.PackageVersion getPackageVersion() {
            if ((packageVersion == null)) {
                prepareAttributes();
            }
            return packageVersion;
        }
        protected internal override void prepareAttributes() {
            if (prepared) {
                return ;
            }
            prepared = true;
            string inputTypeAnnotationsAtt = null;
            string packageVersionAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string lexicalName = attName.getDisplayName();
                string value = att.getValue();
                if ((lexicalName.Equals("name") && getLocalPart().Equals("package"))) {
                    nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    if (lexicalName.Equals("id")) {
                    } else {
                        if (lexicalName.Equals("version")) {
                            if ((version == -1)) {
                                processVersionAttribute("");
                            }
                        } else {
                            if ((lexicalName.Equals("package-version") && getLocalPart().Equals("package"))) {
                                packageVersionAtt = Saxon.Hej.value.Whitespace.trim(value);
                            } else {
                                if ((lexicalName.Equals("declared-modes") && getLocalPart().Equals("package"))) {
                                    declaredModes = processBooleanAttribute("declared-modes", value);
                                } else {
                                    if (lexicalName.Equals("input-type-annotations")) {
                                        inputTypeAnnotationsAtt = value;
                                    } else {
                                        checkUnknownAttribute(attName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((packageVersionAtt == null)) {
                packageVersion = Saxon.Hej.style.PackageVersion.ONE;
            } else {
                try {
                    packageVersion = new Saxon.Hej.style.PackageVersion(packageVersionAtt);
                } catch (Saxon.Hej.trans.XPathException ex) {
                    compileErrorInAttribute(ex.Message, ex.getErrorCodeLocalPart(), "package-version");
                }
            }
            if ((version == -1)) {
                version = 30;
                reportAbsence("version");
            }
            if ((inputTypeAnnotationsAtt != null)) {
                switch (inputTypeAnnotationsAtt) {
                  case "strip":
                    break ;
                  case "preserve":
                    break ;
                  case "unspecified":
                    break ;
                  default:
                    compileError(("Invalid value for input-type-annotations attribute. "+"Permitted values are (strip, preserve, unspecified)"), "XTSE0020");
                    break ;
                }
            }
        }
        public override bool isDeclaredModes() {
            if ((nameAtt == null)) {
                prepareAttributes();
            }
            return declaredModes;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if ((((child.getNodeKind() == Saxon.Hej.type.Type.TEXT) || (((child is Saxon.Hej.style.StyleElement) && ((Saxon.Hej.style.StyleElement)(child)).isDeclaration()))) || (child is Saxon.Hej.style.DataElement))) {
                } else {
                    if ((child is Saxon.Hej.style.StyleElement)) {
                        if ((getLocalPart().Equals("package") && (((fp == Saxon.Hej.om.StandardNames.XSL_USE_PACKAGE) || (fp == Saxon.Hej.om.StandardNames.XSL_EXPOSE))))) {
                        } else {
                            if ((!(Saxon.Hej.lib.NamespaceConstant.XSLT.Equals(child.getURI())) && !("".Equals(child.getURI())))) {
                            } else {
                                if (((child is Saxon.Hej.style.AbsentExtensionElement) && ((Saxon.Hej.style.StyleElement)(child)).forwardsCompatibleModeIsEnabled())) {
                                } else {
                                    if (Saxon.Hej.lib.NamespaceConstant.XSLT.Equals(child.getURI())) {
                                        if ((child is Saxon.Hej.style.AbsentExtensionElement)) {
                                        } else {
                                            ((Saxon.Hej.style.StyleElement)(child)).compileError(((("Element "+child.getDisplayName())+" must not appear directly within ")+getDisplayName()), "XTSE0010");
                                        }
                                    } else {
                                        ((Saxon.Hej.style.StyleElement)(child)).compileError((((("Element "+child.getDisplayName())+" must not appear directly within ")+getDisplayName())+" because it is not in a namespace"), "XTSE0130");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (declaredModes) {
                string defaultMode = getAttributeValue("default-mode");
                if (((defaultMode != null) && (getPrincipalStylesheetModule().getRuleManager().obtainMode(getDefaultMode(), false) == null))) {
                    compileError((("The default mode "+defaultMode)+" has not been declared in an xsl:mode declaration"), "XTSE3085");
                }
            }
        }
    
    }
}