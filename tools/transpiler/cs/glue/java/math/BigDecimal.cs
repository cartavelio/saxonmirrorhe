using net.sf.saxon.query;
using net.sf.saxon.value;
using BD = LongCalc.bd;

namespace java.math {
    public class BigDecimal {
        // An attempt to implement the required subset of the Java BigDecimal API as a wrapper
        // over the Long
        private BD decVal;
        
        public BigDecimal(string str) {
            decVal = new BD(str).Abs();
        }
        public BigDecimal(BD val) {
            decVal = val.Abs();
        }

        public BD DecVal {
            get => decVal;
            set => decVal = value;
        }
        public static BigDecimal Abso(BigDecimal d) {
            return new BigDecimal(d.decVal.Abs());
        } new BigDecimal(BD.a)
    }
}