////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.tree.iter;

import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.ObjectValue;


/**
 * ObjectIterator: a sequence iterator over wrapped external objects
 */

public interface ObjectIterator<T> extends SequenceIterator {

    @Override
    ObjectValue<T> next();

}

