package internaltests;

import com.saxonica.functions.registry.XPath40FunctionSet;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.functions.MathFunctionSet;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.ma.map.MapFunctionSet;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.*;
import net.sf.saxon.s9api.streams.Steps;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.SequenceType;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


/**
 *  Test that all functions in the QT4 function catalog have been implemented with the correct
 *  function signatures
 */
public class SignatureTest {

    // Functions not yet implemented

    private final static Set<String> notImplemented = setOf(
            "fn:analyze-string",
            "fn:csv-to-simple-rows",
            "fn:csv-to-xml",
            "fn:log",
            "fn:format-number#3",
            "fn:parse-html",
            "fn:stack-trace",
            "fn:json",
            "fn:parse-csv",
            "fn:transitive-closure",
            "map:deep-update",
            "map:replace",
            "map:substitute",
            "map:values",
            "array:values",
            "array:deep-update"
    );

    // Function arguments where Saxon knowingly declares the type differently from the language spec.
    // Format is prefix:local#arg where arg is the 1-based argument number (not the arity!)
    private final static Set<String> argExceptions = setOf(
            "fn:namespace-uri-for-prefix#1",
            "fn:subsequence#2",
            "fn:subsequence#3",
            "fn:substring#2",
            "fn:substring#3",
            "math:pow#2"
    );

    private final static Set<String> resultExceptions = setOf(
            "fn:in-scope-namespaces",
            "fn:void"

    );

    private static Set<String> setOf(String... str) {
       Set<String> result = new HashSet<>();
       Collections.addAll(result, str);
       return result;
    }

    @Test
    public void testFunctionSignatures() {
        File catalog = new File(new ConfigTest().getQt4SpecsDir()
                                        + "specifications/xpath-functions-40/src/function-catalog.xml");
        int failures = 0;
        try {
            Processor proc = new Processor(true);
            Configuration config = proc.getUnderlyingConfiguration();
            TypeHierarchy th = config.getTypeHierarchy();
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode catalogDoc = builder.build(catalog);
            for (XdmNode proto : catalogDoc.select(Steps.descendant("proto")).asListOfNodes()) {
                String localName = proto.getAttributeValue(new QName("name"));
                XdmNode fnNode = proto.getParent().getParent();
                String prefix = fnNode.getAttributeValue(new QName("prefix"));
                if (prefix == null) {
                    prefix = "fn";
                }
                //System.err.println("Processing " + prefix + ":" + localName);
                BuiltInFunctionSet functionSet = null;

                if (prefix.equals("fn")) {
                    functionSet = XPath40FunctionSet.getInstance();
                } else if (prefix.equals("math")) {
                    functionSet = MathFunctionSet.getInstance();
                } else if (prefix.equals("map")) {
                    functionSet = MapFunctionSet.getInstance(40);
                } else if (prefix.equals("array")) {
                    functionSet = ArrayFunctionSet.getInstance(40);
                } else {
                    continue;
                }
                List<XdmNode> args = proto.select(Steps.child("arg")).asListOfNodes();
                BuiltInFunctionSet.Entry entry = functionSet.getFunctionDetails(localName, args.size());
                String fnKey = prefix + ":" + localName;
                if (entry == null) {
                    if (!notImplemented.contains(fnKey)) {
                        failures += failure("No entry found for " + fnKey + "#" + args.size());
                    }
                    continue;
                }
                entry = entry.populator.apply(entry);
                String returnTypeAtt = proto.getAttributeValue(new QName("return-type"));
                if (returnTypeAtt == null) {
                    // TODO: process record types
                    returnTypeAtt = "none";
                }
                if (!returnTypeAtt.equals("none")) {
                    SequenceType requiredReturnType = parseType(config, returnTypeAtt);
                    SequenceType actualReturnType = new SequenceType(entry.itemType, entry.cardinality);
                    if (!requiredReturnType.isSameType(actualReturnType, th)) {
                        if (!resultExceptions.contains(fnKey)) {
                            failures += failure("Wrong return type for " + prefix + ":" + localName + "#" + args.size());
                        }
                    }
                }
                if (!localName.equals("concat")) {
                    for (int i = 0; i < args.size(); i++) {
                        XdmNode argNode = args.get(i);
                        if (argNode == null) {
                            failures += failure("No entry for argument " + (i + 1) + " of " + prefix + ":" +localName + "#" + args.size());
                            continue;
                        }
                        final String argType = argNode.getAttributeValue(new QName("type"));
                        if (argType != null) {
                            // TODO: process record types
                            SequenceType requiredArgType = parseType(config, argType);
                            SequenceType actualArgType = entry.paramTypes[i];
                            if (!requiredArgType.isSameType(actualArgType, th)) {
                                String argKey = prefix + ":" + localName + "#" + (i+1);
                                if (!argExceptions.contains(argKey)) {
                                    failures += failure("Wrong type for argument " + (i + 1)
                                                                + " of " + prefix + ":" + localName + "#" + args.size());
                                }
                            }
                        }
                    }
                }
            }
            assertEquals(0, failures);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }

    private int failure(String cause) {
        System.err.println(cause);
        return 1;
    }

    private SequenceType parseType(Configuration config, String typeDesignation) {
        try {
            IndependentContext env = new IndependentContext(config);
            env.setXPathLanguageLevel(40);
            env.declareNamespace("xs", NamespaceUri.SCHEMA);
            env.declareNamespace("fn", NamespaceUri.FN);
            XPathParser parser = new XPathParser(env);
            return parser.parseSequenceType(typeDesignation, env);
        } catch (XPathException e) {
            e.printStackTrace();
            System.err.println("Unknown type " + typeDesignation);
            //fail("Unknown type " + typeDesignation);
            return SequenceType.ANY_SEQUENCE;
        }
    }


}
