<?xml version="1.0" encoding="UTF-8"?>
<scm:schema xmlns:scm="http://ns.saxonica.com/schema-component-model"
            generatedAt="2010-10-19T14:39:10.881+01:00"
            xsdVersion="1.0">
   <scm:complexType id="C0" name="intFacet" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C1"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C2"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C4" default="false"/>
      <scm:attributeWildcard ref="C5"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C1" name="facet" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C8"/>
      <scm:attributeUse required="false" ref="C4" default="false"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C10" base="#anySimpleType" variety="union" memberTypes="#QName C11"/>
   <scm:simpleType id="C12" base="#token" variety="atomic" primitiveType="#string">
      <scm:enumeration value="##local"/>
      <scm:enumeration value="##targetNamespace"/>
   </scm:simpleType>
   <scm:simpleType id="C13" base="#anySimpleType" variety="list" itemType="C14"/>
   <scm:simpleType id="C15" name="derivationSet"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C16 C17"/>
   <scm:simpleType id="C18" name="public" targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#token"
                   variety="atomic"
                   primitiveType="#string"/>
   <scm:complexType id="C19" name="attributeGroupRef"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C20"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="true" ref="C21"/>
      <scm:attributeWildcard ref="C22"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C23" name="localElement" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C24"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C25" default="1"/>
      <scm:attributeUse required="false" ref="C26"/>
      <scm:attributeUse required="false" ref="C27"/>
      <scm:attributeUse required="false" ref="C28" default="1"/>
      <scm:attributeUse required="false" ref="C29"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C30"/>
      <scm:attributeUse required="false" ref="C31"/>
      <scm:attributeUse required="false" ref="C32"/>
      <scm:attributeUse required="false" ref="C33"/>
      <scm:attributeUse required="false" ref="C34"/>
      <scm:attributeUse required="false" ref="C35"/>
      <scm:attributeWildcard ref="C36"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C37"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C38"/>
               </scm:choice>
            </scm:modelGroupParticle>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C39"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded" ref="C40"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C39" to="1"/>
            <scm:edge term="C37" to="2"/>
            <scm:edge term="C41" to="3"/>
            <scm:edge term="C6" to="4"/>
            <scm:edge term="C38" to="2"/>
            <scm:edge term="C42" to="3"/>
            <scm:edge term="C43" to="3"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C39" to="1"/>
            <scm:edge term="C41" to="3"/>
            <scm:edge term="C42" to="3"/>
            <scm:edge term="C43" to="3"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C39" to="1"/>
            <scm:edge term="C41" to="3"/>
            <scm:edge term="C42" to="3"/>
            <scm:edge term="C43" to="3"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C41" to="3"/>
            <scm:edge term="C42" to="3"/>
            <scm:edge term="C43" to="3"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C39" to="1"/>
            <scm:edge term="C37" to="2"/>
            <scm:edge term="C41" to="3"/>
            <scm:edge term="C38" to="2"/>
            <scm:edge term="C42" to="3"/>
            <scm:edge term="C43" to="3"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C44" base="#anySimpleType" variety="list" itemType="C45"/>
   <scm:simpleType id="C46" base="#NCName" variety="atomic" primitiveType="#string">
      <scm:enumeration value="preserve"/>
      <scm:enumeration value="default"/>
   </scm:simpleType>
   <scm:complexType id="C47" name="explicitGroup"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C48"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C25" default="1"/>
      <scm:attributeUse required="false" ref="C28" default="1"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C49"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded" ref="C50"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C54" to="1"/>
            <scm:edge term="C55" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C54" to="1"/>
            <scm:edge term="C55" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C54" to="1"/>
            <scm:edge term="C55" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C56" name="topLevelAttribute"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C57"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C58"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C59"/>
      <scm:attributeUse required="false" ref="C60"/>
      <scm:attributeUse required="false" ref="C61"/>
      <scm:attributeUse required="false" ref="C62"/>
      <scm:attributeWildcard ref="C63"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C64"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C64" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C64" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C65" base="#NMTOKEN" variety="atomic" primitiveType="#string">
      <scm:enumeration value="suffix"/>
      <scm:enumeration value="interleave"/>
      <scm:enumeration value="none"/>
   </scm:simpleType>
   <scm:simpleType id="C11" base="#token" variety="atomic" primitiveType="#string">
      <scm:enumeration value="##definedSibling"/>
      <scm:enumeration value="##defined"/>
   </scm:simpleType>
   <scm:simpleType id="C66" base="#token" variety="atomic" primitiveType="#string"/>
   <scm:simpleType id="C67" base="#token" variety="atomic" primitiveType="#string">
      <scm:enumeration value="##defined"/>
   </scm:simpleType>
   <scm:simpleType id="C68" name="xpathDefaultNamespace"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="#anyURI C69"/>
   <scm:complexType id="C24" name="element" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="true"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C35"/>
      <scm:attributeUse required="false" ref="C70"/>
      <scm:attributeUse required="false" ref="C33"/>
      <scm:attributeUse required="false" ref="C34"/>
      <scm:attributeUse required="false" ref="C30"/>
      <scm:attributeUse required="false" ref="C71" default="false"/>
      <scm:attributeUse required="false" ref="C72"/>
      <scm:attributeUse required="false" ref="C26"/>
      <scm:attributeUse required="false" ref="C32"/>
      <scm:attributeUse required="false" ref="C29"/>
      <scm:attributeUse required="false" ref="C27"/>
      <scm:attributeUse required="false" ref="C31"/>
      <scm:attributeUse required="false" ref="C25" default="1"/>
      <scm:attributeUse required="false" ref="C28" default="1"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
                     <scm:choice>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C73"/>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C74"/>
                     </scm:choice>
                  </scm:modelGroupParticle>
                  <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C75"/>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded" ref="C40"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C41" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C73" to="3"/>
            <scm:edge term="C75" to="4"/>
            <scm:edge term="C74" to="3"/>
            <scm:edge term="C42" to="1"/>
            <scm:edge term="C43" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C41" to="1"/>
            <scm:edge term="C42" to="1"/>
            <scm:edge term="C43" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C41" to="1"/>
            <scm:edge term="C73" to="3"/>
            <scm:edge term="C75" to="4"/>
            <scm:edge term="C74" to="3"/>
            <scm:edge term="C42" to="1"/>
            <scm:edge term="C43" to="1"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C41" to="1"/>
            <scm:edge term="C75" to="4"/>
            <scm:edge term="C42" to="1"/>
            <scm:edge term="C43" to="1"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C41" to="1"/>
            <scm:edge term="C75" to="4"/>
            <scm:edge term="C42" to="1"/>
            <scm:edge term="C43" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C76" name="topLevelSimpleType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C77"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C78"/>
      <scm:attributeUse required="true" ref="C79"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C80"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C81"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C82" to="1"/>
            <scm:edge term="C83" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C84" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2">
            <scm:edge term="C82" to="1"/>
            <scm:edge term="C83" to="1"/>
            <scm:edge term="C84" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C85" name="reducedDerivationControl"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="C86"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="restriction"/>
      <scm:enumeration value="extension"/>
   </scm:simpleType>
   <scm:complexType id="C87" name="topLevelElement"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C24"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C72"/>
      <scm:attributeUse required="false" ref="C26"/>
      <scm:attributeUse required="true" ref="C88"/>
      <scm:attributeUse required="false" ref="C71" default="false"/>
      <scm:attributeUse required="false" ref="C70"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C30"/>
      <scm:attributeUse required="false" ref="C33"/>
      <scm:attributeUse required="false" ref="C34"/>
      <scm:attributeUse required="false" ref="C35"/>
      <scm:attributeWildcard ref="C89"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C90"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C91"/>
               </scm:choice>
            </scm:modelGroupParticle>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C92"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded" ref="C40"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C90" to="1"/>
            <scm:edge term="C91" to="1"/>
            <scm:edge term="C41" to="2"/>
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C6" to="4"/>
            <scm:edge term="C42" to="2"/>
            <scm:edge term="C43" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C41" to="2"/>
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C42" to="2"/>
            <scm:edge term="C43" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C41" to="2"/>
            <scm:edge term="C42" to="2"/>
            <scm:edge term="C43" to="2"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C41" to="2"/>
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C42" to="2"/>
            <scm:edge term="C43" to="2"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C90" to="1"/>
            <scm:edge term="C91" to="1"/>
            <scm:edge term="C41" to="2"/>
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C42" to="2"/>
            <scm:edge term="C43" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C93" name="localSimpleType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C77"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C94"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C81"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C82" to="1"/>
            <scm:edge term="C83" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C84" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2">
            <scm:edge term="C82" to="1"/>
            <scm:edge term="C83" to="1"/>
            <scm:edge term="C84" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C95" name="topLevelComplexType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C96"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C97"/>
      <scm:attributeUse required="false" ref="C98"/>
      <scm:attributeUse required="false" ref="C99"/>
      <scm:attributeUse required="true" ref="C100"/>
      <scm:attributeUse required="false" ref="C101" default="false"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C102" default="true"/>
      <scm:attributeWildcard ref="C103"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C104"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C105" to="7"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C6" to="5"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C110" to="3"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C112" to="3"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C109" to="4"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
         <scm:state nr="3" final="true"/>
         <scm:state nr="4" final="true">
            <scm:edge term="C109" to="4"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C105" to="7"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C110" to="3"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C112" to="3"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
         <scm:state nr="7" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C113" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C114" base="#token" variety="atomic" primitiveType="#string">
      <scm:enumeration value="#all"/>
   </scm:simpleType>
   <scm:complexType id="C115" name="assertion" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C116"/>
      <scm:attributeUse required="false" ref="C117"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C118" base="#anySimpleType" variety="union" memberTypes="#anyURI C12"/>
   <scm:simpleType id="C119" base="#token" variety="atomic" primitiveType="#string"/>
   <scm:complexType id="C48" name="group" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="true"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C27"/>
      <scm:attributeUse required="false" ref="C31"/>
      <scm:attributeUse required="false" ref="C25" default="1"/>
      <scm:attributeUse required="false" ref="C28" default="1"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded" ref="C120"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C113" to="1"/>
            <scm:edge term="C121" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C122" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C113" to="1"/>
            <scm:edge term="C121" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C122" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C113" to="1"/>
            <scm:edge term="C121" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C122" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C123" base="#anySimpleType" variety="union" memberTypes="#language C124"/>
   <scm:simpleType id="C16" base="#token" variety="atomic" primitiveType="#string">
      <scm:enumeration value="#all"/>
   </scm:simpleType>
   <scm:simpleType id="C125" name="qnameListA" targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="list"
                   itemType="C126"/>
   <scm:simpleType id="C127" base="#token" variety="atomic" primitiveType="#string">
      <scm:enumeration value="#all"/>
   </scm:simpleType>
   <scm:complexType id="C96" name="complexType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="true"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C128"/>
      <scm:attributeUse required="false" ref="C98"/>
      <scm:attributeUse required="false" ref="C101" default="false"/>
      <scm:attributeUse required="false" ref="C97"/>
      <scm:attributeUse required="false" ref="C99"/>
      <scm:attributeUse required="false" ref="C102" default="true"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C104"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C105" to="7"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C6" to="5"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C110" to="3"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C112" to="3"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C109" to="4"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
         <scm:state nr="3" final="true"/>
         <scm:state nr="4" final="true">
            <scm:edge term="C109" to="4"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C105" to="7"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C110" to="3"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C112" to="3"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
         <scm:state nr="7" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C113" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C129" base="#anySimpleType" variety="list" itemType="#QName"/>
   <scm:simpleType id="C130" name="simpleDerivationSet"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C114 C13"/>
   <scm:complexType id="C131" name="namedGroup" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C132"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C133"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C134"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C135"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C136"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C137"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C136" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C135" to="1"/>
            <scm:edge term="C137" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2">
            <scm:edge term="C136" to="1"/>
            <scm:edge term="C135" to="1"/>
            <scm:edge term="C137" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C138" name="groupRef" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C132"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C25" default="1"/>
      <scm:attributeUse required="false" ref="C28" default="1"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="true" ref="C139"/>
      <scm:attributeWildcard ref="C140"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C141" name="specialNamespaceList"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#token"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="##other"/>
      <scm:enumeration value="##any"/>
   </scm:simpleType>
   <scm:simpleType id="C142" name="namespaceList"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C141 C143"/>
   <scm:complexType id="C144" name="openAttrs" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeWildcard ref="C9"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C145" base="#NMTOKEN" variety="atomic" primitiveType="#string">
      <scm:enumeration value="collapse"/>
      <scm:enumeration value="replace"/>
      <scm:enumeration value="preserve"/>
   </scm:simpleType>
   <scm:simpleType id="C146" base="#nonNegativeInteger" variety="atomic" primitiveType="#integer">
      <scm:enumeration value="1"/>
      <scm:enumeration value="0"/>
   </scm:simpleType>
   <scm:complexType id="C147" name="simpleExplicitGroup"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C47"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C148"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded" ref="C50"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C54" to="1"/>
            <scm:edge term="C55" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C54" to="1"/>
            <scm:edge term="C55" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C54" to="1"/>
            <scm:edge term="C55" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C149" name="restrictionType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C150"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
                     <scm:choice>
                        <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
                           <scm:sequence>
                              <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C105"/>
                              <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C151"/>
                           </scm:sequence>
                        </scm:modelGroupParticle>
                        <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C152"/>
                     </scm:choice>
                  </scm:modelGroupParticle>
                  <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C153"/>
                  <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C154"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C105" to="8"/>
            <scm:edge term="C155" to="2"/>
            <scm:edge term="C156" to="2"/>
            <scm:edge term="C51" to="3"/>
            <scm:edge term="C157" to="2"/>
            <scm:edge term="C6" to="5"/>
            <scm:edge term="C158" to="2"/>
            <scm:edge term="C159" to="2"/>
            <scm:edge term="C160" to="2"/>
            <scm:edge term="C53" to="3"/>
            <scm:edge term="C106" to="3"/>
            <scm:edge term="C161" to="2"/>
            <scm:edge term="C162" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C163" to="2"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C164" to="7"/>
            <scm:edge term="C165" to="2"/>
            <scm:edge term="C166" to="2"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C167" to="2"/>
            <scm:edge term="C168" to="2"/>
            <scm:edge term="C169" to="2"/>
            <scm:edge term="C170" to="2"/>
            <scm:edge term="C171" to="2"/>
            <scm:edge term="C172" to="2"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C113" to="3"/>
            <scm:edge term="C173" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C109" to="4"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C155" to="2"/>
            <scm:edge term="C156" to="2"/>
            <scm:edge term="C157" to="2"/>
            <scm:edge term="C158" to="2"/>
            <scm:edge term="C159" to="2"/>
            <scm:edge term="C160" to="2"/>
            <scm:edge term="C161" to="2"/>
            <scm:edge term="C162" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C163" to="2"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C165" to="2"/>
            <scm:edge term="C166" to="2"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C167" to="2"/>
            <scm:edge term="C168" to="2"/>
            <scm:edge term="C169" to="2"/>
            <scm:edge term="C170" to="2"/>
            <scm:edge term="C171" to="2"/>
            <scm:edge term="C172" to="2"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C173" to="2"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C109" to="4"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C105" to="8"/>
            <scm:edge term="C155" to="2"/>
            <scm:edge term="C156" to="2"/>
            <scm:edge term="C51" to="3"/>
            <scm:edge term="C157" to="2"/>
            <scm:edge term="C158" to="2"/>
            <scm:edge term="C159" to="2"/>
            <scm:edge term="C160" to="2"/>
            <scm:edge term="C53" to="3"/>
            <scm:edge term="C106" to="3"/>
            <scm:edge term="C161" to="2"/>
            <scm:edge term="C162" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C163" to="2"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C164" to="7"/>
            <scm:edge term="C165" to="2"/>
            <scm:edge term="C166" to="2"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C167" to="2"/>
            <scm:edge term="C168" to="2"/>
            <scm:edge term="C169" to="2"/>
            <scm:edge term="C170" to="2"/>
            <scm:edge term="C171" to="2"/>
            <scm:edge term="C172" to="2"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C113" to="3"/>
            <scm:edge term="C173" to="2"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
         <scm:state nr="7" final="true">
            <scm:edge term="C155" to="2"/>
            <scm:edge term="C156" to="2"/>
            <scm:edge term="C157" to="2"/>
            <scm:edge term="C158" to="2"/>
            <scm:edge term="C159" to="2"/>
            <scm:edge term="C160" to="2"/>
            <scm:edge term="C161" to="2"/>
            <scm:edge term="C162" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C163" to="2"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C165" to="2"/>
            <scm:edge term="C166" to="2"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C167" to="2"/>
            <scm:edge term="C168" to="2"/>
            <scm:edge term="C169" to="2"/>
            <scm:edge term="C170" to="2"/>
            <scm:edge term="C171" to="2"/>
            <scm:edge term="C172" to="2"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C173" to="2"/>
         </scm:state>
         <scm:state nr="8">
            <scm:edge term="C51" to="3"/>
            <scm:edge term="C53" to="3"/>
            <scm:edge term="C106" to="3"/>
            <scm:edge term="C113" to="3"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C174" name="anyType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="mixed">
      <scm:attributeWildcard ref="C175"/>
      <scm:elementWildcard minOccurs="0" maxOccurs="unbounded" ref="C176"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C176" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C176" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C177" base="#anySimpleType" variety="list" itemType="C178"/>
   <scm:complexType id="C179" name="wildcard" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C180"/>
      <scm:attributeUse required="false" ref="C181"/>
      <scm:attributeUse required="false" ref="C182" default="strict"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C183" name="noFixedFacet"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C1"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C8"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C184"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C124" base="#string" variety="atomic" primitiveType="#string">
      <scm:enumeration value=""/>
   </scm:simpleType>
   <scm:complexType id="C20" name="attributeGroup"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="true"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C27"/>
      <scm:attributeUse required="false" ref="C31"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C153"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C111" to="3"/>
            <scm:edge term="C108" to="3"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C111" to="3"/>
            <scm:edge term="C108" to="3"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C111" to="3"/>
            <scm:edge term="C108" to="3"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C17" base="#anySimpleType" variety="list" itemType="C85"/>
   <scm:complexType id="C77" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="true"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C78"/>
      <scm:attributeUse required="false" ref="C185"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C81"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C82" to="1"/>
            <scm:edge term="C83" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C84" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2">
            <scm:edge term="C82" to="1"/>
            <scm:edge term="C83" to="1"/>
            <scm:edge term="C84" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C186" name="keybase" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C187"/>
      <scm:attributeUse required="false" ref="C188"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C189"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C190"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C189" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C189" to="2"/>
         </scm:state>
         <scm:state nr="2">
            <scm:edge term="C190" to="3"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C190" to="4"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C190" to="4"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C191" name="namedAttributeGroup"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C20"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C192"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C193"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C153"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C111" to="3"/>
            <scm:edge term="C108" to="3"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C111" to="3"/>
            <scm:edge term="C108" to="3"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C111" to="3"/>
            <scm:edge term="C108" to="3"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C14" base="C86" variety="atomic" primitiveType="#string">
      <scm:enumeration value="extension"/>
      <scm:enumeration value="restriction"/>
      <scm:enumeration value="union"/>
      <scm:enumeration value="list"/>
   </scm:simpleType>
   <scm:complexType id="C132" name="realGroup" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C48"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C25" default="1"/>
      <scm:attributeUse required="false" ref="C27"/>
      <scm:attributeUse required="false" ref="C28" default="1"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C31"/>
      <scm:attributeWildcard ref="C194"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C113"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C51"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C53"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C113" to="1"/>
            <scm:edge term="C53" to="1"/>
            <scm:edge term="C6" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2" final="true">
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C113" to="1"/>
            <scm:edge term="C53" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C195" name="extensionType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C196"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C105"/>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="1" ref="C151"/>
                  <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C153"/>
                  <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C154"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C105" to="6"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C6" to="4"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="5"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C109" to="3"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C108" to="5"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C109" to="3"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C105" to="6"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="5"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C108" to="5"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C113" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C108" to="5"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C197" name="simpleRestrictionType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C149"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="true" ref="C150"/>
      <scm:attributeWildcard ref="C198"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
               <scm:choice>
                  <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C152"/>
               </scm:choice>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C153"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C154"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C155" to="1"/>
            <scm:edge term="C156" to="1"/>
            <scm:edge term="C157" to="1"/>
            <scm:edge term="C6" to="4"/>
            <scm:edge term="C158" to="1"/>
            <scm:edge term="C159" to="1"/>
            <scm:edge term="C160" to="1"/>
            <scm:edge term="C161" to="1"/>
            <scm:edge term="C162" to="1"/>
            <scm:edge term="C107" to="2"/>
            <scm:edge term="C163" to="1"/>
            <scm:edge term="C108" to="5"/>
            <scm:edge term="C164" to="6"/>
            <scm:edge term="C165" to="1"/>
            <scm:edge term="C166" to="1"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C167" to="1"/>
            <scm:edge term="C168" to="1"/>
            <scm:edge term="C169" to="1"/>
            <scm:edge term="C170" to="1"/>
            <scm:edge term="C171" to="1"/>
            <scm:edge term="C172" to="1"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C173" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C155" to="1"/>
            <scm:edge term="C156" to="1"/>
            <scm:edge term="C157" to="1"/>
            <scm:edge term="C158" to="1"/>
            <scm:edge term="C159" to="1"/>
            <scm:edge term="C160" to="1"/>
            <scm:edge term="C161" to="1"/>
            <scm:edge term="C162" to="1"/>
            <scm:edge term="C107" to="2"/>
            <scm:edge term="C163" to="1"/>
            <scm:edge term="C108" to="5"/>
            <scm:edge term="C165" to="1"/>
            <scm:edge term="C166" to="1"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C167" to="1"/>
            <scm:edge term="C168" to="1"/>
            <scm:edge term="C169" to="1"/>
            <scm:edge term="C170" to="1"/>
            <scm:edge term="C171" to="1"/>
            <scm:edge term="C172" to="1"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C173" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C109" to="3"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C109" to="3"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C155" to="1"/>
            <scm:edge term="C156" to="1"/>
            <scm:edge term="C157" to="1"/>
            <scm:edge term="C158" to="1"/>
            <scm:edge term="C159" to="1"/>
            <scm:edge term="C160" to="1"/>
            <scm:edge term="C161" to="1"/>
            <scm:edge term="C162" to="1"/>
            <scm:edge term="C107" to="2"/>
            <scm:edge term="C163" to="1"/>
            <scm:edge term="C108" to="5"/>
            <scm:edge term="C164" to="6"/>
            <scm:edge term="C165" to="1"/>
            <scm:edge term="C166" to="1"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C167" to="1"/>
            <scm:edge term="C168" to="1"/>
            <scm:edge term="C169" to="1"/>
            <scm:edge term="C170" to="1"/>
            <scm:edge term="C171" to="1"/>
            <scm:edge term="C172" to="1"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C173" to="1"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C107" to="2"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C108" to="5"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C155" to="1"/>
            <scm:edge term="C156" to="1"/>
            <scm:edge term="C157" to="1"/>
            <scm:edge term="C158" to="1"/>
            <scm:edge term="C159" to="1"/>
            <scm:edge term="C160" to="1"/>
            <scm:edge term="C161" to="1"/>
            <scm:edge term="C162" to="1"/>
            <scm:edge term="C107" to="2"/>
            <scm:edge term="C163" to="1"/>
            <scm:edge term="C108" to="5"/>
            <scm:edge term="C165" to="1"/>
            <scm:edge term="C166" to="1"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C167" to="1"/>
            <scm:edge term="C168" to="1"/>
            <scm:edge term="C169" to="1"/>
            <scm:edge term="C170" to="1"/>
            <scm:edge term="C171" to="1"/>
            <scm:edge term="C172" to="1"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C173" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C199" name="qnameList" targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="list"
                   itemType="C10"/>
   <scm:simpleType id="C200" name="formChoice" targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#NMTOKEN"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="unqualified"/>
      <scm:enumeration value="qualified"/>
   </scm:simpleType>
   <scm:complexType id="C201" name="localComplexType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C96"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C98"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C102" default="true"/>
      <scm:attributeWildcard ref="C202"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C104"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C105" to="7"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C6" to="5"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C110" to="3"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C112" to="3"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C109" to="4"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
         <scm:state nr="3" final="true"/>
         <scm:state nr="4" final="true">
            <scm:edge term="C109" to="4"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C105" to="7"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="6"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C110" to="3"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C112" to="3"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
         <scm:state nr="7" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C113" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C109" to="4"/>
            <scm:edge term="C111" to="6"/>
            <scm:edge term="C108" to="6"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C203" base="#NMTOKEN" variety="atomic" primitiveType="#string">
      <scm:enumeration value="suffix"/>
      <scm:enumeration value="interleave"/>
   </scm:simpleType>
   <scm:simpleType id="C204" base="C143" variety="list" itemType="C118">
      <scm:minLength value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C205" base="#token" variety="atomic" primitiveType="#string">
      <scm:enumeration value="#all"/>
   </scm:simpleType>
   <scm:simpleType id="C69" base="#token" variety="atomic" primitiveType="#string">
      <scm:enumeration value="##local"/>
      <scm:enumeration value="##targetNamespace"/>
      <scm:enumeration value="##defaultNamespace"/>
   </scm:simpleType>
   <scm:simpleType id="C86" name="derivationControl"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#NMTOKEN"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="union"/>
      <scm:enumeration value="list"/>
      <scm:enumeration value="restriction"/>
      <scm:enumeration value="extension"/>
      <scm:enumeration value="substitution"/>
   </scm:simpleType>
   <scm:simpleType id="C206" name="allNNI" targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="#nonNegativeInteger C207"/>
   <scm:simpleType id="C208" base="#NMTOKEN" variety="atomic" primitiveType="#string">
      <scm:enumeration value="required"/>
      <scm:enumeration value="optional"/>
      <scm:enumeration value="prohibited"/>
   </scm:simpleType>
   <scm:simpleType id="C45" name="typeDerivationControl"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="C86"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="union"/>
      <scm:enumeration value="list"/>
      <scm:enumeration value="restriction"/>
      <scm:enumeration value="extension"/>
   </scm:simpleType>
   <scm:simpleType id="C209" base="#anySimpleType" variety="list" itemType="#QName"/>
   <scm:simpleType id="C207" base="#NMTOKEN" variety="atomic" primitiveType="#string">
      <scm:enumeration value="unbounded"/>
   </scm:simpleType>
   <scm:complexType id="C210" name="altType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C211"/>
      <scm:attributeUse required="false" ref="C212"/>
      <scm:attributeUse required="false" ref="C213"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C214"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C215"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C215" to="2"/>
            <scm:edge term="C214" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C215" to="2"/>
            <scm:edge term="C214" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C7" name="annotated" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C144"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C216" base="C206" variety="union" memberTypes="#nonNegativeInteger C207">
      <scm:enumeration value="1"/>
      <scm:enumeration value="0"/>
   </scm:simpleType>
   <scm:simpleType id="C217" name="fullDerivationSet"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C127 C44"/>
   <scm:simpleType id="C178" base="C86" variety="atomic" primitiveType="#string">
      <scm:enumeration value="substitution"/>
      <scm:enumeration value="restriction"/>
      <scm:enumeration value="extension"/>
   </scm:simpleType>
   <scm:complexType id="C218" name="numFacet" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C1"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C219"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C4" default="false"/>
      <scm:attributeWildcard ref="C220"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C57" name="attribute" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C7"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C62"/>
      <scm:attributeUse required="false" ref="C221" default="optional"/>
      <scm:attributeUse required="false" ref="C59"/>
      <scm:attributeUse required="false" ref="C60"/>
      <scm:attributeUse required="false" ref="C222"/>
      <scm:attributeUse required="false" ref="C223"/>
      <scm:attributeUse required="false" ref="C224"/>
      <scm:attributeUse required="false" ref="C27"/>
      <scm:attributeUse required="false" ref="C31"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C225"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C225" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C225" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C226" base="#NMTOKEN" variety="atomic" primitiveType="#string">
      <scm:enumeration value="strict"/>
      <scm:enumeration value="lax"/>
      <scm:enumeration value="skip"/>
   </scm:simpleType>
   <scm:simpleType id="C227" base="#NMTOKEN" variety="atomic" primitiveType="#string">
      <scm:enumeration value="prohibited"/>
      <scm:enumeration value="required"/>
      <scm:enumeration value="optional"/>
   </scm:simpleType>
   <scm:simpleType id="C228" name="blockSet" targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C205 C177"/>
   <scm:simpleType id="C126" base="#anySimpleType" variety="union" memberTypes="#QName C67"/>
   <scm:complexType id="C229" name="simpleExtensionType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C195"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="true" ref="C196"/>
      <scm:attributeWildcard ref="C230"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C153"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C154"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="2"/>
            <scm:edge term="C6" to="3"/>
            <scm:edge term="C111" to="4"/>
            <scm:edge term="C108" to="4"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C109" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C109" to="2"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="2"/>
            <scm:edge term="C111" to="4"/>
            <scm:edge term="C108" to="4"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="2"/>
            <scm:edge term="C111" to="4"/>
            <scm:edge term="C108" to="4"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C231" name="all" targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C47"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C232" default="1"/>
      <scm:attributeUse required="false" ref="C233" default="1"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C234"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C235"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C52"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C236"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C235" to="1"/>
            <scm:edge term="C236" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C235" to="1"/>
            <scm:edge term="C236" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C235" to="1"/>
            <scm:edge term="C236" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C237" name="complexRestrictionType"
                    targetNamespace="http://www.w3.org/2001/XMLSchema"
                    base="C149"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="true" ref="C150"/>
      <scm:attributeWildcard ref="C238"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
               <scm:choice>
                  <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
                     <scm:sequence>
                        <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C105"/>
                        <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C151"/>
                     </scm:sequence>
                  </scm:modelGroupParticle>
               </scm:choice>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C153"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C154"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C105" to="6"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C6" to="4"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="5"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C109" to="3"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C108" to="5"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C109" to="3"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C105" to="6"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C53" to="2"/>
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C108" to="5"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C113" to="2"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C107" to="1"/>
            <scm:edge term="C109" to="3"/>
            <scm:edge term="C111" to="5"/>
            <scm:edge term="C108" to="5"/>
         </scm:state>
         <scm:state nr="6">
            <scm:edge term="C106" to="2"/>
            <scm:edge term="C51" to="2"/>
            <scm:edge term="C113" to="2"/>
            <scm:edge term="C53" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C143" name="basicNamespaceList"
                   targetNamespace="http://www.w3.org/2001/XMLSchema"
                   base="#anySimpleType"
                   variety="list"
                   itemType="C118"/>
   <scm:element id="C105" name="openContent" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C239"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C240" name="defaultOpenContent"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C241"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C155" name="facet" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="#anyType"
                global="true"
                nillable="false"
                abstract="true"/>
   <scm:element id="C43" name="unique" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C186"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C82" name="list" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C242"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C243" name="appinfo" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C244"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C245" name="schema" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C246"
                global="true"
                nillable="false"
                abstract="false">
      <scm:key id="C247" name="element" targetNamespace="http://www.w3.org/2001/XMLSchema">
         <scm:selector xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="xs:element"/>
         <scm:field xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="@name"/>
      </scm:key>
      <scm:key id="C248" name="attribute" targetNamespace="http://www.w3.org/2001/XMLSchema">
         <scm:selector xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="xs:attribute"/>
         <scm:field xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="@name"/>
      </scm:key>
      <scm:key id="C249" name="type" targetNamespace="http://www.w3.org/2001/XMLSchema">
         <scm:selector xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="xs:complexType|xs:simpleType"/>
         <scm:field xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="@name"/>
      </scm:key>
      <scm:key id="C250" name="group" targetNamespace="http://www.w3.org/2001/XMLSchema">
         <scm:selector xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="xs:group"/>
         <scm:field xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="@name"/>
      </scm:key>
      <scm:key id="C251" name="attributeGroup"
               targetNamespace="http://www.w3.org/2001/XMLSchema">
         <scm:selector xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="xs:attributeGroup"/>
         <scm:field xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="@name"/>
      </scm:key>
      <scm:key id="C252" name="notation" targetNamespace="http://www.w3.org/2001/XMLSchema">
         <scm:selector xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="xs:notation"/>
         <scm:field xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="@name"/>
      </scm:key>
      <scm:key id="C253" name="identityConstraint"
               targetNamespace="http://www.w3.org/2001/XMLSchema">
         <scm:selector xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       xpath=".//xs:key|.//xs:unique|.//xs:keyref"/>
         <scm:field xmlns:xs="http://www.w3.org/2001/XMLSchema" xpath="@name"/>
      </scm:key>
   </scm:element>
   <scm:element id="C190" name="field" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C254"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C156" name="minInclusive"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C1"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C51" name="choice" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C47"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C157" name="explicitTimezone"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C255"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C42" name="key" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C186"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C6" name="annotation" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C256"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C158" name="pattern" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C257"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C258" name="element" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C87"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C159" name="maxInclusive"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C1"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C160" name="assertion" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C115"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C53" name="sequence" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C47"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C259" name="group" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C131"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C260" name="notation" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C261"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C262" name="complexType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C95"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C84" name="union" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C263"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C161" name="length" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C218"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C162" name="maxScale" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C0"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C107" name="anyAttribute"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C264"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C83" name="restriction" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C265"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C163" name="minExclusive"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C1"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C266" name="attributeGroup"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C191"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C267" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C76"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C268" name="include" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C269"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C270" name="override" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C271"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C272" name="documentation"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C273"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C165" name="whiteSpace" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C274"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C166" name="maxExclusive"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C1"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C189" name="selector" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C275"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C167" name="fractionDigits"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C218"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C168" name="minLength" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C218"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C110" name="complexContent"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C276"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C169" name="totalDigits" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C277"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C170" name="minScale" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C0"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C41" name="keyref" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C278"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C52" name="any" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C279"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C280" name="redefine" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C281"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C171" name="enumeration" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C183"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C172" name="maxLength" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C218"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C155"/>
   </scm:element>
   <scm:element id="C282" name="attribute" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C56"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C112" name="simpleContent"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C283"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C284" name="import" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C285"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C113" name="all" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C231"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C286" name="lang" targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="C123"
                  global="true"/>
   <scm:attribute id="C287" name="space" targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="C46"
                  global="true"/>
   <scm:attribute id="C288" name="id" targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="#ID"
                  global="true"/>
   <scm:attribute id="C289" name="base" targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="#anyURI"
                  global="true"/>
   <scm:modelGroupDefinition id="C152" name="simpleRestrictionModel"
                             targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C164"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C155"/>
                  <scm:elementWildcard minOccurs="1" maxOccurs="1" ref="C173"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C120" name="particle" targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C121"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C122"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C113"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C51"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C53"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C52"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C50" name="nestedParticle"
                             targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C54"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C55"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C51"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C53"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C52"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C290" name="redefinable" targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C267"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C262"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C259"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C266"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C81" name="simpleDerivation"
                             targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C83"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C82"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C84"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C154" name="assertions" targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C109"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C153" name="attrDecls" targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C111"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C108"/>
               </scm:choice>
            </scm:modelGroupParticle>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C107"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C151" name="typeDefParticle"
                             targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C106"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C113"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C51"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C53"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C291" name="schemaTop" targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C290"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C258"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C282"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C260"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C40" name="identityConstraint"
                             targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C43"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C42"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C41"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C104" name="complexTypeModel"
                             targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C112"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C110"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C105"/>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="1" ref="C151"/>
                  <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C153"/>
                  <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C154"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C292" name="allModel" targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C235"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C52"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C236"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:attributeGroup id="C293" name="defRef" targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:attributeUse required="false" ref="C27"/>
      <scm:attributeUse required="false" ref="C31"/>
   </scm:attributeGroup>
   <scm:attributeGroup id="C294" name="occurs" targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:attributeUse required="false" ref="C25" default="1"/>
      <scm:attributeUse required="false" ref="C28" default="1"/>
   </scm:attributeGroup>
   <scm:attributeGroup id="C295" name="specialAttrs"
                       targetNamespace="http://www.w3.org/XML/1998/namespace">
      <scm:attributeUse required="false" ref="C289"/>
      <scm:attributeUse required="false" ref="C286"/>
      <scm:attributeUse required="false" ref="C287"/>
      <scm:attributeUse required="false" ref="C288"/>
   </scm:attributeGroup>
   <scm:attributeGroup id="C296" name="anyAttrGroup"
                       targetNamespace="http://www.w3.org/2001/XMLSchema">
      <scm:attributeUse required="false" ref="C180"/>
      <scm:attributeUse required="false" ref="C181"/>
      <scm:attributeUse required="false" ref="C182" default="strict"/>
   </scm:attributeGroup>
   <scm:notation id="C297" name="XMLSchemaStructures"
                 targetNamespace="http://www.w3.org/2001/XMLSchema"
                 publicId="structures"
                 systemId="http://www.w3.org/2000/08/XMLSchema.xsd"/>
   <scm:notation id="C298" name="XML" targetNamespace="http://www.w3.org/2001/XMLSchema"
                 publicId="REC-xml-19980210"
                 systemId="http://www.w3.org/TR/1998/REC-xml-19980210"/>
   <scm:attribute id="C102" name="defaultAttributesApply" type="#boolean" global="false"
                  containingComplexType="C96"/>
   <scm:complexType id="C273" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="mixed">
      <scm:attributeUse required="false" ref="C299"/>
      <scm:attributeUse required="false" ref="C286"/>
      <scm:attributeWildcard ref="C300"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:sequence>
            <scm:elementWildcard minOccurs="1" maxOccurs="1" ref="C301"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C301" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C301" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C136" name="choice" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C147"
                global="false"
                containingComplexType="C131"
                nillable="false"
                abstract="false"/>
   <scm:element id="C109" name="assert" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C115"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:wildcard id="C89" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C202" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C234" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:element id="C225" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                containingComplexType="C57"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C277" base="C218" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C302"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C4" default="false"/>
      <scm:attributeWildcard ref="C303"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C188" name="ref" type="#QName" global="false" containingComplexType="C186"/>
   <scm:attribute id="C222" name="form" type="C200" global="false" containingComplexType="C57"/>
   <scm:attribute id="C79" name="name" type="#NCName" global="false" containingComplexType="C76"/>
   <scm:attribute id="C35" name="type" type="#QName" global="false" containingComplexType="C24"/>
   <scm:wildcard id="C194" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C71" name="abstract" type="#boolean" global="false"
                  containingComplexType="C24"/>
   <scm:attribute id="C34" name="fixed" type="#string" global="false" containingComplexType="C24"/>
   <scm:attribute id="C28" name="maxOccurs" type="C206" global="false"/>
   <scm:element id="C235" name="element" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C23"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C255" base="C1" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C304"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C4" default="false"/>
      <scm:attributeWildcard ref="C305"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C304" name="value" type="C227" global="false" containingComplexType="C255"/>
   <scm:element id="C54" name="element" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C23"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C264" base="C179" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C306"/>
      <scm:attributeUse required="false" ref="C180"/>
      <scm:attributeUse required="false" ref="C181"/>
      <scm:attributeUse required="false" ref="C182" default="strict"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C215" name="complexType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C201"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C271" base="C144" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C307"/>
      <scm:attributeUse required="false" ref="C308"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded" ref="C291"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C260" to="1"/>
            <scm:edge term="C262" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C267" to="1"/>
            <scm:edge term="C282" to="1"/>
            <scm:edge term="C266" to="1"/>
            <scm:edge term="C258" to="1"/>
            <scm:edge term="C259" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C260" to="1"/>
            <scm:edge term="C262" to="1"/>
            <scm:edge term="C267" to="1"/>
            <scm:edge term="C282" to="1"/>
            <scm:edge term="C266" to="1"/>
            <scm:edge term="C258" to="1"/>
            <scm:edge term="C259" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C260" to="1"/>
            <scm:edge term="C262" to="1"/>
            <scm:edge term="C267" to="1"/>
            <scm:edge term="C282" to="1"/>
            <scm:edge term="C266" to="1"/>
            <scm:edge term="C258" to="1"/>
            <scm:edge term="C259" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C308" name="id" type="#ID" global="false" containingComplexType="C271"/>
   <scm:attribute id="C30" name="nillable" type="#boolean" global="false"
                  containingComplexType="C24"/>
   <scm:wildcard id="C198" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C173" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C193" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C182" name="processContents" type="C226" global="false"/>
   <scm:complexType id="C246" base="C144" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C309"/>
      <scm:attributeUse required="false" ref="C310"/>
      <scm:attributeUse required="false" ref="C311" default=""/>
      <scm:attributeUse required="false" ref="C312" default=""/>
      <scm:attributeUse required="false" ref="C313" default="unqualified"/>
      <scm:attributeUse required="false" ref="C314" default="unqualified"/>
      <scm:attributeUse required="false" ref="C315"/>
      <scm:attributeUse required="false" ref="C316" default="##local"/>
      <scm:attributeUse required="false" ref="C317"/>
      <scm:attributeUse required="false" ref="C286"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
                     <scm:choice>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C268"/>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C284"/>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C280"/>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C270"/>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                     </scm:choice>
                  </scm:modelGroupParticle>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
                     <scm:sequence>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C240"/>
                        <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C6"/>
                     </scm:sequence>
                  </scm:modelGroupParticle>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
                     <scm:sequence>
                        <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C291"/>
                        <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C6"/>
                     </scm:sequence>
                  </scm:modelGroupParticle>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C240" to="3"/>
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C258" to="2"/>
            <scm:edge term="C259" to="2"/>
            <scm:edge term="C260" to="2"/>
            <scm:edge term="C262" to="2"/>
            <scm:edge term="C266" to="2"/>
            <scm:edge term="C267" to="2"/>
            <scm:edge term="C268" to="1"/>
            <scm:edge term="C270" to="1"/>
            <scm:edge term="C280" to="1"/>
            <scm:edge term="C282" to="2"/>
            <scm:edge term="C284" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C240" to="3"/>
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C258" to="2"/>
            <scm:edge term="C259" to="2"/>
            <scm:edge term="C260" to="2"/>
            <scm:edge term="C262" to="2"/>
            <scm:edge term="C266" to="2"/>
            <scm:edge term="C267" to="2"/>
            <scm:edge term="C268" to="1"/>
            <scm:edge term="C270" to="1"/>
            <scm:edge term="C280" to="1"/>
            <scm:edge term="C282" to="2"/>
            <scm:edge term="C284" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C260" to="2"/>
            <scm:edge term="C262" to="2"/>
            <scm:edge term="C6" to="5"/>
            <scm:edge term="C267" to="2"/>
            <scm:edge term="C282" to="2"/>
            <scm:edge term="C266" to="2"/>
            <scm:edge term="C258" to="2"/>
            <scm:edge term="C259" to="2"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C260" to="2"/>
            <scm:edge term="C262" to="2"/>
            <scm:edge term="C6" to="4"/>
            <scm:edge term="C267" to="2"/>
            <scm:edge term="C282" to="2"/>
            <scm:edge term="C266" to="2"/>
            <scm:edge term="C258" to="2"/>
            <scm:edge term="C259" to="2"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C260" to="2"/>
            <scm:edge term="C262" to="2"/>
            <scm:edge term="C6" to="4"/>
            <scm:edge term="C267" to="2"/>
            <scm:edge term="C282" to="2"/>
            <scm:edge term="C266" to="2"/>
            <scm:edge term="C258" to="2"/>
            <scm:edge term="C259" to="2"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C260" to="2"/>
            <scm:edge term="C262" to="2"/>
            <scm:edge term="C6" to="5"/>
            <scm:edge term="C267" to="2"/>
            <scm:edge term="C282" to="2"/>
            <scm:edge term="C266" to="2"/>
            <scm:edge term="C258" to="2"/>
            <scm:edge term="C259" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C310" name="version" type="#token" global="false"
                  containingComplexType="C246"/>
   <scm:attribute id="C316" name="xpathDefaultNamespace" type="C68" global="false"
                  containingComplexType="C246"/>
   <scm:wildcard id="C220" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:complexType id="C257" base="C183" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C318"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C319"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:wildcard id="C22" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C94" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C319" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C36" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C311" name="finalDefault" type="C217" global="false"
                  containingComplexType="C246"/>
   <scm:attribute id="C180" name="namespace" type="C142" global="false"/>
   <scm:element id="C106" name="group" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C138"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:element id="C39" name="alternative" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C210"
                global="false"
                containingComplexType="C23"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C314" name="elementFormDefault" type="C200" global="false"
                  containingComplexType="C246"/>
   <scm:attribute id="C213" name="xpathDefaultNamespace" type="C68" global="false"
                  containingComplexType="C210"/>
   <scm:attribute id="C26" name="block" type="C228" global="false" containingComplexType="C24"/>
   <scm:attribute id="C59" name="default" type="#string" global="false"
                  containingComplexType="C57"/>
   <scm:attribute id="C101" name="abstract" type="#boolean" global="false"
                  containingComplexType="C96"/>
   <scm:attribute id="C72" name="final" type="C15" global="false" containingComplexType="C24"/>
   <scm:complexType id="C269" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C320"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C100" name="name" type="#NCName" global="false" containingComplexType="C95"/>
   <scm:element id="C74" name="complexType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C201"
                global="false"
                containingComplexType="C24"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C313" name="attributeFormDefault" type="C200" global="false"
                  containingComplexType="C246"/>
   <scm:wildcard id="C303" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C5" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:complexType id="C265" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C321"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C152"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C155" to="1"/>
            <scm:edge term="C156" to="1"/>
            <scm:edge term="C157" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C158" to="1"/>
            <scm:edge term="C159" to="1"/>
            <scm:edge term="C160" to="1"/>
            <scm:edge term="C161" to="1"/>
            <scm:edge term="C162" to="1"/>
            <scm:edge term="C163" to="1"/>
            <scm:edge term="C164" to="3"/>
            <scm:edge term="C165" to="1"/>
            <scm:edge term="C166" to="1"/>
            <scm:edge term="C167" to="1"/>
            <scm:edge term="C168" to="1"/>
            <scm:edge term="C169" to="1"/>
            <scm:edge term="C170" to="1"/>
            <scm:edge term="C171" to="1"/>
            <scm:edge term="C172" to="1"/>
            <scm:edge term="C173" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C155" to="1"/>
            <scm:edge term="C156" to="1"/>
            <scm:edge term="C157" to="1"/>
            <scm:edge term="C158" to="1"/>
            <scm:edge term="C159" to="1"/>
            <scm:edge term="C160" to="1"/>
            <scm:edge term="C161" to="1"/>
            <scm:edge term="C162" to="1"/>
            <scm:edge term="C163" to="1"/>
            <scm:edge term="C165" to="1"/>
            <scm:edge term="C166" to="1"/>
            <scm:edge term="C167" to="1"/>
            <scm:edge term="C168" to="1"/>
            <scm:edge term="C169" to="1"/>
            <scm:edge term="C170" to="1"/>
            <scm:edge term="C171" to="1"/>
            <scm:edge term="C172" to="1"/>
            <scm:edge term="C173" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C155" to="1"/>
            <scm:edge term="C156" to="1"/>
            <scm:edge term="C157" to="1"/>
            <scm:edge term="C158" to="1"/>
            <scm:edge term="C159" to="1"/>
            <scm:edge term="C160" to="1"/>
            <scm:edge term="C161" to="1"/>
            <scm:edge term="C162" to="1"/>
            <scm:edge term="C163" to="1"/>
            <scm:edge term="C164" to="3"/>
            <scm:edge term="C165" to="1"/>
            <scm:edge term="C166" to="1"/>
            <scm:edge term="C167" to="1"/>
            <scm:edge term="C168" to="1"/>
            <scm:edge term="C169" to="1"/>
            <scm:edge term="C170" to="1"/>
            <scm:edge term="C171" to="1"/>
            <scm:edge term="C172" to="1"/>
            <scm:edge term="C173" to="1"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C155" to="1"/>
            <scm:edge term="C156" to="1"/>
            <scm:edge term="C157" to="1"/>
            <scm:edge term="C158" to="1"/>
            <scm:edge term="C159" to="1"/>
            <scm:edge term="C160" to="1"/>
            <scm:edge term="C161" to="1"/>
            <scm:edge term="C162" to="1"/>
            <scm:edge term="C163" to="1"/>
            <scm:edge term="C165" to="1"/>
            <scm:edge term="C166" to="1"/>
            <scm:edge term="C167" to="1"/>
            <scm:edge term="C168" to="1"/>
            <scm:edge term="C169" to="1"/>
            <scm:edge term="C170" to="1"/>
            <scm:edge term="C171" to="1"/>
            <scm:edge term="C172" to="1"/>
            <scm:edge term="C173" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C212" name="type" type="#QName" global="false" containingComplexType="C210"/>
   <scm:attribute id="C315" name="defaultAttributes" type="#QName" global="false"
                  containingComplexType="C246"/>
   <scm:attribute id="C211" name="test" type="#string" global="false"
                  containingComplexType="C210"/>
   <scm:attribute id="C116" name="test" type="#string" global="false"
                  containingComplexType="C115"/>
   <scm:attribute id="C61" name="inheritable" type="#boolean" global="false"
                  containingComplexType="C56"/>
   <scm:element id="C73" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                containingComplexType="C24"
                nillable="false"
                abstract="false"/>
   <scm:wildcard id="C140" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C238" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C305" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C312" name="blockDefault" type="C228" global="false"
                  containingComplexType="C246"/>
   <scm:complexType id="C276" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C322"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C323"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C324"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C323" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C324" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2">
            <scm:edge term="C323" to="1"/>
            <scm:edge term="C324" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C324" name="extension" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C195"
                global="false"
                containingComplexType="C276"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C233" name="maxOccurs" type="C216" global="false"
                  containingComplexType="C231"/>
   <scm:attribute id="C128" name="name" type="#NCName" global="false" containingComplexType="C96"/>
   <scm:complexType id="C261" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C325"/>
      <scm:attributeUse required="false" ref="C326"/>
      <scm:attributeUse required="false" ref="C327"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C327" name="system" type="#anyURI" global="false"
                  containingComplexType="C261"/>
   <scm:attribute id="C326" name="public" type="C18" global="false" containingComplexType="C261"/>
   <scm:element id="C164" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C185" name="name" type="#NCName" global="false" containingComplexType="C77"/>
   <scm:complexType id="C285" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C328"/>
      <scm:attributeUse required="false" ref="C329"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C31" name="ref" type="#QName" global="false"/>
   <scm:element id="C37" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                containingComplexType="C23"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C239" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C330" default="interleave"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C331"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C331" to="1"/>
            <scm:edge term="C6" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2" final="true">
            <scm:edge term="C331" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C331" name="any" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C179"
                global="false"
                containingComplexType="C239"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C78" name="final" type="C130" global="false" containingComplexType="C77"/>
   <scm:attribute id="C133" name="name" type="#NCName" global="false"
                  containingComplexType="C131"/>
   <scm:attribute id="C4" name="fixed" type="#boolean" global="false" containingComplexType="C1"/>
   <scm:element id="C75" name="alternative" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C210"
                global="false"
                containingComplexType="C24"
                nillable="false"
                abstract="false"/>
   <scm:element id="C135" name="all" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C332"
                global="false"
                containingComplexType="C131"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C332" base="C231" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C333"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C235"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C52"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C236"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C235" to="1"/>
            <scm:edge term="C236" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C235" to="1"/>
            <scm:edge term="C236" to="1"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C235" to="1"/>
            <scm:edge term="C236" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:wildcard id="C333" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:element id="C55" name="group" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C138"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:wildcard id="C230" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:complexType id="C275" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C334"/>
      <scm:attributeUse required="false" ref="C335"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C335" name="xpathDefaultNamespace" type="C68" global="false"
                  containingComplexType="C275"/>
   <scm:attribute id="C322" name="mixed" type="#boolean" global="false"
                  containingComplexType="C276"/>
   <scm:attribute id="C88" name="name" type="#NCName" global="false" containingComplexType="C87"/>
   <scm:attribute id="C117" name="xpathDefaultNamespace" type="C68" global="false"
                  containingComplexType="C115"/>
   <scm:complexType id="C274" base="C1" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C336"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeUse required="false" ref="C4" default="false"/>
      <scm:attributeWildcard ref="C337"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C336" name="value" type="C145" global="false" containingComplexType="C274"/>
   <scm:element id="C38" name="complexType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C201"
                global="false"
                containingComplexType="C23"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C278" base="C186" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C338"/>
      <scm:attributeUse required="false" ref="C187"/>
      <scm:attributeUse required="false" ref="C188"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
                  <scm:modelGroupParticle minOccurs="0" maxOccurs="1">
                     <scm:sequence>
                        <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C189"/>
                        <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C190"/>
                     </scm:sequence>
                  </scm:modelGroupParticle>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C189" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C189" to="2"/>
         </scm:state>
         <scm:state nr="2">
            <scm:edge term="C190" to="3"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C190" to="4"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C190" to="4"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C241" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C339" default="false"/>
      <scm:attributeUse required="false" ref="C340" default="interleave"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C341"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C341" to="2"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C341" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C340" name="mode" type="C203" global="false" containingComplexType="C241"/>
   <scm:attribute id="C339" name="appliesToEmpty" type="#boolean" global="false"
                  containingComplexType="C241"/>
   <scm:attribute id="C60" name="fixed" type="#string" global="false" containingComplexType="C57"/>
   <scm:complexType id="C263" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C342"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C343"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C343" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C343" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C343" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C343" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                containingComplexType="C263"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C299" name="source" type="#anyURI" global="false"
                  containingComplexType="C273"/>
   <scm:wildcard id="C176" processContents="lax" constraint="any"/>
   <scm:attribute id="C21" name="ref" type="#QName" global="false" containingComplexType="C19"/>
   <scm:attribute id="C29" name="targetNamespace" type="#anyURI" global="false"
                  containingComplexType="C24"/>
   <scm:attribute id="C181" name="notNamespace" type="C204" global="false"/>
   <scm:attribute id="C192" name="name" type="#NCName" global="false"
                  containingComplexType="C191"/>
   <scm:attribute id="C70" name="substitutionGroup" type="C209" global="false"
                  containingComplexType="C24"/>
   <scm:attribute id="C32" name="form" type="C200" global="false" containingComplexType="C24"/>
   <scm:wildcard id="C49" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C317" name="id" type="#ID" global="false" containingComplexType="C246"/>
   <scm:attribute id="C3" name="id" type="#ID" global="false" containingComplexType="C7"/>
   <scm:element id="C122" name="group" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C138"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C58" name="name" type="#NCName" global="false" containingComplexType="C56"/>
   <scm:wildcard id="C175" processContents="lax" constraint="any"/>
   <scm:element id="C90" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                containingComplexType="C87"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C338" name="refer" type="#QName" global="false"
                  containingComplexType="C278"/>
   <scm:element id="C91" name="complexType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C201"
                global="false"
                containingComplexType="C87"
                nillable="false"
                abstract="false"/>
   <scm:wildcard id="C148" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C325" name="name" type="#NCName" global="false"
                  containingComplexType="C261"/>
   <scm:complexType id="C283" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C344"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C345"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C345" to="1"/>
            <scm:edge term="C6" to="2"/>
            <scm:edge term="C344" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2">
            <scm:edge term="C345" to="1"/>
            <scm:edge term="C344" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C344" name="restriction" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C197"
                global="false"
                containingComplexType="C283"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C33" name="default" type="#string" global="false"
                  containingComplexType="C24"/>
   <scm:attribute id="C302" name="value" type="#positiveInteger" global="false"
                  containingComplexType="C277"/>
   <scm:complexType id="C244" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="mixed">
      <scm:attributeUse required="false" ref="C346"/>
      <scm:attributeWildcard ref="C347"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:sequence>
            <scm:elementWildcard minOccurs="1" maxOccurs="1" ref="C348"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C348" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C348" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:wildcard id="C347" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C348" processContents="lax" constraint="any"/>
   <scm:attribute id="C346" name="source" type="#anyURI" global="false"
                  containingComplexType="C244"/>
   <scm:attribute id="C330" name="mode" type="C65" global="false" containingComplexType="C239"/>
   <scm:wildcard id="C337" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C223" name="targetNamespace" type="#anyURI" global="false"
                  containingComplexType="C57"/>
   <scm:complexType id="C256" base="C144" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C349"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C243"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C272"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C243" to="1"/>
            <scm:edge term="C272" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C243" to="1"/>
            <scm:edge term="C272" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C349" name="id" type="#ID" global="false" containingComplexType="C256"/>
   <scm:wildcard id="C184" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:wildcard id="C63" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C224" name="inheritable" type="#boolean" global="false"
                  containingComplexType="C57"/>
   <scm:attribute id="C99" name="block" type="C15" global="false" containingComplexType="C96"/>
   <scm:attribute id="C342" name="memberTypes" type="C129" global="false"
                  containingComplexType="C263"/>
   <scm:attribute id="C25" name="minOccurs" type="#nonNegativeInteger" global="false"/>
   <scm:attribute id="C309" name="targetNamespace" type="#anyURI" global="false"
                  containingComplexType="C246"/>
   <scm:element id="C108" name="attributeGroup"
                targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C19"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:wildcard id="C80" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:element id="C121" name="element" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C23"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C98" name="mixed" type="#boolean" global="false"
                  containingComplexType="C96"/>
   <scm:element id="C92" name="alternative" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C210"
                global="false"
                containingComplexType="C87"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C279" base="C179" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C350"/>
      <scm:attributeUse required="false" ref="C25" default="1"/>
      <scm:attributeUse required="false" ref="C28" default="1"/>
      <scm:attributeUse required="false" ref="C180"/>
      <scm:attributeUse required="false" ref="C181"/>
      <scm:attributeUse required="false" ref="C182" default="strict"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C350" name="notQName" type="C199" global="false"
                  containingComplexType="C279"/>
   <scm:attribute id="C321" name="base" type="#QName" global="false" containingComplexType="C265"/>
   <scm:element id="C236" name="group" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C138"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C97" name="final" type="C15" global="false" containingComplexType="C96"/>
   <scm:attribute id="C221" name="use" type="C208" global="false" containingComplexType="C57"/>
   <scm:attribute id="C318" name="value" type="#string" global="false"
                  containingComplexType="C257"/>
   <scm:wildcard id="C134" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C307" name="schemaLocation" type="#anyURI" global="false"
                  containingComplexType="C271"/>
   <scm:attribute id="C219" name="value" type="#nonNegativeInteger" global="false"
                  containingComplexType="C218"/>
   <scm:attribute id="C320" name="schemaLocation" type="#anyURI" global="false"
                  containingComplexType="C269"/>
   <scm:element id="C214" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C232" name="minOccurs" type="C146" global="false"
                  containingComplexType="C231"/>
   <scm:complexType id="C242" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C351"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C352"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C352" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C352" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C352" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                containingComplexType="C242"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C351" name="itemType" type="#QName" global="false"
                  containingComplexType="C242"/>
   <scm:wildcard id="C300" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:element id="C323" name="restriction" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C237"
                global="false"
                containingComplexType="C276"
                nillable="false"
                abstract="false"/>
   <scm:wildcard id="C9" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C328" name="namespace" type="#anyURI" global="false"
                  containingComplexType="C285"/>
   <scm:attribute id="C329" name="schemaLocation" type="#anyURI" global="false"
                  containingComplexType="C285"/>
   <scm:attribute id="C196" name="base" type="#QName" global="false" containingComplexType="C195"/>
   <scm:wildcard id="C103" processContents="lax" constraint="not"
                 namespaces="##local http://www.w3.org/2001/XMLSchema"/>
   <scm:attribute id="C334" name="xpath" type="C66" global="false" containingComplexType="C275"/>
   <scm:attribute id="C62" name="type" type="#QName" global="false" containingComplexType="C57"/>
   <scm:attribute id="C139" name="ref" type="#QName" global="false" containingComplexType="C138"/>
   <scm:element id="C111" name="attribute" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C57"
                global="false"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C254" base="C7" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C353"/>
      <scm:attributeUse required="false" ref="C354"/>
      <scm:attributeUse required="false" ref="C3"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C6"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C6" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C354" name="xpathDefaultNamespace" type="C68" global="false"
                  containingComplexType="C254"/>
   <scm:attribute id="C353" name="xpath" type="C119" global="false" containingComplexType="C254"/>
   <scm:element id="C64" name="simpleType" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C93"
                global="false"
                containingComplexType="C56"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C306" name="notQName" type="C125" global="false"
                  containingComplexType="C264"/>
   <scm:element id="C345" name="extension" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C229"
                global="false"
                containingComplexType="C283"
                nillable="false"
                abstract="false"/>
   <scm:element id="C341" name="any" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C179"
                global="false"
                containingComplexType="C241"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C8" name="value" type="#anySimpleType" global="false"
                  containingComplexType="C1"/>
   <scm:wildcard id="C301" processContents="lax" constraint="any"/>
   <scm:complexType id="C281" base="C144" derivationMethod="extension" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C355"/>
      <scm:attributeUse required="false" ref="C356"/>
      <scm:attributeWildcard ref="C9"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1" ref="C290"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C262" to="1"/>
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C267" to="1"/>
            <scm:edge term="C266" to="1"/>
            <scm:edge term="C259" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C262" to="1"/>
            <scm:edge term="C6" to="1"/>
            <scm:edge term="C267" to="1"/>
            <scm:edge term="C266" to="1"/>
            <scm:edge term="C259" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C356" name="id" type="#ID" global="false" containingComplexType="C281"/>
   <scm:attribute id="C355" name="schemaLocation" type="#anyURI" global="false"
                  containingComplexType="C281"/>
   <scm:attribute id="C2" name="value" type="#integer" global="false" containingComplexType="C0"/>
   <scm:attribute id="C27" name="name" type="#NCName" global="false"/>
   <scm:attribute id="C150" name="base" type="#QName" global="false" containingComplexType="C149"/>
   <scm:attribute id="C187" name="name" type="#NCName" global="false"
                  containingComplexType="C186"/>
   <scm:element id="C137" name="sequence" targetNamespace="http://www.w3.org/2001/XMLSchema"
                type="C147"
                global="false"
                containingComplexType="C131"
                nillable="false"
                abstract="false"/>
</scm:schema>