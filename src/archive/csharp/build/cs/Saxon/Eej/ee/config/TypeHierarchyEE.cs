using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.config {
    public class TypeHierarchyEE : Saxon.Hej.type.TypeHierarchy {
        public TypeHierarchyEE(Saxon.Hej.Configuration config) : base (config) {
        }
        public override Saxon.Hej.type.ItemType getGenericFunctionItemType() {
            return Saxon.Hej.type.AnyFunctionType.getInstance();
        }
        protected internal override Saxon.Hej.type.Affinity computeContentRelationship(Saxon.Hej.type.ItemType t1, Saxon.Hej.type.ItemType t2, Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> on1, Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> on2) {
            Saxon.Hej.type.Affinity contentRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
            bool nillable1 = true;
            bool nillable2 = true;
            if ((t1 is Saxon.Hej.pattern.DocumentNodeTest)) {
                if ((t2 is Saxon.Hej.pattern.DocumentNodeTest)) {
                    contentRelationship = relationship(((Saxon.Hej.pattern.DocumentNodeTest)(t1)).getElementTest(), ((Saxon.Hej.pattern.DocumentNodeTest)(t2)).getElementTest());
                } else {
                    contentRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                }
            } else {
                if ((t2 is Saxon.Hej.pattern.DocumentNodeTest)) {
                    contentRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                } else {
                    Saxon.Hej.type.SchemaType s1 = ((Saxon.Hej.pattern.NodeTest)(t1)).getContentType();
                    Saxon.Hej.type.SchemaType s2 = ((Saxon.Hej.pattern.NodeTest)(t2)).getContentType();
                    try {
                        if ((((t1 is Saxon.Eej.ee.validate.SchemaElementTest) && on2.isPresent()) && (on2.get().size() == 1))) {
                            Saxon.Hej.z.IntIterator ii2 = on2.get().iterator();
                            int name2 = (ii2.hasNext() ? ii2.next() : -1);
                            Saxon.Hej.type.SchemaDeclaration decl2 = config.getElementDeclaration(name2);
                            if ((decl2 != null)) {
                                s1 = decl2.getType();
                            }
                        }
                    } catch (Saxon.Hej.type.MissingComponentException e) {
                        return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                    }
                    try {
                        if ((((t2 is Saxon.Eej.ee.validate.SchemaElementTest) && on1.isPresent()) && (on1.get().size() == 1))) {
                            Saxon.Hej.z.IntIterator ii1 = on1.get().iterator();
                            int name1 = (ii1.hasNext() ? ii1.next() : -1);
                            Saxon.Hej.type.SchemaDeclaration decl1 = config.getElementDeclaration(name1);
                            if ((decl1 != null)) {
                                s2 = decl1.getType();
                            }
                        }
                    } catch (Saxon.Hej.type.MissingComponentException e) {
                        return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                    }
                    contentRelationship = schemaTypeRelationship(s1, s2);
                    nillable1 = ((Saxon.Hej.pattern.NodeTest)(t1)).isNillable();
                    if ((((nillable1 && (t1 is Saxon.Eej.ee.validate.SchemaElementTest)) && on2.isPresent()) && (on2.get().size()<1000))) {
                        for (Saxon.Hej.z.IntIterator fp = on2.get().iterator(); fp.hasNext(); ) {
                            nillable1 = ((Saxon.Eej.ee.validate.SchemaElementTest)(t1)).isNillable(fp.next());
                            if (!(nillable1)) {
                                break ;
                            }
                        }
                    }
                    nillable2 = ((Saxon.Hej.pattern.NodeTest)(t2)).isNillable();
                    if ((((nillable2 && (t2 is Saxon.Eej.ee.validate.SchemaElementTest)) && on1.isPresent()) && (on1.get().size()<1000))) {
                        for (Saxon.Hej.z.IntIterator fp = on1.get().iterator(); fp.hasNext(); ) {
                            nillable2 = ((Saxon.Eej.ee.validate.SchemaElementTest)(t2)).isNillable(fp.next());
                            if (!(nillable2)) {
                                break ;
                            }
                        }
                    }
                }
            }
            if ((nillable1 != nillable2)) {
                switch (contentRelationship) {
                  case Saxon.Hej.type.Affinity.SUBSUMES:
                    if (nillable2) {
                        contentRelationship = /*implicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                        break ;
                    }
                    break ;
                  case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                    if (nillable1) {
                        contentRelationship = /*implicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                        break ;
                    }
                    break ;
                  case Saxon.Hej.type.Affinity.SAME_TYPE:
                    if (nillable1) {
                        contentRelationship = /*implicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                    } else {
                        contentRelationship = /*implicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    }
                    break ;
                  default:
                    break ;
                }
            }
            return contentRelationship;
        }
    
    }
}