using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class DocumentNodeTest : Saxon.Hej.pattern.NodeTest {
        private readonly Saxon.Hej.pattern.NodeTest elementTest;
        public DocumentNodeTest(Saxon.Hej.pattern.NodeTest elementTest)  {
            this.elementTest = elementTest;
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.DOCUMENT;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            if ((nodeKind != Saxon.Hej.type.Type.DOCUMENT)) {
                return false;
            }
            throw new Saxon.Ejava.lang.UnsupportedOperationException("DocumentNodeTest doesn't support this method");
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            if ((node.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT)) {
                return false;
            }
            Saxon.Hej.tree.iter.AxisIterator iter = node.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
            bool found = false;
            Saxon.Hej.om.NodeInfo n;
            while (((n = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                int kind = n.getNodeKind();
                if ((kind == Saxon.Hej.type.Type.TEXT)) {
                    return false;
                } else {
                    if ((kind == Saxon.Hej.type.Type.ELEMENT)) {
                        if (found) {
                            return false;
                        }
                        if (elementTest.test(n)) {
                            found = true;
                        } else {
                            return false;
                        }
                    }
                }
            }
            return found;
        }
        public override double getDefaultPriority() {
            return elementTest.getDefaultPriority();
        }
        public override int getPrimitiveType() {
            return Saxon.Hej.type.Type.DOCUMENT;
        }
        public Saxon.Hej.pattern.NodeTest getElementTest() {
            return elementTest;
        }
        public override string ToString() {
            return (("document-node("+elementTest)+')');
        }
        public override int GetHashCode() {
            return (elementTest.GetHashCode()^12345);
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.pattern.DocumentNodeTest) && ((Saxon.Hej.pattern.DocumentNodeTest)(other)).elementTest.Equals(elementTest));
        }
        public override string getFullAlphaCode() {
            return (((getBasicAlphaCode()+" e[")+elementTest.getFullAlphaCode())+"]");
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
            Saxon.Hej.tree.iter.AxisIterator iter = node.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
            bool found = false;
            Saxon.Hej.om.NodeInfo n;
            while (((n = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                int kind = n.getNodeKind();
                if ((kind == Saxon.Hej.type.Type.TEXT)) {
                    return Saxon.Ejava.util.Optional<string>.of("The supplied document node has text node children");
                } else {
                    if ((kind == Saxon.Hej.type.Type.ELEMENT)) {
                        if (found) {
                            return Saxon.Ejava.util.Optional<string>.of("The supplied document node has more than one element child");
                        }
                        if (elementTest.test(n)) {
                            found = true;
                        } else {
                            string s = (("The supplied document node has an element child ("+Saxon.Hej.trans.Err.depict(n))+") that does not satisfy the element test");
                            Saxon.Ejava.util.Optional<string> more = elementTest.explainMismatch(n, th);
                            if (more.isPresent()) {
                                s += (". "+more.get());
                            }
                            return Saxon.Ejava.util.Optional<string>.of(s);
                        }
                    }
                }
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
    
    }
}