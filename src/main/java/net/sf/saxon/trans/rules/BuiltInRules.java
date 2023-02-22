package net.sf.saxon.trans.rules;

import net.sf.saxon.transpile.CSharpSimpleEnum;

@CSharpSimpleEnum
public enum BuiltInRules {

    DEEP_COPY,
    DEEP_SKIP,
    FAIL,
    SHALLOW_COPY,
    APPLY_TEMPLATES_TO_ATTRIBUTES,
    APPLY_TEMPLATES_TO_CHILDREN;

}

