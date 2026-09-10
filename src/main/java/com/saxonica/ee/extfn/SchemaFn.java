package com.saxonica.ee.extfn;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Schema;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;


/**
 * This class implements the saxon:schema() extension function.
 * <p>There are two forms:
 * (a) saxon:schema() returns a function that can be called to get top-level schema information
 * (b) saxon:schema(component-kind, name) returns a function that can be called to get information about
 * a specific named schema component</p>
 */

public class SchemaFn extends SystemFunction {


    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
     *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
     *                  If the argument is always a singleton, then the single item may be obtained by calling
     *                  <code>arguments[i].next()</code>. The implementation is not obliged to read all the items in each
     *                  <code>SequenceIterator</code> if they are not required to compute the result; but if any SequenceIterator is not read
     *                  to completion, it is good practice to call its close() method.
     * @return an iterator over the results of the function. If the result is a single item, it can be
     * returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
     * the method should return <code>EmptyIterator.getInstance()</code>
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

        /*@Nullable*/
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        final EnterpriseConfiguration config = (EnterpriseConfiguration) context.getConfiguration();
        final Schema schema = getRetainedStaticContext().getPackageData().getImportedSchema();
        if (arguments.length == 0) {
            return schemaInfo(config, schema);

        } else if (arguments.length == 2) {
            String kind = arguments[0].head().getStringValue();
            QNameValue name = (QNameValue) arguments[1].head();
            return schemaComponentInfo(config, schema, kind, name.getStructuredQName());
        } else {
            throw new XPathException("Call to saxon:schema() requires either 0 or 2 arguments");
        }
    }

    private Sequence schemaInfo(EnterpriseConfiguration config, Schema schema) {
        CallableDelegate schemaFunction = new CallableDelegate((context, arguments) -> {
            PreparedSchema ps = (PreparedSchema)schema;
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Schema");
                case "type definitions": {
                    List<Item> list = new ArrayList<>();
                    for (SchemaType st : ps.getAllTypes()) {
                        list.add(st.getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(list);
                }
                case "attribute declarations": {
                    List<Item> list = new ArrayList<>();
                    for (AttributeDecl decl : ps.getAttributeDeclarations()) {
                        list.add(decl.getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(list);

                }
                case "element declarations": {
                    List<Item> list = new ArrayList<>();
                    for (ElementDecl decl : ps.getElementDeclarations()) {
                        list.add(decl.getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(list);
                }
                case "attribute group definitions": {
                    List<Item> list = new ArrayList<>();
                    for (AttributeGroupDecl group : ps.getAllAttributeGroups()) {
                        list.add(group.getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(list);

                }
                case "model group definitions": {
                    List<Item> list = new ArrayList<>();
                    for (ModelGroupDefinition group : ps.getAllModelGroups()) {
                        list.add(group.getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(list);

                }
                case "notation declarations": {
                    List<Item> list = new ArrayList<>();
                    for (Notation notation : ps.getAllNotations()) {
                        list.add(notation.getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(list);

                }
                case "identity-constraint definitions": {
                    List<Item> list = new ArrayList<>();
                    for (IdentityConstraint constraint : ps.getIdentityConstraints()) {
                        list.add(constraint.getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(list);

                }
                default:
                    return EmptySequence.getInstance();

            }
        });
        return new CallableFunction(1, schemaFunction, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }

    private Sequence schemaComponentInfo(EnterpriseConfiguration config, Schema schema, String kind, StructuredQName name)
            throws XPathException {
        PreparedSchema ps = (PreparedSchema) schema;
        switch (kind) {
            case "attribute declaration": {
                AttributeDecl decl = (AttributeDecl) ps.getAttributeDecl(name);
                return getComponentInfo(config, decl);
            }
            case "element declaration": {
                ElementDecl decl = (ElementDecl) ps.getElementDecl(name);
                return getComponentInfo(config, decl);
            }
            case "attribute group definition": {
                AttributeGroupDecl decl = ps.getAttributeGroup(name);
                return getComponentInfo(config, decl);
            }
            case "model group definition": {
                ModelGroupDefinition decl = ps.getGroup(name);
                return getComponentInfo(config, decl);
            }
            case "notation declarations": {
                Notation decl = ps.getNotation(name);
                return getComponentInfo(config, decl);
            }
            case "identity-constraint definitions": {
                IdentityConstraint decl = ps.getIdentityConstraint(name);
                return getComponentInfo(config, decl);
            }
            case "type definition":
                SchemaType type = ps.getSchemaType(name);
                return getComponentInfo(config, type);
            default:
                throw new XPathException("Unknown component kind " + kind);

        }
    }

    private Sequence getComponentInfo(EnterpriseConfiguration config, SchemaComponent component) {
        if (component == null) {
            return EmptySequence.getInstance();
        }
        return component.getComponentAsFunction();
    }


}

// Copyright (c) 2013-2023 Saxonica Limited
