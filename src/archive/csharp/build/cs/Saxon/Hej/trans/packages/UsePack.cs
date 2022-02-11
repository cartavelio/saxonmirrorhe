using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.packages {
    public class UsePack {
        public string packageName;
        public Saxon.Hej.style.PackageVersionRanges ranges;
        public Saxon.Hej.s9api.Location location;
        public UsePack(string name, string version, Saxon.Hej.s9api.Location location)  {
            this.packageName = name;
            this.ranges = new Saxon.Hej.style.PackageVersionRanges(((version == null) ? "*" : version));
            this.location = location;
        }
    
    }
}