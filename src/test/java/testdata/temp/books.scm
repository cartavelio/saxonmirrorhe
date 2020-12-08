<?xml version="1.0" encoding="UTF-8"?>
<scm:schema xmlns=""
            xmlns:scm="http://ns.saxonica.com/schema-component-model"
            generatedAt="2020-12-01T11:59:12.203Z"
            xsdVersion="1.1">
   <scm:simpleType id="C0"
                   name="currencyType"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="EUR"/>
      <scm:enumeration value="CAD"/>
      <scm:enumeration value="USD"/>
      <scm:enumeration value="GBP"/>
   </scm:simpleType>
   <scm:simpleType id="C1"
                   name="languageType"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="German"/>
      <scm:enumeration value="English"/>
      <scm:enumeration value="French"/>
      <scm:enumeration value="Spanish"/>
   </scm:simpleType>
   <scm:complexType id="C2"
                    name="weightType"
                    base="#decimal"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#decimal">
      <scm:attributeUse required="false" inheritable="false" ref="C3"/>
   </scm:complexType>
   <scm:simpleType id="C4"
                   name="lengthUnitType"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="in"/>
      <scm:enumeration value="cm"/>
   </scm:simpleType>
   <scm:simpleType id="C5"
                   name="dimensionsContentType"
                   base="C6"
                   variety="list"
                   itemType="C7">
      <scm:length value="3"/>
   </scm:simpleType>
   <scm:complexType id="C8"
                    name="dimensionsType"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C5">
      <scm:attributeUse required="false" inheritable="false" ref="C9"/>
   </scm:complexType>
   <scm:simpleType id="C10"
                   name="weightUnitType"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="g"/>
      <scm:enumeration value="oz"/>
   </scm:simpleType>
   <scm:simpleType id="C7"
                   name="dimensionType"
                   base="#decimal"
                   variety="atomic"
                   primitiveType="#decimal">
      <scm:minExclusive value="0"/>
   </scm:simpleType>
   <scm:simpleType id="C11"
                   name="ISBNType"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="[0-9]{9}[0-9X]"/>
   </scm:simpleType>
   <scm:simpleType id="C6" base="#anySimpleType" variety="list" itemType="C7"/>
   <scm:complexType id="C12"
                    name="moneyType"
                    base="#decimal"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#decimal">
      <scm:attributeUse required="false" inheritable="false" ref="C13"/>
   </scm:complexType>
   <scm:element id="C14"
                name="BOOKS"
                type="C15"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C16"
                name="ITEM"
                type="C17"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C18"
                name="BOOKLIST"
                type="C19"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C20"
                name="CATEGORIES"
                type="C21"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C22"
                name="CATEGORY"
                type="C23"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C3"
                  name="UNIT"
                  type="C10"
                  global="false"
                  inheritable="false"
                  containingComplexType="C2"/>
   <scm:attribute id="C9"
                  name="UNIT"
                  type="C4"
                  global="false"
                  inheritable="false"
                  containingComplexType="C8"/>
   <scm:attribute id="C13"
                  name="currency"
                  type="C0"
                  global="false"
                  inheritable="false"
                  containingComplexType="C12"/>
   <scm:complexType id="C15"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C16"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C16" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C16" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C16" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C17"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" inheritable="false" ref="C24"/>
      <scm:attributeUse required="false" inheritable="false" ref="C25" default="V">
         <scm:default lexicalForm="V">
            <scm:item type="#string" value="V"/>
         </scm:default>
      </scm:attributeUse>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C26"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C27"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C28"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C29"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C30"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C31"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C32"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C33"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C34"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C35"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C36"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C26" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C27" to="2"/>
            <scm:edge term="C28" to="3"/>
         </scm:state>
         <scm:state nr="2">
            <scm:edge term="C28" to="3"/>
         </scm:state>
         <scm:state nr="3">
            <scm:edge term="C29" to="4"/>
         </scm:state>
         <scm:state nr="4">
            <scm:edge term="C30" to="5"/>
         </scm:state>
         <scm:state nr="5">
            <scm:edge term="C31" to="6"/>
         </scm:state>
         <scm:state nr="6">
            <scm:edge term="C32" to="7"/>
         </scm:state>
         <scm:state nr="7">
            <scm:edge term="C33" to="8"/>
         </scm:state>
         <scm:state nr="8">
            <scm:edge term="C34" to="9"/>
         </scm:state>
         <scm:state nr="9">
            <scm:edge term="C35" to="10"/>
         </scm:state>
         <scm:state nr="10">
            <scm:edge term="C36" to="11"/>
         </scm:state>
         <scm:state nr="11" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C19"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C14"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C20"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C14" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C20" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C21"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="mixed">
      <scm:attributeUse required="true" inheritable="false" ref="C37"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C22"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C22" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C22" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C23"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C38"/>
      <scm:attributeUse required="true" inheritable="false" ref="C39"/>
      <scm:attributeUse required="false" inheritable="false" ref="C40"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C24"
                  name="CAT"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C17"/>
   <scm:attribute id="C25"
                  name="TAX"
                  type="#NMTOKEN"
                  global="false"
                  inheritable="false"
                  containingComplexType="C17"/>
   <scm:element id="C26"
                name="TITLE"
                type="#string"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C27"
                name="AUTHOR"
                type="#string"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C28"
                name="PUBLISHER"
                type="#string"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C29"
                name="PUB-DATE"
                type="#date"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C30"
                name="LANGUAGE"
                type="C1"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C31"
                name="PRICE"
                type="C12"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C32"
                name="QUANTITY"
                type="#integer"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C33"
                name="ISBN"
                type="C11"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C34"
                name="PAGES"
                type="#integer"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C35"
                name="DIMENSIONS"
                type="C8"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:element id="C36"
                name="WEIGHT"
                type="C2"
                global="false"
                containingComplexType="C17"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C37"
                  name="DESC"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C21"/>
   <scm:attribute id="C38"
                  name="CODE"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C23"/>
   <scm:attribute id="C39"
                  name="DESC"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C23"/>
   <scm:attribute id="C40"
                  name="NOTE"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C23"/>
</scm:schema>
<?Σ fbbf058a?>
