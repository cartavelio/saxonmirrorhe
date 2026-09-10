using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class LicenseException : Saxon.Ejava.lang.RuntimeException {
        private int reason;
        public const int EXPIRED = 1;
        public const int INVALID = 2;
        public const int NOT_FOUND = 3;
        public const int WRONG_FEATURES = 4;
        public const int CANNOT_READ = 5;
        public const int WRONG_CONFIGURATION = 6;
        public LicenseException(string message, int reason) : base (message) {
            this.reason = reason;
        }
        public void setReason(int reason) {
            this.reason = reason;
        }
        public int getReason() {
            return reason;
        }
    
    }
}