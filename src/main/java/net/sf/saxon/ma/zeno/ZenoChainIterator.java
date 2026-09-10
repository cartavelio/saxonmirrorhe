package net.sf.saxon.ma.zeno;

import java.util.ArrayList;
import java.util.Iterator;

public class ZenoChainIterator<U> implements Iterator<U> {

    private int majorIndex = 0;
    private int minorIndex = 0;
    private final ArrayList<ArrayList<U>> masterList;

    public ZenoChainIterator(ArrayList<ArrayList<U>> masterList) {
        this.masterList = masterList;
    }

    public boolean hasNext() {
        return majorIndex < masterList.size() && minorIndex < masterList.get(majorIndex).size();
    }

    public U next() {
        ArrayList<U> currentSegment = masterList.get(majorIndex);
        U result = currentSegment.get(minorIndex);
        if (++minorIndex >= currentSegment.size()) {
            majorIndex++;
            minorIndex = 0;
            // Assumes no zero-length segments
        }
        return result;
    }
}

//
// The contents of this file are subject to the Mozilla Public License Version 1.0 (the "License");
// you may not use this file except in compliance with the License. You may obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Saxonica Limited
//
// Portions created by (your name) are Copyright (C) (your legal entity). All Rights Reserved.
//
// Contributor(s): none.
//
