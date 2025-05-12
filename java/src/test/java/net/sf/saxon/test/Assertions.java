package net.sf.saxon.test;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;

import java.util.function.Supplier;

public class Assertions {
    public static void assertXPathDeepEqual(Sequence expected, Sequence actual) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual);
    }

    public static void assertXPathDeepEqual(Sequence expected, Sequence actual, String message) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, message);
    }

    public static void assertXPathDeepEqual(Sequence expected, Sequence actual, Supplier<String> messageSupplier) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, messageSupplier);
    }

    public static void assertXPathDeepEqual(Sequence expected, GroundedValue actual) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual);
    }

    public static void assertXPathDeepEqual(Sequence expected, GroundedValue actual, String message) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, message);
    }

    public static void assertXPathDeepEqual(Sequence expected, GroundedValue actual, Supplier<String> messageSupplier) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, messageSupplier);
    }

    public static void assertXPathDeepEqual(GroundedValue expected, GroundedValue actual) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual);
    }

    public static void assertXPathDeepEqual(GroundedValue expected, GroundedValue actual, String message) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, message);
    }

    public static void assertXPathDeepEqual(GroundedValue expected, GroundedValue actual, Supplier<String> messageSupplier) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, messageSupplier);
    }

    public static void assertXPathDeepEqual(GroundedValue expected, Sequence actual) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual);
    }

    public static void assertXPathDeepEqual(GroundedValue expected, Sequence actual, String message) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, message);
    }

    public static void assertXPathDeepEqual(GroundedValue expected, Sequence actual, Supplier<String> messageSupplier) {
        AssertXPathDeepEqual.assertXPathDeepEqual(expected, actual, messageSupplier);
    }

    public static void assertSequenceLength(int expected, GroundedValue actual) {
        AssertSequenceLength.assertSequenceLength(expected, actual);
    }

    public static void assertSequenceLength(int expected, GroundedValue actual, String message) {
        AssertSequenceLength.assertSequenceLength(expected, actual, message);
    }

    public static void assertSequenceLength(int expected, GroundedValue actual, Supplier<String> messageSupplier) {
        AssertSequenceLength.assertSequenceLength(expected, actual, messageSupplier);
    }

    public static void assertSequenceLength(int expected, Sequence actual) {
        AssertSequenceLength.assertSequenceLength(expected, actual);
    }

    public static void assertSequenceLength(int expected, Sequence actual, String message) {
        AssertSequenceLength.assertSequenceLength(expected, actual, message);
    }

    public static void assertSequenceLength(int expected, Sequence actual, Supplier<String> messageSupplier) {
        AssertSequenceLength.assertSequenceLength(expected, actual, messageSupplier);
    }

    public static void assertEmptySequence(GroundedValue actual) {
        AssertEmptySequence.assertEmptySequence(actual);
    }

    public static void assertEmptySequence(GroundedValue actual, String message) {
        AssertEmptySequence.assertEmptySequence(actual, message);
    }

    public static void assertEmptySequence(GroundedValue actual, Supplier<String> messageSupplier) {
        AssertEmptySequence.assertEmptySequence(actual, messageSupplier);
    }

    public static void assertEmptySequence(Sequence actual) {
        AssertEmptySequence.assertEmptySequence(actual);
    }

    public static void assertEmptySequence(Sequence actual, String message) {
        AssertEmptySequence.assertEmptySequence(actual, message);
    }

    public static void assertEmptySequence(Sequence actual, Supplier<String> messageSupplier) {
        AssertEmptySequence.assertEmptySequence(actual, messageSupplier);
    }
}
