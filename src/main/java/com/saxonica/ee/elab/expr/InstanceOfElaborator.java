package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.InstanceOfExpression;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for an {@code instance of} expression
 */

public class InstanceOfElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        InstanceOfExpression exp = (InstanceOfExpression) getExpression();
        TypeHierarchy th = getConfiguration().getTypeHierarchy();
        Expression arg = exp.getBaseExpression();

        int requiredCardinality = exp.getRequiredCardinality();
        boolean allowsMany = Cardinality.allowsMany(requiredCardinality);
        boolean allowsZero = Cardinality.allowsZero(requiredCardinality);
        ItemType requiredType = exp.getRequiredItemType();

        if (requiredCardinality == StaticProperty.EXACTLY_ONE && !Cardinality.allowsMany(arg.getCardinality())) {
            final ItemEvaluator itemEval = makeElaborator(arg).elaborateForItem();
            return context -> {
                Item item = itemEval.eval(context);
                return item != null && requiredType.matches(item, th);
            };
        } else {
            final PullEvaluator argPull = makeElaborator(arg).elaborateForPull();
            final boolean itemTypeOK = th.isSubType(arg.getItemType(), requiredType);
            return context -> {
                Item item;
                SequenceIterator iter = argPull.eval(context);
                int count = 0;
                while ((item = iter.next()) != null) {
                    count++;
                    if (!itemTypeOK && !requiredType.matches(item, th)) {
                        iter.close();
                        return false;
                    }
                    if (!allowsMany && count == 2) {
                        iter.close();
                        return false;
                    }
                }
                return allowsZero || count != 0;
            };
        }
    }


}

