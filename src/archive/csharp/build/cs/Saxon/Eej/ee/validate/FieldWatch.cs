using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class FieldWatch : Saxon.Eej.ee.stream.watch.PatternWatch {
        private readonly Saxon.Eej.ee.validate.SelectorWatch parentWatch;
        private readonly Saxon.Hej.om.AtomicSequence[] currentRow;
        private readonly int column;
        public FieldWatch(Saxon.Eej.ee.validate.SelectorWatch parentWatch, Saxon.Hej.pattern.Pattern selection, Saxon.Hej.om.AtomicSequence[] currentRow, int column)  {
            this.parentWatch = parentWatch;
            setSelection(selection);
            this.currentRow = currentRow;
            this.column = column;
        }
        public override Saxon.Eej.ee.validate.FieldChecker startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            Saxon.Eej.ee.validate.FieldChecker fc = new Saxon.Eej.ee.validate.FieldChecker(parentWatch, getSelection(), currentRow, column);
            fc.setPipelineConfiguration(getPipelineConfiguration());
            fc.setNamespaceResolver(getNamespaceResolver());
            return fc;
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
        }
        public override void processItem(Saxon.Hej.om.Item item) {
            Saxon.Eej.ee.validate.FieldChecker fc = new Saxon.Eej.ee.validate.FieldChecker(parentWatch, getSelection(), currentRow, column);
            fc.setPipelineConfiguration(getPipelineConfiguration());
            fc.setNamespaceResolver(getNamespaceResolver());
            fc.notifySelectedAttribute((Saxon.Eej.ee.stream.om.FleetingNode)(item), Saxon.Hej.expr.parser.Loc.NONE);
        }
    
    }
}