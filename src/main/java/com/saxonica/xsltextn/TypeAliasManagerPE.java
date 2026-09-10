package com.saxonica.xsltextn;

import com.saxonica.xsltextn.style.SaxonTypeAlias;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.TypeAliasManager;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.ItemType;

import java.util.*;

/**
 * Manager for saxon:type-alias declarations in a stylesheet.
 *
 * Saxon extension introduced in Saxon 9.8
 */
public class TypeAliasManagerPE extends TypeAliasManager {

    // map of type aliases (Saxon extension)
    private Map<StructuredQName, ComponentDeclaration> unresolvedDeclarations =
            new HashMap<>();

    private Map<StructuredQName, ItemType> typeAliases = new HashMap<>();

    public void registerTypeAlias(StructuredQName name, ItemType type) {
        typeAliases.put(name, type);
        unresolvedDeclarations.remove(name);
    }

    public void processDeclaration(ComponentDeclaration declaration) throws XPathException {
        SaxonTypeAlias sta = (SaxonTypeAlias)declaration.getSourceElement();
        ItemType type = sta.tryToResolve();
        if (type != null) {
            registerTypeAlias(sta.getObjectName(), type);
        } else {
            unresolvedDeclarations.put(sta.getObjectName(), declaration);
        }
    }

    public void processAllDeclarations(List<ComponentDeclaration> topLevel) throws XPathException {
        for (ComponentDeclaration decl : topLevel) {
            if (decl.getSourceElement() instanceof SaxonTypeAlias) {
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
                FastStringBuffer fsb = new FastStringBuffer(256);
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

    public ItemType getItemType(StructuredQName alias) {
        return typeAliases.get(alias);
    }
}

// Copyright (c) 2018 Saxonica Limited

