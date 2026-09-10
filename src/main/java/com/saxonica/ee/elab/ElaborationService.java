package com.saxonica.ee.elab;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.IElaborationService;
import net.sf.saxon.trans.XPathException;

import java.util.HashMap;
import java.util.Map;

public class ElaborationService implements IElaborationService {

    private Map<Component, Elaborator> index = new HashMap<>();

    private synchronized Elaborator obtainElaborator(Component component) {
        if (index.containsKey(component)) {
            return index.get(component);
        } else {
            Expression top = component.getActor().getBody();
            Elaborator elaborator = Elaborator.make(top);
            index.put(component, elaborator);
            return elaborator;
        }
    }

    @Override
    public void push(Component component, Outputter out, XPathContext context) throws XPathException {
        Elaborator elaborator = obtainElaborator(component);
        elaborator.getPushEvaluator().process(out, context);
    }
}

