using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class AnalyzeString : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.ContextOriginator {
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly Saxon.Hej.expr.Operand regexOp;
        private readonly Saxon.Hej.expr.Operand flagsOp;
        private Saxon.Hej.expr.Operand matchingOp;
        private Saxon.Hej.expr.Operand nonMatchingOp;
        private readonly static Saxon.Hej.expr.OperandRole ACTION = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.USES_NEW_FOCUS|Saxon.Hej.expr.OperandRole.HIGHER_ORDER), Saxon.Hej.expr.OperandUsage.NAVIGATION);
        private readonly static Saxon.Hej.expr.OperandRole SELECT = new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.SETS_NEW_FOCUS, Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.SINGLE_STRING);
        private Saxon.Hej.regex.RegularExpression pattern;
        public AnalyzeString(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression regex, Saxon.Hej.expr.Expression flags, Saxon.Hej.expr.Expression matching, Saxon.Hej.expr.Expression nonMatching, Saxon.Hej.regex.RegularExpression pattern)  {
            selectOp = new Saxon.Hej.expr.Operand(this, select, SELECT);
            regexOp = new Saxon.Hej.expr.Operand(this, regex, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            flagsOp = new Saxon.Hej.expr.Operand(this, flags, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            if ((matching != null)) {
                matchingOp = new Saxon.Hej.expr.Operand(this, matching, ACTION);
            }
            if ((nonMatching != null)) {
                nonMatchingOp = new Saxon.Hej.expr.Operand(this, nonMatching, ACTION);
            }
            this.pattern = pattern;
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return selectOp.getChildExpression();
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public Saxon.Hej.expr.Expression getRegex() {
            return regexOp.getChildExpression();
        }
        public void setRegex(Saxon.Hej.expr.Expression regex) {
            regexOp.setChildExpression(regex);
        }
        public Saxon.Hej.expr.Expression getFlags() {
            return flagsOp.getChildExpression();
        }
        public void setFlags(Saxon.Hej.expr.Expression flags) {
            flagsOp.setChildExpression(flags);
        }
        public Saxon.Hej.expr.Expression getMatching() {
            return ((matchingOp == null) ? null : matchingOp.getChildExpression());
        }
        public void setMatching(Saxon.Hej.expr.Expression matching) {
            if ((matchingOp != null)) {
                matchingOp.setChildExpression(matching);
            } else {
                matchingOp = new Saxon.Hej.expr.Operand(this, matching, ACTION);
            }
        }
        public Saxon.Hej.expr.Expression getNonMatching() {
            return ((nonMatchingOp == null) ? null : nonMatchingOp.getChildExpression());
        }
        public void setNonMatching(Saxon.Hej.expr.Expression nonMatching) {
            if ((nonMatchingOp != null)) {
                nonMatchingOp.setChildExpression(nonMatching);
            } else {
                nonMatchingOp = new Saxon.Hej.expr.Operand(this, nonMatching, ACTION);
            }
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_ANALYZE_STRING;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandSparseList(selectOp, regexOp, flagsOp, matchingOp, nonMatchingOp);
        }
        public override int getImplementationMethod() {
            return (Saxon.Hej.expr.Expression.PROCESS_METHOD|Saxon.Hej.expr.Expression.ITERATE_METHOD);
        }
        public Saxon.Hej.regex.RegularExpression getPatternExpression() {
            return pattern;
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            selectOp.typeCheck(visitor, contextInfo);
            regexOp.typeCheck(visitor, contextInfo);
            flagsOp.typeCheck(visitor, contextInfo);
            if ((matchingOp != null)) {
                matchingOp.typeCheck(visitor, config.makeContextItemStaticInfo(Saxon.Hej.type.BuiltInAtomicType.STRING, false));
            }
            if ((nonMatchingOp != null)) {
                nonMatchingOp.typeCheck(visitor, config.makeContextItemStaticInfo(Saxon.Hej.type.BuiltInAtomicType.STRING, false));
            }
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "analyze-string/select", 0);
            Saxon.Hej.value.SequenceType required = Saxon.Hej.value.SequenceType.OPTIONAL_STRING;
            setSelect(tc.staticTypeCheck(getSelect(), required, role, visitor));
            role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "analyze-string/regex", 0);
            setRegex(tc.staticTypeCheck(getRegex(), Saxon.Hej.value.SequenceType.SINGLE_STRING, role, visitor));
            role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "analyze-string/flags", 0);
            setFlags(tc.staticTypeCheck(getFlags(), Saxon.Hej.value.SequenceType.SINGLE_STRING, role, visitor));
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            selectOp.optimize(visitor, contextInfo);
            regexOp.optimize(visitor, contextInfo);
            flagsOp.optimize(visitor, contextInfo);
            if ((matchingOp != null)) {
                matchingOp.optimize(visitor, config.makeContextItemStaticInfo(Saxon.Hej.type.BuiltInAtomicType.STRING, false));
            }
            if ((nonMatchingOp != null)) {
                nonMatchingOp.optimize(visitor, config.makeContextItemStaticInfo(Saxon.Hej.type.BuiltInAtomicType.STRING, false));
            }
            System.Collections.Generic.IList<string> warnings = new System.Collections.Generic.List<string>();
            precomputeRegex(config, warnings);
            foreach (string w in warnings) {
                visitor.getStaticContext().issueWarning(w, Saxon.Hej.trans.SaxonErrorCode.SXWN9022, getLocation());
            }
            return this;
        }
        public void precomputeRegex(Saxon.Hej.Configuration config, System.Collections.Generic.IList<string> warnings) {
            if ((((pattern == null) && (getRegex() is Saxon.Hej.expr.StringLiteral)) && (getFlags() is Saxon.Hej.expr.StringLiteral))) {
                try {
                    string regex = ((Saxon.Hej.expr.StringLiteral)(this.getRegex())).stringify();
                    string flagstr = ((Saxon.Hej.expr.StringLiteral)(getFlags())).stringify();
                    string hostLang = "XP30";
                    pattern = config.compileRegularExpression(Saxon.Hej.str.StringView.tidy(regex), flagstr, hostLang, warnings);
                } catch (Saxon.Hej.trans.XPathException err) {
                    if ("XTDE1150".Equals(err.getErrorCodeLocalPart())) {
                        throw;
                    }
                    if ("FORX0001".Equals(err.getErrorCodeLocalPart())) {
                        invalidRegex(("Error in regular expression flags: "+err), "FORX0001");
                    } else {
                        invalidRegex(("Error in regular expression: "+err), err.getErrorCodeLocalPart());
                    }
                }
            }
        }
        private void invalidRegex(string message, string errorCode) {
            pattern = null;
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, errorCode);
            err.setLocation(getLocation());
            throw err;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rm) {
            Saxon.Hej.expr.instruct.AnalyzeString a2 = new Saxon.Hej.expr.instruct.AnalyzeString(copy(getSelect(), rm), copy(getRegex(), rm), copy(getFlags(), rm), copy(getMatching(), rm), copy(getNonMatching(), rm), pattern);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, a2);
            return a2;
        }
        private Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return ((exp == null) ? null : exp.copy(rebindings));
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            if ((getMatching() != null)) {
                getMatching().checkPermittedContents(parentType, false);
            }
            if ((getNonMatching() != null)) {
                getNonMatching().checkPermittedContents(parentType, false);
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((getMatching() != null)) {
                if ((getNonMatching() != null)) {
                    Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                    return Saxon.Hej.type.Type.getCommonSuperType(getMatching().getItemType(), getNonMatching().getItemType(), th);
                } else {
                    return getMatching().getItemType();
                }
            } else {
                if ((getNonMatching() != null)) {
                    return getNonMatching().getItemType();
                } else {
                    return Saxon.Hej.type.ErrorType.getInstance();
                }
            }
        }
        public override int computeDependencies() {
            int dependencies = 0;
            dependencies |= getSelect().getDependencies();
            dependencies |= getRegex().getDependencies();
            dependencies |= getFlags().getDependencies();
            if ((getMatching() != null)) {
                dependencies |= (getMatching().getDependencies()&~(((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_REGEX_GROUP))));
            }
            if ((getNonMatching() != null)) {
                dependencies |= (getNonMatching().getDependencies()&~(((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_REGEX_GROUP))));
            }
            return dependencies;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override string getExpressionName() {
            return "analyzeString";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("analyzeString", this);
            @out.setChildRole("select");
            getSelect().export(@out);
            @out.setChildRole("regex");
            getRegex().export(@out);
            @out.setChildRole("flags");
            getFlags().export(@out);
            if ((getMatching() != null)) {
                @out.setChildRole("matching");
                getMatching().export(@out);
            }
            if ((getNonMatching() != null)) {
                @out.setChildRole("nonMatching");
                getNonMatching().export(@out);
            }
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.AnalyzeString.AnalyzeStringElaborator();
        }
        private delegate Saxon.Hej.regex.RegularExpression RegexEvaluator (Saxon.Hej.expr.XPathContext context);
        public class AnalyzeStringElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.instruct.AnalyzeString expr = (Saxon.Hej.expr.instruct.AnalyzeString)(getExpression());
                Saxon.Hej.expr.elab.UnicodeStringEvaluator input = expr.getSelect().makeElaborator().elaborateForUnicodeString(true);
                Saxon.Hej.expr.elab.PullEvaluator matching = ((expr.getMatching() == null) ? null : expr.getMatching().makeElaborator().elaborateForPull());
                Saxon.Hej.expr.elab.PullEvaluator nonMatching = ((expr.getNonMatching() == null) ? null : expr.getNonMatching().makeElaborator().elaborateForPull());
                Saxon.Hej.expr.instruct.AnalyzeString.RegexEvaluator regexSupplier = getRegexSupplier(expr);
                return context => {
                    Saxon.Hej.regex.RegularExpression re = /*Delegate.compileRegex()*/regexSupplier(context);
                    Saxon.Hej.str.UnicodeString @in = /*Delegate.eval()*/input(context);
                    Saxon.Hej.regex.RegexIterator iter = re.analyze(@in);
                    Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                    c2.setOrigin(expr);
                    c2.trackFocus(iter);
                    c2.setCurrentRegexIterator(iter);
                    return new Saxon.Hej.expr.ContextMappingIterator(cxt => {
                        if (iter.isMatching()) {
                            if ((matching != null)) {
                                return /*Delegate.iterate()*/matching(c2);
                            }
                        } else {
                            if ((nonMatching != null)) {
                                return /*Delegate.iterate()*/nonMatching(c2);
                            }
                        }
                        return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                    } , c2);
                } ;
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.AnalyzeString expr = (Saxon.Hej.expr.instruct.AnalyzeString)(getExpression());
                Saxon.Hej.expr.elab.UnicodeStringEvaluator input = expr.getSelect().makeElaborator().elaborateForUnicodeString(true);
                Saxon.Hej.expr.elab.PushEvaluator matching = ((expr.getMatching() == null) ? null : expr.getMatching().makeElaborator().elaborateForPush());
                Saxon.Hej.expr.elab.PushEvaluator nonMatching = ((expr.getNonMatching() == null) ? null : expr.getNonMatching().makeElaborator().elaborateForPush());
                Saxon.Hej.expr.instruct.AnalyzeString.RegexEvaluator regexSupplier = getRegexSupplier(expr);
                return (@out, context) => {
                    Saxon.Hej.regex.RegularExpression re = /*Delegate.compileRegex()*/regexSupplier(context);
                    Saxon.Hej.str.UnicodeString @in = /*Delegate.eval()*/input(context);
                    Saxon.Hej.regex.RegexIterator iter = re.analyze(@in);
                    Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                    c2.setOrigin(expr);
                    Saxon.Hej.om.FocusIterator focus = c2.trackFocus(iter);
                    c2.setCurrentRegexIterator(iter);
                    while ((focus.next() != null)) {
                        if (iter.isMatching()) {
                            if ((matching != null)) {
                                dispatchTailCall(/*Delegate.processLeavingTail()*/matching(@out, c2));
                            }
                        } else {
                            if ((nonMatching != null)) {
                                dispatchTailCall(/*Delegate.processLeavingTail()*/nonMatching(@out, c2));
                            }
                        }
                    }
                    return null;
                } ;
            }
            private Saxon.Hej.expr.instruct.AnalyzeString.RegexEvaluator getRegexSupplier(Saxon.Hej.expr.instruct.AnalyzeString expr) {
                Saxon.Hej.regex.RegularExpression pattern = expr.getPatternExpression();
                Saxon.Hej.expr.instruct.AnalyzeString.RegexEvaluator regexSupplier;
                if ((expr.pattern != null)) {
                    regexSupplier = context => pattern;
                } else {
                    Saxon.Hej.expr.elab.StringEvaluator flagsEval = expr.getFlags().makeElaborator().elaborateForString(true);
                    Saxon.Hej.expr.elab.UnicodeStringEvaluator regexEval = expr.getRegex().makeElaborator().elaborateForUnicodeString(false);
                    regexSupplier = context => {
                        string flagsStr = /*Delegate.eval()*/flagsEval(context);
                        Saxon.Hej.str.UnicodeString regexStr = /*Delegate.eval()*/regexEval(context);
                        return context.getConfiguration().compileRegularExpression(regexStr, flagsStr, "XP31", null);
                    } ;
                }
                return regexSupplier;
            }
        
        }
    
    }
}