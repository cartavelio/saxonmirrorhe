using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public sealed class Validation {
        public const int INVALID = -1;
        public const int STRICT = 1;
        public const int LAX = 2;
        public const int PRESERVE = 3;
        public const int STRIP = 4;
        public const int SKIP = 4;
        public const int DEFAULT = 0;
        public const int BY_TYPE = 8;
        private Validation()  {
        }
        public static int getCode(string value) {
            if (value.Equals("strict")) {
                return STRICT;
            } else {
                if (value.Equals("lax")) {
                    return LAX;
                } else {
                    if (value.Equals("preserve")) {
                        return PRESERVE;
                    } else {
                        if (value.Equals("strip")) {
                            return STRIP;
                        } else {
                            return INVALID;
                        }
                    }
                }
            }
        }
        public static string describe(int value) {
            switch (value) {
              case STRICT:
                return "strict";
              case LAX:
                return "lax";
              case PRESERVE:
                return "preserve";
              case STRIP:
                return "skip";
              case BY_TYPE:
                return "by type";
              default:
                return "invalid";
            }
        }
    
    }
}