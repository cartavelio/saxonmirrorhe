using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.packages {
    public class VersionedPackageName {
        public string packageName;
        public Saxon.Hej.style.PackageVersion packageVersion;
        public VersionedPackageName(string packageName, Saxon.Hej.style.PackageVersion version)  {
            this.packageName = packageName;
            this.packageVersion = version;
        }
        public VersionedPackageName(string packageName, string version)  {
            this.packageName = packageName;
            this.packageVersion = new Saxon.Hej.style.PackageVersion(version);
        }
        public override string ToString() {
            return (((packageName+" (")+packageVersion.ToString())+")");
        }
        public bool equalsIgnoringSuffix(Saxon.Hej.trans.packages.VersionedPackageName other) {
            return (packageName.Equals(other.packageName) && packageVersion.equalsIgnoringSuffix(other.packageVersion));
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Hej.trans.packages.VersionedPackageName) && packageName.Equals(((Saxon.Hej.trans.packages.VersionedPackageName)(obj)).packageName)) && packageVersion.Equals(((Saxon.Hej.trans.packages.VersionedPackageName)(obj)).packageVersion));
        }
        public override int GetHashCode() {
            return (packageName.GetHashCode()^packageVersion.GetHashCode());
        }
    
    }
}