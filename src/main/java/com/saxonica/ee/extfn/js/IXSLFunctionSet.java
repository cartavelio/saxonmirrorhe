
package com.saxonica.ee.extfn.js;

//import jdk.nashorn.api.scripting.JSObject;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.DocumentFn;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.*;

import javax.script.Invocable;
import javax.script.ScriptEngine;
import javax.script.ScriptEngineManager;
import javax.script.ScriptException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.List;

/**
 * The IXSLFunctionSet represents additions to the set of Saxon vendor-supplied
 * extension functions that are available on SaxonJS.
 */

public class IXSLFunctionSet extends BuiltInFunctionSet {

    private static final IXSLFunctionSet THE_INSTANCE = new IXSLFunctionSet();

    public static IXSLFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private IXSLFunctionSet() {
        init();
    }

    private void init() {

        register("page", 0, e -> e.populate(Stub::new, NodeKindTest.DOCUMENT, ONE, SIDE|LATE));

        register("source", 0, e -> e.populate(Source::new, NodeKindTest.DOCUMENT, OPT, LATE));

        register("window", 0, e -> e.populate(Stub::new, AnyItemType.getInstance(), ONE, LATE));

        register("query-params", 0, e -> e.populate(Stub::new, MapType.ANY_MAP_TYPE, ONE, SIDE|LATE));

        register("event", 0, e -> e.populate(Stub::new, AnyItemType.getInstance(), OPT, SIDE|LATE));

        register("location", 0, e -> e.populate(Stub::new, BuiltInAtomicType.STRING, ONE, SIDE|LATE));

        register("style", 1, e -> e.populate(Stub::new, MapType.ANY_MAP_TYPE, ONE, SIDE|LATE)
                .arg(0, AnyItemType.getInstance(), OPT, null));

        register("get", 2, e -> e.populate(Stub::new, AnyItemType.getInstance(), STAR, SIDE|LATE)
                .arg(0, AnyItemType.getInstance(), ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("call", 3, e -> e.populate(Call::new, AnyItemType.getInstance(), STAR, SIDE|LATE)
                .arg(0, AnyItemType.getInstance(), ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, ArrayItemType.getInstance(), ONE, null));

        register("contains", 2, e -> e.populate(Stub::new, BuiltInAtomicType.BOOLEAN, ONE, SIDE|LATE)
                .arg(0, AnyItemType.getInstance(), ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("eval", 1, e -> e.populate(Eval::new, AnyItemType.getInstance(), STAR, SIDE|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));

        register("apply", 2, e -> e.populate(Stub::new, AnyItemType.getInstance(), STAR, SIDE|LATE)
                .arg(0, AnyItemType.getInstance(), ONE, null)
                .arg(1, ArrayItemType.getInstance(), ONE, null));

        register("schedule-action", 3, e -> e.populate(Stub::new, AnyItemType.getInstance(), STAR, SIDE|LATE)
                .arg(0, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, AnyItemType.getInstance(), STAR, null));

        register("schedule-action", 4, e -> e.populate(Stub::new, AnyItemType.getInstance(), STAR, SIDE|LATE)
                .arg(0, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, AnyItemType.getInstance(), STAR, null)
                .arg(3, MapType.ANY_MAP_TYPE, OPT, null));

        register("set-attribute", 3, e -> e.populate(Stub::new, AnyItemType.getInstance(), OPT, SIDE|FOCUS|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, AnyItemType.getInstance(), ONE, null));

        register("remove-attribute", 2, e -> e.populate(Stub::new, AnyItemType.getInstance(), OPT, SIDE|FOCUS|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyItemType.getInstance(), ONE, null));

        register("set-property", 3, e -> e.populate(Stub::new, AnyItemType.getInstance(), OPT, SIDE|FOCUS|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyItemType.getInstance(), OPT, null)
                .arg(2, AnyItemType.getInstance(), ONE, null));

        register("remove-property", 2, e -> e.populate(Stub::new, AnyItemType.getInstance(), OPT, SIDE|FOCUS|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyItemType.getInstance(), ONE, null));

        register("set-style", 3, e -> e.populate(Stub::new, AnyItemType.getInstance(), OPT, SIDE|FOCUS|LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, AnyItemType.getInstance(), ONE, null));

    }

    @Override
    public NamespaceUri getNamespace() {
        return NamespaceUri.IXSL;
    }

    @Override
    public String getConventionalPrefix() {
        return "ixsl";
    }

    //#if CSHARP==false
    public static ScriptEngine engine = null;

    public synchronized static ScriptEngine obtainEngine() throws XPathException {
        if (engine == null) {
            engine = new ScriptEngineManager().getEngineByName("nashorn");
            if (engine == null) {
                throw new XPathException("Javascript extensions not available (needs Java 8)");
            }
        }
        return engine;
    }
    //#endif


    /**
     * Most of the functions in this library will only run in SaxonJS, some of them only in the browser
     */
    public static class Stub extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] args) throws XPathException {
            throw new XPathException("ixsl:" + getDetails().name.getLocalPart() + " is available only in SaxonJS");
        }
    }

    public static class Source extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return context.getController().getGlobalContextItem();
        }
    }

    public static class ScheduleActionFn extends SystemFunction {

        Location location;

        @Override
        public Expression makeFunctionCall(Expression... arguments) {
            // The "action" argument must not be pre-evaluated. Wrap it in an ObjectValue
            Expression[] copiedArgs = Arrays.copyOf(arguments, arguments.length);
            copiedArgs[2] = Literal.makeLiteral(new ObjectValue<Expression>(arguments[2]));
            ExpressionTool.copyLocationInfo(arguments[2], copiedArgs[2]);
            location = arguments[0].getLocation();
            return super.makeFunctionCall(copiedArgs);
        }

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            IntegerValue wait = (IntegerValue)arguments[0].head();
            StringValue documents = (StringValue)arguments[1].head();
            ObjectValue<Expression> wrappedActionObject = (ObjectValue<Expression>) arguments[2].head();
            Expression action = wrappedActionObject.getObject();
            if (documents != null) {
                AtomicIterator tokenIter = new Whitespace.Tokenizer(documents.getUnicodeStringValue());
                String baseURI = getRetainedStaticContext().getStaticBaseUriString();
                PackageData pd = getRetainedStaticContext().getPackageData();
                ParseOptions options = context.getConfiguration().getParseOptions();
                ItemMappingFunction mapper = ItemMapper.of(token -> DocumentFn.makeDoc(
                                    token.getStringValue(), baseURI, pd,
                                    options, context, location, false));
                SequenceIterator mapped = context.getConfiguration().getMultithreadedItemMappingIterator(
                        tokenIter, mapper);
                // discard the result; the aim was to put the documents in the document pool
                SequenceTool.toGroundedValue(mapped);
                return new LazySequence(action.iterate(context));
            } else {
                return EmptySequence.getInstance();
            }

        }
    }

    public static class Call extends SystemFunction {

        @Override
        @CSharpReplaceBody(code="return Saxon.Hej.value.EmptySequence.getInstance();")
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            ScriptEngine engine = obtainEngine();
            Object target = convertToJavaScript(arguments[0]);
            String method = convertToJavaScript(arguments[1]).toString();
            Object[] args = new Object[arguments.length - 2];
            for (int i=2; i<arguments.length; i++) {
                args[i-2] = convertToJavaScript(arguments[i]);
            }
            try {
                Object result = ((Invocable)engine).invokeMethod(target, method, args);
                return convertFromJavaScript(result);
            } catch (ScriptException e) {
                throw new XPathException("Failure in ixsl:call(): ", e);
            } catch (NoSuchMethodException e) {
                throw new XPathException("Failure in ixsl:call(): method " + method + " is not defined", e);
            }
        }

    }

    public static class Eval extends SystemFunction {

        @Override
        @CSharpReplaceBody(code = "return Saxon.Hej.value.EmptySequence.getInstance();")
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            ScriptEngine engine = obtainEngine();
            try {
                Object result = engine.eval(arguments[0].head().getUnicodeStringValue().toString());
                return JPConverter.allocate(result.getClass(), null, context.getConfiguration())
                        .convert(result, context);
            } catch (ScriptException e) {
                throw new XPathException("Failure in ixsl:eval(): ", e);
            }
        }

    }

    //#if CSHARP==false
    private static Sequence convertFromJavaScript(Object result) throws XPathException {
        //try {
            if (result instanceof String) {
                return new StringValue((String)result);
            } else if (result instanceof Double) {
                return new DoubleValue((Double)result);
            } else if (result instanceof Boolean) {
                return BooleanValue.get((Boolean) result);
            } else if (result instanceof Collection) {
                List<Item> items = new ArrayList<>();
                for (Object val : (Collection)result) {
                    SequenceIterator converted = convertFromJavaScript(val).iterate();
                    SequenceTool.supply(converted, (ItemConsumer<? super Item>) items::add);
                }
                return new SequenceExtent.Of<>(items);
            } else {
                return EmptySequence.getInstance();
            }
        //} catch ()
    }


    private static Object convertToJavaScript(Sequence supplied) throws XPathException {
        if (supplied instanceof NumericValue) {
            return ((NumericValue)supplied).getDoubleValue();
        } else if (supplied instanceof BooleanValue) {
            return ((BooleanValue)supplied).getBooleanValue();
        } else if (supplied instanceof AtomicValue) {
            return ((AtomicValue)supplied).getUnicodeStringValue();
        } else {
            List<Object> list = new ArrayList<>();
            SequenceIterator iter = supplied.iterate();
            SequenceTool.supply(iter, (ItemConsumer<? super Item>) next -> list.add(convertToJavaScript(next)));
            if (list.isEmpty()) {
                return null;
            } else if (list.size() > 1) {
                return list;
            } else {
                return list.get(0);
            }
        }
    }
    //#endif
}

// Copyright (c) 2018-2023 Saxonica Limited