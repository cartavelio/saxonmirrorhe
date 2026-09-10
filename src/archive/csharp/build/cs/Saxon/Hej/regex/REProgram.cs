using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class REProgram {
        protected internal const int OPT_HASBACKREFS = 1;
        protected internal const int OPT_HASBOL = 2;
        protected internal Saxon.Hej.regex.Operation operation;
        protected internal Saxon.Hej.regex.REFlags flags;
        protected internal Saxon.Hej.str.UnicodeString prefix;
        protected internal Saxon.Hej.z.IntPredicateProxy initialCharClass;
        protected internal System.Collections.Generic.IList<Saxon.Hej.regex.RegexPrecondition> preconditions = new System.Collections.Generic.List<Saxon.Hej.regex.RegexPrecondition>();
        protected internal int minimumLength = 0;
        protected internal int fixedLength = -1;
        protected internal int optimizationFlags;
        protected internal int maxParens = -1;
        protected internal int backtrackingLimit = -1;
        public REProgram(Saxon.Hej.regex.Operation operation, int parens, Saxon.Hej.regex.REFlags flags)  {
            this.flags = flags;
            setOperation(operation);
            this.maxParens = parens;
        }
        private void setOperation(Saxon.Hej.regex.Operation operation) {
            this.operation = operation;
            this.optimizationFlags = 0;
            this.prefix = null;
            this.operation = operation.optimize(this, flags);
            if ((operation is Saxon.Hej.regex.OpSequence)) {
                Saxon.Hej.regex.Operation first = ((Saxon.Hej.regex.OpSequence)(operation)).getOperations()[0];
                if ((first is Saxon.Hej.regex.OpBOL)) {
                    optimizationFlags |= Saxon.Hej.regex.REProgram.OPT_HASBOL;
                } else {
                    if ((first is Saxon.Hej.regex.OpAtom)) {
                        prefix = ((Saxon.Hej.regex.OpAtom)(first)).getAtom();
                    } else {
                        if ((first is Saxon.Hej.regex.OpCharClass)) {
                            initialCharClass = ((Saxon.Hej.regex.OpCharClass)(first)).getPredicate();
                        }
                    }
                }
                addPrecondition(operation, -1, 0);
            }
            minimumLength = operation.getMinimumMatchLength();
            fixedLength = operation.getMatchLength();
        }
        public void setBacktrackingLimit(int limit) {
            this.backtrackingLimit = limit;
        }
        public int getBacktrackingLimit() {
            return backtrackingLimit;
        }
        private void addPrecondition(Saxon.Hej.regex.Operation op, int fixedPosition, int minPosition) {
            if (((op is Saxon.Hej.regex.OpAtom) || (op is Saxon.Hej.regex.OpCharClass))) {
                preconditions.Add(new Saxon.Hej.regex.RegexPrecondition(op, fixedPosition, minPosition));
            } else {
                if (((op is Saxon.Hej.regex.OpRepeat) && (((Saxon.Hej.regex.OpRepeat)(op)).min>=1))) {
                    Saxon.Hej.regex.OpRepeat parent = (Saxon.Hej.regex.OpRepeat)(op);
                    Saxon.Hej.regex.Operation child = parent.op;
                    if (((child is Saxon.Hej.regex.OpAtom) || (child is Saxon.Hej.regex.OpCharClass))) {
                        if ((parent.min == 1)) {
                            preconditions.Add(new Saxon.Hej.regex.RegexPrecondition(parent, fixedPosition, minPosition));
                        } else {
                            Saxon.Hej.regex.OpRepeat parent2 = new Saxon.Hej.regex.OpRepeat(child, parent.min, parent.min, true);
                            preconditions.Add(new Saxon.Hej.regex.RegexPrecondition(parent2, fixedPosition, minPosition));
                        }
                    } else {
                        addPrecondition(child, fixedPosition, minPosition);
                    }
                } else {
                    if ((op is Saxon.Hej.regex.OpCapture)) {
                        addPrecondition(((Saxon.Hej.regex.OpCapture)(op)).childOp, fixedPosition, minPosition);
                    } else {
                        if ((op is Saxon.Hej.regex.OpSequence)) {
                            int fp = fixedPosition;
                            int mp = minPosition;
                            foreach (Saxon.Hej.regex.Operation o in ((Saxon.Hej.regex.OpSequence)(op)).getOperations()) {
                                if ((o is Saxon.Hej.regex.OpBOL)) {
                                    fp = 0;
                                }
                                addPrecondition(o, fp, mp);
                                if (((fp != -1) && (o.getMatchLength() != -1))) {
                                    fp += o.getMatchLength();
                                } else {
                                    fp = -1;
                                }
                                mp += o.getMinimumMatchLength();
                            }
                        }
                    }
                }
            }
        }
        public bool isNullable() {
            int m = operation.matchesEmptyString();
            return (((m&Saxon.Hej.regex.Operation.MATCHES_ZLS_ANYWHERE)) != 0);
        }
        public Saxon.Hej.str.UnicodeString getPrefix() {
            return prefix;
        }
    
    }
}