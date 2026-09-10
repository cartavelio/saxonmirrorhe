using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class TypeAliasManager {
        public TypeAliasManager()  {
        }
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.style.ComponentDeclaration> unresolvedDeclarations = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.style.ComponentDeclaration>();
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.type.ItemType> typeAliases = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.type.ItemType>();
        public void registerTypeAlias(Saxon.Hej.om.StructuredQName name, Saxon.Hej.type.ItemType type) {
            Saxon.Impl.Helpers.MapUtils.Put(typeAliases, name, type);
            unresolvedDeclarations.Remove(name);
        }
        public void processDeclaration(Saxon.Hej.style.ComponentDeclaration declaration) {
            Saxon.Hej.style.XSLItemType sta = (Saxon.Hej.style.XSLItemType)(declaration.getSourceElement());
            Saxon.Hej.type.ItemType type = sta.tryToResolve();
            if ((type != null)) {
                registerTypeAlias(sta.getObjectName(), type);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(unresolvedDeclarations, sta.getObjectName(), declaration);
            }
        }
        public void processAllDeclarations(System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> topLevel) {
            foreach (Saxon.Hej.style.ComponentDeclaration decl in topLevel) {
                if ((decl.getSourceElement() is Saxon.Hej.style.XSLItemType)) {
                    processDeclaration(decl);
                }
            }
            int unresolved = unresolvedDeclarations.Count;
            while ((unresolved>0)) {
                System.Collections.Generic.ISet<Saxon.Hej.style.ComponentDeclaration> pending = new System.Collections.Generic.HashSet<Saxon.Hej.style.ComponentDeclaration>(unresolvedDeclarations.Values);
                foreach (Saxon.Hej.style.ComponentDeclaration decl in pending) {
                    processDeclaration(decl);
                }
                if ((unresolvedDeclarations.Count>=unresolved)) {
                    Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
                    fsb.append("Cannot resolve all type aliases, because of missing or circular definitions. Unresolved names: ");
                    foreach (Saxon.Hej.om.StructuredQName name in Saxon.Impl.Helpers.MapUtils.KeySet(unresolvedDeclarations)) {
                        fsb.append(name.getDisplayName());
                        fsb.append(' ');
                    }
                    throw new Saxon.Hej.trans.XPathException(fsb.ToString(), Saxon.Hej.trans.SaxonErrorCode.SXTA0001);
                }
                unresolved = unresolvedDeclarations.Count;
            }
        }
        public Saxon.Hej.type.ItemType getItemType(Saxon.Hej.om.StructuredQName alias) {
            return Saxon.Impl.Helpers.MapUtils.Get(typeAliases, alias);
        }
    
    }
}