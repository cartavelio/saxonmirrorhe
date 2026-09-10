package com.saxonica.ee.bytecode.simtype;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import com.saxonica.ee.schema.*;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.StringConverter;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;
import org.objectweb.asm.commons.GeneratorAdapter;

import java.util.Arrays;
import java.util.List;

/**
 * Class to handle bytecode generation for user-defined atomic types
 */
public class AtomicTypeValidatorCompiler {

    private final UserAtomicType type;

    /**
     * Create a compiler (=bytecode generator) for a user-defined atomic type*
     *
     * @param type the user-defined atomic type: this must be compilable.
     */

    public AtomicTypeValidatorCompiler(UserAtomicType type) {
        this.type = type;
        assert type.isCompilable();
    }

    /**
     * Make a validator for user-defined atomic types
     *
     * @param compiler the compiler service
     * @param rules    the conversion rules
     * @throws CannotCompileException if bytecode generation for this type is not possible for any reason
     */

    public void compileAtomicValidator(CompilerService compiler, ConversionRules rules)
            throws CannotCompileException {

        BuiltInAtomicType base = (BuiltInAtomicType) type.getBuiltInBaseType();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo mi = compiler.getCurrentMethod();

        if (base.getFingerprint() == StandardNames.XS_STRING) {

            // First apply pre-lexical facets, i.e. Whitespace

            int action = type.getWhitespaceAction();
            compileWhitespaceProcessing(ga, action);

            boolean needUnicode = false;
            boolean needLength = false;
            for (Facet f : type.getExtendedFacetList()) {
                if (f instanceof LengthRangeFacet) {
                    needLength = true;
                } else if (f instanceof PatternFacet) {
                    needUnicode = true;
                }
            }
            int unicodeLoc = -1;
            int lengthLoc = -1;
            if (needUnicode) {
                unicodeLoc = ga.newLocal(StringValue.class);
                ga.loadArg(0);
                ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
                ga.storeLocal(unicodeLoc);
            }
            if (needLength) {
                lengthLoc = ga.newLocal(Integer.TYPE);
                if (needUnicode) {
                    ga.loadLocal(unicodeLoc);
                    ga.invokeInstanceMethod(StringValue.class, "length");
                    ga.storeLocal(lengthLoc);
                } else {
                    ga.loadArg(0);
                    ga.invokeInstanceMethod(UnicodeString.class, "length32");
                    ga.storeLocal(lengthLoc);
                }
            }
            LabelInfo failLabel = mi.newLabel("fail");
            for (Facet f : type.getExtendedFacetList()) {
                if (f instanceof LengthFacet) {
                    int required = (int) ((LengthFacet) f).toLong();
                    ga.loadLocal(lengthLoc);
                    ga.push(required);
                    LabelInfo ok = mi.newLabel("lengthOK");
                    ga.ifICmp(GeneratorAdapter.EQ, ok.label());
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);
                } else if (f instanceof MinLengthFacet) {
                    int required = (int) ((MinLengthFacet) f).toLong();
                    ga.loadLocal(lengthLoc);
                    ga.push(required);
                    LabelInfo ok = mi.newLabel("minLengthOK");
                    ga.ifICmp(GeneratorAdapter.GE, ok.label());
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);
                } else if (f instanceof MaxLengthFacet) {
                    int required = (int) ((MaxLengthFacet) f).toLong();
                    ga.loadLocal(lengthLoc);
                    ga.push(required);
                    LabelInfo ok = mi.newLabel("maxLengthOK");
                    ga.ifICmp(GeneratorAdapter.LE, ok.label());
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);
                } else if (f instanceof PatternFacet) {
                    RegularExpression regex = ((PatternFacet) f).getRegularExpression();
                    ExpressionCompiler.allocateStatic(compiler, regex);
                    ga.loadLocal(unicodeLoc);
                    ga.invokeInstanceMethod(RegularExpression.class, "matches", UnicodeString.class);
                    LabelInfo ok = mi.newLabel("patternOK");
                    ga.ifTrue(ok);
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);
                } else if (f instanceof EnumerationFacetSet) {
                    List<String> values = ((EnumerationFacetSet) f).getStringValues();
                    String[] valueArray = values.toArray(new String[0]);
                    Arrays.sort(valueArray);
                    ExpressionCompiler.allocateStatic(compiler, valueArray);
                    ga.loadArg(0);
                    ga.invokeInstanceMethod(Object.class, "toString");
                    ga.invokeStaticMethod(Arrays.class, "binarySearch", Object[].class, Object.class);
                    LabelInfo ok = mi.newLabel("enumOK");
                    ga.ifZCmp(GeneratorAdapter.GE, ok.label());
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);
                } else if (f instanceof AssertionFacet) {
                    // TODO: compile the XPath assertion to bytecode
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.loadArg(0);
                    ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
                    ExpressionCompiler.allocateStatic(compiler, rules);
                    ga.invokeInstanceMethod(AssertionFacet.class, "testAtomicValue", AtomicValue.class, ConversionRules.class);
                    LabelInfo ok = mi.newLabel("assertOK");
                    ga.ifTrue(ok);
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);
                } else if (f instanceof WhitespaceFacet) {
                    // no action, already dealt with
                } else {
                    throw new CannotCompileException(f.getName(), false);
                }
            }
            LabelInfo exit = mi.newLabel("exit");
            ga.pushNull();
            ga.goTo(exit);
            mi.placeLabel(failLabel);

            // Stack: facet
            ga.loadArg(0);
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
            // Stack: facet StringValue
            ga.swap();
            // Stack: StringValue facet
            ExpressionCompiler.allocateStatic(compiler, type);
            // Stack: StringValue facet SimpleType
            ga.swap();
            // Stack: StringValue SimpleType facet
            ga.invokeStaticMethod(UserSimpleType.class, "makeValidationFailure", AtomicSequence.class, net.sf.saxon.type.SimpleType.class, Facet.class);
            // Stack: ValidationFailure

            mi.placeLabel(exit);

        } else {

            // General-purpose code for non-string data types

            LabelInfo failLabel = mi.newLabel("fail");
            int action = type.getWhitespaceAction();
            compileWhitespaceProcessing(ga, action);

            int atomicLoc = ga.newLocal(AtomicValue.class);
            BuiltInAtomicType builtInBase = (BuiltInAtomicType) type.getBuiltInBaseType();
            StringConverter converter = (StringConverter) rules.getConverter(BuiltInAtomicType.STRING, builtInBase);
            ExpressionCompiler.allocateStatic(compiler, converter);
            ga.loadArg(0);
            ga.invokeInstanceMethod(StringConverter.class, "convertString", UnicodeString.class);
            ga.dup();
            LabelInfo failBasic = mi.newLabel("failBasic");
            ga.ifInstance(ValidationFailure.class, failBasic);
            ga.checkClass(AtomicValue.class);
            ga.storeLocal(atomicLoc);

            for (Facet f : type.getExtendedFacetList()) {
                if (f instanceof WhitespaceFacet) {
                    // already handled; do nothing
                } else if (f instanceof PatternFacet) {
                    RegularExpression regex = ((PatternFacet) f).getRegularExpression();
                    ExpressionCompiler.allocateStatic(compiler, regex);
                    ga.loadArg(0);
                    ga.invokeInstanceMethod(RegularExpression.class, "matches", UnicodeString.class);
                    LabelInfo ok = mi.newLabel("patternOK");
                    ga.ifTrue(ok);
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);

                } else if (f instanceof AssertionFacet) {
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.loadLocal(atomicLoc);
                    ExpressionCompiler.allocateStatic(compiler, rules);
                    ga.invokeInstanceMethod(AssertionFacet.class, "testAtomicValue", AtomicValue.class, ConversionRules.class);
                    LabelInfo ok = mi.newLabel("valueOK");
                    ga.ifTrue(ok);
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);

                } else {
                    // Any other facet:
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.loadLocal(atomicLoc);
                    ga.invokeInstanceMethod(Facet.class, "testAtomicValue", AtomicValue.class);
                    LabelInfo ok = mi.newLabel("valueOK");
                    ga.ifTrue(ok);
                    ExpressionCompiler.allocateStatic(compiler, f);
                    ga.goTo(failLabel);
                    mi.placeLabel(ok);
                }
            }
            LabelInfo exit = mi.newLabel("exit");
            ga.pushNull();
            ga.goTo(exit);

            mi.placeLabel(failBasic);
            ga.checkClass(ValidationFailure.class);
            ga.goTo(exit);

            mi.placeLabel(failLabel);

            // Stack: facet
            ga.loadLocal(atomicLoc);
            // Stack: facet AtomicValue
            ga.swap();
            // Stack: AtomicValue facet
            ExpressionCompiler.allocateStatic(compiler, type);
            // Stack: AtomicValue facet SimpleType
            ga.swap();
            // Stack: AtomicValue SimpleType facet
            ga.invokeStaticMethod(UserSimpleType.class, "makeValidationFailure", AtomicSequence.class, net.sf.saxon.type.SimpleType.class, Facet.class);
            // Stack: ValidationFailure
            mi.placeLabel(exit);

        }
    }

    private void compileWhitespaceProcessing(Generator ga, int action) {
        if (action == Whitespace.COLLAPSE) {
            ga.loadArg(0);
            ga.invokeStaticMethod(Whitespace.class, "collapseWhitespace", UnicodeString.class);
            ga.storeArg(0);
        } else if (action == Whitespace.TRIM) {
            ga.loadArg(0);
            ga.invokeStaticMethod(Whitespace.class, "trim", UnicodeString.class);
            ga.storeArg(0);
        } else if (action == Whitespace.REPLACE) {
            ga.loadArg(0);
            ga.invokeStaticMethod(Whitespace.class, "normalize", UnicodeString.class);
            ga.storeArg(0);
        }
    }

}

// Copyright (c) 2018-2020 Saxonica Limited