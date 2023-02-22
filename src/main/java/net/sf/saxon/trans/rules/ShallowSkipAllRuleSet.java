////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.trans.rules;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.instruct.ParameterSet;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;

/**
 * The built-in shallow-skip-all rule set proposed for XSLT 4.0, which is the same as
 * shallow-skip, except for maps and arrays.
 */
public class ShallowSkipAllRuleSet extends ShallowSkipRuleSet {

    private static final ShallowSkipAllRuleSet THE_INSTANCE = new ShallowSkipAllRuleSet();

    /**
     * Get the singleton instance of this class
     *
     * @return the singleton instance
     */

    public static ShallowSkipAllRuleSet getInstance() {
        return THE_INSTANCE;
    }

    private ShallowSkipAllRuleSet() {
    }

    /**
     * Perform the built-in template action for a given node.
     * @param item the item to be processed by this built-in rule
     * @param parameters   the parameters supplied to apply-templates
     * @param tunnelParams the tunnel parameters to be passed through
     * @param out  the destination for output
     * @param context      the dynamic evaluation context
     * @param locationId   location of the instruction (apply-templates, apply-imports etc) that caused
     */

    @Override
    public void process(Item item, ParameterSet parameters,
                        ParameterSet tunnelParams, Outputter out, XPathContext context,
                        Location locationId) throws XPathException {
        if (item instanceof ArrayItem) {
            SequenceIterator iter = ((ArrayItem)item).parcels();
            XPathContextMajor c2 = context.newContext();
            c2.setOrigin(this);
            c2.trackFocus(iter);
            c2.setCurrentComponent(c2.getCurrentMode());
            TailCall tc = context.getCurrentMode().getActor().applyTemplates(
                    parameters, tunnelParams, null, out, c2, locationId);
            while (tc != null) {
                tc = tc.processLeavingTail();
            }

        } else if (item instanceof MapItem) {

            int size = ((MapItem)item).size();
            if (size <= 1) {
                // If it's a singleton map and didn't match anything, there's no point processing it again...
                return;
            }
            SequenceIterator iter = ((MapItem) item).entries();
            XPathContextMajor c2 = context.newContext();
            c2.setOrigin(this);
            c2.trackFocus(iter);
            c2.setCurrentComponent(c2.getCurrentMode());
            TailCall tc = context.getCurrentMode().getActor().applyTemplates(
                    parameters, tunnelParams, null, out, c2, locationId);
            while (tc != null) {
                tc = tc.processLeavingTail();
            }

        } else {
            super.process(item, parameters, tunnelParams, out, context, locationId);
        }

    }

    /**
     * Identify this built-in rule set
     *
     * @return "shallow-copy"
     */

    @Override
    public String getName() {
        return "shallow-skip-all";
    }


}
