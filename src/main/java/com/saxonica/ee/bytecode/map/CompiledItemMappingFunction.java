package com.saxonica.ee.bytecode.map;

import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

public abstract class CompiledItemMappingFunction implements ItemMappingFunction {

    @Override
    public abstract Item mapItem(Item item) throws XPathException;

}

// Copyright (c) 2011-2022 Saxonica Limited
