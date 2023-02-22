using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn {
    public class ValidateFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            System.Collections.Generic.IList<Saxon.Hej.lib.Invalidity> errorList = new System.Collections.Generic.List<Saxon.Hej.lib.Invalidity>(5);
            bool returnTreeOnFailure = false;
            Saxon.Hej.om.NodeInfo resultNode = null;
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.om.NodeInfo input = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if ((input == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.type.SchemaType schemaType = null;
            int validation = Saxon.Hej.lib.Validation.STRICT;
            bool expand = true;
            bool buildTree = true;
            if ((arguments.Length>=2)) {
                Saxon.Hej.ma.map.MapItem options = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
                Saxon.Hej.om.Sequence val;
                val = options.get(Saxon.Hej.value.StringValue.bmp("buildTree"));
                if (((val != null) && (val.head() != null))) {
                    if ((val is Saxon.Hej.value.BooleanValue)) {
                        buildTree = ((Saxon.Hej.value.BooleanValue)(val)).getBooleanValue();
                        returnTreeOnFailure = buildTree;
                    } else {
                        throw new Saxon.Hej.trans.XPathException("buildTree must be true() or false() or ()");
                    }
                }
                val = options.get(Saxon.Hej.value.StringValue.bmp("type"));
                if ((val != null)) {
                    if ((val is Saxon.Hej.value.QNameValue)) {
                        schemaType = config.getSchemaType(((Saxon.Hej.value.QNameValue)(val)).getStructuredQName());
                        if ((schemaType == null)) {
                            throw new Saxon.Hej.trans.XPathException(("Unknown type "+val), "XPTY0004");
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException("'type' option in second argument of saxon:validate() must be a QName", "XPTY0004");
                    }
                }
                val = options.get(Saxon.Hej.value.StringValue.bmp("processContents"));
                if ((val != null)) {
                    if ((val is Saxon.Hej.value.StringValue)) {
                        string s = ((Saxon.Hej.value.StringValue)(val)).getStringValue();
                        switch (s) {
                          case "strict":
                            break ;
                          case "lax":
                            validation = Saxon.Hej.lib.Validation.LAX;
                            break ;
                          default:
                            throw new Saxon.Hej.trans.XPathException("processContents must be 'strict' or 'lax'");
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException("'processContents' option in second argument of saxon:validate() must be a string", "XPTY0004");
                    }
                }
                val = options.get(Saxon.Hej.value.StringValue.bmp("expandDefaults"));
                if ((val != null)) {
                    if ((val is Saxon.Hej.value.BooleanValue)) {
                        expand = ((Saxon.Hej.value.BooleanValue)(val)).getBooleanValue();
                    } else {
                        throw new Saxon.Hej.trans.XPathException("processContents must be true() or false()");
                    }
                }
            } else {
            }
            Saxon.Hej.type.ValidationParams @params = new Saxon.Hej.type.ValidationParams();
            if ((arguments.Length>=3)) {
                Saxon.Hej.ma.map.MapItem rawParams = (Saxon.Hej.ma.map.MapItem)(arguments[2].head());
                foreach (Saxon.Hej.ma.map.KeyValuePair pair in rawParams.keyValuePairs()) {
                    Saxon.Hej.value.AtomicValue key = pair.key;
                    Saxon.Hej.value.QNameValue qKey;
                    if ((key is Saxon.Hej.value.QNameValue)) {
                        qKey = (Saxon.Hej.value.QNameValue)(key);
                    } else {
                        if ((key is Saxon.Hej.value.StringValue)) {
                            qKey = new Saxon.Hej.value.QNameValue("", Saxon.Hej.om.NamespaceUri.NULL, key.getStringValue());
                        } else {
                            throw new Saxon.Hej.trans.XPathException("key value in third argument to saxon:validate must be a string or QName");
                        }
                    }
                    Saxon.Impl.Helpers.MapUtils.Put(@params, qKey.getStructuredQName(), pair.value);
                }
            } else {
                @params = new Saxon.Hej.type.ValidationParams();
            }
            Saxon.Hej.lib.ParseOptions parseOptions = new Saxon.Hej.lib.ParseOptions().withValidationParams(@params).withSchemaValidationMode(validation).withTopLevelType(schemaType).withExpandAttributeDefaults(expand).withContinueAfterValidationErrors(true);
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> currentErrorNode = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(1);
            Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
            pipe.setParseOptions(parseOptions);
            pipe.setXPathContext(context);
            pipe.setCopyInformee(element => {
                currentErrorNode.Clear();
                currentErrorNode.Add(element);
                return null;
            } );
            parseOptions = parseOptions.withInvalidityHandler(new Saxon.Eej.ee.extfn.ValidateFn.LocalInvalidityHandler(config, currentErrorNode, errorList));
            int kind = input.getNodeKind();
            switch (kind) {
              case Saxon.Hej.type.Type.DOCUMENT:
                {
                    Saxon.Hej.@event.Receiver @out = (buildTree ? new Saxon.Hej.@event.SequenceCollector(pipe) : new Saxon.Hej.@event.Sink(pipe));
                    Saxon.Hej.@event.Receiver val = config.getDocumentValidator(@out, input.getBaseURI(), parseOptions, null);
                    val.setPipelineConfiguration(@out.getPipelineConfiguration());
                    val.setSystemId(input.getBaseURI());
                    input.copy(val, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES, Saxon.Hej.expr.parser.Loc.NONE);
                    resultNode = (buildTree ? (Saxon.Hej.om.NodeInfo)(((Saxon.Hej.@event.SequenceCollector)(@out)).getFirstItem()) : null);
                    break ;
                }
              case Saxon.Hej.type.Type.ELEMENT:
                {
                    Saxon.Hej.@event.SequenceReceiver @out = (buildTree ? new Saxon.Hej.@event.SequenceCollector(pipe) : new Saxon.Hej.@event.Sink(pipe));
                    parseOptions = parseOptions.withTopLevelElement(Saxon.Hej.om.NameOfNode.makeName(input).getStructuredQName());
                    Saxon.Hej.@event.Receiver val = config.getElementValidator(@out, parseOptions, Saxon.Hej.expr.parser.Loc.NONE);
                    val.setPipelineConfiguration(@out.getPipelineConfiguration());
                    val.setSystemId(input.getBaseURI());
                    input.copy(val, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES, Saxon.Hej.expr.parser.Loc.NONE);
                    resultNode = (buildTree ? (Saxon.Hej.om.NodeInfo)(((Saxon.Hej.@event.SequenceCollector)(@out)).getFirstItem()) : null);
                    break ;
                }
              case Saxon.Hej.type.Type.ATTRIBUTE:
                Saxon.Hej.type.SimpleType simpleType = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                if ((schemaType is Saxon.Hej.type.SimpleType)) {
                    simpleType = (Saxon.Hej.type.SimpleType)(schemaType);
                    Saxon.Hej.type.ValidationFailure err = simpleType.validateContent(input.getUnicodeStringValue(), Saxon.Hej.expr.instruct.DummyNamespaceResolver.getInstance(), context.getConfiguration().getConversionRules());
                    if ((err != null)) {
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((((("Attribute value "+Saxon.Hej.trans.Err.wrap(input.getUnicodeStringValue(), Saxon.Hej.trans.Err.VALUE))+" does not match the required type ")+schemaType.getDescription())+". ")+err.getMessage()));
                        ve.setErrorCode("XTTE1540");
                        ve.setSchemaType(schemaType);
                        ve.setPath(Saxon.Hej.om.AbsolutePath.pathToNode(input));
                        errorList.Add(ve);
                    }
                } else {
                    if (((validation == Saxon.Hej.lib.Validation.STRICT) || (validation == Saxon.Hej.lib.Validation.LAX))) {
                        try {
                            simpleType = config.validateAttribute(Saxon.Hej.om.NameOfNode.makeName(input).getStructuredQName(), input.getUnicodeStringValue(), validation);
                        } catch (Saxon.Hej.type.ValidationException e) {
                            Saxon.Hej.type.ValidationFailure vf = e.getValidationFailure();
                            vf.setInvalidNode(input);
                            errorList.Add(vf);
                        }
                    } else {
                        resultNode = input;
                        break ;
                    }
                }
                if (buildTree) {
                    Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(config);
                    orphan.setNodeKind(Saxon.Hej.type.Type.ATTRIBUTE);
                    orphan.setNodeName(Saxon.Hej.om.NameOfNode.makeName(input));
                    orphan.setStringValue(input.getUnicodeStringValue());
                    orphan.setTypeAnnotation(simpleType);
                    resultNode = orphan;
                } else {
                    resultNode = null;
                }
                break ;
              default:
                throw new Saxon.Hej.trans.XPathException("Input to saxon:validate must be a document, element, or attribute node", "XPTY0004");
            }
            Saxon.Hej.ma.map.DictionaryMap result = new Saxon.Hej.ma.map.DictionaryMap();
            if (errorList.Count==0) {
                if (buildTree) {
                    result.initialPut("node", resultNode);
                }
                result.initialPut("valid", Saxon.Hej.value.BooleanValue.TRUE);
                return result;
            } else {
                result.initialPut("valid", Saxon.Hej.value.BooleanValue.FALSE);
                if (returnTreeOnFailure) {
                    result.initialPut("node", resultNode);
                }
                System.Collections.Generic.IList<Saxon.Hej.om.Item> errors = new System.Collections.Generic.List<Saxon.Hej.om.Item>(errorList.Count);
                foreach (Saxon.Hej.lib.Invalidity te in errorList) {
                    Saxon.Hej.type.ValidationFailure v = (Saxon.Hej.type.ValidationFailure)(te);
                    Saxon.Hej.ma.map.DictionaryMap im = new Saxon.Hej.ma.map.DictionaryMap();
                    if ((v.getErrorCodeQName() != null)) {
                        im.initialPut("code", new Saxon.Hej.value.QNameValue(v.getErrorCodeQName(), Saxon.Hej.type.BuiltInAtomicType.QNAME));
                    }
                    im.initialPut("message", new Saxon.Hej.value.StringValue(v.getMessage()));
                    if ((v.getSchemaPart() != -1)) {
                        im.initialPut("specPart", new Saxon.Hej.value.Int64Value(v.getSchemaPart()));
                    }
                    if ((v.getConstraintName() != null)) {
                        im.initialPut("constraint", new Saxon.Hej.value.StringValue(v.getConstraintName()));
                    }
                    if ((v.getConstraintClauseNumber() != null)) {
                        im.initialPut("clause", new Saxon.Hej.value.StringValue(v.getConstraintClauseNumber()));
                    }
                    if ((v.getLineNumber() != -1)) {
                        im.initialPut("line", new Saxon.Hej.value.Int64Value(v.getLineNumber()));
                    }
                    if ((v.getColumnNumber() != -1)) {
                        im.initialPut("colummn", new Saxon.Hej.value.Int64Value(v.getColumnNumber()));
                    }
                    if ((v.getSystemId() != null)) {
                        im.initialPut("systemId", new Saxon.Hej.value.StringValue(v.getSystemId()));
                    }
                    if ((v.getPublicId() != null)) {
                        im.initialPut("publicId", new Saxon.Hej.value.StringValue(v.getPublicId()));
                    }
                    if ((v.getInvalidNode() != null)) {
                        im.initialPut("node", v.getInvalidNode());
                    }
                    if ((v.getPath() != null)) {
                        im.initialPut("path", new Saxon.Hej.value.StringValue(v.getPath().getPathUsingUris()));
                    }
                    errors.Add(im);
                }
                result.initialPut("errors", Saxon.Hej.value.SequenceExtent.makeSequenceExtent(errors));
                return result;
            }
        }
        private class LocalInvalidityHandler : Saxon.Hej.lib.StandardInvalidityHandler {
            internal System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> currentErrorNode;
            internal System.Collections.Generic.IList<Saxon.Hej.lib.Invalidity> errorList;
            public LocalInvalidityHandler(Saxon.Hej.Configuration config, System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> currentErrorNode, System.Collections.Generic.IList<Saxon.Hej.lib.Invalidity> errorList) : base (config) {
                this.errorList = errorList;
                this.currentErrorNode = currentErrorNode;
            }
            public override void startReporting(string systemId) {
            }
            public override void reportInvalidity(Saxon.Hej.lib.Invalidity failure) {
                if (!(currentErrorNode.Count==0)) {
                    ((Saxon.Hej.type.ValidationFailure)(failure)).setSourceLocator(currentErrorNode[0]);
                    ((Saxon.Hej.type.ValidationFailure)(failure)).setPath(Saxon.Hej.om.AbsolutePath.pathToNode(currentErrorNode[0]));
                    ((Saxon.Hej.type.ValidationFailure)(failure)).setInvalidNode(currentErrorNode[0]);
                }
                errorList.Add(failure);
            }
            public override Saxon.Hej.om.Sequence endReporting() {
                return null;
            }
        
        }
    
    }
}