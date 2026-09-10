////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.trans;

import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.XSLItemType;
import net.sf.saxon.type.ItemType;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Stack;


/**
 * Manager for named item type declarations in XSLT 4.0; it handles both xsl:item-type
 * declarations in a stylesheet.
 *
 * <p>The type alias manager is referenced by a static context, and is used to
 * resolve type names appearing in expressions.</p>
 */
public class TypeAliasManager {

    public TypeAliasManager() {
    }

    /**
     * Map of named item types. When first built, an item type may contain unresolved
     * references to other item types, represented using the class DraftItemType. The
     * method {@link #processAllDeclarations(List)} resolves all such unresolved
     * references (checking for cycles as it does so), and by the time item types in
     * other XSLT elements (such as xsl:variable/@as) are processed, there should
     * be no outstanding unresolved references.
     */

    private Map<StructuredQName, ItemType> typeAliases = new HashMap<>();

    /**
     * Process all xsl:item-type declarations appearing in a stylesheet package, resolving
     * all references to other xsl:item-type declarations, and checking for cycles
     * @param topLevel the top-level components in the package, including xsl:item-type declarations
     * @throws XPathException if any type declaration is found to be invalid or cyclic
     */

    public void processAllDeclarations(List<ComponentDeclaration> topLevel) throws XPathException {
        // Keep track of import precedences
        Map<StructuredQName, Integer> precedences = new HashMap<>();
        // First process all the xsl:item-type declarations
        for (ComponentDeclaration decl : topLevel) {
            int prec = decl.getPrecedence();
            if (decl.getSourceElement() instanceof XSLItemType it) {
                StructuredQName qname = it.getObjectName();
                if (typeAliases.containsKey(qname)) {
                    int existingPrec = precedences.get(qname);
                    if (existingPrec == prec) {
                        decl.getSourceElement().compileError("Duplicate declaration for item-type " + qname.getEQName(), "XTSE4030");
                    } else if (existingPrec > prec) {
                        continue;
                    }
                }
                ItemType target = it.makeItemType();
                typeAliases.put(it.getObjectName(), target);
                precedences.put(it.getObjectName(), prec);
            }
        }
        // Now process the resulting map, knowing that all item type aliases should now be resolvable
        Map<StructuredQName, ItemType> resolvedAliases = new HashMap<>(this.typeAliases);
        for (Map.Entry<StructuredQName, ItemType> entry : typeAliases.entrySet()) {
            ItemType resolved = entry.getValue().resolveAliases(new Stack<>());
            resolvedAliases.put(entry.getKey(), resolved);
        }
        // Replace the original table of named item types with the new table, all fully resolved.
        typeAliases = resolvedAliases;
    }

    /**
     * Get a named item type from the static context
     * @param alias the required name
     * @return the corresponding item type; or null if unknown
     */
    public ItemType getItemType(StructuredQName alias) {
        return typeAliases.get(alias);
    }
}

