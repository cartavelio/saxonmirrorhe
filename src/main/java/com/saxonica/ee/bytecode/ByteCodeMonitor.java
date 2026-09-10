package com.saxonica.ee.bytecode;

import net.sf.saxon.lib.Feature;
import net.sf.saxon.z.IntHashMap;

import java.io.PrintWriter;
import java.util.Locale;

/**
 * Class to accumulate statistics on the effectiveness of byte code generation.
 * Enabled by setting the configuration property {@link Feature#MONITOR_HOT_SPOT_BYTE_CODE}
 */
public class ByteCodeMonitor {

    public IntHashMap<ByteCodeDetails> stats = new IntHashMap<ByteCodeDetails>();

    public void logCompilation(ByteCodeCandidate candidate, long time) {
        ByteCodeDetails details = obtainDetails(candidate);
        if (details.isCompiled) {
            throw new IllegalStateException();
        }
        details.isCompiled = true;
        details.compileTime = time;
    }

    private ByteCodeDetails obtainDetails(ByteCodeCandidate candidate) {
        int id = candidate.getId();
        ByteCodeDetails details = stats.get(id);
        if (details == null) {
            details = new ByteCodeDetails();
            details.candidate = candidate;
            stats.put(id, details);
        }
        return details;
    }

    public void logExecution(ByteCodeCandidate candidate, String evalMode, long time) {
        ByteCodeDetails details = obtainDetails(candidate);
        if (details.isCompiled) {
            details.byteCodeExecutions++;
            details.timeInByteCode += time;
        } else {
            details.interpretedExecutions++;
            details.timeInInterpreter += time;
        }
        if ("ebv".equals(evalMode)) {
            details.ebvCount++;
        } else if ("eval".equals(evalMode)) {
            details.evalCount++;
        } else if ("process".equals(evalMode)) {
            details.processCount++;
        } else if ("iterate".equals(evalMode)) {
            details.iterateCount++;
        }
    }

    /**
     * Generate an XML report of the collected statistics.
     *
     * <p>The following attributes are output for each candidate expression. Timings are in microseconds:</p>
     * <ul>
     * <li><code>text</code> Abbreviated text of the expression</li>
     * <li><code>intCount</code> Number of times the expression was evaluated in the interpreter</li>
     * <li><code>intTime</code> Cumulative time spent evaluating the expression (including subexpressions) in the interpreter</li>
     * <li><code>bcCount</code> Number of times the expression was evaluated as byte-code</li>
     * <li><code>bcTime</code> Cumulative time spent evaluating the expression (including subexpressions) as byte-code</li>
     * <li><code>compileTime</code> Time spent generating bytecode for the expression</li>
     * <li><code>ratio</code> Average byte-code execution time as a proportion of average interpreted execution time</li>
     * <li><code>ebv</code> Number of evaluations of the expressions <code>effectiveBooleanValue()</code> method</li>
     * <li><code>eval</code> Number of evaluations of the expressions <code>evaluateItem()</code> method</li>
     * <li><code>iterate</code> Number of evaluations of the expressions <code>iterate()</code> method</li>
     * <li><code>process</code> Number of evaluations of the expressions <code>process()</code> method</li>
     * <li><code>module</code> URI of the module containing the expression</li>
     * <li><code>line</code> Line number of the expression within this module</li>
     * </ul>
     * @param w the destination to which the generated XML will be written
     */

    public void report(PrintWriter w) {
        w.println("<stats unit='microseconds'>");
        for (ByteCodeDetails details : stats.valueSet()) {
            StringBuilder sb = new StringBuilder();
            sb.append(" <exp ");
            att(sb, "id", ""+details.candidate.getId());
            att(sb, "text", details.candidate.getBaseExpression().toShortString());
            att(sb, "intCount", "" + details.interpretedExecutions);
            att(sb, "intTime", "" + details.timeInInterpreter / 1000);
            att(sb, "bcCount", "" + details.byteCodeExecutions);
            att(sb, "bcTime", "" + details.timeInByteCode / 1000);
            att(sb, "compileTime", "" + details.compileTime / 1000);
            if (details.byteCodeExecutions > 0 && details.interpretedExecutions > 0) {
                double avBC = details.timeInByteCode / details.byteCodeExecutions;
                double avInt = details.timeInInterpreter / details.interpretedExecutions;
                double ratio = avBC / avInt;
                // Beware IKVM problems: see bugs 4247 and 4305
                att(sb, "ratio", String.format((Locale)null, "%.3f", ratio));
            }
            att(sb, "ebv", "" + details.ebvCount);
            att(sb, "eval", "" + details.evalCount);
            att(sb, "iterate", "" + details.iterateCount);
            att(sb, "process", "" + details.processCount);
            att(sb, "module", details.candidate.getLocation().getSystemId());
            att(sb, "line", "" + details.candidate.getLocation().getLineNumber());
            sb.append("/>");
            w.println(sb);
        }
        w.println("</stats>");
    }

    private static void att(StringBuilder sb, String name, String value) {
        sb.append(name);
        sb.append("=\"");
        sb.append(value.replace("\"", "&quot;").replace("&", "&amp;").replace("<", "&lt;"));
        sb.append("\" ");
    }

    private static class ByteCodeDetails {
        ByteCodeCandidate candidate;
        boolean isCompiled;
        long compileTime;
        long interpretedExecutions;
        long timeInInterpreter;
        long byteCodeExecutions;
        long timeInByteCode;
        long ebvCount;
        long evalCount;
        long processCount;
        long iterateCount;
    }
}

// Copyright (c) 2018-2022 Saxonica Limited