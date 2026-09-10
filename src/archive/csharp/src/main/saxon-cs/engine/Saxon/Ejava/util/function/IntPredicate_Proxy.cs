using Saxon.Hej.trans;

namespace Saxon.Ejava.util.function {
    public abstract class IntPredicate_Proxy {
        public abstract bool test(int value);
        public IntPredicate lambda() {
            return val => test(val);
        }
    }
}