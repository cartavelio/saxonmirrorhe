package net.sf.saxon.expr.sort;

import net.sf.saxon.transpile.CSharpReplaceBody;

import java.util.List;

public class CompositeAtomicKey {

    private List<AtomicMatchKey> keys;

    public CompositeAtomicKey(List<AtomicMatchKey> keys) {
        this.keys = keys;
    }

    @Override
    @CSharpReplaceBody(code="return obj is CompositeAtomicKey && Saxon.Impl.Helpers.ListUtils.EqualLists(keys, ((CompositeAtomicKey)obj).keys);")
    public boolean equals(Object obj) {
        return obj instanceof CompositeAtomicKey && keys.equals(((CompositeAtomicKey)obj).keys);
    }

    @Override
    @CSharpReplaceBody(code = "return Saxon.Impl.Helpers.ListUtils.ListHashCode(keys);")
    public int hashCode() {
        return keys.hashCode();
    }

}

