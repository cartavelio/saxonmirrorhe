using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class ParserExtension {
        public ParserExtension()  {
        }
        private void needExtension(Saxon.Hej.expr.parser.XPathParser p, string what) {
            p.grumble((what+" require support for Saxon extensions, available in Saxon-PE or higher"));
        }
        private void needUpdate(Saxon.Hej.expr.parser.XPathParser p, string what) {
            p.grumble((what+" requires support for XQuery Update, available in Saxon-EE or higher"));
        }
        public Saxon.Hej.expr.Expression parseNamedFunctionReference(Saxon.Hej.expr.parser.XPathParser p) {
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            string fname = t.currentTokenValue;
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.StaticContext env = p.getStaticContext();
            p.nextToken();
            p.expect(Saxon.Hej.expr.parser.Token.NUMBER);
            Saxon.Hej.value.NumericValue number = Saxon.Hej.value.NumericValue.parseNumber(t.currentTokenValue);
            if (!(((number is Saxon.Hej.value.IntegerValue)))) {
                p.grumble("Number following '#' must be an integer");
            }
            if (((number.compareTo(0)<0) || (number.compareTo(System.Int32.MaxValue)>0))) {
                p.grumble("Number following '#' is out of range", "FOAR0002");
            }
            int arity = (int)(number.longValue());
            p.nextToken();
            Saxon.Hej.om.StructuredQName functionName = null;
            try {
                functionName = p.getQNameParser().parse(fname, env.getDefaultFunctionNamespace());
                if (functionName.getPrefix().Equals("")) {
                    if (Saxon.Hej.expr.parser.XPathParser.isReservedFunctionName31(functionName.getLocalPart())) {
                        p.grumble((("The unprefixed function name '"+functionName.getLocalPart())+"' is reserved in XPath 3.1"));
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                p.grumble(e.Message, e.getErrorCodeLocalPart());
                System.Diagnostics.Debug.Assert((functionName != null));
            }
            Saxon.Hej.om.Function fcf = null;
            try {
                Saxon.Hej.functions.FunctionLibrary lib = env.getFunctionLibrary();
                Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(functionName, arity);
                fcf = lib.getFunctionItem(sn, env);
                if ((fcf == null)) {
                    p.grumble((((("Function "+functionName.getEQName())+"#")+arity)+" not found"), "XPST0017", offset);
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                p.grumble(e.Message, "XPST0017", offset);
            }
            if ((functionName.hasURI(Saxon.Hej.lib.NamespaceConstant.FN) && (fcf is Saxon.Hej.functions.SystemFunction))) {
                Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry details = ((Saxon.Hej.functions.SystemFunction)(fcf)).getDetails();
                if (((details != null) && (((details.properties&((Saxon.Hej.functions.registry.BuiltInFunctionSet.FOCUS|Saxon.Hej.functions.registry.BuiltInFunctionSet.DEPENDS_ON_STATIC_CONTEXT)))) != 0))) {
                    Saxon.Hej.functions.SystemFunction lookup = Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance().makeFunction("function-lookup", 2);
                    lookup.setRetainedStaticContext(env.makeRetainedStaticContext());
                    return lookup.makeFunctionCall(Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.QNameValue(functionName, Saxon.Hej.type.BuiltInAtomicType.QNAME)), Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(arity)));
                }
            }
            Saxon.Hej.expr.Expression @ref = makeNamedFunctionReference(functionName, fcf);
            p.setLocation(@ref, offset);
            return @ref;
        }
        private static Saxon.Hej.expr.Expression makeNamedFunctionReference(Saxon.Hej.om.StructuredQName functionName, Saxon.Hej.om.Function fcf) {
            if (((fcf is Saxon.Hej.expr.instruct.UserFunction) && !(functionName.hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT)))) {
                return new Saxon.Hej.functions.hof.UserFunctionReference((Saxon.Hej.expr.instruct.UserFunction)(fcf));
            } else {
                if ((fcf is Saxon.Hej.functions.hof.UnresolvedXQueryFunctionItem)) {
                    return ((Saxon.Hej.functions.hof.UnresolvedXQueryFunctionItem)(fcf)).getFunctionReference();
                } else {
                    return new Saxon.Hej.functions.hof.FunctionLiteral(fcf);
                }
            }
        }
        public Saxon.Hej.type.ItemType parseFunctionItemType(Saxon.Hej.expr.parser.XPathParser p, Saxon.Hej.query.AnnotationList annotations) {
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            p.nextToken();
            System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> argTypes = new System.Collections.Generic.List<Saxon.Hej.value.SequenceType>(3);
            Saxon.Hej.value.SequenceType resultType;
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.STAR) || (t.currentToken == Saxon.Hej.expr.parser.Token.MULT))) {
                p.nextToken();
                p.expect(Saxon.Hej.expr.parser.Token.RPAR);
                p.nextToken();
                if (annotations.isEmpty()) {
                    return Saxon.Hej.type.AnyFunctionType.getInstance();
                } else {
                    return new Saxon.Hej.type.AnyFunctionTypeWithAssertions(annotations, p.getStaticContext().getConfiguration());
                }
            } else {
                while ((t.currentToken != Saxon.Hej.expr.parser.Token.RPAR)) {
                    Saxon.Hej.value.SequenceType arg = p.parseSequenceType();
                    argTypes.Add(arg);
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                        break ;
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                            p.nextToken();
                        } else {
                            p.grumble((("Expected ',' or ')' after function argument type, found '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+'\''));
                        }
                    }
                }
                p.nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                    p.nextToken();
                    resultType = p.parseSequenceType();
                    Saxon.Hej.value.SequenceType[] argArray = new Saxon.Hej.value.SequenceType[argTypes.Count];
                    argArray = argTypes.ToArray();
                    return new Saxon.Hej.type.SpecificFunctionType(argArray, resultType, annotations);
                } else {
                    if (!(argTypes.Count==0)) {
                        p.grumble("Result type must be given if an argument type is given: expected 'as (type)'");
                        return null;
                    } else {
                        p.grumble("function() is no longer allowed for a general function type: must be function(*)");
                        return null;
                    }
                }
            }
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
        public virtual Saxon.Hej.expr.Expression parseDotFunction(Saxon.Hej.expr.parser.XPathParser p) {
            needExtension(p, "Dot functions");
            return null;
        }
        public virtual Saxon.Hej.expr.Expression parseUnderscoreFunction(Saxon.Hej.expr.parser.XPathParser p) {
            needExtension(p, "Underscore functions");
            return null;
        }
        public virtual Saxon.Hej.expr.Expression bindNumericParameterReference(Saxon.Hej.expr.parser.XPathParser p) {
            needExtension(p, "Underscore functions");
            return null;
        }
        public Saxon.Hej.expr.Expression makeCurriedFunction(Saxon.Hej.expr.parser.XPathParser parser, int offset, Saxon.Hej.om.StructuredQName name, Saxon.Hej.expr.Expression[] args, Saxon.Hej.z.IntSet placeMarkers) {
            Saxon.Hej.expr.StaticContext env = parser.getStaticContext();
            Saxon.Hej.functions.FunctionLibrary lib = env.getFunctionLibrary();
            Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(name, args.Length);
            Saxon.Hej.om.Function target = lib.getFunctionItem(sn, env);
            if ((target == null)) {
                System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
                return parser.reportMissingFunction(offset, name, args, reasons);
            }
            Saxon.Hej.expr.Expression targetExp = makeNamedFunctionReference(name, target);
            parser.setLocation(targetExp, offset);
            return curryFunction(targetExp, args, placeMarkers);
        }
        public static Saxon.Hej.expr.Expression curryFunction(Saxon.Hej.expr.Expression functionExp, Saxon.Hej.expr.Expression[] args, Saxon.Hej.z.IntSet placeMarkers) {
            Saxon.Hej.z.IntIterator ii = placeMarkers.iterator();
            while (ii.hasNext()) {
                args[ii.next()] = null;
            }
            return new Saxon.Hej.functions.hof.PartialApply(functionExp, args);
        }
        public Saxon.Hej.expr.Expression createDynamicCurriedFunction(Saxon.Hej.expr.parser.XPathParser p, Saxon.Hej.expr.Expression functionItem, System.Collections.Generic.List<Saxon.Hej.expr.Expression> args, Saxon.Hej.z.IntSet placeMarkers) {
            Saxon.Hej.expr.Expression[] arguments = new Saxon.Hej.expr.Expression[args.Count];
            arguments = args.ToArray();
            Saxon.Hej.expr.Expression result = curryFunction(functionItem, arguments, placeMarkers);
            p.setLocation(result, p.getTokenizer().currentTokenStartOffset);
            return result;
        }
        public virtual void handleExternalFunctionDeclaration(Saxon.Hej.query.XQueryParser p, Saxon.Hej.query.XQueryFunction func) {
            needExtension(p, "External function declarations");
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