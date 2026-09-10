using SysColl = System.Collections;
using SysCollGen = System.Collections.Generic;
    namespace Saxon.Ejava.text {
    public class Collator {
        public static SysColl.IComparer getInstance() {
            return new SysColl.CaseInsensitiveComparer();
        }
    }
}