package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.InstanceOfExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;

public class InstanceOfElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        InstanceOfExpression exp = (InstanceOfExpression) getExpression();
        TypeHierarchy th = getConfiguration().getTypeHierarchy();
        Expression arg = exp.getBaseExpression();
        PullEvaluator argPull = Elaborator.make(arg).getPullEvaluator();
        int requiredCardinality = exp.getRequiredCardinality();
        boolean allowsMany = Cardinality.allowsMany(requiredCardinality);
        boolean allowsZero = Cardinality.allowsZero(requiredCardinality);
        ItemType requiredType = exp.getRequiredItemType();
        boolean itemTypeOK = th.isSubType(arg.getItemType(), requiredType);
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

