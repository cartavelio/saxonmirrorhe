using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMergeSource : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression forEachItem;
        private Saxon.Hej.expr.Expression forEachSource;
        private Saxon.Hej.expr.Expression select;
        private bool sortBeforeMerge = false;
        private int mergeKeyCount = 0;
        private string sourceName;
        private int validationAction = Saxon.Hej.lib.Validation.STRIP;
        private Saxon.Hej.type.SchemaType schemaType = null;
        private bool streamable = false;
        private System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
        public override bool isInstruction() {
            return false;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        public Saxon.Hej.expr.Expression getForEachItem() {
            return forEachItem;
        }
        public Saxon.Hej.expr.Expression getForEachSource() {
            return forEachSource;
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return select;
        }
        public bool isSortBeforeMerge() {
            return sortBeforeMerge;
        }
        public string getSourceName() {
            return sourceName;
        }
        public int getValidationAction() {
            return validationAction;
        }
        public Saxon.Hej.type.SchemaType getSchemaTypeAttribute() {
            return schemaType;
        }
        public Saxon.Hej.expr.sort.MergeInstr.MergeSource makeMergeSource(Saxon.Hej.expr.sort.MergeInstr mi, Saxon.Hej.expr.Expression select) {
            Saxon.Hej.expr.sort.MergeInstr.MergeSource ms = new Saxon.Hej.expr.sort.MergeInstr.MergeSource(mi);
            if ((forEachItem != null)) {
                ms.initForEachItem(mi, forEachItem);
            }
            if ((forEachSource != null)) {
                ms.initForEachStream(mi, forEachSource);
            }
            if ((select != null)) {
                this.select = select;
                ms.initRowSelect(mi, select);
            }
            ms.baseURI = getBaseURI();
            ms.sourceName = sourceName;
            ms.validation = validationAction;
            ms.schemaType = schemaType;
            ms.setStreamable(streamable);
            ms.accumulators = accumulators;
            return ms;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return null;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string forEachItemAtt = null;
            string forEachSourceAtt = null;
            string validationAtt = null;
            string typeAtt = null;
            string streamableAtt = null;
            string useAccumulatorsAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "for-each-item":
                    forEachItemAtt = value;
                    forEachItem = makeExpression(forEachItemAtt, att);
                    break ;
                  case "for-each-source":
                    forEachSourceAtt = value;
                    forEachSource = makeExpression(forEachSourceAtt, att);
                    break ;
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "sort-before-merge":
                    sortBeforeMerge = processBooleanAttribute("sort-before-merge", value);
                    break ;
                  case "name":
                    string nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                    if (Saxon.Hej.om.NameChecker.isValidNCName(nameAtt)) {
                        sourceName = nameAtt;
                    } else {
                        compileError((("xsl:merge-source/@name ("+nameAtt)+") is not a valid NCName"), "XTSE0020");
                    }
                    break ;
                  case "validation":
                    validationAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "type":
                    typeAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "streamable":
                    streamableAtt = value;
                    break ;
                  case "use-accumulators":
                    useAccumulatorsAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((sourceName == null)) {
                sourceName = ("saxon-merge-source-"+GetHashCode());
            }
            if ((forEachItemAtt != null)) {
                if ((forEachSourceAtt != null)) {
                    compileError("The for-each-item and for-each-source attributes must not both be present", "XTSE3195");
                }
            }
            if ((selectAtt == null)) {
                reportAbsence("select");
            }
            if ((validationAtt == null)) {
                validationAction = getDefaultValidation();
            } else {
                validationAction = validateValidationAttribute(validationAtt);
            }
            if ((typeAtt != null)) {
                if (!(isSchemaAware())) {
                    compileError("The @type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                }
                schemaType = getSchemaType(typeAtt);
                validationAction = Saxon.Hej.lib.Validation.BY_TYPE;
            }
            if (((typeAtt != null) && (validationAtt != null))) {
                compileError("The @validation and @type attributes are mutually exclusive", "XTSE1505");
            }
            if (((((typeAtt != null) || (validationAtt != null))) && (forEachSourceAtt == null))) {
                compileError("The @type and @validation attributes can be used only when @for-each-stream is specified", "XTSE0020");
            }
            if ((streamableAtt != null)) {
                streamable = processStreamableAtt(streamableAtt);
                if ((streamable && (forEachSource == null))) {
                    compileError("Streaming on xsl:merge-source is possible only when @for-each-source is used", "XTSE3195");
                }
            } else {
                if ((forEachSource != null)) {
                    streamable = false;
                }
            }
            if ((useAccumulatorsAtt == null)) {
                useAccumulatorsAtt = "";
            }
            Saxon.Hej.expr.accum.AccumulatorRegistry registry = getPrincipalStylesheetModule().getStylesheetPackage().getAccumulatorRegistry();
            accumulators = registry.getUsedAccumulators(useAccumulatorsAtt, this);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            forEachItem = typeCheck("for-each-item", forEachItem);
            forEachSource = typeCheck("for-each-source", forEachSource);
            select = typeCheck("select", select);
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLMergeKey)) {
                    mergeKeyCount++;
                } else {
                    if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                        if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                            compileError("No character data is allowed within xsl:merge-source", "XTSE0010");
                        }
                    } else {
                        if ((child is Saxon.Hej.style.StyleElement)) {
                            ((Saxon.Hej.style.StyleElement)(child)).compileError("No children other than xsl:merge-key are allowed within xsl:merge-source", "XTSE0010");
                        }
                    }
                }
            }
            if ((mergeKeyCount == 0)) {
                compileError("xsl:merge-source must have exactly at least one xsl:merge-key child element", "XTSE0010");
            }
        }
    
    }
}