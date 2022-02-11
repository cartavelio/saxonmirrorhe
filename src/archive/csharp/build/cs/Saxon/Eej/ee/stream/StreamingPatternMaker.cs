using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class StreamingPatternMaker {
        public static Saxon.Hej.pattern.Pattern makeStreamingPattern(Saxon.Hej.expr.Expression expression, Saxon.Hej.Configuration config, System.Collections.Generic.IList<string> reasonForFailure) {
            Saxon.Hej.pattern.Pattern result = /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(expression, config);
            if ((result == null)) {
                if (((reasonForFailure != null) && reasonForFailure.Count==0)) {
                    reasonForFailure.Add((("Cannot convert the "+expression.getExpressionName())+" expression to a streamed selection"));
                }
                return null;
            } else {
                return result;
            }
        }
    
    }
}