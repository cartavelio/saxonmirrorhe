using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.packages {
    public class PackageLibrary {
        private readonly Saxon.Hej.Configuration config;
        private readonly Saxon.Hej.trans.CompilerInfo compilerInfo;
        private System.Collections.Generic.IDictionary<string,System.Collections.Generic.IList<Saxon.Hej.style.PackageVersion>> packageVersions = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IList<Saxon.Hej.style.PackageVersion>>();
        private System.Collections.Generic.IDictionary<Saxon.Hej.trans.packages.VersionedPackageName,Saxon.Hej.trans.packages.PackageDetails> packages = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.packages.VersionedPackageName, Saxon.Hej.trans.packages.PackageDetails>();
        public PackageLibrary(Saxon.Hej.trans.CompilerInfo compilerInfo)  {
            this.compilerInfo = compilerInfo;
            this.config = compilerInfo.getConfiguration();
        }
        public Saxon.Hej.trans.CompilerInfo getCompilerInfo() {
            return compilerInfo;
        }
        public PackageLibrary(Saxon.Hej.trans.packages.PackageLibrary library)  {
            packageVersions = new System.Collections.Generic.Dictionary<string,System.Collections.Generic.IList<Saxon.Hej.style.PackageVersion>>(library.packageVersions);
            packages = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.packages.VersionedPackageName, Saxon.Hej.trans.packages.PackageDetails>(library.packages);
            compilerInfo = library.compilerInfo;
            config = library.config;
        }
        public PackageLibrary(Saxon.Hej.trans.CompilerInfo info, System.Collections.Generic.ISet<Saxon.Ejava.io.File> files)  {
            compilerInfo = info;
            config = info.getConfiguration();
            foreach (Saxon.Ejava.io.File file in files) {
                Saxon.Hej.trans.packages.PackageInspector inspector = new Saxon.Hej.trans.packages.PackageInspector(config.makePipelineConfiguration());
                Saxon.Hej.trans.packages.PackageDetails details = inspector.getPackageDetails(file, config);
                if ((details == null)) {
                    string message = ("Unable to get package name and version for file "+file.getName());
                    string diagnostics = inspector.getDiagnostics();
                    if ((diagnostics != null)) {
                        message += ((" ("+diagnostics)+")");
                    }
                    throw new Saxon.Hej.trans.XPathException(message);
                }
                addPackage(details);
            }
        }
        public void addPackage(Saxon.Hej.style.StylesheetPackage packageIn) { lock(this) {
            string name = packageIn.getPackageName();
            Saxon.Hej.style.PackageVersion version = packageIn.getPackageVersion();
            Saxon.Hej.trans.packages.VersionedPackageName vp = new Saxon.Hej.trans.packages.VersionedPackageName(name, version);
            Saxon.Hej.trans.packages.PackageDetails details = new Saxon.Hej.trans.packages.PackageDetails();
            details.nameAndVersion = vp;
            details.loadedPackage = packageIn;
            if ((vp.packageName != null)) {
                Saxon.Impl.Helpers.MapUtils.Put(packages, vp, details);
                addPackage(details);
            }
        }}
        public void addPackage(Saxon.Hej.trans.packages.PackageDetails details) { lock(this) {
            Saxon.Hej.trans.packages.VersionedPackageName vp = details.nameAndVersion;
            string name = vp.packageName;
            Saxon.Hej.style.PackageVersion version = vp.packageVersion;
            System.Collections.Generic.IList<Saxon.Hej.style.PackageVersion> versions = Saxon.Impl.Helpers.MapUtils.Get(packageVersions, name);
            if ((versions == null)) {
                versions = new System.Collections.Generic.List<Saxon.Hej.style.PackageVersion>();
                Saxon.Impl.Helpers.MapUtils.Put(packageVersions, name, versions);
            }
            versions.Add(version);
            Saxon.Impl.Helpers.MapUtils.Put(packages, vp, details);
        }}
        public void addPackage(Saxon.Ejava.io.File file) {
            Saxon.Hej.trans.packages.PackageInspector inspector = new Saxon.Hej.trans.packages.PackageInspector(config.makePipelineConfiguration());
            Saxon.Hej.trans.packages.PackageDetails details = inspector.getPackageDetails(file, config);
            if ((details == null)) {
                string message = ("Unable to get package name and version for file "+file.getName());
                string diagnostics = inspector.getDiagnostics();
                if ((diagnostics != null)) {
                    message += ((" ("+diagnostics)+")");
                }
                throw new Saxon.Hej.trans.XPathException(message);
            }
            addPackage(details);
        }
        public Saxon.Hej.trans.packages.PackageDetails findPackage(string name, Saxon.Hej.style.PackageVersionRanges ranges) { lock(this) {
            System.Collections.Generic.ISet<Saxon.Hej.trans.packages.PackageDetails> candidates = new System.Collections.Generic.HashSet<Saxon.Hej.trans.packages.PackageDetails>();
            System.Collections.Generic.IList<Saxon.Hej.style.PackageVersion> available = Saxon.Impl.Helpers.MapUtils.Get(packageVersions, name);
            if ((available == null)) {
                return null;
            }
            int maxPriority = System.Int32.MinValue;
            foreach (Saxon.Hej.style.PackageVersion pv in available) {
                Saxon.Hej.trans.packages.PackageDetails details = Saxon.Impl.Helpers.MapUtils.Get(packages, new Saxon.Hej.trans.packages.VersionedPackageName(name, pv));
                if (ranges.contains(pv)) {
                    candidates.Add(details);
                    int priority = details.priority;
                    if ((priority>maxPriority)) {
                        maxPriority = priority;
                    }
                }
            }
            if (candidates.Count==0) {
                return null;
            } else {
                if ((candidates.Count == 1)) {
                    System.Collections.Generic.IEnumerator<Saxon.Hej.trans.packages.PackageDetails> iter = candidates.GetEnumerator();
                    if (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                        return iter.Current;
                    } else {
                        return null;
                    }
                } else {
                    System.Collections.Generic.ISet<Saxon.Hej.style.PackageVersion> shortList = new System.Collections.Generic.HashSet<Saxon.Hej.style.PackageVersion>();
                    Saxon.Hej.trans.packages.PackageDetails highest = null;
                    if ((maxPriority == System.Int32.MinValue)) {
                        foreach (Saxon.Hej.trans.packages.PackageDetails details in candidates) {
                            if (((highest == null) || (details.nameAndVersion.packageVersion.compareTo(highest.nameAndVersion.packageVersion)>0))) {
                                highest = details;
                            }
                        }
                    } else {
                        foreach (Saxon.Hej.trans.packages.PackageDetails details in candidates) {
                            int priority = details.priority;
                            Saxon.Hej.style.PackageVersion pv = details.nameAndVersion.packageVersion;
                            if ((((priority != System.Int32.MinValue) && (priority == maxPriority)) && (((highest == null) || (pv.compareTo(highest.nameAndVersion.packageVersion)>0))))) {
                                highest = details;
                            }
                        }
                    }
                    return highest;
                }
            }
        }}
        public Saxon.Hej.trans.packages.PackageDetails findDetailsForAlias(string shortName) { lock(this) {
            System.Diagnostics.Debug.Assert((shortName != null));
            Saxon.Hej.trans.packages.PackageDetails selected = null;
            foreach (Saxon.Hej.trans.packages.PackageDetails details in packages.Values) {
                if (shortName.Equals(details.shortName)) {
                    if ((selected == null)) {
                        selected = details;
                    } else {
                        throw new Saxon.Ejava.lang.IllegalStateException(("Non-unique shortName in package library: "+shortName));
                    }
                }
            }
            return selected;
        }}
        public Saxon.Hej.style.StylesheetPackage obtainLoadedPackage(Saxon.Hej.trans.packages.PackageDetails details, System.Collections.Generic.IList<Saxon.Hej.trans.packages.VersionedPackageName> disallowed) {
            if ((details.loadedPackage != null)) {
                return details.loadedPackage;
            } else {
                if ((details.exportLocation != null)) {
                    testForCycles(details, disallowed);
                    details.beingProcessed = Saxon.Ejava.lang.Thread.currentThread();
                    Saxon.Ejavax.xml.transform.Source input = details.exportLocation;
                    Saxon.Hej.trans.packages.IPackageLoader loader = config.makePackageLoader();
                    Saxon.Hej.style.StylesheetPackage pack = loader.loadPackage(input);
                    checkNameAndVersion(pack, details);
                    details.loadedPackage = pack;
                    details.beingProcessed = null;
                    return pack;
                } else {
                    if ((details.sourceLocation != null)) {
                        testForCycles(details, disallowed);
                        details.beingProcessed = Saxon.Ejava.lang.Thread.currentThread();
                        Saxon.Hej.style.Compilation compilation = new Saxon.Hej.style.Compilation(config, compilerInfo);
                        compilation.setUsingPackages(disallowed);
                        compilation.setExpectedNameAndVersion(details.nameAndVersion);
                        compilation.clearParameters();
                        compilation.setLibraryPackage(true);
                        if ((details.staticParams != null)) {
                            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.GroundedValue> entry in details.staticParams) {
                                compilation.setParameter(entry.Key, entry.Value);
                            }
                        }
                        Saxon.Hej.style.PrincipalStylesheetModule psm = compilation.compilePackage(details.sourceLocation);
                        details.beingProcessed = null;
                        if ((compilation.getErrorCount()>0)) {
                            throw new Saxon.Hej.trans.XPathException(("Errors found in package "+details.nameAndVersion.packageName));
                        }
                        Saxon.Hej.style.StylesheetPackage styPack = psm.getStylesheetPackage();
                        checkNameAndVersion(styPack, details);
                        details.loadedPackage = styPack;
                        return styPack;
                    } else {
                        return null;
                    }
                }
            }
        }
        private void testForCycles(Saxon.Hej.trans.packages.PackageDetails details, System.Collections.Generic.IList<Saxon.Hej.trans.packages.VersionedPackageName> disallowed) {
            if ((details.beingProcessed == Saxon.Ejava.lang.Thread.currentThread())) {
                Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(1024);
                foreach (Saxon.Hej.trans.packages.VersionedPackageName n in disallowed) {
                    buffer.append(n.packageName);
                    buffer.append(", ");
                }
                buffer.append("and ");
                buffer.append(details.nameAndVersion.packageName);
                throw new Saxon.Hej.trans.XPathException(("There is a cycle of package dependencies involving "+buffer), "XTSE3005");
            }
        }
        private void checkNameAndVersion(Saxon.Hej.style.StylesheetPackage pack, Saxon.Hej.trans.packages.PackageDetails details) {
            string storedName = pack.getPackageName();
            if ((details.baseName != null)) {
                if (!(details.baseName.Equals(storedName))) {
                    throw new Saxon.Hej.trans.XPathException((((("Base name of package ("+details.baseName)+") does not match the value in the XSLT source (")+storedName)+")"));
                }
            } else {
                if (!(details.nameAndVersion.packageName.Equals(storedName))) {
                    throw new Saxon.Hej.trans.XPathException((((("Registered name of package ("+details.nameAndVersion.packageName)+") does not match the value in the XSLT source (")+storedName)+")"));
                }
            }
            Saxon.Hej.style.PackageVersion actualVersion = pack.getPackageVersion();
            if (!(actualVersion.Equals(details.nameAndVersion.packageVersion))) {
                throw new Saxon.Hej.trans.XPathException((((("Registered version number of package ("+details.nameAndVersion.packageVersion)+") does not match the value in the XSLT source (")+actualVersion)+")"));
            }
        }
        public System.Collections.Generic.IList<Saxon.Hej.style.StylesheetPackage> getPackages() { lock(this) {
            System.Collections.Generic.IList<Saxon.Hej.style.StylesheetPackage> result = new System.Collections.Generic.List<Saxon.Hej.style.StylesheetPackage>();
            foreach (Saxon.Hej.trans.packages.PackageDetails details in packages.Values) {
                if ((details.loadedPackage != null)) {
                    result.Add(details.loadedPackage);
                }
            }
            return result;
        }}
    
    }
}