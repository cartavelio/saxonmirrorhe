using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.expr {
    public class SaxonSyntaxExtension : Saxon.Hej.expr.parser.ParserExtension {
        public SaxonSyntaxExtension()  {
        }
        public override Saxon.Hej.expr.Expression parseTypePattern(Saxon.Hej.expr.parser.XPathParser p) {
            Saxon.Hej.type.ItemType type = p.parseItemType();
            return new Saxon.Hej.pattern.ItemTypePattern(type);
        }
        public class UnderscoreFunctionDetails : Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails {
            internal System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> numberedParams = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>();
        
        }
        public override Saxon.Hej.expr.Expression parseDotFunction(Saxon.Hej.expr.parser.XPathParser p) {
            p.checkSyntaxExtensions("dot function");
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails details = new Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails();
            details.outerVariables = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>();
            foreach (Saxon.Hej.expr.LocalBinding lb in p.getRangeVariables()) {
                details.outerVariables.push(lb);
            }
            details.outerVariablesUsed = new System.Collections.Generic.List<Saxon.Hej.expr.LocalBinding>(4);
            details.implicitParams = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>(4);
            p.inlineFunctionStack.push(details);
            p.setRangeVariables(new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>());
            p.nextToken();
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> @params = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>(1);
            Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            Saxon.Hej.om.StructuredQName argQName = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "dot");
            Saxon.Hej.expr.instruct.UserFunctionParameter arg = new Saxon.Hej.expr.instruct.UserFunctionParameter();
            arg.setRequiredType(Saxon.Hej.value.SequenceType.SINGLE_ITEM);
            arg.setVariableQName(argQName);
            arg.setSlotNumber(0);
            @params.Add(arg);
            Saxon.Hej.expr.Expression body = p.parseExpression();
            p.expect(Saxon.Hej.expr.parser.Token.RCURLY);
            t.lookAhead();
            p.nextToken();
            body.setRetainedStaticContext(p.getStaticContext().makeRetainedStaticContext());
            Saxon.Hej.expr.LocalVariableReference @ref = new Saxon.Hej.expr.LocalVariableReference(arg);
            body = new Saxon.Hej.expr.instruct.ForEach(@ref, body);
            Saxon.Hej.expr.Expression result = Saxon.Hej.expr.parser.XPathParser.makeInlineFunctionValue(p, Saxon.Hej.query.AnnotationList.EMPTY, details, @params, resultType, body);
            p.setLocation(result, offset);
            p.setRangeVariables(details.outerVariables);
            p.inlineFunctionStack.pop();
            return result;
        }
        public override Saxon.Hej.expr.Expression parseUnderscoreFunction(Saxon.Hej.expr.parser.XPathParser p) {
            p.checkSyntaxExtensions("underscore function");
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            int offset = t.currentTokenStartOffset;
            Saxon.Eej.expr.SaxonSyntaxExtension.UnderscoreFunctionDetails details = new Saxon.Eej.expr.SaxonSyntaxExtension.UnderscoreFunctionDetails();
            details.outerVariables = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>();
            foreach (Saxon.Hej.expr.LocalBinding lb in p.getRangeVariables()) {
                details.outerVariables.push(lb);
            }
            details.outerVariablesUsed = new System.Collections.Generic.List<Saxon.Hej.expr.LocalBinding>(4);
            details.implicitParams = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>(4);
            p.inlineFunctionStack.push(details);
            p.setRangeVariables(new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>());
            p.nextToken();
            Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            Saxon.Hej.expr.Expression body = p.parseExpression();
            p.expect(Saxon.Hej.expr.parser.Token.RCURLY);
            t.lookAhead();
            p.nextToken();
            body.setRetainedStaticContext(p.getStaticContext().makeRetainedStaticContext());
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> @params = details.numberedParams;
            Saxon.Hej.expr.Expression result = Saxon.Hej.expr.parser.XPathParser.makeInlineFunctionValue(p, Saxon.Hej.query.AnnotationList.EMPTY, details, @params, resultType, body);
            p.setLocation(result, offset);
            p.setRangeVariables(details.outerVariables);
            p.inlineFunctionStack.pop();
            return result;
        }
        public override Saxon.Hej.expr.Expression bindNumericParameterReference(Saxon.Hej.expr.parser.XPathParser p) {
            p.checkSyntaxExtensions("underscore function");
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            int n = -1;
            try {
                n = System.Int32.Parse(t.currentTokenValue);
            } catch (Saxon.Ejava.lang.NumberFormatException err) {
                p.grumble("Numeric parameter reference must be an integer");
            }
            if ((n<=0)) {
                p.grumble("Numeric parameter reference must be a positive integer");
            }
            if ((p.inlineFunctionStack.isEmpty() || !(((p.inlineFunctionStack.peek() is Saxon.Eej.expr.SaxonSyntaxExtension.UnderscoreFunctionDetails))))) {
                p.grumble("Numeric parameter reference must be contained directly in an underscore function");
            }
            Saxon.Eej.expr.SaxonSyntaxExtension.UnderscoreFunctionDetails fn = (Saxon.Eej.expr.SaxonSyntaxExtension.UnderscoreFunctionDetails)(p.inlineFunctionStack.peek());
            if ((n>fn.numberedParams.Count)) {
                for (int k = fn.numberedParams.Count; (k<n); k++) {
                    Saxon.Hej.expr.instruct.UserFunctionParameter arg = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                    arg.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                    Saxon.Hej.om.StructuredQName argQName = new Saxon.Hej.om.StructuredQName("_", Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, ("_"+((k+1))));
                    arg.setVariableQName(argQName);
                    arg.setSlotNumber(k);
                    fn.numberedParams.Add(arg);
                }
            }
            p.nextToken();
            return new Saxon.Hej.expr.LocalVariableReference(fn.numberedParams[(n-1)]);
        }
        public override void handleExternalFunctionDeclaration(Saxon.Hej.query.XQueryParser p, Saxon.Hej.query.XQueryFunction func) {
            Saxon.Hej.query.QueryModule qenv = (Saxon.Hej.query.QueryModule)(p.getStaticContext());
            Saxon.Hej.trans.SymbolicName.F fName = new Saxon.Hej.trans.SymbolicName.F(func.getFunctionName(), func.getNumberOfArguments());
            if (!(qenv.getFunctionLibrary().isAvailable(fName))) {
                p.grumble((("External function "+fName)+" is not available"), "XPST0017");
            }
            Saxon.Hej.om.Function ext = qenv.getFunctionLibrary().getFunctionItem(fName, qenv);
            if ((ext == null)) {
                p.grumble((("External function "+fName)+" is not available"), "XPST0017");
            } else {
                Saxon.Hej.type.TypeHierarchy th = qenv.getConfiguration().getTypeHierarchy();
                if (!(ext.getFunctionItemType().getResultType().isSameType(func.getResultType(), th))) {
                    p.grumble(((("The declared return type of the external function ("+ext.getFunctionItemType().getResultType())+") does not match the return type of the actual function (")+func.getResultType()));
                }
                for (int i = 0; (i<ext.getArity()); i++) {
                    if (!(ext.getFunctionItemType().getArgumentTypes()[i].isSameType(func.getArgumentTypes()[i], th))) {
                        p.grumble(((((("The declared type of argument "+i)+" of the external function (")+ext.getFunctionItemType().getArgumentTypes()[i])+") does not match the argument type of the actual function (")+func.getArgumentTypes()[i]));
                    }
                }
            }
        }
        protected internal override Saxon.Hej.expr.Expression parseForMemberExpression(Saxon.Hej.expr.parser.XPathParser p) {
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            p.checkSyntaxExtensions("'for member'");
            int clauses = 0;
            int @operator = t.currentToken;
            Saxon.Hej.expr.ForMemberExpression first = null;
            Saxon.Hej.expr.ForMemberExpression previous = null;
            do {
                int offset = t.currentTokenStartOffset;
                p.nextToken();
                p.expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                p.nextToken();
                p.expect(Saxon.Hej.expr.parser.Token.NAME);
                string var = t.currentTokenValue;
                clauses++;
                Saxon.Hej.expr.ForMemberExpression v = new Saxon.Hej.expr.ForMemberExpression();
                p.setLocation(v, offset);
                v.setVariableQName(p.makeStructuredQName(var, ""));
                p.nextToken();
                p.expect(Saxon.Hej.expr.parser.Token.IN);
                p.nextToken();
                v.setSequence(p.parseExprSingle());
                p.declareRangeVariable(v);
                if ((previous != null)) {
                    previous.setAction(v);
                } else {
                    first = v;
                }
                previous = v;
            } while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA));
            p.expect(Saxon.Hej.expr.parser.Token.RETURN);
            p.nextToken();
            previous.setAction(p.parseExprSingle());
            for (int i = 0; (i<clauses); i++) {
                p.undeclareRangeVariable();
            }
            return p.makeTracer(first, first.getVariableQName());
        }
    
    }
}