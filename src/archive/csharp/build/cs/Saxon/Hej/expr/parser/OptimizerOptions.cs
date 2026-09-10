using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class OptimizerOptions {
        public const int LOOP_LIFTING = 1;
        public const int EXTRACT_GLOBALS = 2;
        public const int INLINE_VARIABLES = 4;
        public const int INLINE_FUNCTIONS = 8;
        public const int INDEX_VARIABLES = 16;
        public const int CREATE_KEYS = 32;
        public const int BYTE_CODE_NOT_USED = 64;
        public const int COMMON_SUBEXPRESSIONS = 128;
        public const int MISCELLANEOUS = 256;
        public const int SWITCH = 512;
        public const int JIT = 1024;
        public const int RULE_SET = 2048;
        public const int REGEX_CACHE = 4096;
        public const int VOID_EXPRESSIONS = 8192;
        public const int TAIL_CALLS = 16384;
        public const int CONSTANT_FOLDING = 32768;
        public const int REORDER_PREDICATES = 65536;
        private readonly int options;
        public readonly static Saxon.Hej.expr.parser.OptimizerOptions FULL_HE_OPTIMIZATION = new Saxon.Hej.expr.parser.OptimizerOptions("lvmt");
        public readonly static Saxon.Hej.expr.parser.OptimizerOptions FULL_EE_OPTIMIZATION = new Saxon.Hej.expr.parser.OptimizerOptions(-1);
        public OptimizerOptions(int options)  {
            this.options = options;
        }
        public OptimizerOptions(string flags)  {
            int opt = 0;
            if (flags.StartsWith("-")) {
                opt = -1;
                for (int i = 1; (i<flags.Length); i++) {
                    char c = flags[i];
                    opt &= ~(decodeFlag(c));
                }
            } else {
                for (int i = 0; (i<flags.Length); i++) {
                    char c = flags[i];
                    opt |= decodeFlag(c);
                }
            }
            this.options = opt;
        }
        private int decodeFlag(char flag) {
            switch (flag) {
              case 'c':
                return BYTE_CODE_NOT_USED;
              case 'd':
                return VOID_EXPRESSIONS;
              case 'e':
                return REGEX_CACHE;
              case 'f':
                return INLINE_FUNCTIONS;
              case 'g':
                return EXTRACT_GLOBALS;
              case 'j':
                return JIT;
              case 'k':
                return CREATE_KEYS;
              case 'l':
                return LOOP_LIFTING;
              case 'm':
                return MISCELLANEOUS;
              case 'n':
                return CONSTANT_FOLDING;
              case 'p':
                return REORDER_PREDICATES;
              case 'r':
                return RULE_SET;
              case 's':
                return COMMON_SUBEXPRESSIONS;
              case 't':
                return TAIL_CALLS;
              case 'v':
                return INLINE_VARIABLES;
              case 'w':
                return SWITCH;
              case 'x':
                return INDEX_VARIABLES;
              default:
                return 0;
            }
        }
        public Saxon.Hej.expr.parser.OptimizerOptions intersect(Saxon.Hej.expr.parser.OptimizerOptions other) {
            return new Saxon.Hej.expr.parser.OptimizerOptions((options&other.options));
        }
        public Saxon.Hej.expr.parser.OptimizerOptions union(Saxon.Hej.expr.parser.OptimizerOptions other) {
            return new Saxon.Hej.expr.parser.OptimizerOptions((options|other.options));
        }
        public Saxon.Hej.expr.parser.OptimizerOptions except(Saxon.Hej.expr.parser.OptimizerOptions other) {
            return new Saxon.Hej.expr.parser.OptimizerOptions((options&~(other.options)));
        }
        public override string ToString() {
            string result = "";
            if (isSet(VOID_EXPRESSIONS)) {
                result += "d";
            }
            if (isSet(REGEX_CACHE)) {
                result += "e";
            }
            if (isSet(INLINE_FUNCTIONS)) {
                result += "f";
            }
            if (isSet(EXTRACT_GLOBALS)) {
                result += "g";
            }
            if (isSet(JIT)) {
                result += "j";
            }
            if (isSet(CREATE_KEYS)) {
                result += "k";
            }
            if (isSet(LOOP_LIFTING)) {
                result += "l";
            }
            if (isSet(MISCELLANEOUS)) {
                result += "m";
            }
            if (isSet(CONSTANT_FOLDING)) {
                result += "n";
            }
            if (isSet(REORDER_PREDICATES)) {
                result += "p";
            }
            if (isSet(RULE_SET)) {
                result += "r";
            }
            if (isSet(COMMON_SUBEXPRESSIONS)) {
                result += "s";
            }
            if (isSet(TAIL_CALLS)) {
                result += "t";
            }
            if (isSet(INLINE_VARIABLES)) {
                result += "v";
            }
            if (isSet(SWITCH)) {
                result += "w";
            }
            if (isSet(INDEX_VARIABLES)) {
                result += "x";
            }
            return result;
        }
        public bool isSet(int option) {
            return (((options&option)) != 0);
        }
        public int getOptions() {
            return options;
        }
    
    }
}