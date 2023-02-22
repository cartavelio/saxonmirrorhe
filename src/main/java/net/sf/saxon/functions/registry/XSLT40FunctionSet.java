////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.registry;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XSLT 4.0.
 * This includes the functions defined in XPath 4.0 by reference.
 */

public class XSLT40FunctionSet extends XSLT30FunctionSet {

    private static final XSLT40FunctionSet THE_INSTANCE = new XSLT40FunctionSet();

    public static XSLT40FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    @Override
    protected BuiltInFunctionSet correspondingXPathFunctionSet() {
        return XPath40FunctionSet.getInstance();
    }

    private XSLT40FunctionSet() {

    }




}

