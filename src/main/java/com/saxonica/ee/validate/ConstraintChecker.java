package com.saxonica.ee.validate;

import com.saxonica.ee.schema.*;
import com.saxonica.ee.stream.watch.Watch;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.jiter.ConcatenatingIterator;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;


/**
 * This class handles the checking of Schema-defined unique, key, and keyref
 * constraints. It keeps track of the nesting of elements, and notifies a Watcher
 * class when an element is found that matches a particular Selector.
 */


public class ConstraintChecker extends WatchManager {

    private ValidationContext validationContext;
    private final List<Watch> prearrangedWatches = new ArrayList<>(4);

    public ConstraintChecker(PipelineConfiguration pipe) {
        super(pipe);
    }

    public void setValidationContext(ValidationContext vc) {
        this.validationContext = vc;
    }

    public ValidationContext getValidationContext() {
        return validationContext;
    }

    @Override
    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration pipe) {
        super.setPipelineConfiguration(pipe);
        Controller controller = pipe.getController();
        if (controller == null) {
            Configuration config = getPipelineConfiguration().getConfiguration();
            controller = new Controller(config);
            controller.getExecutable().setSchemaAware(true);
            getPipelineConfiguration().setController(controller);
        }
        XPathContextMajor context = controller.newXPathContext();
        setXPathContext(context);
    }

    /**
     * setElementDeclaration is called immediately before the call on startElement.
     * It identifies the element declaration that defines the just-started
     * element, which in turn is used to create a Watch for constraints originating
     * at this element.
     *
     * @param decl the element declaration
     * @throws net.sf.saxon.trans.XPathException
     *          if an error occurs
     */

    public void setElementDeclaration(ElementDecl decl) throws XPathException {
        for (IdentityConstraintReference icr : decl.getIdentityConstraints()) {
            IdentityConstraint constraint = icr.getTarget();
            if (constraint instanceof KeyRef) {
                RefSelectorWatch watch = new RefSelectorWatch(this, (KeyRef) constraint);
                //addWatch(watch);
                prearrangedWatches.add(watch);
            }
        }
        for (IdentityConstraintReference icr : decl.getIdentityConstraints()) {
            IdentityConstraint constraint = icr.getTarget();
            if (constraint instanceof Unique || constraint instanceof Key) {
                KeySelectorWatch watch = new KeySelectorWatch(this, constraint);
                //addWatch(watch);
                prearrangedWatches.add(watch);
                // for each active KeyRef with the same name, tell the key/unique constraint about the keyref constraint
                @SuppressWarnings("Convert2MethodRef")
                Iterator<Watch> watches = new ConcatenatingIterator<>(
                        watchList.iterator(), () -> prearrangedWatches.iterator());
                while (watches.hasNext()) {
                    Watch ref = watches.next();
                    if (ref instanceof RefSelectorWatch) {
                        StructuredQName reffp = ((KeyRef) ((RefSelectorWatch) ref).getIdentityConstraint()).getTargetComponentName();
                        if (reffp.equals(constraint.getConstraintName())) {
                            watch.addKeyRef((RefSelectorWatch) ref);
                        }
                    }
                }
            }
        }
    }

    @Override
    protected void addPrearrangedWatches() throws XPathException {
        for (Watch prearrangedWatch : prearrangedWatches) {
            addWatch(prearrangedWatch, true);
        }
        prearrangedWatches.clear();
    }

}

// Copyright (c) 2004-2022 Saxonica Limited No warranty.
