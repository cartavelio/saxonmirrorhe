using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class SchemaNodeFactory : Saxon.Hej.tree.linked.NodeFactory {
        private readonly Saxon.Hej.@event.PipelineConfiguration pipe;
        private readonly Saxon.Eej.ee.schema.SchemaCompiler compiler;
        private readonly Saxon.Eej.config.EnterpriseConfiguration config;
        private readonly System.Collections.Generic.Dictionary<string,Saxon.Eej.ee.schema.sdoc.SchemaElement> idTable = new System.Collections.Generic.Dictionary<string,Saxon.Eej.ee.schema.sdoc.SchemaElement>(32);
        private readonly Saxon.Eej.ee.schema.sdoc.SchemaElement referrer;
        public SchemaNodeFactory(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Eej.ee.schema.sdoc.SchemaElement referrer)  {
            this.compiler = compiler;
            this.pipe = pipe;
            config = (Saxon.Eej.config.EnterpriseConfiguration)(pipe.getConfiguration());
            this.referrer = referrer;
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public Saxon.Eej.ee.schema.SchemaCompiler getSchemaCompiler() {
            return compiler;
        }
        public Saxon.Hej.tree.linked.ElementImpl makeElementNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType elemType, bool isNilled, Saxon.Hej.om.AttributeMap attlist, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.s9api.Location locationId, int sequence) {
            string baseURI = null;
            int lineNumber = -1;
            int columnNumber = -1;
            baseURI = locationId.getSystemId();
            lineNumber = locationId.getLineNumber();
            columnNumber = locationId.getColumnNumber();
            int f = elemName.obtainFingerprint(pipe.getConfiguration().getNamePool());
            Saxon.Eej.ee.schema.sdoc.SchemaElement e = makeXSDElement(f, parent.getFingerprint());
            if ((e is Saxon.Eej.ee.schema.sdoc.XSDSchema)) {
                config.checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
            }
            e.setSchemaNodeFactory(this);
            e.setNamespaceMap(namespaces);
            e.initialise(elemName, elemType, attlist, parent, sequence);
            e.setLocation(baseURI, lineNumber, columnNumber);
            return e;
        }
        private Saxon.Eej.ee.schema.sdoc.SchemaElement makeXSDElement(int f, int parent) {
            switch (f) {
              case Saxon.Hej.om.StandardNames.XS_ALTERNATIVE:
                return new Saxon.Eej.ee.schema.sdoc.XSDAlternative();
              case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                return new Saxon.Eej.ee.schema.sdoc.XSDAnnotation();
              case Saxon.Hej.om.StandardNames.XS_ASSERT:
                return new Saxon.Eej.ee.schema.sdoc.XSDAssert();
              case Saxon.Hej.om.StandardNames.XS_ASSERTION:
                return new Saxon.Eej.ee.schema.sdoc.XSDAssert();
              case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE:
                return new Saxon.Eej.ee.schema.sdoc.XSDAttribute();
              case Saxon.Hej.om.StandardNames.XS_ALL:
                return new Saxon.Eej.ee.schema.sdoc.XSDCompositor();
              case Saxon.Hej.om.StandardNames.XS_ANY:
                return new Saxon.Eej.ee.schema.sdoc.XSDAny();
              case Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE:
                return new Saxon.Eej.ee.schema.sdoc.XSDAnyAttribute();
              case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                return new Saxon.Eej.ee.schema.sdoc.XSDAttributeGroup();
              case Saxon.Hej.om.StandardNames.XS_CHOICE:
                return new Saxon.Eej.ee.schema.sdoc.XSDCompositor();
              case Saxon.Hej.om.StandardNames.XS_COMPLEX_CONTENT:
                return new Saxon.Eej.ee.schema.sdoc.XSDComplexContent();
              case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                return new Saxon.Eej.ee.schema.sdoc.XSDComplexType();
              case Saxon.Hej.om.StandardNames.XS_DEFAULT_OPEN_CONTENT:
                return new Saxon.Eej.ee.schema.sdoc.XSDDefaultOpenContent();
              case Saxon.Hej.om.StandardNames.XS_ELEMENT:
                return new Saxon.Eej.ee.schema.sdoc.XSDElement();
              case Saxon.Hej.om.StandardNames.XS_EXTENSION:
                return new Saxon.Eej.ee.schema.sdoc.XSDExtension();
              case Saxon.Hej.om.StandardNames.XS_GROUP:
                return new Saxon.Eej.ee.schema.sdoc.XSDGroup();
              case Saxon.Hej.om.StandardNames.XS_IMPORT:
                return new Saxon.Eej.ee.schema.sdoc.XSDImport();
              case Saxon.Hej.om.StandardNames.XS_INCLUDE:
                return new Saxon.Eej.ee.schema.sdoc.XSDInclude();
              case Saxon.Hej.om.StandardNames.XS_KEY:
                return new Saxon.Eej.ee.schema.sdoc.XSDIdentityConstraint();
              case Saxon.Hej.om.StandardNames.XS_KEYREF:
                return new Saxon.Eej.ee.schema.sdoc.XSDIdentityConstraint();
              case Saxon.Hej.om.StandardNames.XS_LIST:
                return new Saxon.Eej.ee.schema.sdoc.XSDList();
              case Saxon.Hej.om.StandardNames.XS_notation:
                return new Saxon.Eej.ee.schema.sdoc.XSDNotation();
              case Saxon.Hej.om.StandardNames.XS_OPEN_CONTENT:
                return new Saxon.Eej.ee.schema.sdoc.XSDOpenContent();
              case Saxon.Hej.om.StandardNames.XS_OVERRIDE:
                return new Saxon.Eej.ee.schema.sdoc.XSDOverride();
              case Saxon.Hej.om.StandardNames.XS_REDEFINE:
                return new Saxon.Eej.ee.schema.sdoc.XSDRedefine();
              case Saxon.Hej.om.StandardNames.XS_RESTRICTION:
                if ((parent == Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE)) {
                    return new Saxon.Eej.ee.schema.sdoc.XSDSimpleTypeRestriction();
                } else {
                    if ((parent == Saxon.Hej.om.StandardNames.XS_SIMPLE_CONTENT)) {
                        return new Saxon.Eej.ee.schema.sdoc.XSDSimpleContentRestriction();
                    } else {
                        return new Saxon.Eej.ee.schema.sdoc.XSDComplexContentRestriction();
                    }
                }
              case Saxon.Hej.om.StandardNames.XS_SCHEMA:
                Saxon.Eej.ee.schema.sdoc.XSDSchema s = new Saxon.Eej.ee.schema.sdoc.XSDSchema();
                Saxon.Eej.ee.schema.SingleNamespaceSchema preparedSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema(config, Saxon.Hej.om.NamespaceUri.NULL);
                s.setSchema(preparedSchema);
                s.setSchemaCompiler(compiler);
                s.setReferrer(referrer);
                return s;
              case Saxon.Hej.om.StandardNames.XS_SEQUENCE:
                return new Saxon.Eej.ee.schema.sdoc.XSDCompositor();
              case Saxon.Hej.om.StandardNames.XS_SIMPLE_CONTENT:
                return new Saxon.Eej.ee.schema.sdoc.XSDSimpleContent();
              case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                return new Saxon.Eej.ee.schema.sdoc.XSDSimpleType();
              case Saxon.Hej.om.StandardNames.XS_UNIQUE:
                return new Saxon.Eej.ee.schema.sdoc.XSDIdentityConstraint();
              case Saxon.Hej.om.StandardNames.XS_UNION:
                return new Saxon.Eej.ee.schema.sdoc.XSDUnion();
              case Saxon.Hej.om.StandardNames.XS_APPINFO:
              case Saxon.Hej.om.StandardNames.XS_DOCUMENTATION:
                return new Saxon.Eej.ee.schema.sdoc.XSDDocumentation();
              case Saxon.Hej.om.StandardNames.XS_FIELD:
              case Saxon.Hej.om.StandardNames.XS_SELECTOR:
                return new Saxon.Eej.ee.schema.sdoc.XSDFieldOrSelector();
              case Saxon.Hej.om.StandardNames.XS_ENUMERATION:
              case Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS:
              case Saxon.Hej.om.StandardNames.XS_LENGTH:
              case Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE:
              case Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE:
              case Saxon.Hej.om.StandardNames.XS_MAX_LENGTH:
              case Saxon.Hej.om.StandardNames.XS_MAX_SCALE:
              case Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE:
              case Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE:
              case Saxon.Hej.om.StandardNames.XS_MIN_LENGTH:
              case Saxon.Hej.om.StandardNames.XS_MIN_SCALE:
              case Saxon.Hej.om.StandardNames.XS_NOTATION:
              case Saxon.Hej.om.StandardNames.XS_PATTERN:
              case Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE:
              case Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS:
              case Saxon.Hej.om.StandardNames.XS_WHITE_SPACE:
                return new Saxon.Eej.ee.schema.sdoc.XSDFacet();
              case Saxon.Hej.om.StandardNames.SAXON_PARAM:
                return new Saxon.Eej.ee.schema.sdoc.SaxonParam();
              case Saxon.Hej.om.StandardNames.SAXON_PREPROCESS:
                return new Saxon.Eej.ee.schema.sdoc.SaxonPreprocess();
              case Saxon.Hej.om.StandardNames.SAXON_DISTINCT:
              case Saxon.Hej.om.StandardNames.SAXON_ORDER:
                return new Saxon.Eej.ee.schema.sdoc.XSDFacet();
              default:
                return new Saxon.Eej.ee.schema.sdoc.UnknownElement();
            }
        }
        public Saxon.Hej.tree.linked.TextImpl makeTextNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.str.UnicodeString content) {
            return new Saxon.Hej.tree.linked.TextImpl(content);
        }
        public Saxon.Hej.type.SchemaException registerId(string id, Saxon.Eej.ee.schema.sdoc.SchemaElement element) {
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(id))) {
                return new Saxon.Hej.type.SchemaException(("Invalid id value "+Saxon.Hej.trans.Err.wrap(id)));
            }
            Saxon.Eej.ee.schema.sdoc.SchemaElement e = Saxon.Impl.Helpers.MapUtils.Get(idTable, id);
            if ((e != null)) {
                return new Saxon.Hej.type.SchemaException(("Non-unique id value "+Saxon.Hej.trans.Err.wrap(id)));
            }
            Saxon.Impl.Helpers.MapUtils.Put(idTable, id, element);
            return null;
        }
    
    }
}