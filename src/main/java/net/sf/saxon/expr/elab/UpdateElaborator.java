package net.sf.saxon.expr.elab;

public abstract class UpdateElaborator extends Elaborator {

    private void notAllowed() {
        throw new UnsupportedOperationException("Cannot evaluate updating expression");
    }

    @Override
    public EagerEvaluator eagerly() {
        notAllowed();
        return null;
    }

    @Override
    public PullEvaluator elaborateForPull() {
        notAllowed();
        return null;
    }

    @Override
    public PushEvaluator elaborateForPush() {
        notAllowed();
        return null;
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        notAllowed();
        return null;
    }

    public BooleanEvaluator elaborateForBoolean() {
        notAllowed();
        return null;
    }

    @Override
    public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
        notAllowed();
        return null;
    }

    @Override
    public abstract UpdateEvaluator elaborateForUpdate();
}

