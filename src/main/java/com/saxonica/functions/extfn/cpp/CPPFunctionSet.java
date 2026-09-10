package com.saxonica.functions.extfn.cpp;

import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;

/**
 * The CPPFunctionSet represents additions to the set of Saxon vendor-supplied
 * extension functions that are available on Saxon-PE/C and Saxon-EE/C
 */

public class CPPFunctionSet extends BuiltInFunctionSet {

    private static final CPPFunctionSet THE_INSTANCE = new CPPFunctionSet();

      public static CPPFunctionSet getInstance() {
              return THE_INSTANCE;
          }

      private CPPFunctionSet() {
              init();
          }

      private void init() {
          register("native", 2, NativeCall::new, AnyItemType.getInstance(), OPT, SIDE|LATE)
                  .arg(0, BuiltInAtomicType.STRING, ONE, null)
                  .arg(1, ArrayItemType.getInstance(), STAR, null);

      }

      @Override
      public String getNamespace() {
              return NamespaceConstant.SAXON;
          }



}

// Copyright (c) 2020-2022 Saxonica Limited
