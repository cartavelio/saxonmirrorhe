using net.sf.saxon.trans;

namespace java.util.function {
    public abstract class IntPredicate_Proxy {
        public abstract bool test(int value);
        public IntPredicate lambda() {
            return val => test(val);
        }
    }
}