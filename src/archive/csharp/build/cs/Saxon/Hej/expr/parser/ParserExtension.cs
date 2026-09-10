using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class ParserExtension {
        public ParserExtension()  {
        }
        public void needExtension(Saxon.Hej.expr.parser.XPathParser p, string what) {
            p.grumble((what+" require support for Saxon extensions, available in Saxon-PE or higher"));
        }
        private void needUpdate(Saxon.Hej.expr.parser.XPathParser p, string what) {
            p.grumble((what+" requires support for XQuery Update, available in Saxon-EE or higher"));
        }
        public virtual void handleExternalFunctionDeclaration(Saxon.Hej.query.XQueryParser p, Saxon.Hej.query.XQueryFunction func) {
            needExtension(p, "External function declarations");
        }
        public Saxon.Hej.type.ItemType parseExtendedItemType(Saxon.Hej.expr.parser.XPathParser p) {
            return null;
        }
        public virtual Saxon.Hej.expr.Expression parseTypePattern(Saxon.Hej.expr.parser.XPathParser p) {
            needExtension(p, "type-based patterns");
            return null;
        }
        public class TemporaryXSLTVariableBinding : Saxon.Hej.expr.LocalBinding {
            internal Saxon.Hej.style.SourceBinding declaration;
            public TemporaryXSLTVariableBinding(Saxon.Hej.style.SourceBinding decl)  {
                this.declaration = decl;
            }
            public Saxon.Hej.value.SequenceType getRequiredType() {
                return declaration.getInferredType(true);
            }
            public Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext context) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
            public bool isGlobal() {
                return false;
            }
            public bool isAssignable() {
                return false;
            }
            public int getLocalSlotNumber() {
                return 0;
            }
            public Saxon.Hej.om.StructuredQName getVariableQName() {
                return declaration.getVariableQName();
            }
            public void addReference(Saxon.Hej.expr.VariableReference @ref, bool isLoopingReference) {
            }
            public Saxon.Hej.value.IntegerValue[] getIntegerBoundsForVariable() {
                return null;
            }
            public void setIndexedVariable() {
            }
            public bool isIndexedVariable() {
                return false;
            }
        
        }
        public virtual void parseTypeAliasDeclaration(Saxon.Hej.query.XQueryParser p) {
            needExtension(p, "Type alias declarations");
        }
        public virtual void parseRevalidationDeclaration(Saxon.Hej.query.XQueryParser p) {
            needUpdate(p, "A revalidation declaration");
        }
        public virtual void parseUpdatingFunctionDeclaration(Saxon.Hej.query.XQueryParser p) {
            needUpdate(p, "An updating function");
        }
        protected internal virtual Saxon.Hej.expr.Expression parseExtendedExprSingle(Saxon.Hej.expr.parser.XPathParser p) {
            return null;
        }
        protected internal virtual Saxon.Hej.expr.Expression parseForMemberExpression(Saxon.Hej.expr.parser.XPathParser p) {
            return null;
        }
    
    }
}