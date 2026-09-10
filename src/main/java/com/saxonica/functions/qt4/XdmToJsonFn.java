package com.saxonica.functions.qt4;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.serialize.JSONEmitter;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;

import java.util.Properties;

/**
 * Implementation of the fn:json function proposed for 4.0
 */
public class JsonFn extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences
     * @return the result of the evaluation, in the form of a Sequence
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        GroundedValue val = arguments[0].materialize();
        PipelineConfiguration pipe = context.getConfiguration().makePipelineConfiguration();
        UnicodeBuilder builder = new UnicodeBuilder();
        Properties outputProperties = new Properties();
        outputProperties.setProperty(SaxonOutputKeys.UNFAILING, "yes");
        JSONEmitter emitter = new JSONEmitter(pipe, builder, outputProperties);
        emitSequence(val, emitter);
        return new StringValue(builder.toUnicodeString());
    }

    public void emitSequence(GroundedValue val, JSONEmitter emitter) throws XPathException {
        if (val.getLength() == 0) {
            emitter.writeAtomicValue(null);
        } else if (val.getLength() == 1) {
            emitItem((Item)val, emitter);
        } else {
            emitter.startArray(true);
            for (Item it : val.asIterable()) {
                emitItem(it, emitter);
            }
            emitter.endArray();
        }
    }

    private void emitItem(Item item, JSONEmitter emitter) throws XPathException {
        if (item instanceof AtomicValue) {
            if (item instanceof NumericValue) {
                if (((NumericValue)item).isNaN()) {
                    emitter.writeAtomicValue(
                            new StringValue("NaN"));
                } else if (Double.isInfinite(((NumericValue) item).getDoubleValue())) {
                    emitter.writeAtomicValue(
                            new StringValue(((NumericValue) item).getDoubleValue() < 0 ? "-INF": "INF"));
                } else {
                    emitter.writeAtomicValue((AtomicValue) item);
                }
            } else {
                emitter.writeAtomicValue((AtomicValue) item);
            }
        } else if (item instanceof NodeInfo) {
            switch (((NodeInfo)item).getNodeKind()) {
                case Type.COMMENT:
                    emitter.startMap(true);
                    emitter.writeKey("#comment");
                    emitter.writeAtomicValue(new StringValue(item.getStringValue()));
                    emitter.endMap();
                    break;
                case Type.TEXT:
                    emitter.startMap(true);
                    emitter.writeKey("#text");
                    emitter.writeAtomicValue(new StringValue(item.getStringValue()));
                    emitter.endMap();
                    break;
            }
        } else if (item instanceof ArrayItem) {
            emitter.startArray(true);
            for (GroundedValue member : ((ArrayItem)item).members()) {
                emitSequence(member, emitter);
            }
            emitter.endArray();
        } else if (item instanceof MapItem) {
            emitter.startMap(true);
            for (KeyValuePair pair : ((MapItem)item).keyValuePairs()) {
                emitter.writeKey(pair.key.getStringValue());
                emitSequence(pair.value, emitter);
            }
            emitter.endMap();
        }
    }
    
}

// Copyright (c) 2023 Saxonica Limited.

