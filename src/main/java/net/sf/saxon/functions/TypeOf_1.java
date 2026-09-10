////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.HashSet;
import java.util.Set;

/**
 * This class supports the 4.0 type-of() function with a single argument
 */

public class TypeOf_1 extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        SequenceIterator iter = arguments[0].iterate();
        int count = 0;
        Item it;
        String latest = null;
        Set<String> itemTypes = new HashSet<>();
        while ((it = iter.next()) != null) {
            count++;
            itemTypes.add(latest = itemType(it));
        }
        if (count == 0) {
            return new StringValue("empty-sequence()");
        } else if (count == 1) {
            return new StringValue(latest);
        } else if (itemTypes.size() == 1) {
            return new StringValue(latest + "+");
        } else {
            StringBuilder sb = new StringBuilder();
            for (String s : itemTypes) {
                if (sb.length() > 0) {
                    sb.append("|");
                }
                sb.append(s);
            }
            return new StringValue("(" + sb + ")+");
        }
    }

    private static String itemType(Item it) {
        switch (it.getGenre()) {
            case NODE:
                return nodeKind(((NodeInfo)it));
            case ATOMIC:
                AtomicType at = ((AtomicValue) it).getItemType();
                while (at.isAnonymousType()) {
                    at = (AtomicType)at.getBaseType();
                }
                StructuredQName name = at.getTypeName();
                if (name.hasURI(NamespaceUri.SCHEMA)) {
                    return "xs:" + name.getLocalPart();
                } else {
                    return name.getEQName();
                }
            case MAP:
                return "map(*)";
            case ARRAY:
                return "array(*)";
            case FUNCTION:
                return "function(*)";
            default:
                return it.getClass().getCanonicalName();
        }
    }

    public static String nodeKind(NodeInfo node) {
        int k = node.getNodeKind();
        switch (k) {
            case Type.DOCUMENT:
                return "document-node()";
            case Type.ELEMENT:
                return "element()";
            case Type.ATTRIBUTE:
                return "attribute()";
            case Type.TEXT:
                return "text()";
            case Type.COMMENT:
                return "comment()";
            case Type.PROCESSING_INSTRUCTION:
                return "processing-instruction()";
            case Type.NAMESPACE:
            default:
                return "namespace-node()";
        }
    }



}

