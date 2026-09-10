using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class ConstraintChecker : Saxon.Eej.ee.stream.watch.WatchManager {
        private Saxon.Eej.ee.validate.ValidationContext validationContext;
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.Watch> prearrangedWatches = new System.Collections.Generic.List<Saxon.Eej.ee.stream.watch.Watch>(4);
        public ConstraintChecker(Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
        }
        public void setValidationContext(Saxon.Eej.ee.validate.ValidationContext vc) {
            this.validationContext = vc;
        }
        public Saxon.Eej.ee.validate.ValidationContext getValidationContext() {
            return validationContext;
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            base.setPipelineConfiguration(pipe);
            Saxon.Hej.Controller controller = pipe.getController();
            if ((controller == null)) {
                Saxon.Hej.Configuration config = getPipelineConfiguration().getConfiguration();
                controller = new Saxon.Hej.Controller(config);
                controller.getExecutable().setSchemaAware(true);
                getPipelineConfiguration().setController(controller);
            }
            Saxon.Hej.expr.XPathContextMajor context = controller.newXPathContext();
            setXPathContext(context);
        }
        public void setElementDeclaration(Saxon.Eej.ee.schema.ElementDecl decl) {
            foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in decl.getIdentityConstraints()) {
                Saxon.Eej.ee.schema.IdentityConstraint constraint = ((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)icr.getTarget());
                if ((constraint is Saxon.Eej.ee.schema.KeyRef)) {
                    Saxon.Eej.ee.validate.RefSelectorWatch watch = new Saxon.Eej.ee.validate.RefSelectorWatch(this, (Saxon.Eej.ee.schema.KeyRef)(constraint));
                    prearrangedWatches.Add(watch);
                }
            }
            foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in decl.getIdentityConstraints()) {
                Saxon.Eej.ee.schema.IdentityConstraint constraint = ((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)icr.getTarget());
                if (((constraint is Saxon.Eej.ee.schema.Unique) || (constraint is Saxon.Eej.ee.schema.Key))) {
                    Saxon.Eej.ee.validate.KeySelectorWatch watch = new Saxon.Eej.ee.validate.KeySelectorWatch(this, constraint);
                    prearrangedWatches.Add(watch);
                    System.Collections.Generic.IEnumerator<Saxon.Eej.ee.stream.watch.Watch> watches = new Saxon.Hej.tree.jiter.ConcatenatingIterator<Saxon.Eej.ee.stream.watch.Watch>(watchList.GetEnumerator(), () => prearrangedWatches.GetEnumerator());
                    while (watches.MoveNext() /*UNSAFE CONVERSION*/) {
                        Saxon.Eej.ee.stream.watch.Watch @ref = watches.Current;
                        if ((@ref is Saxon.Eej.ee.validate.RefSelectorWatch)) {
                            Saxon.Hej.om.StructuredQName reffp = ((Saxon.Eej.ee.schema.KeyRef)(((Saxon.Eej.ee.validate.RefSelectorWatch)(@ref)).getIdentityConstraint())).getTargetComponentName();
                            if (reffp.Equals(constraint.getConstraintName())) {
                                watch.addKeyRef((Saxon.Eej.ee.validate.RefSelectorWatch)(@ref));
                            }
                        }
                    }
                }
            }
        }
        protected internal override void addPrearrangedWatches() {
            foreach (Saxon.Eej.ee.stream.watch.Watch prearrangedWatch in prearrangedWatches) {
                addWatch(prearrangedWatch, true);
            }
            prearrangedWatches.Clear();
        }
    
    }
}