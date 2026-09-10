<?xml version="1.0" encoding="UTF-8"?>
<scm:schema xmlns=""
            xmlns:scm="http://ns.saxonica.com/schema-component-model"
            generatedAt="2023-08-03T14:22:48.790405+01:00"
            xsdVersion="1.1">
   <scm:complexType id="C0"
                    name="group-type"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="mixed">
      <scm:attributeUse required="false" inheritable="false" ref="C1"/>
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C2"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C2" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C2" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C3"
                   name="finiteNumberType"
                   targetNamespace="http://www.w3.org/2005/xpath-functions"
                   base="#double"
                   variety="atomic"
                   primitiveType="#double">
      <scm:minExclusive value="-INF"/>
      <scm:maxExclusive value="INF"/>
   </scm:simpleType>
   <scm:complexType id="C4"
                    name="mapType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeWildcard ref="C5"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C8"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C9"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C10"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C11"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C10" to="1"/>
            <scm:edge term="C11" to="1"/>
            <scm:edge term="C8" to="1"/>
            <scm:edge term="C7" to="1"/>
            <scm:edge term="C9" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C10" to="1"/>
            <scm:edge term="C11" to="1"/>
            <scm:edge term="C8" to="1"/>
            <scm:edge term="C7" to="1"/>
            <scm:edge term="C9" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C12"
                    name="arrayWithinMapType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="C13"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C14"/>
      <scm:attributeUse required="false" inheritable="false" ref="C15" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:attributeWildcard ref="C16"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C17"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C18"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C19"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C20"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C21"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C22"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C19" to="1"/>
            <scm:edge term="C21" to="1"/>
            <scm:edge term="C22" to="1"/>
            <scm:edge term="C17" to="1"/>
            <scm:edge term="C18" to="1"/>
            <scm:edge term="C20" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C19" to="1"/>
            <scm:edge term="C21" to="1"/>
            <scm:edge term="C22" to="1"/>
            <scm:edge term="C17" to="1"/>
            <scm:edge term="C18" to="1"/>
            <scm:edge term="C20" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C23"
                    name="stringType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#string"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#string">
      <scm:attributeUse required="false" inheritable="false" ref="C24" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:attributeWildcard ref="C25"/>
   </scm:complexType>
   <scm:complexType id="C26"
                    name="stringWithinMapType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="C23"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#string">
      <scm:attributeUse required="false" inheritable="false" ref="C14"/>
      <scm:attributeUse required="false" inheritable="false" ref="C15" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:attributeUse required="false" inheritable="false" ref="C24" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:attributeWildcard ref="C25"/>
   </scm:complexType>
   <scm:complexType id="C27"
                    name="mapWithinMapType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="C4"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C14"/>
      <scm:attributeUse required="false" inheritable="false" ref="C15" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:attributeWildcard ref="C5"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C8"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C9"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C10"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C11"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C10" to="1"/>
            <scm:edge term="C11" to="1"/>
            <scm:edge term="C8" to="1"/>
            <scm:edge term="C7" to="1"/>
            <scm:edge term="C9" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C10" to="1"/>
            <scm:edge term="C11" to="1"/>
            <scm:edge term="C8" to="1"/>
            <scm:edge term="C7" to="1"/>
            <scm:edge term="C9" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C28"
                    name="nullType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeWildcard ref="C29"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C30"
                    name="analyze-string-result-type"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="mixed">
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C31"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C32"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C31" to="1"/>
            <scm:edge term="C32" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C31" to="1"/>
            <scm:edge term="C32" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C33"
                    name="booleanType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#boolean"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#boolean">
      <scm:attributeWildcard ref="C34"/>
   </scm:complexType>
   <scm:complexType id="C13"
                    name="arrayType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#anyType"
                    block="extension"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeWildcard ref="C16"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C17"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C18"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C19"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C20"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C21"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C22"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C19" to="1"/>
            <scm:edge term="C21" to="1"/>
            <scm:edge term="C22" to="1"/>
            <scm:edge term="C17" to="1"/>
            <scm:edge term="C18" to="1"/>
            <scm:edge term="C20" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C19" to="1"/>
            <scm:edge term="C21" to="1"/>
            <scm:edge term="C22" to="1"/>
            <scm:edge term="C17" to="1"/>
            <scm:edge term="C18" to="1"/>
            <scm:edge term="C20" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C35"
                    name="numberWithinMapType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="C36"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C3">
      <scm:attributeUse required="false" inheritable="false" ref="C14"/>
      <scm:attributeUse required="false" inheritable="false" ref="C15" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:attributeWildcard ref="C37"/>
   </scm:complexType>
   <scm:complexType id="C38"
                    name="match-type"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="mixed">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C2"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C2" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C2" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C36"
                    name="numberType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="C3"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C3">
      <scm:attributeWildcard ref="C37"/>
   </scm:complexType>
   <scm:complexType id="C39"
                    name="booleanWithinMapType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="C33"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#boolean">
      <scm:attributeUse required="false" inheritable="false" ref="C14"/>
      <scm:attributeUse required="false" inheritable="false" ref="C15" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:attributeWildcard ref="C34"/>
   </scm:complexType>
   <scm:complexType id="C40"
                    name="nullWithinMapType"
                    targetNamespace="http://www.w3.org/2005/xpath-functions"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="false" inheritable="false" ref="C14"/>
      <scm:attributeUse required="false" inheritable="false" ref="C15" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C21"
                name="boolean"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C33"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C31"
                name="match"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C38"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C18"
                name="array"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C13"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C17"
                name="map"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C4"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false">
      <scm:identityConstraint ref="C41"/>
   </scm:element>
   <scm:element id="C19"
                name="string"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C23"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C22"
                name="null"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C28"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C32"
                name="non-match"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="#string"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C20"
                name="number"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C36"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C42"
                name="analyze-string-result"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C30"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C2"
                name="group"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C0"
                global="true"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:attributeGroup id="C43"
                       name="key-group"
                       targetNamespace="http://www.w3.org/2005/xpath-functions">
      <scm:attributeUse required="false" inheritable="false" ref="C14"/>
      <scm:attributeUse required="false" inheritable="false" ref="C15" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
   </scm:attributeGroup>
   <scm:attribute id="C1"
                  name="nr"
                  type="#positiveInteger"
                  global="false"
                  inheritable="false"
                  containingComplexType="C0"/>
   <scm:wildcard id="C5"
                 processContents="skip"
                 constraint="not"
                 namespaces="http://www.w3.org/2005/xpath-functions ##local"/>
   <scm:element id="C6"
                name="map"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C27"
                global="false"
                containingComplexType="C4"
                nillable="false"
                block="extension restriction substitution"
                abstract="false">
      <scm:identityConstraint ref="C44"/>
   </scm:element>
   <scm:element id="C7"
                name="array"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C12"
                global="false"
                containingComplexType="C4"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C8"
                name="string"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C26"
                global="false"
                containingComplexType="C4"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C9"
                name="number"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C35"
                global="false"
                containingComplexType="C4"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C10"
                name="boolean"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C39"
                global="false"
                containingComplexType="C4"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:element id="C11"
                name="null"
                targetNamespace="http://www.w3.org/2005/xpath-functions"
                type="C40"
                global="false"
                containingComplexType="C4"
                nillable="false"
                block="extension restriction substitution"
                abstract="false"/>
   <scm:attribute id="C14"
                  name="key"
                  type="#string"
                  global="false"
                  inheritable="false"/>
   <scm:attribute id="C15"
                  name="escaped-key"
                  type="#boolean"
                  global="false"
                  inheritable="false"/>
   <scm:wildcard id="C16"
                 processContents="skip"
                 constraint="not"
                 namespaces="http://www.w3.org/2005/xpath-functions ##local"/>
   <scm:attribute id="C24"
                  name="escaped"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C23"/>
   <scm:wildcard id="C25"
                 processContents="skip"
                 constraint="not"
                 namespaces="http://www.w3.org/2005/xpath-functions ##local"/>
   <scm:wildcard id="C29"
                 processContents="skip"
                 constraint="not"
                 namespaces="http://www.w3.org/2005/xpath-functions ##local"/>
   <scm:wildcard id="C34"
                 processContents="skip"
                 constraint="not"
                 namespaces="http://www.w3.org/2005/xpath-functions ##local"/>
   <scm:wildcard id="C37"
                 processContents="skip"
                 constraint="not"
                 namespaces="http://www.w3.org/2005/xpath-functions ##local"/>
   <scm:unique id="C41"
               name="unique-key"
               targetNamespace="http://www.w3.org/2005/xpath-functions">
      <scm:selector xpath="*" defaultNamespace=""/>
      <scm:field xpath="@key" defaultNamespace=""/>
   </scm:unique>
   <scm:unique id="C44"
               name="unique-key-2"
               targetNamespace="http://www.w3.org/2005/xpath-functions">
      <scm:selector xpath="*" defaultNamespace=""/>
      <scm:field xpath="@key" defaultNamespace=""/>
   </scm:unique>
</scm:schema>
<?Σ 884ef701?>
