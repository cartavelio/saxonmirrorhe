package net.sf.saxon.type;

import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;

import java.util.Stack;

/**
 * A {@code DraftItemType} is used in XQuery when a type includes a (possible) forwards reference to an
 * item type declaration. The draft item type simply has a name, and is converted to a real item type
 * during the type-checking phase of processing, which happens when all item type declarations have been read.
 *
 * <p>Extending {@link ErrorType} is a convenience. (a) It saves having to implement all the methods of
 * {@link ItemType}; (b) it is substitutable for any other type, (c) it provides reasonable fallback behaviour
 * if for some reason the type is actually used before it has been resolved (which should not happen).</p>
 */

public class DraftItemType extends ErrorType {

    private final StructuredQName name;

    public DraftItemType(StructuredQName name) {
        this.name = name;
    }

    public StructuredQName getTargetName() {
        return name;
    }

    @Override
    public String getName() {
        return name.getLocalPart();
    }

    @Override
    public NamespaceUri getTargetNamespace() {
        return name.getNamespaceUri();
    }

    public String toString() {
        return "named item type " + name.getEQName();
    }

    @Override
    public String getEQName() {
        return name.getEQName();
    }

    /**
     * Resolve the item type alias to a real item type
     *
     * @param env  map from QNames of named item types to their definitions (which may
     *             themselves contain references to other named item types)
     * @param path stack of names being resolved, used to detect cycles
     * @return the item type after resolving any item type name references
     */

    @Override
    public ItemType resolveAliases(StaticContext env, Stack<Object> path) throws XPathException {
        ItemType target = env.resolveTypeAlias(getTargetName());
        if (target == null) {
            throw new XPathException("Item type " + name.getEQName() + " is not defined in the static context " + getTargetName(), "XPST0051");
        }
        if (path.contains(getTargetName())) {
            throw new XPathException("Definition of item type " + name.getEQName() + " is cyclic " + getTargetName(), "XQST0140");
        }
        path.push(getTargetName());
        ItemType finalTarget = target.resolveAliases(env, path);
        path.pop();
        return finalTarget;
    }
}

