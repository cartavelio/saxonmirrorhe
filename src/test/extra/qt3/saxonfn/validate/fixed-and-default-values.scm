<?xml version="1.0" encoding="UTF-8"?>
<scm:schema xmlns:scm="http://ns.saxonica.com/schema-component-model"
            generatedAt="2018-11-14T17:56:39.89Z"
            xsdVersion="1.1">
   <scm:element id="C0"
                name="e1"
                targetNamespace="http://saxonica.com/ns/fixedAndDefaultTest"
                type="C1"
                global="true"
                nillable="false"
                abstract="false">
      <scm:fixed lexicalForm="23">
         <scm:item type="#integer" value="23"/>
      </scm:fixed>
   </scm:element>
   <scm:element id="C2"
                name="e2"
                targetNamespace="http://saxonica.com/ns/fixedAndDefaultTest"
                type="C3"
                global="true"
                default="23"
                nillable="false"
                abstract="false">
      <scm:default lexicalForm="23">
         <scm:item type="#integer" value="23"/>
      </scm:default>
   </scm:element>
   <scm:element id="C4"
                name="e3"
                targetNamespace="http://saxonica.com/ns/fixedAndDefaultTest"
                type="C5"
                global="true"
                default="46"
                nillable="false"
                abstract="false">
      <scm:default lexicalForm="46">
         <scm:item type="#integer" value="46"/>
      </scm:default>
   </scm:element>
   <scm:attribute id="C6"
                  name="a1"
                  targetNamespace="http://saxonica.com/ns/fixedAndDefaultTest"
                  type="#date"
                  global="true"
                  inheritable="false"/>
   <scm:attribute id="C7"
                  name="b1"
                  targetNamespace="http://saxonica.com/ns/fixedAndDefaultTest"
                  type="#date"
                  global="true"
                  inheritable="false"/>
   <scm:attribute id="C8"
                  name="a2"
                  targetNamespace="http://saxonica.com/ns/fixedAndDefaultTest"
                  type="#date"
                  global="true"
                  inheritable="false">
      <scm:fixed lexicalForm="2018-11-14">
         <scm:item type="#date" value="2018-11-14"/>
      </scm:fixed>
   </scm:attribute>
   <scm:attribute id="C9"
                  name="b2"
                  targetNamespace="http://saxonica.com/ns/fixedAndDefaultTest"
                  type="#date"
                  global="true"
                  inheritable="false"
                  default="2018-11-14">
      <scm:default lexicalForm="2018-11-14">
         <scm:item type="#date" value="2018-11-14"/>
      </scm:default>
   </scm:attribute>
   <scm:complexType id="C1"
                    base="#integer"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#integer">
      <scm:attributeUse required="true" inheritable="false" ref="C10">
         <scm:fixed lexicalForm="2018-11-14">
            <scm:item type="#date" value="2018-11-14"/>
         </scm:fixed>
      </scm:attributeUse>
      <scm:attributeUse required="false"
                        inheritable="false"
                        ref="C11"
                        default="2018-11-14">
         <scm:default lexicalForm="2018-11-14">
            <scm:item type="#date" value="2018-11-14"/>
         </scm:default>
      </scm:attributeUse>
   </scm:complexType>
   <scm:complexType id="C3"
                    base="#integer"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#integer">
      <scm:attributeUse required="true" inheritable="false" ref="C6">
         <scm:fixed lexicalForm="2018-11-14">
            <scm:item type="#date" value="2018-11-14"/>
         </scm:fixed>
      </scm:attributeUse>
      <scm:attributeUse required="false"
                        inheritable="false"
                        ref="C7"
                        default="2018-11-14">
         <scm:default lexicalForm="2018-11-14">
            <scm:item type="#date" value="2018-11-14"/>
         </scm:default>
      </scm:attributeUse>
   </scm:complexType>
   <scm:complexType id="C5"
                    base="#integer"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#integer">
      <scm:attributeUse required="true" inheritable="false" ref="C8">
         <scm:fixed lexicalForm="2018-11-14">
            <scm:item type="#date" value="2018-11-14"/>
         </scm:fixed>
      </scm:attributeUse>
      <scm:attributeUse required="false"
                        inheritable="false"
                        ref="C9"
                        default="2018-11-14">
         <scm:default lexicalForm="2018-11-14">
            <scm:item type="#date" value="2018-11-14"/>
         </scm:default>
      </scm:attributeUse>
   </scm:complexType>
   <scm:attribute id="C10"
                  name="a"
                  type="#date"
                  global="false"
                  inheritable="false"
                  containingComplexType="C1"/>
   <scm:attribute id="C11"
                  name="b"
                  type="#date"
                  global="false"
                  inheritable="false"
                  containingComplexType="C1"/>
</scm:schema>
<?Σ 992bfec8?>
