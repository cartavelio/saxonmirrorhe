////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.registry;

import net.sf.saxon.functions.*;
import net.sf.saxon.type.BuiltInAtomicType;

/**
 * Function signatures (and pointers to implementations) of the functions available for use
 * in static expressions (including use-when expressions) in XSLT 3.0 stylesheets
 */

public class UseWhen30FunctionSet extends BuiltInFunctionSet {

    private static final UseWhen30FunctionSet THE_INSTANCE = new UseWhen30FunctionSet();

    public static UseWhen30FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    protected UseWhen30FunctionSet() {
        init();
    }

    protected void init() {

        addXPathFunctions();

        register("available-system-properties", 0, e -> e.populate(AvailableSystemProperties::new, BuiltInAtomicType.QNAME,
                 STAR, LATE));

        register("element-available", 1, e -> e.populate(ElementAvailable::new, BuiltInAtomicType.BOOLEAN, ONE, NS)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("function-available", 1, e -> e.populate(FunctionAvailable::new, BuiltInAtomicType.BOOLEAN, ONE, NS | LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("function-available", 2, e -> e.populate(FunctionAvailable::new, BuiltInAtomicType.BOOLEAN, ONE, NS | LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null));

        register("system-property", 1, e -> e.populate(SystemProperty::new, BuiltInAtomicType.STRING, ONE, NS | LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("type-available", 1, e -> e.populate(TypeAvailable::new, BuiltInAtomicType.BOOLEAN, ONE, NS)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));


    }

    protected void addXPathFunctions() {
        importFunctionSet(XPath31FunctionSet.getInstance());
    }


}

