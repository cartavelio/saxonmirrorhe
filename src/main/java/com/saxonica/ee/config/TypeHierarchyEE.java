package com.saxonica.ee.config;

import com.saxonica.ee.validate.SchemaElementTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.pattern.DocumentNodeTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.type.*;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSet;

import java.util.Optional;

import static net.sf.saxon.type.Affinity.OVERLAPS;
import static net.sf.saxon.type.Affinity.SUBSUMED_BY;
import static net.sf.saxon.type.Affinity.SUBSUMES;

/**
 * Refinement of the TypeHierarchy class to handle schema-aware types such as schema-element(A).
 */

public class TypeHierarchyEE extends TypeHierarchy {

    public TypeHierarchyEE(Configuration config) {
        super(config);
    }


    /**
     * Compute the relationship between the allowed content-types of two types
     *
     * @param t1 the first type
     * @param t2 the second type
     * @return the relationship, as one of the constants
     *         {@link Affinity#SAME_TYPE}, {@link Affinity#SUBSUMES},
     *         {@link Affinity#SUBSUMED_BY}, {@link Affinity#DISJOINT}, {@link Affinity#OVERLAPS}
     */
    @Override
    protected Affinity computeContentRelationship(ItemType t1, ItemType t2) {


        if (t1 instanceof DocumentNodeTest || t2 instanceof DocumentNodeTest) {
            return super.computeContentRelationship(t1,t2);
        }

        Affinity contentRelationship;
        boolean nillable1;
        boolean nillable2;

        // This is changed as a result of spec bug 10065. If t2 is schema-element(e2), it is
        // no longer sufficient to check the content type of t1 against the declared type of e2
        // (which might be xs:anyType, as in test schvalid010). Instead it must be checked against
        // the declared type of the element declaration corresponding to the actual name N of
        // the element being tested.
        SchemaType s1 = ((NodeTest) t1).getContentType();
        SchemaType s2 = ((NodeTest) t2).getContentType();

        Optional<IntSet> on1 = ((NodeTest) t1).getRequiredNodeNames();
        Optional<IntSet> on2 = ((NodeTest) t2).getRequiredNodeNames();

        try {
            if (t1 instanceof SchemaElementTest && on2.isPresent() && on2.get().size() == 1) {
                IntIterator ii2 = on2.get().iterator();
                int name2 = ii2.hasNext() ? ii2.next() : -1;
                SchemaDeclaration decl2 = config.getElementDeclaration(name2);
                if (decl2 != null) {
                    s1 = decl2.getType();
                }
            }
        } catch (MissingComponentException e) {
            return OVERLAPS;
        }
        try {
            if (t2 instanceof SchemaElementTest && on1.isPresent() && on1.get().size() == 1) {
                IntIterator ii1 = on1.get().iterator();
                int name1 = ii1.hasNext() ? ii1.next() : -1;
                SchemaDeclaration decl1 = config.getElementDeclaration(name1);
                if (decl1 != null) {
                    s2 = decl1.getType();
                }
            }
        } catch (MissingComponentException e) {
            return OVERLAPS;
        }

        contentRelationship = schemaTypeRelationship(s1, s2);

        nillable1 = ((NodeTest) t1).isNillable();
        if (nillable1 && t1 instanceof SchemaElementTest && on2.isPresent() && on2.get().size() < 1000) {
            for (IntIterator fp = on2.get().iterator(); fp.hasNext(); ) {
                nillable1 = ((SchemaElementTest) t1).isNillable(fp.next());
                if (!nillable1) {
                    break;
                }
            }
        }
        nillable2 = ((NodeTest) t2).isNillable();
        if (nillable2 && t2 instanceof SchemaElementTest && on1.isPresent() && on1.get().size() < 1000) {
            for (IntIterator fp = on1.get().iterator(); fp.hasNext(); ) {
                nillable2 = ((SchemaElementTest) t2).isNillable(fp.next());
                if (!nillable2) {
                    break;
                }
            }
        }

        // Adjust the results to take nillability into account

        if (nillable1 != nillable2) {
            switch (contentRelationship) {
                case SUBSUMES:
                    if (nillable2) {
                        contentRelationship = OVERLAPS;
                        break;
                    }
                    break;
                case SUBSUMED_BY:
                    if (nillable1) {
                        contentRelationship = OVERLAPS;
                        break;
                    }
                    break;
                case SAME_TYPE:
                    if (nillable1) {
                        contentRelationship = SUBSUMES;
                    } else {
                        contentRelationship = SUBSUMED_BY;
                    }
                    break;
                default:
                    break;
            }
        }
        return contentRelationship;
    }


}

// Copyright (c) 2018-2023 Saxonica Limited