using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class FilterExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.FilteringAdjunct {
        internal bool isBuffering;
        public override void setExpression(Saxon.Hej.expr.Expression expression) {
            base.setExpression(expression);
            isBuffering = (((((Saxon.Hej.expr.FilterExpression)(getExpression())).getActionExpression().getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST)) != 0);
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.FilterExpression exp = (Saxon.Hej.expr.FilterExpression)(getExpression());
            Saxon.Hej.expr.Expression start = exp.getSelectExpression();
            Saxon.Hej.expr.Expression filter = exp.getActionExpression();
            Saxon.Eej.ee.stream.PostureAndSweep startPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(start, contextInfo, reasons);
            Saxon.Eej.ee.trans.ContextItemStaticInfoEE info = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(start.getItemType(), false, startPS.getPosture());
            Saxon.Eej.ee.stream.PostureAndSweep filterPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(filter, info, reasons);
            if ((filterPS.getSweep() == Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                return startPS;
            } else {
                if ((reasons != null)) {
                    reasons.Add((((("Predicate ["+filter.toShortString())+"] at line ")+filter.getLocation().getLineNumber())+" is not motionless"));
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.expr.FilterExpression exp = (Saxon.Hej.expr.FilterExpression)(getExpression());
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.Expression @base = exp.getSelectExpression();
            Saxon.Hej.expr.Expression filter = exp.getFilter();
            Saxon.Eej.ee.stream.PostureAndSweep fs = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(filter, Saxon.Eej.ee.trans.ContextItemStaticInfoEE.DEFAULT, null);
            if ((fs.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                return null;
            }
            Saxon.Hej.pattern.Pattern basePattern = /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(@base, config);
            if ((basePattern == null)) {
                return null;
            }
            if (exp.isPositional(th)) {
                return null;
            } else {
                if (Saxon.Hej.expr.parser.ExpressionTool.containsLocalVariableReference(exp)) {
                    return null;
                } else {
                    return new Saxon.Hej.pattern.BasePatternWithPredicate(basePattern, filter);
                }
            }
        }
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            return new Saxon.Eej.ee.stream.adjunct.FilterExpressionAdjunct.PredicateFilter(((Saxon.Hej.expr.FilterExpression)(getExpression())).getActionExpression(), context);
        }
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            Saxon.Hej.expr.FilterExpression f = (Saxon.Hej.expr.FilterExpression)(getExpression());
            if (isBuffering) {
                return new Saxon.Eej.ee.stream.feed.FeedMakerDelegate((watchManager, @out, context) => new Saxon.Eej.ee.stream.feed.BufferingFilterExpressionFeed(getExpression(), @out, context));
            } else {
                return base.getFeedMaker(arg);
            }
        }
        private class PredicateFilter : Saxon.Eej.ee.stream.feed.FilteringFeed.Filter {
            private readonly Saxon.Hej.expr.Expression predicate;
            private readonly Saxon.Hej.expr.XPathContext context;
            private readonly Saxon.Hej.tree.iter.ManualIterator iter;
            public PredicateFilter(Saxon.Hej.expr.Expression predicate, Saxon.Hej.expr.XPathContext context)  {
                this.predicate = predicate;
                this.context = context.newMinorContext();
                iter = new Saxon.Hej.tree.iter.ManualIterator();
                this.context.setCurrentIterator(iter);
            }
            public Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult matches(Saxon.Hej.om.Item item, int position) {
                iter.setContextItem(item);
                iter.setPosition(position);
                Saxon.Hej.om.SequenceIterator predicateValue = predicate.iterate(context);
                bool matches = Saxon.Hej.expr.FilterIterator.testPredicateValue(predicateValue, position, predicate);
                return (matches ? /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP);
            }
        
        }
    
    }
}