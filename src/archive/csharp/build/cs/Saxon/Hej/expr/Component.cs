using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class Component {
        protected internal Saxon.Hej.expr.instruct.Actor actor;
        private Saxon.Hej.trans.Visibility visibility;
        private System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> bindings = new System.Collections.Generic.List<Saxon.Hej.expr.ComponentBinding>();
        private Saxon.Hej.style.StylesheetPackage containingPackage;
        private Saxon.Hej.style.StylesheetPackage declaringPackage;
        private Saxon.Hej.trans.VisibilityProvenance provenance;
        private Saxon.Hej.expr.Component baseComponent;
        private Component()  {
        }
        public static Saxon.Hej.expr.Component makeComponent(Saxon.Hej.expr.instruct.Actor actor, Saxon.Hej.trans.Visibility visibility, Saxon.Hej.trans.VisibilityProvenance provenance, Saxon.Hej.style.StylesheetPackage containingPackage, Saxon.Hej.style.StylesheetPackage declaringPackage) {
            Saxon.Hej.expr.Component c;
            if ((actor is Saxon.Hej.trans.Mode)) {
                c = new Saxon.Hej.expr.Component.M();
            } else {
                c = new Saxon.Hej.expr.Component();
            }
            c.actor = actor;
            c.visibility = visibility;
            c.provenance = provenance;
            c.containingPackage = containingPackage;
            c.declaringPackage = declaringPackage;
            return c;
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> getComponentBindings() {
            return bindings;
        }
        public void setComponentBindings(System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> bindings) {
            this.bindings = bindings;
        }
        public void setVisibility(Saxon.Hej.trans.Visibility visibility, Saxon.Hej.trans.VisibilityProvenance provenance) {
            this.visibility = visibility;
            this.provenance = provenance;
        }
        public Saxon.Hej.trans.Visibility getVisibility() {
            return visibility;
        }
        public Saxon.Hej.trans.VisibilityProvenance getVisibilityProvenance() {
            return provenance;
        }
        public bool isHiddenAbstractComponent() {
            return (((visibility == Saxon.Hej.trans.Visibility.HIDDEN) && (baseComponent != null)) && (baseComponent.getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT));
        }
        public virtual Saxon.Hej.expr.instruct.Actor getActor() {
            return actor;
        }
        public Saxon.Hej.style.StylesheetPackage getDeclaringPackage() {
            return declaringPackage;
        }
        public Saxon.Hej.style.StylesheetPackage getContainingPackage() {
            return containingPackage;
        }
        public Saxon.Hej.expr.Component getBaseComponent() {
            return baseComponent;
        }
        public void setBaseComponent(Saxon.Hej.expr.Component original) {
            baseComponent = original;
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter @out, System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentIdMap, System.Collections.Generic.IDictionary<Saxon.Hej.style.StylesheetPackage,int> packageIdMap) {
            @out.startElement("co");
            int id = obtainComponentId(this, componentIdMap);
            @out.emitAttribute("id", (""+id));
            if ((provenance != Saxon.Hej.trans.VisibilityProvenance.DEFAULTED)) {
                @out.emitAttribute("vis", getVisibility().ToString());
            }
            string refs = listComponentReferences(componentIdMap);
            @out.emitAttribute("binds", refs);
            if (((baseComponent != null) && (getActor() == baseComponent.getActor()))) {
                int baseId = obtainComponentId(baseComponent, componentIdMap);
                @out.emitAttribute("base", (""+baseId));
                @out.emitAttribute("dpack", (Saxon.Impl.Helpers.MapUtils.Get(packageIdMap, declaringPackage)+""));
            } else {
                actor.export(@out);
            }
            @out.endElement();
        }
        public string listComponentReferences(System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentIdMap) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(128);
            foreach (Saxon.Hej.expr.ComponentBinding @ref in getComponentBindings()) {
                Saxon.Hej.expr.Component target = @ref.getTarget();
                int targetId = obtainComponentId(target, componentIdMap);
                if ((fsb.length() != 0)) {
                    fsb.append(" ");
                }
                fsb.append((""+targetId));
            }
            return fsb.ToString();
        }
        private int obtainComponentId(Saxon.Hej.expr.Component component, System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentIdMap) {
            int id = Saxon.Impl.Helpers.MapUtils.GetOrDefault(componentIdMap, component, System.Int32.MinValue);
            if ((id == System.Int32.MinValue)) {
                id = componentIdMap.Count;
                Saxon.Impl.Helpers.MapUtils.Put(componentIdMap, component, id);
            }
            return id;
        }
        public class M : Saxon.Hej.expr.Component {
            public override Saxon.Hej.trans.Mode getActor() {
                return (Saxon.Hej.trans.Mode)(base.getActor());
            }
            public void setActor(Saxon.Hej.trans.Mode m) {
                this.actor = m;
            }
        
        }
        public int getComponentKind() {
            if ((actor is Saxon.Hej.expr.instruct.NamedTemplate)) {
                return Saxon.Hej.om.StandardNames.XSL_TEMPLATE;
            } else {
                if ((actor is Saxon.Hej.expr.instruct.GlobalVariable)) {
                    return Saxon.Hej.om.StandardNames.XSL_VARIABLE;
                } else {
                    if ((actor is Saxon.Hej.om.FunctionItem)) {
                        return Saxon.Hej.om.StandardNames.XSL_FUNCTION;
                    } else {
                        if ((actor is Saxon.Hej.expr.instruct.AttributeSet)) {
                            return Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET;
                        } else {
                            if ((actor is Saxon.Hej.trans.Mode)) {
                                return Saxon.Hej.om.StandardNames.XSL_MODE;
                            } else {
                                return -1;
                            }
                        }
                    }
                }
            }
        }
    
    }
}