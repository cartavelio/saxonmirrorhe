using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public sealed class RuleManager {
        private readonly Saxon.Hej.style.StylesheetPackage stylesheetPackage;
        private readonly Saxon.Hej.Configuration config;
        private readonly Saxon.Hej.trans.SimpleMode unnamedMode;
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.Mode> modes;
        private Saxon.Hej.trans.SimpleMode omniMode = null;
        private bool unnamedModeExplicit;
        private Saxon.Hej.trans.CompilerInfo compilerInfo;
        private int nextSequenceNumber = 0;
        public RuleManager(Saxon.Hej.style.StylesheetPackage pack) : this (pack, pack.getConfiguration().getDefaultXsltCompilerInfo()) {
        }
        public RuleManager(Saxon.Hej.style.StylesheetPackage pack, Saxon.Hej.trans.CompilerInfo compilerInfo)  {
            this.stylesheetPackage = pack;
            this.config = pack.getConfiguration();
            this.compilerInfo = compilerInfo;
            this.unnamedMode = config.makeMode(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME, this.compilerInfo);
            Saxon.Hej.expr.Component c = unnamedMode.makeDeclaringComponent(Saxon.Hej.trans.Visibility.PRIVATE, stylesheetPackage);
            c.setVisibility(Saxon.Hej.trans.Visibility.PRIVATE, Saxon.Hej.trans.VisibilityProvenance.DEFAULTED);
            this.stylesheetPackage.addComponent(c);
            this.modes = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.Mode>(5);
        }
        public void setUnnamedModeExplicit(bool declared) {
            unnamedModeExplicit = declared;
        }
        public bool isUnnamedModeExplicit() {
            return unnamedModeExplicit;
        }
        public void setCompilerInfo(Saxon.Hej.trans.CompilerInfo compilerInfo) {
            this.compilerInfo = compilerInfo;
        }
        public Saxon.Hej.style.StylesheetPackage getStylesheetPackage() {
            return stylesheetPackage;
        }
        public System.Collections.Generic.ICollection<Saxon.Hej.trans.Mode> getAllNamedModes() {
            return modes.Values;
        }
        public Saxon.Hej.trans.SimpleMode getUnnamedMode() {
            return unnamedMode;
        }
        public Saxon.Hej.trans.Mode obtainMode(Saxon.Hej.om.StructuredQName modeName, bool createIfAbsent) {
            if (((modeName == null) || modeName.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME))) {
                return unnamedMode;
            }
            if (modeName.Equals(Saxon.Hej.trans.Mode.OMNI_MODE)) {
                if ((omniMode == null)) {
                    omniMode = config.makeMode(modeName, compilerInfo);
                }
                return omniMode;
            }
            Saxon.Hej.trans.Mode m = Saxon.Impl.Helpers.MapUtils.Get(modes, modeName);
            if (((m == null) && createIfAbsent)) {
                m = config.makeMode(modeName, compilerInfo);
                Saxon.Impl.Helpers.MapUtils.Put(modes, modeName, m);
                Saxon.Hej.expr.Component c = m.makeDeclaringComponent(Saxon.Hej.trans.Visibility.PRIVATE, stylesheetPackage);
                c.setVisibility(Saxon.Hej.trans.Visibility.PRIVATE, Saxon.Hej.trans.VisibilityProvenance.DEFAULTED);
                stylesheetPackage.addComponent(c);
            }
            return m;
        }
        public void registerMode(Saxon.Hej.trans.Mode mode) {
            Saxon.Impl.Helpers.MapUtils.Put(modes, mode.getModeName(), mode);
        }
        public bool existsOmniMode() {
            return (omniMode != null);
        }
        public int allocateSequenceNumber() {
            return nextSequenceNumber++;
        }
        public int registerRule(Saxon.Hej.pattern.Pattern pattern, Saxon.Hej.expr.instruct.TemplateRule eh, Saxon.Hej.trans.Mode mode, Saxon.Hej.style.StylesheetModule module, double priority, int position, int part) {
            if ((pattern is Saxon.Hej.pattern.UnionPattern)) {
                Saxon.Hej.pattern.UnionPattern up = (Saxon.Hej.pattern.UnionPattern)(pattern);
                Saxon.Hej.pattern.Pattern p1 = up.getLHS();
                Saxon.Hej.pattern.Pattern p2 = up.getRHS();
                int lhsParts = registerRule(p1, eh, mode, module, priority, position, part);
                int rhsParts = registerRule(p2, eh, mode, module, priority, position, lhsParts);
                return (lhsParts+rhsParts);
            }
            if ((((pattern is Saxon.Hej.pattern.NodeTestPattern) && (pattern.getItemType() is Saxon.Hej.pattern.CombinedNodeTest)) && (((Saxon.Hej.pattern.CombinedNodeTest)(pattern.getItemType())).getOperator() == Saxon.Hej.expr.parser.Token.UNION))) {
                Saxon.Hej.pattern.CombinedNodeTest cnt = (Saxon.Hej.pattern.CombinedNodeTest)(pattern.getItemType());
                Saxon.Hej.pattern.NodeTest[] nt = cnt.getComponentNodeTests();
                Saxon.Hej.pattern.NodeTestPattern nt0 = new Saxon.Hej.pattern.NodeTestPattern(nt[0]);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(pattern, nt0);
                int lhsParts = registerRule(nt0, eh, mode, module, priority, position, part);
                Saxon.Hej.pattern.NodeTestPattern nt1 = new Saxon.Hej.pattern.NodeTestPattern(nt[1]);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(pattern, nt1);
                int rhsParts = registerRule(nt1, eh, mode, module, priority, position, lhsParts);
                return (lhsParts+rhsParts);
            }
            if (System.Double.IsNaN(priority)) {
                priority = pattern.getDefaultPriority();
            } else {
                part = 0;
            }
            if ((mode is Saxon.Hej.trans.SimpleMode)) {
                ((Saxon.Hej.trans.SimpleMode)(mode)).addRule(pattern, eh, module, module.getPrecedence(), priority, position, part);
            } else {
                mode.getActivePart().addRule(pattern, eh, module, mode.getMaxPrecedence(), priority, position, part);
            }
            return 1;
        }
        public void computeRankings() {
            unnamedMode.computeRankings(0);
            foreach (Saxon.Hej.trans.Mode mode in modes.Values) {
                mode.computeRankings(0);
            }
        }
        public void invertStreamableTemplates() {
            unnamedMode.invertStreamableTemplates();
            foreach (Saxon.Hej.trans.Mode mode in modes.Values) {
                mode.getActivePart().invertStreamableTemplates();
            }
        }
        public void checkConsistency() {
            unnamedMode.resolveProperties(this);
            foreach (Saxon.Hej.trans.Mode mode in modes.Values) {
                mode.getActivePart().resolveProperties(this);
            }
        }
        public void explainTemplateRules(Saxon.Hej.trace.ExpressionPresenter presenter) {
            unnamedMode.explain(presenter);
            foreach (Saxon.Hej.trans.Mode mode in modes.Values) {
                mode.explain(presenter);
            }
        }
        public void optimizeRules() {
            unnamedMode.optimizeRules();
            foreach (Saxon.Hej.trans.Mode mode in modes.Values) {
                mode.getActivePart().optimizeRules();
            }
        }
    
    }
}