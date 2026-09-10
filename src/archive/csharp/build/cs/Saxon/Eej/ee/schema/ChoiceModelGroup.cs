using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class ChoiceModelGroup : Saxon.Eej.ee.schema.ModelGroup {
        public override bool isEmptiable() {
            foreach (Saxon.Eej.ee.schema.Particle aContentModel in contentModel) {
                if (aContentModel.isEmptiable()) {
                    return true;
                }
            }
            return false;
        }
        public override bool isPointless(Saxon.Eej.ee.schema.ModelGroup container) {
            return ((contentModel.Count==0 || (contentModel.Count == 1)) || (container is Saxon.Eej.ee.schema.ChoiceModelGroup));
        }
        public override void markVulnerableSubParticles(bool isRepeatable) {
            if (isRepeatable) {
                foreach (Saxon.Eej.ee.schema.Particle p in contentModel) {
                    p.setVulnerable(true);
                }
            }
            foreach (Saxon.Eej.ee.schema.Particle p in contentModel) {
                if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                    p.markVulnerableSubParticles();
                }
            }
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (containsAll(false)) {
                compiler.error("An <xs:choice> model group must not contain an <xs:all> model group", this);
                return false;
            }
            return base.validate(compiler);
        }
        public override string getCompositorName() {
            return "choice";
        }
    
    }
}