package com.saxonica.expr;

import com.saxonica.functions.hof.FunctionLiteral;
import com.saxonica.functions.hof.PartialApply;
import com.saxonica.functions.hof.UnresolvedXQueryFunctionItem;
import com.saxonica.functions.hof.UserFunctionReference;
import com.saxonica.functions.registry.XPath31HOFunctionSet;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.instruct.UserFunctionParameter;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.map.TupleItemType;
import net.sf.saxon.om.*;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.XQueryFunction;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.style.ExpressionContext;
import net.sf.saxon.style.SourceBinding;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.*;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSet;

import java.util.*;

/**
 * Parser extension for syntax in XPath that is accepted only in Saxon-PE and -EE. Primarily, this means
 * XPath 3.0 syntax associated with higher-order functions.
 */
public class HofParserExtension extends ParserExtension {

    protected Stack<InlineFunctionDetails> inlineFunctionStack = new Stack<>();

    public HofParserExtension() {
    }

    /**
     * Parse a literal function item (function#arity). On entry, the function name and
     * the '#' token have already been read
     *
     * @param p the parser
     * @return an expression representing the function value
     * @throws XPathException in the event of a syntax error
     */

    public Expression parseNamedFunctionReference(XPathParser p) throws XPathException {

        Tokenizer t = p.getTokenizer();
        String fname = t.currentTokenValue;
        int offset = t.currentTokenStartOffset;

        StaticContext env = p.getStaticContext();

        // the "#" has already been read by the Tokenizer: now parse the arity

        p.nextToken();
        p.expect(Token.NUMBER);
        NumericValue number = NumericValue.parseNumber(t.currentTokenValue);
        if (!(number instanceof IntegerValue)) {
            p.grumble("Number following '#' must be an integer");
        }
        if (number.compareTo(0) < 0 || number.compareTo(Integer.MAX_VALUE) > 0) {
            p.grumble("Number following '#' is out of range", "FOAR0002");
        }
        int arity = (int) number.longValue();
        p.nextToken();
        StructuredQName functionName = null;

        p.getQNameParser().setDefaultNamespace(env.getDefaultFunctionNamespace());
        try {
            functionName = p.getQNameParser().parse(fname);
            if (functionName.getPrefix().equals("")) {
                if (XPathParser.isReservedFunctionName31(functionName.getLocalPart())) {
                    p.grumble("The unprefixed function name '" + functionName.getLocalPart() + "' is reserved in XPath 3.1");
                }
            }
        } catch (XPathException e) {
            p.grumble(e.getMessage(), e.getErrorCodeLocalPart());
            assert functionName != null;
        }

        Function fcf = null;
        try {
            FunctionLibrary lib = env.getFunctionLibrary();
            SymbolicName.F sn = new SymbolicName.F(functionName, arity);
            fcf = lib.getFunctionItem(sn, env);
            if (fcf == null) {
                p.grumble("Function " + functionName.getEQName() + "#" + arity + " not found", "XPST0017", offset);
            }
        } catch (XPathException e) {
            p.grumble(e.getMessage(), "XPST0017", offset);
        }

        // Special treatment of functions in the system function library that depend on dynamic context; turn these
        // into calls on function-lookup()

        if (functionName.hasURI(NamespaceConstant.FN) && fcf instanceof SystemFunction) {
            final BuiltInFunctionSet.Entry details = ((SystemFunction)fcf).getDetails();
            if (details != null &&
                    (details.properties & (BuiltInFunctionSet.FOCUS | BuiltInFunctionSet.DEPENDS_ON_STATIC_CONTEXT)) != 0) {
                // For a context-dependent function, return a call on function-lookup(), which saves the context
                SystemFunction lookup = XPath31HOFunctionSet.getInstance().makeFunction("function-lookup", 2);
                lookup.setRetainedStaticContext(env.makeRetainedStaticContext());
                return lookup.makeFunctionCall(Literal.makeLiteral(new QNameValue(functionName, BuiltInAtomicType.QNAME)),
                                               Literal.makeLiteral(Int64Value.makeIntegerValue(arity)));
            }
        }

        Expression ref = makeNamedFunctionReference(functionName, fcf);
        p.setLocation(ref, offset);
        return ref;
    }

    private static Expression makeNamedFunctionReference(StructuredQName functionName, Function fcf) {
        if (fcf instanceof UserFunction && !functionName.hasURI(NamespaceConstant.XSLT)) {
            // This case is treated specially because a UserFunctionReference in XSLT can be redirected
            // at link time to an overriding function. However, this doesn't apply to xsl:original
            return new UserFunctionReference((UserFunction) fcf);
        } else if (fcf instanceof UnresolvedXQueryFunctionItem) {
            return ((UnresolvedXQueryFunctionItem) fcf).getFunctionReference();
        } else {
            return new FunctionLiteral(fcf);
        }
    }

    /**
     * Parse the item type used for function items (XQuery 3.0 higher order functions)
     * Syntax (changed by WG decision on 2009-09-22):
     * function '(' '*' ') |
     * function '(' (SeqType (',' SeqType)*)? ')' 'as' SeqType
     * For backwards compatibility with Saxon 9.2 we allow the "*" to be omitted for the time being
     * The "function(" has already been read
     * @param annotations the list of annotation assertions for this function item type
     */

    public ItemType parseFunctionItemType(XPathParser p, AnnotationList annotations) throws XPathException {
        Tokenizer t = p.getTokenizer();
        p.nextToken();
        List<SequenceType> argTypes = new ArrayList<>(3);
        SequenceType resultType;

        if (t.currentToken == Token.STAR || t.currentToken == Token.MULT) {
            // Allow both to be safe
            p.nextToken();
            p.expect(Token.RPAR);
            p.nextToken();
            if (annotations.isEmpty()) {
                return AnyFunctionType.getInstance();
            } else {
                return new AnyFunctionTypeWithAssertions(annotations);
            }
        } else {
            while (t.currentToken != Token.RPAR) {
                SequenceType arg = p.parseSequenceType();
                argTypes.add(arg);
                if (t.currentToken == Token.RPAR) {
                    break;
                } else if (t.currentToken == Token.COMMA) {
                    p.nextToken();
                } else {
                    p.grumble("Expected ',' or ')' after function argument type, found '" +
                                      Token.tokens[t.currentToken] + '\'');
                }
            }
            p.nextToken();
            if (t.currentToken == Token.AS) {
                p.nextToken();
                resultType = p.parseSequenceType();
                SequenceType[] argArray = new SequenceType[argTypes.size()];
                argArray = argTypes.toArray(argArray);
                return new SpecificFunctionType(argArray, resultType, annotations);
            } else if (!argTypes.isEmpty()) {
                p.grumble("Result type must be given if an argument type is given: expected 'as (type)'");
                return null;
            } else {
                p.grumble("function() is no longer allowed for a general function type: must be function(*)");
                return null;
                // in the new syntax adopted on 2009-09-22, this case is an error
                // return AnyFunctionType.getInstance();
            }
        }
    }

    /**
     * Parse an ItemType within a SequenceType
     *
     * @return the ItemType after parsing
     * @throws XPathException if a static error is found
     */

    public ItemType parseExtendedItemType(XPathParser p) throws XPathException {
        Tokenizer t = p.getTokenizer();
        if (t.currentToken == Token.TILDE) {
            // Saxon extension: type aliases
            p.checkSyntaxExtensions("type alias");
            p.nextToken();
            p.expect(Token.NAME);
            StructuredQName typeAlias = p.getQNameParser().parse(t.currentTokenValue);
            ItemType realType = p.getStaticContext().resolveTypeAlias(typeAlias);
            if (realType == null) {
                p.grumble("The type alias ~" + typeAlias.getDisplayName() + " has not been defined");
            }
            p.nextToken();
            return realType;
        } else if (t.currentToken == Token.NODEKIND && t.currentTokenValue.equals("tuple")) {
            // Saxon 9.8 extension: tuple types
            return parseTupleType(p);
        } else if (t.currentToken == Token.NODEKIND && t.currentTokenValue.equals("union")) {
            // Saxon 9.8 extension: union types
            return parseUnionType(p);
        } else {
            return null;
        }
    }

    /**
     * Parse an extended XSLT pattern in the form ~type-alias (predicate)*  (but excluding the
     * predicates)
     *
     * @return the equivalent expression in the form .[. instance of type] (predicate)*
     */

    public Expression parseTildePattern(XPathParser p) throws XPathException {
        Tokenizer t = p.getTokenizer();
        ItemType type = parseExtendedItemType(p);
        Expression expr = new InstanceOfExpression(
                new ContextItemExpression(), SequenceType.makeSequenceType(type, StaticProperty.EXACTLY_ONE));
        expr = new FilterExpression(new ContextItemExpression(), expr);
        return expr;
    }

    /**
     * Parse a tuple type (Saxon 9.8 extension).
     * Syntax: "tuple" "(" (name (":" sequenceType)?) ("," (name (":" sequenceType)?))* ")"
     */

    private ItemType parseTupleType(XPathParser p) throws XPathException {
        // The initial "tuple(" has been read
        Tokenizer t = p.getTokenizer();
        p.nextToken();
        List<String> fieldNames = new ArrayList<>(6);
        List<SequenceType> fieldTypes = new ArrayList<>(6);
        Set<String> optionalFields = new HashSet<>(6);
        boolean extensible = false;
        while (true) {
            p.expect(Token.NAME);
            String name = t.currentTokenValue;
            if (!NameChecker.isValidNCName(name)) {
                p.grumble(Err.wrap(name) + " is not a valid NCName");
            }
            fieldNames.add(name);
            p.nextToken();
            if (t.currentToken == Token.QMARK) {
                p.nextToken();
                optionalFields.add(name);
            }
            SequenceType arg = SequenceType.ANY_SEQUENCE;
            if (t.currentToken == Token.COLON) {
                p.nextToken();
                arg = p.parseSequenceType();
            }
            fieldTypes.add(arg);
            if (t.currentToken == Token.RPAR) {
                break;
            } else if (t.currentToken == Token.COMMA) {
                p.nextToken();
                if (t.currentToken == Token.STAR || t.currentToken == Token.MULT) {
                    extensible = true;
                    p.nextToken();
                    p.expect(Token.RPAR);
                    break;
                }
            } else {
                p.grumble("Expected ',' or ')' after field in tuple type, found '" +
                                  Token.tokens[t.currentToken] + '\'');
            }
        }
        p.nextToken();
        if (fieldNames.isEmpty()) {
            p.grumble("Tuple type must contain at least one field");
            return null;
        }
        return new TupleItemType(fieldNames, fieldTypes, optionalFields, extensible);

    }

    /**
     * Parse a union type (Saxon 9.8 extension).
     * Syntax: "union" "(" qname ("," qname)* ")"
     */

    public ItemType parseUnionType(XPathParser p) throws XPathException {
        p.getStaticContext().getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION, "union types", -1);
        // The initial "union(" has been read
        Tokenizer t = p.getTokenizer();
        p.nextToken();
        List<AtomicType> memberTypes = new ArrayList<>(6);

        while (true) {
            p.expect(Token.NAME);
            StructuredQName member = p.getQNameParser().parse(t.currentTokenValue);
            ItemType type = p.getPlainType(member);
            if (type instanceof AtomicType) {
                memberTypes.add((AtomicType) type);
            } else {
                p.grumble("Type " + t.currentTokenValue + " exists, but is not atomic");
            }
            p.nextToken();
            if (t.currentToken == Token.RPAR) {
                break;
            } else if (t.currentToken == Token.COMMA) {
                p.nextToken();
            } else {
                p.grumble("Expected ',' or ')' after member name in union type, found '" +
                                  Token.tokens[t.currentToken] + '\'');
            }
        }
        p.nextToken();
        ItemType type = p.getStaticContext().getConfiguration().makeUserUnionType(memberTypes);
        return type == null ? BuiltInAtomicType.ANY_ATOMIC : type;

    }


    /**
     * Parse a function argument. The special marker "?" is allowed and causes "null"
     * to be returned
     */

    public Expression makeArgumentPlaceMarker(XPathParser p) {
        return null;
    }

    public static class InlineFunctionDetails {
        Stack<LocalBinding> outerVariables;    // Local variables defined in the immediate outer scope (the father scope)
        List<LocalBinding> outerVariablesUsed; // Local variables from the outer scope that are actually used
        List<UserFunctionParameter> implicitParams; // Parameters corresponding (1:1) with the above
    }

    public static class TemporaryXSLTVariableBinding implements LocalBinding {
        SourceBinding declaration;

        public TemporaryXSLTVariableBinding(SourceBinding decl) {
            this.declaration = decl;
        }

        public SequenceType getRequiredType() {
            return declaration.getInferredType(true);
        }

        public Sequence<?> evaluateVariable(XPathContext context) {
            throw new UnsupportedOperationException();
        }

        public boolean isGlobal() {
            return false;
        }


        public boolean isAssignable() {
            return false;
        }

        public int getLocalSlotNumber() {
            return 0;
        }

        public StructuredQName getVariableQName() {
            return declaration.getVariableQName();
        }

        public void addReference(VariableReference ref, boolean isLoopingReference) {

        }

        public IntegerValue[] getIntegerBoundsForVariable() {
            return null;
        }

        @Override
        public void setIndexedVariable() {}

        @Override
        public boolean isIndexedVariable() {
            return false;
        }
    }

    protected Expression parseInlineFunction(XPathParser p, AnnotationList annotations) throws XPathException {
        // the next token should be the < QNAME "("> pair

        Tokenizer t = p.getTokenizer();
        int offset = t.currentTokenStartOffset;

        InlineFunctionDetails details = new InlineFunctionDetails();
        details.outerVariables = new Stack<>();
        for (LocalBinding lb : p.getRangeVariables()) {
            details.outerVariables.push(lb);
        }
        details.outerVariablesUsed = new ArrayList<>(4);
        details.implicitParams = new ArrayList<>(4);
        inlineFunctionStack.push(details);
        p.setRangeVariables(new Stack<>());
        p.nextToken();
        HashSet<StructuredQName> paramNames = new HashSet<>(8);
        List<UserFunctionParameter> params = new ArrayList<>(8);
        SequenceType resultType = SequenceType.ANY_SEQUENCE;
        int paramSlot = 0;
        while (t.currentToken != Token.RPAR) {
            //     ParamList   ::=     Param ("," Param)*
            //     Param       ::=     "$" VarName  TypeDeclaration?
            p.expect(Token.DOLLAR);
            p.nextToken();
            p.expect(Token.NAME);
            String argName = t.currentTokenValue;
            StructuredQName argQName = p.makeStructuredQName(argName, "");
            if (paramNames.contains(argQName)) {
                p.grumble("Duplicate parameter name " + Err.wrap(t.currentTokenValue, Err.VARIABLE), "XQST0039");
            }
            paramNames.add(argQName);
            SequenceType paramType = SequenceType.ANY_SEQUENCE;
            p.nextToken();
            if (t.currentToken == Token.AS) {
                p.nextToken();
                paramType = p.parseSequenceType();
            }

            UserFunctionParameter arg = new UserFunctionParameter();
            arg.setRequiredType(paramType);
            arg.setVariableQName(argQName);
            arg.setSlotNumber(paramSlot++);
            params.add(arg);
            p.declareRangeVariable(arg);
            if (t.currentToken == Token.RPAR) {
                break;
            } else if (t.currentToken == Token.COMMA) {
                p.nextToken();
            } else {
                p.grumble("Expected ',' or ')' after function argument, found '" +
                                  Token.tokens[t.currentToken] + '\'');
            }
        }
        t.setState(Tokenizer.BARE_NAME_STATE);
        p.nextToken();
        if (t.currentToken == Token.AS) {
            t.setState(Tokenizer.SEQUENCE_TYPE_STATE);
            p.nextToken();
            resultType = p.parseSequenceType();
        }
        p.expect(Token.LCURLY);
        t.setState(Tokenizer.DEFAULT_STATE);
        p.nextToken();
        Expression body;
        if (t.currentToken == Token.RCURLY && p.isAllowXPath31Syntax()) {
            t.lookAhead();
            p.nextToken();
            body = Literal.makeEmptySequence();
        } else {
            body = p.parseExpression();
            p.expect(Token.RCURLY);
            t.lookAhead();  // must be done manually after an RCURLY
            p.nextToken();
        }
        ExpressionTool.setDeepRetainedStaticContext(body, p.getStaticContext().makeRetainedStaticContext());

        int arity = paramNames.size();
        for (int i = 0; i < arity; i++) {
            p.undeclareRangeVariable();
        }

        Expression result = makeInlineFunctionValue(p, annotations, details, params, resultType, body);

        p.setLocation(result, offset);
        // restore the previous stack of range variables
        p.setRangeVariables(details.outerVariables);
        inlineFunctionStack.pop();
        return result;
    }

    public static Expression makeInlineFunctionValue(
            XPathParser p, AnnotationList annotations,
            InlineFunctionDetails details, List<UserFunctionParameter> params,
            SequenceType resultType, Expression body) {
        // Does this function access any outer variables?
        // If so, we create a UserFunction in which the outer variables are defined as extra parameters
        // in addition to the declared parameters, and then we return a call to partial-apply() that
        // sets these additional parameters to the values they have in the calling context.
        int arity = params.size();

        UserFunction uf = new UserFunction();
        uf.setFunctionName(new StructuredQName("anon", NamespaceConstant.ANONYMOUS, "f_" + uf.hashCode()));
        uf.setPackageData(p.getStaticContext().getPackageData());
        uf.setBody(body);
        uf.setAnnotations(annotations);
        uf.setResultType(resultType);
        uf.incrementReferenceCount();

        if (uf.getPackageData() instanceof StylesheetPackage) {
            // Add the inline function as a private component to the package, so that it can have binding
            // slots allocated for any references to global variables or functions, and so that it will
            // be copied as a hidden component into any using packages
            StylesheetPackage pack = (StylesheetPackage) uf.getPackageData();
            Component comp = Component.makeComponent(uf, Visibility.PRIVATE, VisibilityProvenance.DEFAULTED, pack, pack);
            uf.setDeclaringComponent(comp);
        }

        Expression result;
        List<UserFunctionParameter> implicitParams = details.implicitParams;
        if (!implicitParams.isEmpty()) {
            int extraParams = implicitParams.size();
            int expandedArity = params.size() + extraParams;
            UserFunctionParameter[] paramArray = new UserFunctionParameter[expandedArity];
            for (int i = 0; i < params.size(); i++) {
                paramArray[i] = params.get(i);
            }
            int k = params.size();
            for (UserFunctionParameter implicitParam : implicitParams) {
                paramArray[k++] = implicitParam;
            }
            uf.setParameterDefinitions(paramArray);
            SlotManager stackFrame = p.getStaticContext().getConfiguration().makeSlotManager();
            for (int i = 0; i < expandedArity; i++) {
                int slot = stackFrame.allocateSlotNumber(paramArray[i].getVariableQName());
                paramArray[i].setSlotNumber(slot);
            }

            ExpressionTool.allocateSlots(body, expandedArity, stackFrame);
            uf.setStackFrameMap(stackFrame);
            result = new UserFunctionReference(uf);

            Expression[] partialArgs = new Expression[expandedArity];
            for (int i = 0; i < arity; i++) {
                partialArgs[i] = null;
            }
            for (int ip = 0; ip < implicitParams.size(); ip++) {
                UserFunctionParameter ufp = implicitParams.get(ip);
                LocalBinding binding = details.outerVariablesUsed.get(ip);
                VariableReference var;
                if (binding instanceof TemporaryXSLTVariableBinding) {
                    var = new LocalVariableReference(binding);
                    ((TemporaryXSLTVariableBinding) binding).declaration.registerReference(var);
                } else {
                    var = new LocalVariableReference(binding);
                }
                var.setStaticType(binding.getRequiredType(), null, 0);
                ufp.setRequiredType(binding.getRequiredType());
                partialArgs[ip + arity] = var;
            }
            result = new PartialApply(result, partialArgs);

        } else {

            // there are no implicit parameters
            UserFunctionParameter[] paramArray = params.toArray(new UserFunctionParameter[0]);
            uf.setParameterDefinitions(paramArray);

            SlotManager stackFrame = p.getStaticContext().getConfiguration().makeSlotManager();
            for (UserFunctionParameter param : paramArray) {
                stackFrame.allocateSlotNumber(param.getVariableQName());
            }

            ExpressionTool.allocateSlots(body, params.size(), stackFrame);
            uf.setStackFrameMap(stackFrame);
            result = new UserFunctionReference(uf);
        }

        if (uf.getPackageData() instanceof StylesheetPackage) {
            // Note: inline functions in XSLT are registered as components; but not if they
            // are declared within a static expression, e.g. the initializer of a static
            // global variable
            ((StylesheetPackage)uf.getPackageData()).addComponent(uf.getDeclaringComponent());
        }
        return result;
    }

    public Expression parseSimpleInlineFunction(XPathParser p) throws XPathException {
        p.checkSyntaxExtensions("simple inline function");
        p.checkHofFeature();
        Tokenizer t = p.getTokenizer();
        int offset = t.currentTokenStartOffset;

        InlineFunctionDetails details = new InlineFunctionDetails();
        details.outerVariables = new Stack<>();
        for (LocalBinding lb : p.getRangeVariables()) {
            details.outerVariables.push(lb);
        }
        details.outerVariablesUsed = new ArrayList<>(4);
        details.implicitParams = new ArrayList<>(4);
        inlineFunctionStack.push(details);
        p.setRangeVariables(new Stack<>());
        p.nextToken();
        List<UserFunctionParameter> params = new ArrayList<>(8);
        SequenceType resultType = SequenceType.ANY_SEQUENCE;

        StructuredQName argQName = new StructuredQName("saxon", NamespaceConstant.SAXON, "dot");

        UserFunctionParameter arg = new UserFunctionParameter();
        arg.setRequiredType(SequenceType.SINGLE_ITEM);
        arg.setVariableQName(argQName);
        arg.setSlotNumber(0);
        params.add(arg);

        Expression body = p.parseExpression();
        p.expect(Token.RCURLY);
        t.lookAhead();  // must be done manually after an RCURLY
        p.nextToken();
        body.setRetainedStaticContext(p.getStaticContext().makeRetainedStaticContext());

        LocalVariableReference ref = new LocalVariableReference(arg);
        body = new ForEach(ref, body);
        Expression result = makeInlineFunctionValue(p, AnnotationList.EMPTY, details, params, resultType, body);

        p.setLocation(result, offset);
        // restore the previous stack of range variables
        p.setRangeVariables(details.outerVariables);
        inlineFunctionStack.pop();
        return result;
    }

    /**
     * Process a function call in which one or more of the argument positions are
     * represented as "?" placemarkers (indicating partial application or currying)
     *
     * @param offset       offset in the query source of the start of the expression
     * @param name         the function call (as if there were no currying)
     * @param args         the arguments (with EmptySequence in the placemarker positions)
     * @param placeMarkers the positions of the placemarkers    @return the curried function
     * @throws XPathException if a dynamic error occurs
     */
    @Override
    public Expression makeCurriedFunction(
            XPathParser parser, int offset,
            StructuredQName name, Expression[] args, IntSet placeMarkers) throws XPathException {
        StaticContext env = parser.getStaticContext();
        FunctionLibrary lib = env.getFunctionLibrary();
        SymbolicName.F sn = new SymbolicName.F(name, args.length);
        Function target = lib.getFunctionItem(sn, env);
        if (target == null) {
            // This will not happen in XQuery; instead, a dummy function will be created in the
            // UnboundFunctionLibrary in case it's a forward reference to a function not yet compiled
            return parser.reportMissingFunction(offset, name, args, new ArrayList<>());
        }
        Expression targetExp = makeNamedFunctionReference(name, target);
        parser.setLocation(targetExp, offset);
        return curryFunction(targetExp, args, placeMarkers);
    }

    /**
     * Process a function expression in which one or more of the argument positions are
     * represented as "?" placemarkers (indicating partial application or currying)
     *
     * @param functionExp  an expression that returns the function to be curried
     * @param args         the arguments (with EmptySequence in the placemarker positions)
     * @param placeMarkers the positions of the placemarkers
     * @return the curried function
     */

    public static Expression curryFunction(Expression functionExp, Expression[] args, IntSet placeMarkers) {
        IntIterator ii = placeMarkers.iterator();
        while (ii.hasNext()) {
            args[ii.next()] = null;
        }
        return new PartialApply(functionExp, args);
    }


    /**
     * Locate a range variable with a given name. (By "range variable", we mean a
     * variable declared within the expression where it is used.)
     *
     * @param qName identifies the name of the range variable
     * @return null if not found (this means the variable is probably a
     * context variable); otherwise the relevant RangeVariable
     */

    /*@Nullable*/
    public LocalBinding findOuterRangeVariable(XPathParser p, StructuredQName qName) {
        return findOuterRangeVariable(qName, inlineFunctionStack, p.getStaticContext());
    }

    /**
     * When a variable reference occurs within an inline function, it might be a reference to a variable declared
     * outside the inline function (which needs to become part of the closure). This code looks for such an outer
     * variable
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of inline functions that we are within
     * @param env                 the static context
     * @return a binding for the variable; this will typically be a binding to a newly added parameter
     * for the innermost function in which the variable reference appears. As a side effect, all the inline
     * functions between the declaration of the variable and its use will have this variable as an additional
     * parameter, each one bound to the corresponding parameter in the containing function.
     */

    public static LocalBinding findOuterRangeVariable(StructuredQName qName, Stack<InlineFunctionDetails> inlineFunctionStack, StaticContext env) {
        // If we didn't find the variable, it might be defined in an outer scope.
        LocalBinding b2 = findOuterXPathRangeVariable(qName, inlineFunctionStack);
        if (b2 != null) {
            return b2;
        }
        // It's not an in-scope range variable. If we're in XSLT, it might be an XSLT-defined local variable
        if (env instanceof ExpressionContext && !inlineFunctionStack.isEmpty()) {
            b2 = findOuterXSLTVariable(qName, inlineFunctionStack, env);
        }
        return b2;  // if null, it's not an in-scope range variable
    }

    /**
     * Look for an XPath/XQuery declaration of a variable used inside an inline function, but declared outside
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of inline functions that we are within
     * @return a binding to the innermost declaration of the variable
     */
    private static LocalBinding findOuterXPathRangeVariable(StructuredQName qName, Stack<InlineFunctionDetails> inlineFunctionStack) {
        for (int s = inlineFunctionStack.size() - 1; s >= 0; s--) {
            InlineFunctionDetails details = inlineFunctionStack.get(s);
            Stack<LocalBinding> outerVariables = details.outerVariables;
            for (int v = outerVariables.size() - 1; v >= 0; v--) {
                LocalBinding b2 = outerVariables.elementAt(v);
                if (b2.getVariableQName().equals(qName)) {
                    for (int bs = s; bs <= inlineFunctionStack.size() - 1; bs++) {
                        details = inlineFunctionStack.get(bs);
                        boolean found = false;
                        for (int p = 0; p < details.outerVariablesUsed.size() - 1; p++) {
                            if (details.outerVariablesUsed.get(p) == b2) {
                                // the inner function already uses the outer variable
                                b2 = details.implicitParams.get(p);
                                found = true;
                                break;
                            }
                        }
                        if (!found) {
                            // Need to add an implicit parameter to the inner function
                            details.outerVariablesUsed.add(b2);
                            UserFunctionParameter ufp = new UserFunctionParameter();
                            ufp.setVariableQName(qName);
                            ufp.setRequiredType(b2.getRequiredType());
                            details.implicitParams.add(ufp);
                            b2 = ufp;
                        }
                    }
                    return b2;
                }
            }
            LocalBinding b2 = bindParametersInNestedFunctions(qName, inlineFunctionStack, s);
            if (b2 != null) {
                return b2;
            }
        }
        return null;
    }

    /**
     * Given that a variable is referenced within an inline function and is declared outside it,
     * add implicit parameters to all the functions that appear in the containment stack between
     * the declaration and the reference, in each case binding the value of the argument to the inner
     * function to the corresponding declared parameter in its containing function.
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of nested inline functions
     * @param start               the position in this stack of the function that contains the variable
     *                            declaration
     * @return a binding to the relevant (newly declared) parameter of the innermost function.
     */

    private static LocalBinding bindParametersInNestedFunctions(
            StructuredQName qName, Stack<InlineFunctionDetails> inlineFunctionStack, int start) {
        InlineFunctionDetails details = inlineFunctionStack.get(start);
        List<UserFunctionParameter> params = details.implicitParams;
        for (UserFunctionParameter param : params) {
            if (param.getVariableQName().equals(qName)) {
                // The variable reference corresponds to a parameter of an outer inline function
                // We potentially need to add implicit parameters to any inner inline functions, and
                // bind the variable reference to the innermost of these implicit parameters
                LocalBinding b2 = param;
                for (int bs = start + 1; bs <= inlineFunctionStack.size() - 1; bs++) {
                    details = inlineFunctionStack.get(bs);
                    boolean found = false;
                    for (int p = 0; p < details.outerVariablesUsed.size() - 1; p++) {
                        if (details.outerVariablesUsed.get(p) == param) {
                            // the inner function already uses the outer variable
                            b2 = details.implicitParams.get(p);
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        // Need to add an implicit parameter to the inner function
                        details.outerVariablesUsed.add(param);
                        UserFunctionParameter ufp = new UserFunctionParameter();
                        ufp.setVariableQName(qName);
                        ufp.setRequiredType(param.getRequiredType());
                        details.implicitParams.add(ufp);
                        b2 = ufp;
                    }
                }
                if (b2 != null) {
                    return b2;
                }
            }
        }
        return null;
    }

    /**
     * Look for an XSLT declaration of a variable used inside an inline function, but declared outside
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of inline functions that we are within
     * @return a binding to the innermost declaration of the variable
     */

    private static LocalBinding findOuterXSLTVariable(
            StructuredQName qName, Stack<InlineFunctionDetails> inlineFunctionStack, StaticContext env) {
        SourceBinding decl = ((ExpressionContext) env).getStyleElement().bindLocalVariable(qName);
        if (decl != null) {
            InlineFunctionDetails details = inlineFunctionStack.get(0);
            LocalBinding innermostBinding;
            boolean found = false;
            for (int p = 0; p < details.outerVariablesUsed.size(); p++) {
                if (details.outerVariablesUsed.get(p).getVariableQName().equals(qName)) {
                    // the inner function already uses the outer variable
                    found = true;
                    break;
                }
            }
            if (!found) {
                // Need to add an implicit parameter to the inner function
                details.outerVariablesUsed.add(new TemporaryXSLTVariableBinding(decl));
                UserFunctionParameter ufp = new UserFunctionParameter();
                ufp.setVariableQName(qName);
                ufp.setRequiredType(decl.getInferredType(true));
                details.implicitParams.add(ufp);
            }
            // Now do the same for all inner inline functions, but this time binding to the
            // relevant parameter of the next containing function
            innermostBinding = bindParametersInNestedFunctions(qName, inlineFunctionStack, 0);
            if (innermostBinding != null) {
                return innermostBinding;
            }
        }
        return null;
    }

    public Expression createDynamicCurriedFunction(
            XPathParser p, Expression functionItem, ArrayList<Expression> args, IntSet placeMarkers) {
        Expression[] arguments = new Expression[args.size()];
        args.toArray(arguments);
        Expression result = curryFunction(functionItem, arguments, placeMarkers);
        p.setLocation(result, p.getTokenizer().currentTokenStartOffset);
        return result;
    }

    public void handleExternalFunctionDeclaration(XQueryParser p, XQueryFunction func) throws XPathException {
        QueryModule qenv = (QueryModule)p.getStaticContext();
        SymbolicName.F fName = new SymbolicName.F(func.getFunctionName(), func.getNumberOfArguments());
        if (!qenv.getFunctionLibrary().isAvailable(fName)) {
            p.grumble("External function " + fName + " is not available", "XPST0017");
        }
        Function ext = qenv.getFunctionLibrary().getFunctionItem(fName, qenv);
        if (ext == null) {
            p.grumble("External function " + fName + " is not available", "XPST0017");
        } else {
            TypeHierarchy th = qenv.getConfiguration().getTypeHierarchy();
            if (!ext.getFunctionItemType().getResultType().isSameType(func.getResultType(), th)) {
                p.grumble("The declared return type of the external function (" +
                                ext.getFunctionItemType().getResultType() +
                                ") does not match the return type of the actual function (" +
                                func.getResultType());
            }

            for (int i = 0; i < ext.getArity(); i++) {
                if (!ext.getFunctionItemType().getArgumentTypes()[i].isSameType(func.getArgumentTypes()[i], th)) {
                    p.grumble("The declared type of argument " + i + " of the external function (" +
                                    ext.getFunctionItemType().getArgumentTypes()[i] +
                                    ") does not match the argument type of the actual function (" +
                                    func.getArgumentTypes()[i]);
                }
            }
        }
    }
}

// Copyright (c) 2010-2017 Saxonica Limited.