using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class PackageVersion : Saxon.Ejava.lang.Comparable<Saxon.Hej.style.PackageVersion> {
        public System.Collections.Generic.IList<int> parts;
        public string suffix;
        public static Saxon.Hej.style.PackageVersion ZERO = new Saxon.Hej.style.PackageVersion(new int[]{0});
        public static Saxon.Hej.style.PackageVersion ONE = new Saxon.Hej.style.PackageVersion(new int[]{1});
        public static Saxon.Hej.style.PackageVersion MAX_VALUE = new Saxon.Hej.style.PackageVersion(new int[]{System.Int32.MaxValue});
        public PackageVersion(int[] values)  {
            parts = new System.Collections.Generic.List<int>(values.Length);
            foreach (int value in values) {
                parts.Add(value);
            }
            trimTrailingZeroes();
        }
        private void trimTrailingZeroes() {
            for (int i = (parts.Count-1); (i>0); i--) {
                if ((parts[i] != 0)) {
                    return ;
                } else {
                    Saxon.Impl.Helpers.ListUtils.Remove(parts, i);
                }
            }
        }
        public PackageVersion(string s)  {
            parts = new System.Collections.Generic.List<int>();
            string original = s;
            if (s.Contains("-")) {
                int i = s.IndexOf('-');
                suffix = s.Substring((i+1));
                if (!(Saxon.Hej.om.NameChecker.isValidNCName(suffix))) {
                    throw new Saxon.Hej.trans.XPathException(("Illegal NCName as package-version NamePart: "+original), "XTSE0020");
                }
                s = s.Substring(0, i - 0);
            }
            if (s.Equals("")) {
                throw new Saxon.Hej.trans.XPathException(("No numeric component of package-version: "+original), "XTSE0020");
            }
            if (s.StartsWith(".")) {
                throw new Saxon.Hej.trans.XPathException("The package-version cannot start with '.'", "XTSE0020");
            }
            if (s.EndsWith(".")) {
                throw new Saxon.Hej.trans.XPathException("The package-version cannot end with '.'", "XTSE0020");
            }
            foreach (string p in System.Text.RegularExpressions.Regex.Split(s.Trim(), "\\.")) {
                try {
                    parts.Add(System.Int32.Parse(p));
                } catch (Saxon.Ejava.lang.NumberFormatException e) {
                    throw new Saxon.Hej.trans.XPathException(("Error in package-version: "+e.Message), "XTSE0020");
                }
            }
            trimTrailingZeroes();
        }
        public override bool Equals(System.Object o) {
            if ((o is Saxon.Hej.style.PackageVersion)) {
                Saxon.Hej.style.PackageVersion p = (Saxon.Hej.style.PackageVersion)(o);
                if (equalParts(parts, p.parts)) {
                    if ((suffix != null)) {
                        return suffix.Equals(p.suffix);
                    } else {
                        return (p.suffix == null);
                    }
                }
            }
            return false;
        }
        private static bool equalParts(System.Collections.Generic.IList<int> a, System.Collections.Generic.IList<int> b) {
            return a.SequenceEqual(b);
        }
        public override int GetHashCode() {
            int h = 772211;
            foreach (int p in parts) {
                h = (((h<<3))^p);
            }
            if ((suffix != null)) {
                h = (((h<<3))^suffix.GetHashCode());
            }
            return h;
        }
        public bool equalsIgnoringSuffix(Saxon.Hej.style.PackageVersion other) {
            return parts.Equals(other.parts);
        }
        public int compareTo(Saxon.Hej.style.PackageVersion o) {
            Saxon.Hej.style.PackageVersion pv = o;
            System.Collections.Generic.IList<int> p = pv.parts;
            int extent = (parts.Count-p.Count);
            int len = System.Math.Min(parts.Count, p.Count);
            for (int i = 0; (i<len); i++) {
                int comp = parts[i].CompareTo(p[i]);
                if ((comp != 0)) {
                    return comp;
                }
            }
            if ((extent == 0)) {
                if ((suffix != null)) {
                    if ((pv.suffix == null)) {
                        return -1;
                    } else {
                        return System.String.Compare(suffix, pv.suffix, System.StringComparison.Ordinal);
                    }
                } else {
                    if ((pv.suffix != null)) {
                        return +(1);
                    }
                }
            }
            return extent;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder result = new Saxon.Ejava.lang.StringBuilder();
            foreach (int i in parts) {
                result.append(".").append(i);
            }
            if (!(parts.Count==0)) {
                result = new Saxon.Ejava.lang.StringBuilder(result.substring(1));
            }
            if ((suffix != null)) {
                result.append("-").append(suffix);
            }
            return result.ToString();
        }
        public bool isPrefix(Saxon.Hej.style.PackageVersion v) {
            if ((v.parts.Count>=parts.Count)) {
                for (int i = 0; (i<parts.Count); i++) {
                    if (!(parts[i].Equals(v.parts[i]))) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    
    }
}