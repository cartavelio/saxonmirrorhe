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

import java.util.*;
import java.util.function.BiConsumer;


/**
 * Manager for named item type declarations in XSLT 4.0; it handles both xsl:item-type
 * declarations in a stylesheet.
 *
 * <p>The type alias manager is referenced by a static context, and is used to
 * resolve type names appearing in expressions.</p>
 */
public class NamedTypeManager {

    public NamedTypeManager() {
    }

    /**
     * Map of named item types. When first built, an item type may contain unresolved
     * references to other item types, represented using the class DraftItemType. The
     * method {@link #processAllDeclarations(List)} resolves all such unresolved
     * references (checking for cycles as it does so), and by the time item types in
     * other XSLT elements (such as xsl:variable/@as) are processed, there should
     * be no outstanding unresolved references.
     */

    public record NamedItemType(StructuredQName name, ItemType itemType, Visibility visibility, int precedence) {
        public NamedItemType withVisibility(Visibility visibility) {
            return new NamedItemType(name, itemType, visibility, precedence);
        }

        public NamedItemType withItemType(ItemType type) {
            return new NamedItemType(name, type, visibility, precedence);
        }
    }

    private Map<StructuredQName, NamedItemType> typeAliases = new HashMap<>();

    /**
     * Process all xsl:item-type declarations appearing in a stylesheet package, resolving
     * all references to other xsl:item-type declarations, and checking for cycles
     * @param topLevel the top-level components in the package, including xsl:item-type declarations
     * @throws XPathException if any type declaration is found to be invalid or cyclic
     */

    public void processAllDeclarations(List<ComponentDeclaration> topLevel) throws XPathException {
        // First process all the xsl:item-type declarations
        for (ComponentDeclaration decl : topLevel) {
            int precedence = decl.getPrecedence();
            if (decl.getSourceElement() instanceof XSLItemType it) {
                StructuredQName qName = it.getObjectName();
                NamedItemType existing = typeAliases.get(qName);
                if (existing != null    ) {
                    int existingPrec = existing.precedence();
                    if (existingPrec == precedence) {
                        decl.getSourceElement().compileError("Duplicate declaration for item-type " + qName.getEQName(), "XTSE4030");
                    } else if (existingPrec > precedence) {
                        continue;
                    }
                }
                ItemType target = it.makeItemType();
                Visibility visibility = it.getVisibility();
                typeAliases.put(it.getObjectName(), new NamedItemType(qName, target, visibility, precedence));
            }
        }
        // Now process the resulting map, knowing that all item type aliases should now be resolvable
        Map<StructuredQName, NamedItemType> resolvedAliases = new HashMap<>(this.typeAliases);
        for (Map.Entry<StructuredQName, NamedItemType> entry : typeAliases.entrySet()) {
            NamedItemType namedItemType = entry.getValue();
            ItemType resolved = entry.getValue().itemType().resolveAliases(new Stack<>());
            resolvedAliases.put(entry.getKey(), namedItemType.withItemType(resolved));
        }
        // Replace the original table of named item types with the new table, all fully resolved.
        typeAliases = resolvedAliases;
    }

    public Iterable<NamedItemType> getAllNamedItemTypes() {
        return typeAliases.values();
    }

    /**
     * Set the declared visibility of a named item type
     */

    public void setDeclaredVisibility(StructuredQName qname, Visibility visibility) {
        NamedItemType namedItemType = typeAliases.get(qname);
        if (namedItemType != null) {
            typeAliases.put(qname, namedItemType.withVisibility(visibility));
        }
    }

    /**
     * Get the declared visibility of a named item type
     */

    public Visibility getDeclaredVisibility(StructuredQName qname) {
        NamedItemType namedItemType = typeAliases.get(qname);
        if (namedItemType != null) {
            return namedItemType.visibility();
        } else {
            return Visibility.UNDEFINED;
        }
    }

    /**
     * Add a type declaration from a used package
     */

    public void addImportedTypeAlias(StructuredQName name, ItemType type, Visibility visibility) {
        typeAliases.put(name, new NamedItemType(name, type, visibility, Integer.MAX_VALUE));
    }

//    public void addPublicTypeAlias(StructuredQName name) {
//        publicTypeAliases.add(name);
//    }
//
//    public boolean isPublicTypeAlias(StructuredQName name) {
//        return publicTypeAliases.contains(name);
//    }

    /**
     * Process all the public type declarations
     */

    public void processPublicTypeAliases(BiConsumer<StructuredQName, ItemType> consumer) {
        for (NamedItemType namedItemType : typeAliases.values()) {
            if (namedItemType.visibility() == Visibility.PUBLIC) {
                consumer.accept(namedItemType.name(), namedItemType.itemType());
            }
        }
    }

    /**
     * Get a named item type from the static context
     *
     * @param alias the required name
     * @return the corresponding item type; or null if unknown
     */
    public NamedItemType getNamedItemType(StructuredQName alias) {
        return typeAliases.get(alias);
    }

    /**
     * Get a named item type from the static context
     * @param alias the required name
     * @return the corresponding item type; or null if unknown
     */
    public ItemType getItemType(StructuredQName alias) {
        NamedItemType namedItemType = typeAliases.get(alias);
        return namedItemType == null ? null : namedItemType.itemType();
    }
}

