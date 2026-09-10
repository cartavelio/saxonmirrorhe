using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class ComponentTest {
        private readonly int componentKind;
        private readonly Saxon.Hej.pattern.QNameTest nameTest;
        private readonly int arity;
        public ComponentTest(int componentKind, Saxon.Hej.pattern.QNameTest nameTest, int arity)  {
            this.componentKind = componentKind;
            this.nameTest = nameTest;
            this.arity = arity;
        }
        public int getComponentKind() {
            return componentKind;
        }
        public Saxon.Hej.pattern.QNameTest getQNameTest() {
            return nameTest;
        }
        public int getArity() {
            return arity;
        }
        public bool isPartialWildcard() {
            return ((nameTest is Saxon.Hej.pattern.LocalNameTest) || (nameTest is Saxon.Hej.pattern.NamespaceTest));
        }
        public bool matches(Saxon.Hej.expr.instruct.Actor component) {
            return matches(component.getSymbolicName());
        }
        public bool matches(Saxon.Hej.trans.SymbolicName sn) {
            return (((((componentKind == -1) || (sn.getComponentKind() == componentKind))) && nameTest.matches(sn.getComponentName())) && !((((((componentKind == Saxon.Hej.om.StandardNames.XSL_FUNCTION)) && (arity != -1)) && (arity != ((Saxon.Hej.trans.SymbolicName.F)(sn)).getArity())))));
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicNameIfExplicit() {
            if ((nameTest is Saxon.Hej.pattern.NameTest)) {
                if ((componentKind == Saxon.Hej.om.StandardNames.XSL_FUNCTION)) {
                    return new Saxon.Hej.trans.SymbolicName.F(((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName(), arity);
                } else {
                    return new Saxon.Hej.trans.SymbolicName(componentKind, ((Saxon.Hej.pattern.NameTest)(nameTest)).getMatchingNodeName());
                }
            } else {
                return null;
            }
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.trans.ComponentTest) && (((Saxon.Hej.trans.ComponentTest)(other)).componentKind == componentKind)) && (((Saxon.Hej.trans.ComponentTest)(other)).arity == arity)) && ((Saxon.Hej.trans.ComponentTest)(other)).nameTest.Equals(nameTest));
        }
        public override int GetHashCode() {
            return ((componentKind^arity)^nameTest.GetHashCode());
        }
    
    }
}