using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class PackageVersionRanges {
        private readonly System.Collections.Generic.List<Saxon.Hej.style.PackageVersionRanges.PackageVersionRange> ranges;
        private class PackageVersionRange {
            private readonly string display;
            private Saxon.Hej.style.PackageVersion low;
            private Saxon.Hej.style.PackageVersion high;
            private bool all = false;
            private bool prefix = false;
            public PackageVersionRange(string s)  {
                display = s;
                if ("*".Equals(s)) {
                    all = true;
                } else {
                    if (s.EndsWith("+")) {
                        low = new Saxon.Hej.style.PackageVersion(s.Replace("+", ""));
                        high = Saxon.Hej.style.PackageVersion.MAX_VALUE;
                    } else {
                        if (s.EndsWith(".*")) {
                            prefix = true;
                            low = new Saxon.Hej.style.PackageVersion(s.Replace(".*", ""));
                        } else {
                            if (System.Text.RegularExpressions.Regex.IsMatch(s, ".*\\s*to\\s+.*")) {
                                string[] range = System.Text.RegularExpressions.Regex.Split(s, "\\s*to\\s+");
                                if ((range.Length>2)) {
                                    throw new Saxon.Hej.trans.XPathException(("Invalid version range:"+s), "XTSE0020");
                                }
                                low = (range[0].Equals("") ? Saxon.Hej.style.PackageVersion.ZERO : new Saxon.Hej.style.PackageVersion(range[0]));
                                high = new Saxon.Hej.style.PackageVersion(range[1]);
                            } else {
                                low = new Saxon.Hej.style.PackageVersion(s);
                                high = low;
                            }
                        }
                    }
                }
            }
            internal bool contains(Saxon.Hej.style.PackageVersion v) {
                if (all) {
                    return true;
                } else {
                    if (prefix) {
                        return low.isPrefix(v);
                    } else {
                        return ((low.compareTo(v)<=0) && (v.compareTo(high)<=0));
                    }
                }
            }
            public override string ToString() {
                return display;
            }
        
        }
        public PackageVersionRanges(string s)  {
            ranges = new System.Collections.Generic.List<Saxon.Hej.style.PackageVersionRanges.PackageVersionRange>();
            foreach (string p in System.Text.RegularExpressions.Regex.Split(s.Trim(), "\\s*,\\s*")) {
                ranges.Add(new Saxon.Hej.style.PackageVersionRanges.PackageVersionRange(p));
            }
        }
        public bool contains(Saxon.Hej.style.PackageVersion version) {
            foreach (Saxon.Hej.style.PackageVersionRanges.PackageVersionRange r in ranges) {
                if (r.contains(version)) {
                    return true;
                }
            }
            return false;
        }
        public override string ToString() {
            if ((ranges.Count == 1)) {
                return ranges[0].ToString();
            } else {
                Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(256);
                foreach (Saxon.Hej.style.PackageVersionRanges.PackageVersionRange r in ranges) {
                    buffer.append(r.ToString()).append(",");
                }
                buffer.setLength((buffer.length()-1));
                return buffer.ToString();
            }
        }
    
    }
}