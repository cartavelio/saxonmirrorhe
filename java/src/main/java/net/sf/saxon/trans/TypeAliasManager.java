////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.trans;

import net.sf.saxon.style.XSLItemType;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.type.ItemType;

import java.util.*;

/**
 * Manager for named item type declarations; it handles both xsl:item-type
 * declarations in a stylesheet, and "declare type" declarations in XQuery.
 *
 * <p>The type alias manager is referenced by a static context, and is used to
 * resolve type names appearing in expressions. In XQuery, the complication arises
 * that these can be forward references.</p>
 *
 * Language extension proposed for XSLT 4.0 and XQuery 4.0
 */
public class TypeAliasManager {

    public TypeAliasManager() {
    }


    // map of named item types
    private final Map<StructuredQName, ComponentDeclaration> unresolvedDeclarations =
            new HashMap<>();

    private Map<StructuredQName, ItemType> typeAliases = new HashMap<>();

    /**
     * Register a type alias
     * @param name the alias of the type
     * @param type the item type
     * @throws XPathException if there is already an entry for this type name
     */

    public void registerTypeAlias(StructuredQName name, ItemType type) throws XPathException {
        if (typeAliases.containsKey(name)) {
            throw new XPathException("Duplicate type alias: " + name.toString(), "XQST0146");
        }
        typeAliases.put(name, type);
        unresolvedDeclarations.remove(name);
    }

    public Map<StructuredQName, ItemType> getTypeAliases() {
        return typeAliases;
    }

    public void setTypeAliases(Map<StructuredQName, ItemType> typeAliases) {
        this.typeAliases = typeAliases;
    }

    /**
     * Process an xsl:item-type declaration appearing in a stylesheet package
     * @param declaration the relevant declaration
     * @throws XPathException if found to be invalid
     */

    public void processDeclaration(ComponentDeclaration declaration) throws XPathException {
        XSLItemType sta = (XSLItemType) declaration.getSourceElement();
        ItemType type = sta.tryToResolve();
        if (type != null) {
            registerTypeAlias(sta.getObjectName(), type);
        } else {
            unresolvedDeclarations.put(sta.getObjectName(), declaration);
        }
    }

    /**
     * Process all xsl:item-type declarations appearing in a stylesheet package
     * @param topLevel the top-level components in the package, including xsl:item-type declarations
     * @throws XPathException if any type declaration is found to be invalid
     */

    public void processAllDeclarations(List<ComponentDeclaration> topLevel) throws XPathException {
        for (ComponentDeclaration decl : topLevel) {
            if (decl.getSourceElement() instanceof XSLItemType) {
                processDeclaration(decl);
            }
        }
        int unresolved = unresolvedDeclarations.size();
        while (unresolved > 0) {
            Set<ComponentDeclaration> pending = new HashSet<>(unresolvedDeclarations.values());
            for (ComponentDeclaration decl : pending) {
                processDeclaration(decl);
            }
            if (unresolvedDeclarations.size() >= unresolved) {
                StringBuilder fsb = new StringBuilder(256);
                fsb.append("Cannot resolve all type aliases, because of missing or circular definitions. Unresolved names: ");
                for (StructuredQName name : unresolvedDeclarations.keySet()) {
                    fsb.append(name.getDisplayName());
                    fsb.append(' ');
                }
                throw new XPathException(fsb.toString(), SaxonErrorCode.SXTA0001);
            }
            unresolved = unresolvedDeclarations.size();
        }
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

