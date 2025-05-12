<?xml version="1.0" encoding="UTF-8"?>
<scm:schema xmlns=""
            xmlns:scm="http://ns.saxonica.com/schema-component-model"
            generatedAt="2023-09-17T19:19:09.424946+01:00"
            xsdVersion="1.1"
            dmk="TGljZW5zb3I9U2F4b25pY2EKTGljZW5zZWU9TWljaGFlbCBLYXkKQ29tcGFueT1TYXhvbmljYQpFbWFpbD1taWtlQHNheG9uaWNhLmNvbQpFZGl0aW9uPUVFClNBVD15ZXMKU0FRPXllcwpTQVY9eWVzCklzc3VlZD0yMDIzLTA5LTE3ClNlcmllcz1WClNlcmlhbD1WMDExNzE0ClVzZXI9UDAwMDEKRXZhbHVhdGlvbj1ubwpFeHBpcmF0aW9uPW5ldmVyClVwZ3JhZGVEYXlzPTEwMDAKTWFpbnRlbmFuY2VEYXlzPTEwMDAKClNpZ25hdHVyZT0zMDJEMDIxNDRCREFEREJGNjdENjI4Qzk3RTEwQzFFQThBMEIwQzY1OURGOEI2MTUwMjE1MDA4REUyMUVEQTZEODVFNTQ5MTc0QjRDNzVBMTdEQzUyM0NFRjg5OUEz">
   <scm:simpleType id="C0"
                   name="_langType"
                   targetNamespace="http://www.w3.org/XML/1998/namespace"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="#language C1"/>
   <scm:simpleType id="C2"
                   name="processContentsType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#NCName"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="skip"/>
      <scm:enumeration value="lax"/>
      <scm:enumeration value="strict"/>
   </scm:simpleType>
   <scm:simpleType id="C3"
                   name="explicitTimezoneType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#NCName"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="required"/>
      <scm:enumeration value="prohibited"/>
      <scm:enumeration value="optional"/>
   </scm:simpleType>
   <scm:simpleType id="C4"
                   name="_spaceType"
                   targetNamespace="http://www.w3.org/XML/1998/namespace"
                   base="#NCName"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="default"/>
      <scm:enumeration value="preserve"/>
   </scm:simpleType>
   <scm:simpleType id="C5"
                   name="complexVarietyType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#NCName"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="simple"/>
      <scm:enumeration value="empty"/>
      <scm:enumeration value="mixed"/>
      <scm:enumeration value="element-only"/>
   </scm:simpleType>
   <scm:simpleType id="C6"
                   name="xsdVersionType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="1.0"/>
      <scm:enumeration value="1.1"/>
   </scm:simpleType>
   <scm:simpleType id="C7"
                   name="notQNameType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C8 C9 #NCName"/>
   <scm:simpleType id="C10"
                   name="namespaceListType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="list"
                   itemType="C11"/>
   <scm:simpleType id="C12"
                   name="openContentModeType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#NCName"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="suffix"/>
      <scm:enumeration value="interleave"/>
   </scm:simpleType>
   <scm:simpleType id="C13"
                   name="xpathExpressionType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value=".+"/>
   </scm:simpleType>
   <scm:simpleType id="C14"
                   name="unboundedType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#NCName"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="unbounded"/>
   </scm:simpleType>
   <scm:simpleType id="C11"
                   name="namespaceType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C15 C16"/>
   <scm:simpleType id="C17"
                   name="maxOccursType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="#nonNegativeInteger C14"/>
   <scm:simpleType id="C18"
                   name="typeReferenceListType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="list"
                   itemType="C19"/>
   <scm:simpleType id="C20"
                   name="whitespaceType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#NCName"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="replace"/>
      <scm:enumeration value="preserve"/>
      <scm:enumeration value="collapse"/>
   </scm:simpleType>
   <scm:simpleType id="C8"
                   name="pseudoQNameType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="##definedSibling"/>
      <scm:enumeration value="##defined"/>
   </scm:simpleType>
   <scm:complexType id="C21"
                    name="abstractParticleType"
                    targetNamespace="http://ns.saxonica.com/schema-component-model"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="true"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C22"/>
      <scm:attributeUse required="true" inheritable="false" ref="C23"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C15"
                   name="pseudoNamespaceType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="##absent"/>
      <scm:enumeration value="##any"/>
      <scm:enumeration value="##defaultNamespace"/>
      <scm:enumeration value="##other"/>
      <scm:enumeration value="##local"/>
      <scm:enumeration value="##targetNamespace"/>
   </scm:simpleType>
   <scm:simpleType id="C24"
                   name="notQNameListType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="list"
                   itemType="C7"/>
   <scm:complexType id="C25"
                    name="typedValueType"
                    targetNamespace="http://ns.saxonica.com/schema-component-model"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C26"/>
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C27"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C27" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C27" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C1"
                   name="zero-length-string"
                   targetNamespace="http://saxon.sf.net/"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="0"/>
   </scm:simpleType>
   <scm:simpleType id="C28"
                   name="blockType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="list"
                   itemType="C29"/>
   <scm:simpleType id="C29"
                   name="derivationMethodType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#NCName"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="extension"/>
      <scm:enumeration value="restriction"/>
      <scm:enumeration value="union"/>
      <scm:enumeration value="list"/>
      <scm:enumeration value="substitution"/>
   </scm:simpleType>
   <scm:simpleType id="C30"
                   name="nsContextType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="([^=]*=[^=]+\s)*([^=]*=[^=]+)?"/>
   </scm:simpleType>
   <scm:complexType id="C31"
                    name="xpathContainerType"
                    targetNamespace="http://ns.saxonica.com/schema-component-model"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C32"/>
      <scm:attributeUse required="true" inheritable="false" ref="C33"/>
      <scm:attributeUse required="true" inheritable="false" ref="C34"/>
      <scm:attributeUse required="false" inheritable="false" ref="C35"/>
      <scm:attributeUse required="false" inheritable="false" ref="C36"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C37"
                    name="identityConstraintType"
                    targetNamespace="http://ns.saxonica.com/schema-component-model"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" inheritable="false" ref="C38"/>
      <scm:attributeUse required="true" inheritable="false" ref="C39"/>
      <scm:attributeUse required="false" inheritable="false" ref="C40"/>
      <scm:attributeUse required="false" inheritable="false" ref="C41"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C42"/>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C43"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C42" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C43" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C43" to="3"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C43" to="3"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C19"
                   name="typeReferenceType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C44 #IDREF"/>
   <scm:simpleType id="C45"
                   name="finalType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#anySimpleType"
                   variety="list"
                   itemType="C29"/>
   <scm:simpleType id="C44"
                   name="builtInTypeReferenceType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="#[a-zA-Z0-9]+\*?"/>
   </scm:simpleType>
   <scm:simpleType id="C16"
                   name="uriType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#token"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="[^\s\r\n\t]*"/>
   </scm:simpleType>
   <scm:simpleType id="C9"
                   name="clarkNameType"
                   targetNamespace="http://ns.saxonica.com/schema-component-model"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="\{[^{}]*\}\i\c*"/>
   </scm:simpleType>
   <scm:element id="C46"
                name="keyref"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C37"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C47"
                name="alternativeType"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C48"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C49"
                name="minLength"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C50"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C52"
                name="attributeGroup"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C53"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C54"
                name="minInclusive"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C55"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C56"
                name="state"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C57"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C58"
                name="element"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C59"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C60"
                name="enumeration"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C61"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C62"
                name="edge"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C63"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C64"
                name="preprocess"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C65"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C66"
                name="modelGroupDefinition"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C67"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C68"
                name="maxInclusive"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C69"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C70"
                name="substitutionGroupAffiliation"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C71"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C72"
                name="notation"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C73"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C74"
                name="minExclusive"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C75"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C76"
                name="maxExclusive"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C77"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C78"
                name="attributeUse"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C79"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C80"
                name="simpleType"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C81"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C82"
                name="all"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C83"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C84"/>
   </scm:element>
   <scm:element id="C85"
                name="complexType"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C86"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C87"
                name="totalDigits"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C88"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C89"
                name="fixed"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C25"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C90"
                name="maxLength"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C91"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C92"
                name="finiteStateMachine"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C93"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C94"
                name="sequence"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C83"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C84"/>
   </scm:element>
   <scm:element id="C95"
                name="unique"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C37"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C96"
                name="attributeWildcard"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C97"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C98"
                name="openContent"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C99"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C43"
                name="field"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C31"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C100"
                name="modelGroupParticle"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C101"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C102"/>
   </scm:element>
   <scm:element id="C103"
                name="pattern"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C104"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C105"
                name="wildcard"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C106"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C51"
                name="abstractFacet"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="#anyType"
                global="true"
                nillable="false"
                abstract="true"/>
   <scm:element id="C107"
                name="schema"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C108"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C109"
                name="elementWildcard"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C110"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C102"/>
   </scm:element>
   <scm:element id="C111"
                name="attribute"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C112"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C113"
                name="minScale"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C114"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C115"
                name="elementParticle"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C116"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C102"/>
   </scm:element>
   <scm:element id="C42"
                name="selector"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C31"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C117"
                name="assertion"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C118"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C119"
                name="key"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C37"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C120"
                name="default"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C25"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C121"
                name="assert"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C122"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C123"
                name="fractionDigits"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C124"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C125"
                name="identityConstraint"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C126"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C127"
                name="length"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C128"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C102"
                name="abstractParticle"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C21"
                global="true"
                nillable="false"
                abstract="true"/>
   <scm:element id="C84"
                name="abstractModelGroup"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C83"
                global="true"
                nillable="false"
                abstract="true"/>
   <scm:element id="C129"
                name="whiteSpace"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C130"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C131"
                name="maxScale"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C132"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:element id="C133"
                name="choice"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C83"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C84"/>
   </scm:element>
   <scm:element id="C134"
                name="explicitTimezone"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C135"
                global="true"
                nillable="false"
                abstract="false">
      <scm:substitutionGroupAffiliation ref="C51"/>
   </scm:element>
   <scm:attribute id="C136"
                  name="space"
                  targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="C4"
                  global="true"
                  inheritable="false"/>
   <scm:attribute id="C36"
                  name="base"
                  targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="#anyURI"
                  global="true"
                  inheritable="false"/>
   <scm:attribute id="C137"
                  name="lang"
                  targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="C0"
                  global="true"
                  inheritable="false"/>
   <scm:attribute id="C138"
                  name="id"
                  targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="#ID"
                  global="true"
                  inheritable="false"/>
   <scm:attribute id="C22"
                  name="maxOccurs"
                  type="C17"
                  global="false"
                  inheritable="false"
                  containingComplexType="C21"/>
   <scm:attribute id="C23"
                  name="minOccurs"
                  type="#nonNegativeInteger"
                  global="false"
                  inheritable="false"
                  containingComplexType="C21"/>
   <scm:attribute id="C26"
                  name="lexicalForm"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C25"/>
   <scm:element id="C27"
                name="item"
                targetNamespace="http://ns.saxonica.com/schema-component-model"
                type="C139"
                global="false"
                containingComplexType="C25"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C32"
                  name="xpath"
                  type="C13"
                  global="false"
                  inheritable="false"
                  containingComplexType="C31"/>
   <scm:attribute id="C33"
                  name="nsContext"
                  type="C30"
                  global="false"
                  inheritable="false"
                  containingComplexType="C31"/>
   <scm:attribute id="C34"
                  name="defaultNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C31"/>
   <scm:attribute id="C35"
                  name="type"
                  type="C44"
                  global="false"
                  inheritable="false"
                  containingComplexType="C31"/>
   <scm:attribute id="C38"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C37"/>
   <scm:attribute id="C39"
                  name="name"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C37"/>
   <scm:attribute id="C40"
                  name="targetNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C37"/>
   <scm:attribute id="C41"
                  name="key"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C37"/>
   <scm:complexType id="C48"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C140"/>
      <scm:attributeUse required="true" inheritable="false" ref="C141"/>
      <scm:attributeUse required="true" inheritable="false" ref="C142"/>
      <scm:attributeUse required="true" inheritable="false" ref="C143"/>
      <scm:attributeUse required="false" inheritable="false" ref="C36"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C50"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C144"/>
      <scm:attributeUse required="false" inheritable="false" ref="C145" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C53"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" inheritable="false" ref="C146"/>
      <scm:attributeUse required="false" inheritable="false" ref="C147"/>
      <scm:attributeUse required="false" inheritable="false" ref="C148"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C78"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C96"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C96" to="1"/>
            <scm:edge term="C78" to="2"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2" final="true">
            <scm:edge term="C96" to="1"/>
            <scm:edge term="C78" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C55"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C149"/>
      <scm:attributeUse required="false" inheritable="false" ref="C150" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C57"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C151"/>
      <scm:attributeUse required="false" inheritable="false" ref="C152"/>
      <scm:attributeUse required="false" inheritable="false" ref="C153"/>
      <scm:attributeUse required="false" inheritable="false" ref="C154"/>
      <scm:attributeUse required="true" inheritable="false" ref="C155"/>
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C62"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C62" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C62" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C59"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" inheritable="false" ref="C156"/>
      <scm:attributeUse required="false" inheritable="false" ref="C157"/>
      <scm:attributeUse required="false" inheritable="false" ref="C158"/>
      <scm:attributeUse required="false" inheritable="false" ref="C159"/>
      <scm:attributeUse required="false" inheritable="false" ref="C160"/>
      <scm:attributeUse required="true" inheritable="false" ref="C161"/>
      <scm:attributeUse required="true" inheritable="false" ref="C162"/>
      <scm:attributeUse required="true" inheritable="false" ref="C163"/>
      <scm:attributeUse required="true" inheritable="false" ref="C164"/>
      <scm:attributeUse required="false" inheritable="false" ref="C165"/>
      <scm:attributeUse required="true" inheritable="false" ref="C166"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C70"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C47"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C125"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C89"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C120"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C89" to="1"/>
            <scm:edge term="C125" to="2"/>
            <scm:edge term="C47" to="3"/>
            <scm:edge term="C70" to="4"/>
            <scm:edge term="C120" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
         <scm:state nr="2" final="true">
            <scm:edge term="C89" to="1"/>
            <scm:edge term="C125" to="2"/>
            <scm:edge term="C120" to="1"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C89" to="1"/>
            <scm:edge term="C125" to="2"/>
            <scm:edge term="C47" to="3"/>
            <scm:edge term="C120" to="1"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C89" to="1"/>
            <scm:edge term="C125" to="2"/>
            <scm:edge term="C47" to="3"/>
            <scm:edge term="C70" to="4"/>
            <scm:edge term="C120" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
      <scm:assertion nsContext="scm=http://ns.saxonica.com/schema-component-model vc=http://www.w3.org/2007/XMLSchema-versioning xs=~"
                     test="not(@default and scm:fixed)"
                     defaultNamespace=""
                     xml:base="file:/Users/mike/GitHub/saxon2020/src/samples/scm/scmschema.xsd"/>
   </scm:complexType>
   <scm:complexType id="C61"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C167"/>
      <scm:attributeUse required="false" inheritable="false" ref="C168" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C63"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C169"/>
      <scm:attributeUse required="true" inheritable="false" ref="C170"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C65"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C171" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:elementParticle minOccurs="1" maxOccurs="2" ref="C117"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C117" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true" minOccurs="1" maxOccurs="2">
            <scm:edge term="C117" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C67"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" inheritable="false" ref="C172"/>
      <scm:attributeUse required="false" inheritable="false" ref="C173"/>
      <scm:attributeUse required="false" inheritable="false" ref="C174"/>
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C102"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C115" to="1"/>
            <scm:edge term="C100" to="1"/>
            <scm:edge term="C102" to="1"/>
            <scm:edge term="C109" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C115" to="1"/>
            <scm:edge term="C100" to="1"/>
            <scm:edge term="C102" to="1"/>
            <scm:edge term="C109" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C69"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C175"/>
      <scm:attributeUse required="false" inheritable="false" ref="C176" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C71"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C177"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C73"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C178"/>
      <scm:attributeUse required="false" inheritable="false" ref="C179"/>
      <scm:attributeUse required="false" inheritable="false" ref="C180"/>
      <scm:attributeUse required="false" inheritable="false" ref="C181"/>
      <scm:attributeUse required="false" inheritable="false" ref="C182"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C75"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C183"/>
      <scm:attributeUse required="false" inheritable="false" ref="C184" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C77"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C185"/>
      <scm:attributeUse required="false" inheritable="false" ref="C186" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C79"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C187"/>
      <scm:attributeUse required="true" inheritable="false" ref="C188"/>
      <scm:attributeUse required="true" inheritable="false" ref="C189"/>
      <scm:attributeUse required="true" inheritable="false" ref="C190"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C89"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C120"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C89" to="1"/>
            <scm:edge term="C120" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C81"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" inheritable="false" ref="C191"/>
      <scm:attributeUse required="false" inheritable="false" ref="C192"/>
      <scm:attributeUse required="true" inheritable="false" ref="C193"/>
      <scm:attributeUse required="false" inheritable="false" ref="C194"/>
      <scm:attributeUse required="false" inheritable="false" ref="C195"/>
      <scm:attributeUse required="false" inheritable="false" ref="C196"/>
      <scm:attributeUse required="false" inheritable="false" ref="C197"/>
      <scm:attributeUse required="false" inheritable="false" ref="C198"/>
      <scm:attributeUse required="true" inheritable="false" ref="C199"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C51"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C121" to="1"/>
            <scm:edge term="C87" to="1"/>
            <scm:edge term="C64" to="1"/>
            <scm:edge term="C129" to="1"/>
            <scm:edge term="C131" to="1"/>
            <scm:edge term="C113" to="1"/>
            <scm:edge term="C103" to="1"/>
            <scm:edge term="C123" to="1"/>
            <scm:edge term="C60" to="1"/>
            <scm:edge term="C74" to="1"/>
            <scm:edge term="C76" to="1"/>
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C54" to="1"/>
            <scm:edge term="C68" to="1"/>
            <scm:edge term="C90" to="1"/>
            <scm:edge term="C134" to="1"/>
            <scm:edge term="C127" to="1"/>
            <scm:edge term="C49" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C121" to="1"/>
            <scm:edge term="C87" to="1"/>
            <scm:edge term="C64" to="1"/>
            <scm:edge term="C129" to="1"/>
            <scm:edge term="C131" to="1"/>
            <scm:edge term="C113" to="1"/>
            <scm:edge term="C103" to="1"/>
            <scm:edge term="C123" to="1"/>
            <scm:edge term="C60" to="1"/>
            <scm:edge term="C74" to="1"/>
            <scm:edge term="C76" to="1"/>
            <scm:edge term="C51" to="1"/>
            <scm:edge term="C54" to="1"/>
            <scm:edge term="C68" to="1"/>
            <scm:edge term="C90" to="1"/>
            <scm:edge term="C134" to="1"/>
            <scm:edge term="C127" to="1"/>
            <scm:edge term="C49" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C83"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C102"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C100" to="1"/>
            <scm:edge term="C115" to="1"/>
            <scm:edge term="C102" to="1"/>
            <scm:edge term="C109" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C100" to="1"/>
            <scm:edge term="C115" to="1"/>
            <scm:edge term="C102" to="1"/>
            <scm:edge term="C109" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C86"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" inheritable="false" ref="C200"/>
      <scm:attributeUse required="true" inheritable="false" ref="C201"/>
      <scm:attributeUse required="true" inheritable="false" ref="C202"/>
      <scm:attributeUse required="false" inheritable="false" ref="C203"/>
      <scm:attributeUse required="false" inheritable="false" ref="C204"/>
      <scm:attributeUse required="true" inheritable="false" ref="C205"/>
      <scm:attributeUse required="false" inheritable="false" ref="C206"/>
      <scm:attributeUse required="false" inheritable="false" ref="C207"/>
      <scm:attributeUse required="false" inheritable="false" ref="C208"/>
      <scm:attributeUse required="true" inheritable="false" ref="C209"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C98"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C78"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C96"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C102"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C92"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C117"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C102" to="1"/>
            <scm:edge term="C98" to="2"/>
            <scm:edge term="C109" to="1"/>
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C100" to="1"/>
            <scm:edge term="C117" to="4"/>
            <scm:edge term="C96" to="5"/>
            <scm:edge term="C78" to="6"/>
            <scm:edge term="C115" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C117" to="4"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C102" to="1"/>
            <scm:edge term="C109" to="1"/>
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C100" to="1"/>
            <scm:edge term="C117" to="4"/>
            <scm:edge term="C96" to="5"/>
            <scm:edge term="C78" to="6"/>
            <scm:edge term="C115" to="1"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C117" to="4"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C117" to="4"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C102" to="1"/>
            <scm:edge term="C109" to="1"/>
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C100" to="1"/>
            <scm:edge term="C117" to="4"/>
            <scm:edge term="C115" to="1"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C102" to="1"/>
            <scm:edge term="C109" to="1"/>
            <scm:edge term="C92" to="3"/>
            <scm:edge term="C100" to="1"/>
            <scm:edge term="C117" to="4"/>
            <scm:edge term="C96" to="5"/>
            <scm:edge term="C78" to="6"/>
            <scm:edge term="C115" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C88"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C210"/>
      <scm:attributeUse required="false" inheritable="false" ref="C211" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C91"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C212"/>
      <scm:attributeUse required="false" inheritable="false" ref="C213" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C93"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" inheritable="false" ref="C214"/>
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C56"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C56" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C56" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C56" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C97"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C215"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C99"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="false" inheritable="false" ref="C216"/>
      <scm:attributeUse required="false" inheritable="false" ref="C217"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C101"
                    base="C21"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C218"/>
      <scm:attributeUse required="true" inheritable="false" ref="C22"/>
      <scm:attributeUse required="true" inheritable="false" ref="C23"/>
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C84"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C84" to="1"/>
            <scm:edge term="C82" to="1"/>
            <scm:edge term="C133" to="1"/>
            <scm:edge term="C94" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C84" to="1"/>
            <scm:edge term="C82" to="1"/>
            <scm:edge term="C133" to="1"/>
            <scm:edge term="C94" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C104"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C219"/>
      <scm:attributeUse required="false" inheritable="false" ref="C220" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C106"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C221"/>
      <scm:attributeUse required="true" inheritable="false" ref="C222"/>
      <scm:attributeUse required="false" inheritable="false" ref="C223"/>
      <scm:attributeUse required="true" inheritable="false" ref="C224"/>
      <scm:attributeUse required="false" inheritable="false" ref="C225"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C108"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C226"/>
      <scm:attributeUse required="false" inheritable="false" ref="C227"/>
      <scm:attributeUse required="false" inheritable="false" ref="C228"/>
      <scm:modelGroupParticle minOccurs="0" maxOccurs="unbounded">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C58"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C111"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C85"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C80"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C52"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C66"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C72"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C105"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C95"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C119"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C46"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C85" to="1"/>
            <scm:edge term="C58" to="1"/>
            <scm:edge term="C66" to="1"/>
            <scm:edge term="C111" to="1"/>
            <scm:edge term="C119" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C105" to="1"/>
            <scm:edge term="C95" to="1"/>
            <scm:edge term="C80" to="1"/>
            <scm:edge term="C72" to="1"/>
            <scm:edge term="C46" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C85" to="1"/>
            <scm:edge term="C58" to="1"/>
            <scm:edge term="C66" to="1"/>
            <scm:edge term="C111" to="1"/>
            <scm:edge term="C119" to="1"/>
            <scm:edge term="C52" to="1"/>
            <scm:edge term="C105" to="1"/>
            <scm:edge term="C95" to="1"/>
            <scm:edge term="C80" to="1"/>
            <scm:edge term="C72" to="1"/>
            <scm:edge term="C46" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C110"
                    base="C21"
                    derivationMethod="extension"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C229"/>
      <scm:attributeUse required="true" inheritable="false" ref="C22"/>
      <scm:attributeUse required="true" inheritable="false" ref="C23"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C112"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C230"/>
      <scm:attributeUse required="false" inheritable="false" ref="C231"/>
      <scm:attributeUse required="true" inheritable="false" ref="C232"/>
      <scm:attributeUse required="true" inheritable="false" ref="C233"/>
      <scm:attributeUse required="true" inheritable="false" ref="C234"/>
      <scm:attributeUse required="false" inheritable="false" ref="C235"/>
      <scm:attributeUse required="true" inheritable="false" ref="C236"/>
      <scm:attributeUse required="true" inheritable="false" ref="C237"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C89"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C120"/>
         </scm:choice>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C120" to="1"/>
            <scm:edge term="C89" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C114"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C238"/>
      <scm:attributeUse required="false" inheritable="false" ref="C239" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C116"
                    base="C21"
                    derivationMethod="extension"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C240"/>
      <scm:attributeUse required="true" inheritable="false" ref="C22"/>
      <scm:attributeUse required="true" inheritable="false" ref="C23"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C118"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C241"/>
      <scm:attributeUse required="true" inheritable="false" ref="C242"/>
      <scm:attributeUse required="true" inheritable="false" ref="C243"/>
      <scm:attributeUse required="false" inheritable="false" ref="C36"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C122"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" inheritable="false" ref="C244" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C117"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C117" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C124"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C245"/>
      <scm:attributeUse required="false" inheritable="false" ref="C246" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C126"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C247"/>
      <scm:attributeUse required="false" inheritable="false" ref="C36"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C128"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C248"/>
      <scm:attributeUse required="false" inheritable="false" ref="C249" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C130"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C250"/>
      <scm:attributeUse required="false" inheritable="false" ref="C251" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C132"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C252"/>
      <scm:attributeUse required="false" inheritable="false" ref="C253" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C135"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C254"/>
      <scm:attributeUse required="false" inheritable="false" ref="C255" default="false">
         <scm:default lexicalForm="false">
            <scm:item type="#boolean" value="false"/>
         </scm:default>
      </scm:attributeUse>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C139"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty">
      <scm:attributeUse required="true" inheritable="false" ref="C256"/>
      <scm:attributeUse required="true" inheritable="false" ref="C257"/>
      <scm:attributeUse required="false" inheritable="false" ref="C258"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C140"
                  name="type"
                  type="C19"
                  global="false"
                  inheritable="false"
                  containingComplexType="C48"/>
   <scm:attribute id="C141"
                  name="test"
                  type="C13"
                  global="false"
                  inheritable="false"
                  containingComplexType="C48"/>
   <scm:attribute id="C142"
                  name="nsContext"
                  type="C30"
                  global="false"
                  inheritable="false"
                  containingComplexType="C48"/>
   <scm:attribute id="C143"
                  name="defaultNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C48"/>
   <scm:attribute id="C144"
                  name="value"
                  type="#nonNegativeInteger"
                  global="false"
                  inheritable="false"
                  containingComplexType="C50"/>
   <scm:attribute id="C145"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C50"/>
   <scm:attribute id="C146"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C53"/>
   <scm:attribute id="C147"
                  name="name"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C53"/>
   <scm:attribute id="C148"
                  name="targetNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C53"/>
   <scm:attribute id="C149"
                  name="value"
                  type="#anySimpleType"
                  global="false"
                  inheritable="false"
                  containingComplexType="C55"/>
   <scm:attribute id="C150"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C55"/>
   <scm:attribute id="C151"
                  name="afterMax"
                  type="#integer"
                  global="false"
                  inheritable="false"
                  containingComplexType="C57"/>
   <scm:attribute id="C152"
                  name="final"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C57"/>
   <scm:attribute id="C153"
                  name="maxOccurs"
                  type="C17"
                  global="false"
                  inheritable="false"
                  containingComplexType="C57"/>
   <scm:attribute id="C154"
                  name="minOccurs"
                  type="#integer"
                  global="false"
                  inheritable="false"
                  containingComplexType="C57"/>
   <scm:attribute id="C155"
                  name="nr"
                  type="#integer"
                  global="false"
                  inheritable="false"
                  containingComplexType="C57"/>
   <scm:attribute id="C156"
                  name="abstract"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C157"
                  name="block"
                  type="C28"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C158"
                  name="containingComplexType"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C159"
                  name="default"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C160"
                  name="final"
                  type="C45"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C161"
                  name="global"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C162"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C163"
                  name="name"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C164"
                  name="nillable"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C165"
                  name="targetNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C166"
                  name="type"
                  type="C19"
                  global="false"
                  inheritable="false"
                  containingComplexType="C59"/>
   <scm:attribute id="C167"
                  name="value"
                  type="#anySimpleType"
                  global="false"
                  inheritable="false"
                  containingComplexType="C61"/>
   <scm:attribute id="C168"
                  name="namespaceSensitive"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C61"/>
   <scm:attribute id="C169"
                  name="term"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C63"/>
   <scm:attribute id="C170"
                  name="to"
                  type="#integer"
                  global="false"
                  inheritable="false"
                  containingComplexType="C63"/>
   <scm:attribute id="C171"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C65"/>
   <scm:attribute id="C172"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C67"/>
   <scm:attribute id="C173"
                  name="name"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C67"/>
   <scm:attribute id="C174"
                  name="targetNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C67"/>
   <scm:attribute id="C175"
                  name="value"
                  type="#anySimpleType"
                  global="false"
                  inheritable="false"
                  containingComplexType="C69"/>
   <scm:attribute id="C176"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C69"/>
   <scm:attribute id="C177"
                  name="ref"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C71"/>
   <scm:attribute id="C178"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C73"/>
   <scm:attribute id="C179"
                  name="name"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C73"/>
   <scm:attribute id="C180"
                  name="targetNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C73"/>
   <scm:attribute id="C181"
                  name="systemId"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C73"/>
   <scm:attribute id="C182"
                  name="publicId"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C73"/>
   <scm:attribute id="C183"
                  name="value"
                  type="#anySimpleType"
                  global="false"
                  inheritable="false"
                  containingComplexType="C75"/>
   <scm:attribute id="C184"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C75"/>
   <scm:attribute id="C185"
                  name="value"
                  type="#anySimpleType"
                  global="false"
                  inheritable="false"
                  containingComplexType="C77"/>
   <scm:attribute id="C186"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C77"/>
   <scm:attribute id="C187"
                  name="default"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C79"/>
   <scm:attribute id="C188"
                  name="ref"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C79"/>
   <scm:attribute id="C189"
                  name="required"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C79"/>
   <scm:attribute id="C190"
                  name="inheritable"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C79"/>
   <scm:attribute id="C191"
                  name="base"
                  type="C19"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C192"
                  name="final"
                  type="C45"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C193"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C194"
                  name="itemType"
                  type="C19"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C195"
                  name="memberTypes"
                  type="C18"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C196"
                  name="name"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C197"
                  name="primitiveType"
                  type="C44"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C198"
                  name="targetNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C199"
                  name="variety"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C81"/>
   <scm:attribute id="C200"
                  name="abstract"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C201"
                  name="base"
                  type="C19"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C202"
                  name="derivationMethod"
                  type="C29"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C203"
                  name="block"
                  type="C28"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C204"
                  name="final"
                  type="C45"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C205"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C206"
                  name="name"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C207"
                  name="simpleType"
                  type="C19"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C208"
                  name="targetNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C209"
                  name="variety"
                  type="C5"
                  global="false"
                  inheritable="false"
                  containingComplexType="C86"/>
   <scm:attribute id="C210"
                  name="value"
                  type="#positiveInteger"
                  global="false"
                  inheritable="false"
                  containingComplexType="C88"/>
   <scm:attribute id="C211"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C88"/>
   <scm:attribute id="C212"
                  name="value"
                  type="#nonNegativeInteger"
                  global="false"
                  inheritable="false"
                  containingComplexType="C91"/>
   <scm:attribute id="C213"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C91"/>
   <scm:attribute id="C214"
                  name="initialState"
                  type="#integer"
                  global="false"
                  inheritable="false"
                  containingComplexType="C93"/>
   <scm:attribute id="C215"
                  name="ref"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C97"/>
   <scm:attribute id="C216"
                  name="mode"
                  type="C12"
                  global="false"
                  inheritable="false"
                  containingComplexType="C99"/>
   <scm:attribute id="C217"
                  name="wildcard"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C99"/>
   <scm:attribute id="C218"
                  name="ref"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C101"/>
   <scm:attribute id="C219"
                  name="value"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C104"/>
   <scm:attribute id="C220"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C104"/>
   <scm:attribute id="C221"
                  name="constraint"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C106"/>
   <scm:attribute id="C222"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C106"/>
   <scm:attribute id="C223"
                  name="namespaces"
                  type="C10"
                  global="false"
                  inheritable="false"
                  containingComplexType="C106"/>
   <scm:attribute id="C224"
                  name="processContents"
                  type="C2"
                  global="false"
                  inheritable="false"
                  containingComplexType="C106"/>
   <scm:attribute id="C225"
                  name="notQName"
                  type="C24"
                  global="false"
                  inheritable="false"
                  containingComplexType="C106"/>
   <scm:attribute id="C226"
                  name="generatedAt"
                  type="#dateTime"
                  global="false"
                  inheritable="false"
                  containingComplexType="C108"/>
   <scm:attribute id="C227"
                  name="xsdVersion"
                  type="C6"
                  global="false"
                  inheritable="false"
                  containingComplexType="C108"/>
   <scm:attribute id="C228"
                  name="dmk"
                  type="#base64Binary"
                  global="false"
                  inheritable="false"
                  containingComplexType="C108"/>
   <scm:attribute id="C229"
                  name="ref"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C110"/>
   <scm:attribute id="C230"
                  name="containingComplexType"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C112"/>
   <scm:attribute id="C231"
                  name="default"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C112"/>
   <scm:attribute id="C232"
                  name="global"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C112"/>
   <scm:attribute id="C233"
                  name="id"
                  type="#ID"
                  global="false"
                  inheritable="false"
                  containingComplexType="C112"/>
   <scm:attribute id="C234"
                  name="name"
                  type="#NCName"
                  global="false"
                  inheritable="false"
                  containingComplexType="C112"/>
   <scm:attribute id="C235"
                  name="targetNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C112"/>
   <scm:attribute id="C236"
                  name="type"
                  type="C19"
                  global="false"
                  inheritable="false"
                  containingComplexType="C112"/>
   <scm:attribute id="C237"
                  name="inheritable"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C112"/>
   <scm:attribute id="C238"
                  name="value"
                  type="#anySimpleType"
                  global="false"
                  inheritable="false"
                  containingComplexType="C114"/>
   <scm:attribute id="C239"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C114"/>
   <scm:attribute id="C240"
                  name="ref"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C116"/>
   <scm:attribute id="C241"
                  name="test"
                  type="C13"
                  global="false"
                  inheritable="false"
                  containingComplexType="C118"/>
   <scm:attribute id="C242"
                  name="nsContext"
                  type="C30"
                  global="false"
                  inheritable="false"
                  containingComplexType="C118"/>
   <scm:attribute id="C243"
                  name="defaultNamespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C118"/>
   <scm:attribute id="C244"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C122"/>
   <scm:attribute id="C245"
                  name="value"
                  type="#nonNegativeInteger"
                  global="false"
                  inheritable="false"
                  containingComplexType="C124"/>
   <scm:attribute id="C246"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C124"/>
   <scm:attribute id="C247"
                  name="ref"
                  type="#IDREF"
                  global="false"
                  inheritable="false"
                  containingComplexType="C126"/>
   <scm:attribute id="C248"
                  name="value"
                  type="#nonNegativeInteger"
                  global="false"
                  inheritable="false"
                  containingComplexType="C128"/>
   <scm:attribute id="C249"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C128"/>
   <scm:attribute id="C250"
                  name="value"
                  type="C20"
                  global="false"
                  inheritable="false"
                  containingComplexType="C130"/>
   <scm:attribute id="C251"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C130"/>
   <scm:attribute id="C252"
                  name="value"
                  type="#anySimpleType"
                  global="false"
                  inheritable="false"
                  containingComplexType="C132"/>
   <scm:attribute id="C253"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C132"/>
   <scm:attribute id="C254"
                  name="value"
                  type="C3"
                  global="false"
                  inheritable="false"
                  containingComplexType="C135"/>
   <scm:attribute id="C255"
                  name="fixed"
                  type="#boolean"
                  global="false"
                  inheritable="false"
                  containingComplexType="C135"/>
   <scm:attribute id="C256"
                  name="type"
                  type="C44"
                  global="false"
                  inheritable="false"
                  containingComplexType="C139"/>
   <scm:attribute id="C257"
                  name="value"
                  type="#string"
                  global="false"
                  inheritable="false"
                  containingComplexType="C139"/>
   <scm:attribute id="C258"
                  name="namespace"
                  type="C16"
                  global="false"
                  inheritable="false"
                  containingComplexType="C139"/>
</scm:schema>
<?Σ 35cb6f3e?>
