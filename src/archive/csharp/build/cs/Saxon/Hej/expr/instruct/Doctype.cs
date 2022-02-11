using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class Doctype : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand contentOp;
        public Doctype(Saxon.Hej.expr.Expression content)  {
            contentOp = new Saxon.Hej.expr.Operand(this, content, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public Saxon.Hej.expr.Expression getContent() {
            return contentOp.getChildExpression();
        }
        public void setContent(Saxon.Hej.expr.Expression content) {
            contentOp.setChildExpression(content);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return contentOp;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Doctype.copy()");
        }
        public override bool mayCreateNewNodes() {
            return true;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.SAXON_DOCTYPE;
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
            pipe.setXPathContext(context);
            pipe.setHostLanguage(getPackageData().getHostLanguage());
            Saxon.Hej.tree.tiny.TinyBuilder builder = new Saxon.Hej.tree.tiny.TinyBuilder(pipe);
            builder.setStatistics(pipe.getConfiguration().getTreeStatistics().RESULT_TREE_STATISTICS);
            builder.open();
            builder.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            getContent().process(Saxon.Hej.@event.ComplexContentOutputter.makeComplexContentReceiver(builder, null), context);
            builder.endDocument();
            builder.close();
            Saxon.Hej.om.NodeInfo dtdRoot = builder.getCurrentRoot();
            Saxon.Hej.om.SequenceIterator children = dtdRoot.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
            Saxon.Hej.om.NodeInfo docType = (Saxon.Hej.om.NodeInfo)(children.next());
            if (((docType == null) || !("doctype".Equals(docType.getLocalPart())))) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("saxon:doctype instruction must contain dtd:doctype");
                e.setXPathContext(context);
                throw e;
            }
            string name = docType.getAttributeValue("", "name");
            string system = docType.getAttributeValue("", "system");
            string publicid = docType.getAttributeValue("", "public");
            if ((name == null)) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:doctype must have a name attribute");
                e.setXPathContext(context);
                throw e;
            }
            write(@out, (("<!DOCTYPE "+name)+' '));
            if ((system != null)) {
                if ((publicid != null)) {
                    write(@out, (((("PUBLIC \""+publicid)+"\" \"")+system)+'\"'));
                } else {
                    write(@out, (("SYSTEM \""+system)+'\"'));
                }
            }
            bool openSquare = false;
            children = docType.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
            Saxon.Hej.om.NodeInfo child = (Saxon.Hej.om.NodeInfo)(children.next());
            if ((child != null)) {
                write(@out, " [");
                openSquare = true;
            }
            while ((child != null)) {
                string localname = child.getLocalPart();
                if ("element".Equals(localname)) {
                    string elname = child.getAttributeValue("", "name");
                    string content = child.getAttributeValue("", "content");
                    if ((elname == null)) {
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:element must have a name attribute");
                        e.setXPathContext(context);
                        throw e;
                    }
                    if ((content == null)) {
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:element must have a content attribute");
                        e.setXPathContext(context);
                        throw e;
                    }
                    write(@out, (((("\n  <!ELEMENT "+elname)+' ')+content)+'>'));
                } else {
                    if (localname.Equals("attlist")) {
                        string elname = child.getAttributeValue("", "element");
                        if ((elname == null)) {
                            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:attlist must have an attribute named 'element'");
                            e.setXPathContext(context);
                            throw e;
                        }
                        write(@out, (("\n  <!ATTLIST "+elname)+' '));
                        Saxon.Hej.om.SequenceIterator attributes = child.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                        while (true) {
                            Saxon.Hej.om.NodeInfo attDef = (Saxon.Hej.om.NodeInfo)(attributes.next());
                            if ((attDef == null)) {
                                break ;
                            }
                            if ("attribute".Equals(attDef.getLocalPart())) {
                                string atname = attDef.getAttributeValue("", "name");
                                string type = attDef.getAttributeValue("", "type");
                                string value = attDef.getAttributeValue("", "value");
                                if ((atname == null)) {
                                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:attribute must have a name attribute");
                                    e.setXPathContext(context);
                                    throw e;
                                }
                                if ((type == null)) {
                                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:attribute must have a type attribute");
                                    e.setXPathContext(context);
                                    throw e;
                                }
                                if ((value == null)) {
                                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:attribute must have a value attribute");
                                    e.setXPathContext(context);
                                    throw e;
                                }
                                write(@out, ((((("\n    "+atname)+' ')+type)+' ')+value));
                            } else {
                                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Unrecognized element within dtd:attlist");
                                e.setXPathContext(context);
                                throw e;
                            }
                        }
                        write(@out, ">");
                    } else {
                        if (localname.Equals("entity")) {
                            string entname = child.getAttributeValue("", "name");
                            string parameter = child.getAttributeValue("", "parameter");
                            string esystem = child.getAttributeValue("", "system");
                            string epublicid = child.getAttributeValue("", "public");
                            string notation = child.getAttributeValue("", "notation");
                            if ((entname == null)) {
                                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:entity must have a name attribute");
                                e.setXPathContext(context);
                                throw e;
                            }
                            write(@out, "\n  <!ENTITY ");
                            if ("yes".Equals(parameter)) {
                                write(@out, "% ");
                            }
                            write(@out, (entname+' '));
                            if ((esystem != null)) {
                                if ((epublicid != null)) {
                                    write(@out, (((("PUBLIC \""+epublicid)+"\" \"")+esystem)+"\" "));
                                } else {
                                    write(@out, (("SYSTEM \""+esystem)+"\" "));
                                }
                            }
                            if ((notation != null)) {
                                write(@out, (("NDATA "+notation)+' '));
                            }
                            foreach (Saxon.Hej.om.NodeInfo content in child.children()) {
                                content.copy(@out, 0, getLocation());
                            }
                            write(@out, ">");
                        } else {
                            if (localname.Equals("notation")) {
                                string notname = child.getAttributeValue("", "name");
                                string nsystem = child.getAttributeValue("", "system");
                                string npublicid = child.getAttributeValue("", "public");
                                if ((notname == null)) {
                                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:notation must have a name attribute");
                                    e.setXPathContext(context);
                                    throw e;
                                }
                                if ((((nsystem == null)) && ((npublicid == null)))) {
                                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("dtd:notation must have a system attribute or a public attribute");
                                    e.setXPathContext(context);
                                    throw e;
                                }
                                write(@out, ("\n  <!NOTATION "+notname));
                                if ((npublicid != null)) {
                                    write(@out, ((" PUBLIC \""+npublicid)+"\" "));
                                    if ((nsystem != null)) {
                                        write(@out, (('\"'+nsystem)+"\" "));
                                    }
                                } else {
                                    write(@out, ((" SYSTEM \""+nsystem)+"\" "));
                                }
                                write(@out, ">");
                            } else {
                                if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                                    write(@out, child.getStringValue());
                                } else {
                                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Unrecognized element "+localname)+" in DTD output"));
                                    e.setXPathContext(context);
                                    throw e;
                                }
                            }
                        }
                    }
                }
                child = (Saxon.Hej.om.NodeInfo)(children.next());
            }
            if (openSquare) {
                write(@out, "\n]");
            }
            write(@out, ">\n");
            return null;
        }
        private void write(Saxon.Hej.@event.Outputter @out, string s) {
            @out.characters(Saxon.Hej.str.StringView.of(s), getLocation(), Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("saxonDoctype", this);
            getContent().export(@out);
            @out.endElement();
        }
    
    }
}