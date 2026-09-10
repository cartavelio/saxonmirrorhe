using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.oper {
    public class OperandArray : System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> {
        private readonly Saxon.Hej.expr.Operand[] operandArray;
        public OperandArray(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.Expression[] args)  {
            this.operandArray = new Saxon.Hej.expr.Operand[args.Length];
            for (int i = 0; (i<args.Length); i++) {
                operandArray[i] = new Saxon.Hej.expr.Operand(parent, args[i], Saxon.Hej.expr.OperandRole.NAVIGATE);
            }
        }
        public OperandArray(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.Expression[] args, Saxon.Hej.expr.OperandRole[] roles)  {
            this.operandArray = new Saxon.Hej.expr.Operand[args.Length];
            for (int i = 0; (i<args.Length); i++) {
                operandArray[i] = new Saxon.Hej.expr.Operand(parent, args[i], roles[i]);
            }
        }
        public OperandArray(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.Expression[] args, Saxon.Hej.expr.OperandRole role)  {
            this.operandArray = new Saxon.Hej.expr.Operand[args.Length];
            for (int i = 0; (i<args.Length); i++) {
                operandArray[i] = new Saxon.Hej.expr.Operand(parent, args[i], role);
            }
        }
        private OperandArray(Saxon.Hej.expr.Operand[] operands)  {
            this.operandArray = operands;
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.expr.Operand> GetEnumerator() {
            return Saxon.Ejava.util.Arrays.asList(operandArray).GetEnumerator();
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public Saxon.Hej.expr.Operand[] copy() {
            return Saxon.Ejava.util.Arrays.copyOf(operandArray, operandArray.Length);
        }
        public Saxon.Hej.expr.OperandRole[] getRoles() {
            Saxon.Hej.expr.OperandRole[] or = new Saxon.Hej.expr.OperandRole[operandArray.Length];
            for (int i = 0; (i<or.Length); i++) {
                or[i] = operandArray[i].getOperandRole();
            }
            return or;
        }
        public Saxon.Hej.expr.Operand getOperand(int n) {
            try {
                return operandArray[n];
            } catch (System.IndexOutOfRangeException a) {
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        public Saxon.Hej.expr.Expression getOperandExpression(int n) {
            try {
                return operandArray[n].getChildExpression();
            } catch (System.IndexOutOfRangeException a) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(a);
            }
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return this;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Expression> operandExpressions() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(operandArray.Length);
            foreach (Saxon.Hej.expr.Operand o in this) {
                list.Add(o.getChildExpression());
            }
            return list;
        }
        public void setOperand(int n, Saxon.Hej.expr.Expression child) {
            try {
                if ((operandArray[n].getChildExpression() != child)) {
                    operandArray[n].setChildExpression(child);
                }
            } catch (System.IndexOutOfRangeException a) {
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        public int getNumberOfOperands() {
            return operandArray.Length;
        }
        public static bool every<T> (T[] args, System.Predicate<T> condition) {
            foreach (T arg in args) {
                if (!(condition(arg))) {
                    return false;
                }
            }
            return true;
        }
        public static bool some<T> (T[] args, System.Predicate<T> condition) {
            foreach (T arg in args) {
                if (condition(arg)) {
                    return true;
                }
            }
            return false;
        }
    
    }
}