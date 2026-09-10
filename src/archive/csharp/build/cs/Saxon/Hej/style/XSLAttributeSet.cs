using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLAttributeSet : Saxon.Hej.style.StyleElement, Saxon.Hej.style.StylesheetComponent {
        private string nameAtt;
        private string useAtt;
        private string visibilityAtt;
        private Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        private readonly System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> attributeSetElements = new System.Collections.Generic.List<Saxon.Hej.style.ComponentDeclaration>();
        private Saxon.Hej.om.StructuredQName[] useAttributeSetNames;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.Expression> containedInstructions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
        private bool validated = false;
        private Saxon.Hej.trans.Visibility visibility;
        private bool streamable = false;
        public Saxon.Hej.expr.instruct.Actor getActor() {
            return (Saxon.Hej.expr.instruct.AttributeSet)(getPrincipalStylesheetModule().getStylesheetPackage().getComponent(new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET, getObjectName())).getActor());
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET, getObjectName());
        }
        public void checkCompatibility(Saxon.Hej.expr.Component component) {
            if ((((Saxon.Hej.expr.instruct.AttributeSet)(component.getActor())).isDeclaredStreamable() && !(isDeclaredStreamable()))) {
                compileError(("The overridden attribute set is declared streamable, "+"so the overriding attribute set must also be declared streamable"));
            }
        }
        public override bool isDeclaration() {
            return true;
        }
        public Saxon.Hej.om.StructuredQName getAttributeSetName() {
            return getObjectName();
        }
        public bool isDeclaredStreamable() {
            return streamable;
        }
        protected internal override void prepareAttributes() {
            useAtt = null;
            string streamableAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "name":
                    nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "use-attribute-sets":
                    useAtt = value;
                    break ;
                  case "streamable":
                    streamableAtt = value;
                    break ;
                  case "visibility":
                    visibilityAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((nameAtt == null)) {
                reportAbsence("name");
                setObjectName(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "attribute-set-error-name"));
                return ;
            }
            if ((visibilityAtt == null)) {
                visibility = Saxon.Hej.trans.Visibility.PRIVATE;
            } else {
                visibility = interpretVisibilityValue(visibilityAtt, "");
            }
            if ((streamableAtt != null)) {
                streamable = processStreamableAtt(streamableAtt);
            }
            setObjectName(makeQName(nameAtt, null, "name"));
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            Saxon.Hej.om.StructuredQName o = base.getObjectName();
            if ((o == null)) {
                prepareAttributes();
                o = getObjectName();
            }
            return o;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (validated) {
                return ;
            }
            checkTopLevel("XTSE0010", true);
            stackFrameMap = getConfiguration().makeSlotManager();
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLAttribute)) {
                    if ((visibility == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                        compileError("An abstract attribute-set must contain no xsl:attribute instructions");
                    }
                } else {
                    compileError("Only xsl:attribute is allowed within xsl:attribute-set", "XTSE0010");
                }
            }
            if ((useAtt != null)) {
                if ((visibility == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                    compileError("An abstract attribute-set must have no @use-attribute-sets attribute");
                }
                useAttributeSetNames = getUsedAttributeSets(useAtt);
            }
            validated = true;
        }
        public Saxon.Hej.om.StructuredQName[] getUseAttributeSetNames() {
            return useAttributeSetNames;
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            top.indexAttributeSet(decl);
        }
        public void checkCircularity(Saxon.Hej.style.XSLAttributeSet origin) {
            if ((this == origin)) {
                compileError("The definition of the attribute set is circular", "XTSE0720");
            } else {
                if (!(validated)) {
                    return ;
                }
                if ((attributeSetElements != null)) {
                    foreach (Saxon.Hej.style.ComponentDeclaration attributeSetElement in attributeSetElements) {
                        Saxon.Hej.style.XSLAttributeSet element = (Saxon.Hej.style.XSLAttributeSet)(attributeSetElement.getSourceElement());
                        element.checkCircularity(origin);
                        if ((streamable && !(element.streamable))) {
                            compileError(("Attribute-set is declared streamable but references a non-streamable attribute set "+element.getAttributeSetName().getDisplayName()), "XTSE3430");
                        }
                    }
                }
            }
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.Expression> getContainedInstructions() {
            return containedInstructions;
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManager() {
            return stackFrameMap;
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            if (isActionCompleted(ACTION_COMPILE)) {
                return ;
            }
            if ((useAtt != null)) {
                System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UseAttributeSet> invocations = Saxon.Hej.expr.instruct.UseAttributeSet.makeUseAttributeSetInstructions(useAttributeSetNames, this);
                if (!(invocations.Count==0)) {
                    containedInstructions.Add(Saxon.Hej.expr.instruct.UseAttributeSet.makeCompositeExpression(invocations));
                }
                foreach (Saxon.Hej.om.StructuredQName name in useAttributeSetNames) {
                    getPrincipalStylesheetModule().getAttributeSets(name, attributeSetElements);
                }
                foreach (Saxon.Hej.style.ComponentDeclaration attributeSetElement in attributeSetElements) {
                    ((Saxon.Hej.style.XSLAttributeSet)(attributeSetElement.getSourceElement())).checkCircularity(this);
                }
                if (streamable) {
                    foreach (Saxon.Hej.style.ComponentDeclaration attributeSetElement in attributeSetElements) {
                        if (!(((Saxon.Hej.style.XSLAttributeSet)(attributeSetElement.getSourceElement())).streamable)) {
                            compileError(("Attribute set is declared streamable, "+"but references an attribute set that is not declared streamable"), "XTSE0730");
                        }
                    }
                }
            }
            Saxon.Hej.style.XSLAttribute node;
            Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            while (((node = (Saxon.Hej.style.XSLAttribute)(((/*1430*/Saxon.Hej.om.NodeInfo)iter.next()))) != null)) {
                Saxon.Hej.expr.Expression inst = node.compile(compilation, decl);
                inst.setRetainedStaticContext(makeRetainedStaticContext());
                inst = inst.simplify();
                setInstructionLocation(this, inst);
                containedInstructions.Add(inst);
            }
            setActionCompleted(ACTION_COMPILE);
        }
        public void optimize(Saxon.Hej.style.ComponentDeclaration declaration) {
        }
    
    }
}