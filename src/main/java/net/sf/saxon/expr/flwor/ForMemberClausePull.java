////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.flwor;

import net.sf.saxon.ma.Parcel;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;

/**
 * This class implements the changes to the tuple stream effected by a "for member" clause in a FLWOR expression
 */
public class ForMemberClausePull extends ForClausePull {


    public ForMemberClausePull(TuplePull base, ForMemberClause forClause) {
        super(base, forClause);
    }

    @Override
    protected GroundedValue variableValue(Item item) {
        return ((Parcel)item).getValue();
    }


}

