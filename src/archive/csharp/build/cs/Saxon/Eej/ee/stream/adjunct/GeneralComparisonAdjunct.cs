using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class GeneralComparisonAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int streamedArg) {
            return new Saxon.Eej.ee.stream.feed.FeedMakerDelegate((watchManager, @out, context) => {
                int op = ((Saxon.Hej.expr.GeneralComparison)(getExpression())).getSingletonOperator();
                if ((op == Saxon.Hej.expr.parser.Token.FEQ)) {
                    return new Saxon.Eej.ee.stream.feed.GeneralComparisonEqualityFeed(getExpression(), streamedArg, @out, context);
                } else {
                    if ((op == Saxon.Hej.expr.parser.Token.FNE)) {
                        return new Saxon.Eej.ee.stream.feed.GeneralComparisonInequalityFeed(getExpression(), streamedArg, @out, context);
                    } else {
                        return new Saxon.Eej.ee.stream.feed.GeneralComparisonMinimaxFeed(getExpression(), streamedArg, @out, context);
                    }
                }
            } );
        }
    
    }
}