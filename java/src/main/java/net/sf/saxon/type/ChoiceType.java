package net.sf.saxon.type;

public interface ChoiceType {

    Iterable<? extends ItemType> getAlternatives();
}

