using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn {
    public class SchemaFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Eej.config.EnterpriseConfiguration config = (Saxon.Eej.config.EnterpriseConfiguration)(context.getConfiguration());
            if ((arguments.Length == 0)) {
                return schemaInfo(config);
            } else {
                if ((arguments.Length == 2)) {
                    string kind = arguments[0].head().getStringValue();
                    Saxon.Hej.value.QNameValue name = (Saxon.Hej.value.QNameValue)(arguments[1].head());
                    return schemaComponentInfo(config, kind, name.getStructuredQName());
                } else {
                    throw new Saxon.Hej.trans.XPathException("Call to saxon:schema() requires either 0 or 2 arguments");
                }
            }
        }
        private Saxon.Hej.om.Sequence schemaInfo(Saxon.Eej.config.EnterpriseConfiguration config) {
            Saxon.Hej.expr.CallableDelegate schemaFunction = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Schema");
                  case "type definitions":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Hej.type.SchemaType st in config.getSuperSchema().getAllTypes()) {
                            list.Add(st.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    }
                  case "attribute declarations":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.AttributeDecl decl in config.getSuperSchema().getAttributeDeclarations()) {
                            list.Add(decl.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    }
                  case "element declarations":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.ElementDecl decl in config.getSuperSchema().getElementDeclarations()) {
                            list.Add(decl.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    }
                  case "attribute group definitions":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.AttributeGroupDecl group in config.getSuperSchema().getAllAttributeGroups()) {
                            list.Add(group.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    }
                  case "model group definitions":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.ModelGroupDefinition group in config.getSuperSchema().getAllModelGroups()) {
                            list.Add(group.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    }
                  case "notation declarations":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.Notation notation in config.getSuperSchema().getAllNotations()) {
                            list.Add(notation.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    }
                  case "identity-constraint definitions":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.IdentityConstraint constraint in config.getSuperSchema().getIdentityConstraints()) {
                            list.Add(constraint.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    }
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, schemaFunction, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
        private Saxon.Hej.om.Sequence schemaComponentInfo(Saxon.Eej.config.EnterpriseConfiguration config, string kind, Saxon.Hej.om.StructuredQName name) {
            switch (kind) {
              case "attribute declaration":
                {
                    Saxon.Eej.ee.schema.AttributeDecl decl = (Saxon.Eej.ee.schema.AttributeDecl)(config.getAttributeDeclaration(name));
                    return getComponentInfo(config, decl);
                }
              case "element declaration":
                {
                    Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(config.getElementDeclaration(name));
                    return getComponentInfo(config, decl);
                }
              case "attribute group definition":
                {
                    Saxon.Eej.ee.schema.AttributeGroupDecl decl = config.getSuperSchema().getAttributeGroup(name);
                    return getComponentInfo(config, decl);
                }
              case "model group definition":
                {
                    Saxon.Eej.ee.schema.ModelGroupDefinition decl = config.getSuperSchema().getGroup(name);
                    return getComponentInfo(config, decl);
                }
              case "notation declarations":
                {
                    Saxon.Eej.ee.schema.Notation decl = config.getSuperSchema().getNotation(name);
                    return getComponentInfo(config, decl);
                }
              case "identity-constraint definitions":
                {
                    Saxon.Eej.ee.schema.IdentityConstraint decl = config.getSuperSchema().getIdentityConstraint(name);
                    return getComponentInfo(config, decl);
                }
              case "type definition":
                Saxon.Hej.type.SchemaType type = config.getSchemaType(name);
                return getComponentInfo(config, type);
              default:
                throw new Saxon.Hej.trans.XPathException(("Unknown component kind "+kind));
            }
        }
        private Saxon.Hej.om.Sequence getComponentInfo(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Hej.type.SchemaComponent component) {
            if ((component == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return component.getComponentAsFunction();
        }
    
    }
}