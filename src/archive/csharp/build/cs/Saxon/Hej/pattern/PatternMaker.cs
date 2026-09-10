using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class PatternMaker {
        public static Saxon.Hej.pattern.Pattern fromExpression(Saxon.Hej.expr.Expression expression, Saxon.Hej.Configuration config, bool is30) {
            Saxon.Hej.pattern.Pattern result = expression.toPattern(config);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(expression, result);
            return result;
        }
        public static int getAxisForPathStep(Saxon.Hej.expr.Expression step) {
            if ((step is Saxon.Hej.expr.AxisExpression)) {
                return Saxon.Hej.om.AxisInfo.inverseAxis[((Saxon.Hej.expr.AxisExpression)(step)).getAxis()];
            } else {
                if ((step is Saxon.Hej.expr.FilterExpression)) {
                    return getAxisForPathStep(((Saxon.Hej.expr.FilterExpression)(step)).getSelectExpression());
                } else {
                    if ((step is Saxon.Hej.expr.FirstItemExpression)) {
                        return getAxisForPathStep(((Saxon.Hej.expr.FirstItemExpression)(step)).getBaseExpression());
                    } else {
                        if ((step is Saxon.Hej.expr.SubscriptExpression)) {
                            return getAxisForPathStep(((Saxon.Hej.expr.SubscriptExpression)(step)).getBaseExpression());
                        } else {
                            if ((step is Saxon.Hej.expr.SlashExpression)) {
                                return getAxisForPathStep(((Saxon.Hej.expr.SlashExpression)(step)).getFirstStep());
                            } else {
                                if ((step is Saxon.Hej.expr.ContextItemExpression)) {
                                    return Saxon.Hej.om.AxisInfo.SELF;
                                } else {
                                    throw new Saxon.Hej.trans.XPathException("The path in a pattern must contain simple steps");
                                }
                            }
                        }
                    }
                }
            }
        }
    
    }
}