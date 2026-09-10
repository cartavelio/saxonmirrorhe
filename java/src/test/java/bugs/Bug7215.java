package bugs;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.ExtensionFunctionCall;
import net.sf.saxon.lib.ExtensionFunctionDefinition;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertFalse;

public class Bug7215 {

    private static final String QUERY = "xquery version \"3.1\";\n"
            + "            \n"
            + "            declare namespace bug = \"http://example.org/bug\";\n"
            + "            declare namespace ex = \"http://example.org/ext\";\n"
            + "            \n"
            + "            (: Shared body: a side-effecting extension function call whose\n"
            + "               first argument is a multi-item sequence ending in a throw,\n"
            + "               and whose second argument (always returned on success) is a\n"
            + "               trivially type-valid xs:boolean. :)\n"
            + "            declare function local:body() as item()* {\n"
            + "                ex:force-evaluate(\n"
            + "                    (\n"
            + "                        1, 2, 3, 4,\n"
            + "                        fn:error(xs:QName('bug:example'), 'boom', <detail>payload</detail>)\n"
            + "                    ),\n"
            + "                    false()\n"
            + "                )\n"
            + "            };\n"
            + "            \n"
            + "            (: Same body, wrapped by functions differing only in declared\n"
            + "               return type. :)\n"
            + "            declare function local:untyped() as item()* {\n"
            + "                local:body()\n"
            + "            };\n"
            + "            \n"
            + "            declare function local:typed() as xs:boolean {\n"
            + "                local:body()\n"
            + "            };\n"
            + "            \n"
            + "            declare function local:probe($label as xs:string, $call as function() as item()*) as element(result) {\n"
            + "                <result label=\"{$label}\">{\n"
            + "                    try {\n"
            + "                        $call()\n"
            + "                    } catch bug:example {\n"
            + "                        'count($err:value) = ' || count($err:value)\n"
            + "                    }\n"
            + "                }</result>\n"
            + "            };\n"
            + "            \n"
            + "            <results>\n"
            + "                {local:probe('as item()*  (no return-type check)', function() { local:untyped() })}\n"
            + "                {local:probe('as xs:boolean (return-type check)', function() { local:typed() })}\n"
            + "            </results>";

    private void ForceEvaluateErrorObjectBug() {
    }

    @Test
    public void testErrorValue() throws Exception {
        final Processor processor = new Processor(false);
        registerForceEvaluate(processor.getUnderlyingConfiguration());

        final XQueryCompiler compiler = processor.newXQueryCompiler();
        final XQueryExecutable executable = compiler.compile(QUERY);
        final XQueryEvaluator evaluator = executable.load();
        final XdmValue result = evaluator.evaluate();

        assertFalse(result.getUnderlyingValue().getStringValue().contains("count($err:value) = 0"));
        //System.out.println(result);
    }

    /**
     * Registers {@code ex:force-evaluate($discard as item()*, $keep as item()*)
     * as item()*}: eagerly drains {@code $discard}'s iterator (discarding
     * every item, so that any error or side effect it contains is forced),
     * then returns {@code $keep} unchanged. {@code hasSideEffects()} is
     * {@code true}, matching the pattern used to keep Saxon's optimizer
     * from eliding a call whose return value would otherwise appear
     * unused.
     */
    private static void registerForceEvaluate(final Configuration configuration) {
        configuration.registerExtensionFunction(new ExtensionFunctionDefinition() {
            @Override
            public StructuredQName getFunctionQName() {
                return new StructuredQName("ex", "http://example.org/ext", "force-evaluate");
            }

            @Override
            public net.sf.saxon.value.SequenceType[] getArgumentTypes() {
                return new net.sf.saxon.value.SequenceType[]{net.sf.saxon.value.SequenceType.ANY_SEQUENCE, net.sf.saxon.value.SequenceType.ANY_SEQUENCE};
            }

            @Override
            public net.sf.saxon.value.SequenceType getResultType(final net.sf.saxon.value.SequenceType[] suppliedArgumentTypes) {
                return net.sf.saxon.value.SequenceType.ANY_SEQUENCE;
            }

            @Override
            public boolean hasSideEffects() {
                return true;
            }

            @Override
            public ExtensionFunctionCall makeCallExpression() {
                return new ExtensionFunctionCall() {
                    @Override
                    public Sequence call(final XPathContext context, final Sequence[] arguments) throws XPathException {
                        final SequenceIterator iterator = arguments[0].iterate();
                        while (iterator.next() != null) {
                            // Result intentionally discarded -- only
                            // evaluation order and any resulting exception
                            // matter here.
                        }
                        return arguments[1];
                    }
                };
            }
        });
    }
}

