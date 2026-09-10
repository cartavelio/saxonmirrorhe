<?xml version="1.0" encoding="UTF-8"?>
<scm:schema xmlns:scm="http://ns.saxonica.com/schema-component-model"
            generatedAt="2009-10-17T17:06:40.752+01:00"
            xsdVersion="1.0">
   <scm:simpleType id="C0" name="SchemeName" targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:complexType id="C1" name="Location"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="C2"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C2">
      <scm:attributeUse required="true" ref="C3"/>
   </scm:complexType>
   <scm:simpleType id="C4" name="NonEmptyString" targetNamespace="http://purl.org/dc/terms/"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
   </scm:simpleType>
   <scm:complexType id="C5" name="FreeText" targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="C6"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C6">
      <scm:attributeUse required="false" ref="C7"/>
   </scm:complexType>
   <scm:simpleType id="C8" name="OverheidInformationTypeScheme"
                   targetNamespace="http://purl.org/dc/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Informatietype"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C9" name="OverheidAgentClassScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Doelgroep"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:complexType id="C10" name="BeslissendOrgaan"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="C2"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C2">
      <scm:attributeUse required="true" ref="C11"/>
   </scm:complexType>
   <scm:complexType id="C12" name="AgentClassOrFreeText"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="C2"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C2">
      <scm:attributeUse required="false" ref="C13"/>
   </scm:complexType>
   <scm:complexType id="C14" name="Agent"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="C2"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C2">
      <scm:attributeUse required="true" ref="C15"/>
   </scm:complexType>
   <scm:simpleType id="C16" name="LocationScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C17 C18"/>
   <scm:simpleType id="C19" name="AgentClassScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C20 C21"/>
   <scm:simpleType id="C22" base="C23" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C24" base="C25" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:complexType id="C26" name="Location" targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="C6"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C6">
      <scm:attributeUse required="true" ref="C27"/>
   </scm:complexType>
   <scm:simpleType id="C28" base="C0" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C29" base="#string" variety="atomic" primitiveType="#string">
      <scm:enumeration value="opmaak"/>
      <scm:enumeration value="inhoud"/>
      <scm:enumeration value="tekst"/>
   </scm:simpleType>
   <scm:simpleType id="C2" name="NonEmptyString"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C30" name="OverheidInformationTypeScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Informatietype"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C31" name="BeslissendOrgaanScheme"
                   targetNamespace="http://purl.org/dc/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C32 C33"/>
   <scm:simpleType id="C20" name="OverheidAgentClassScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Doelgroep"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:complexType id="C34" name="BeslissendOrgaan"
                    targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="C6"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C6">
      <scm:attributeUse required="true" ref="C35"/>
   </scm:complexType>
   <scm:complexType id="C36" name="AgentClassOrFreeText"
                    targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="C6"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C6">
      <scm:attributeUse required="false" ref="C37"/>
   </scm:complexType>
   <scm:complexType id="C38" name="Agent" targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="C6"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C6">
      <scm:attributeUse required="true" ref="C39"/>
   </scm:complexType>
   <scm:simpleType id="C40" name="BeslissendOrgaanScheme_25610032"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C41 C24"/>
   <scm:simpleType id="C42" name="AgentScheme" targetNamespace="http://purl.org/dc/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C43 C44"/>
   <scm:simpleType id="C45" name="LocationScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C46 C47"/>
   <scm:simpleType id="C18" base="C25" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:complexType id="C48" name="TripleType" targetNamespace="http://purl.org/dc/terms/"
                    base="#string"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#string">
      <scm:attributeUse required="false" ref="C49"/>
      <scm:attributeUse required="false" ref="C50"/>
   </scm:complexType>
   <scm:simpleType id="C6" name="NonEmptyString"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C51" name="OverheidInformationTypeScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Informatietype"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C52" name="BeslissendOrgaanScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="C40"
                   variety="union"
                   memberTypes="C41 C24">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanWaterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanRegionaalSamenwerkingsorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanProvincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanMinisterie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanGemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanDeelgemeente"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C53" base="#string" variety="atomic" primitiveType="#string">
      <scm:enumeration value="website"/>
      <scm:enumeration value="post"/>
      <scm:enumeration value="telefoon"/>
      <scm:enumeration value="sms"/>
      <scm:enumeration value="e-mail"/>
      <scm:enumeration value="chat"/>
      <scm:enumeration value="balie"/>
      <scm:enumeration value="generiek"/>
   </scm:simpleType>
   <scm:simpleType id="C44" base="C23" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C47" base="C0" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C54" name="AgentScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="C55"
                   variety="union"
                   memberTypes="C56 C57">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:ZelfstandigBestuursorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Waterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:RegionaalSamenwerkingsorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Regering"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:RechterlijkeMacht"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Provincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Politiekorps"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:OpenbaarLichaamVoorBedrijfEnBeroep"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Ministerie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:KoninklijkHuis"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Koepelorganisatie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:HoogCollegeVanStaat"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Gemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:DienstAgentschapInstellingOfProject"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:EersteOfTweedeKamer"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Deelgemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Adviescollege"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C58" base="C25" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:complexType id="C59" name="AgentOrFreeText" targetNamespace="http://purl.org/dc/terms/"
                    base="C4"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C4">
      <scm:attributeUse required="false" ref="C60"/>
   </scm:complexType>
   <scm:simpleType id="C61" base="C0" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C62" base="#string" variety="atomic" primitiveType="#string">
      <scm:enumeration value="hoog"/>
      <scm:enumeration value="medium"/>
      <scm:enumeration value="laag"/>
   </scm:simpleType>
   <scm:complexType id="C63" name="TripleType"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="#string"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#string">
      <scm:attributeUse required="false" ref="C64"/>
      <scm:attributeUse required="false" ref="C65"/>
   </scm:complexType>
   <scm:complexType id="C66" name="InformationObject" targetNamespace="http://purl.org/dc/terms/"
                    base="#string"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#string">
      <scm:attributeUse required="true" ref="C67"/>
   </scm:complexType>
   <scm:simpleType id="C68" name="BeslissendOrgaanScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C69 C28"/>
   <scm:simpleType id="C70" name="URI" targetNamespace="http://purl.org/dc/terms/" base="#anyURI"
                   variety="atomic"
                   primitiveType="#anyURI">
      <scm:minLength value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C71" name="OverheidLocationScheme"
                   targetNamespace="http://purl.org/dc/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Waterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Provincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Gemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:PostcodeHuisnummer"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C72" name="AgentScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C73 C61"/>
   <scm:complexType id="C74" name="InformationType" targetNamespace="http://purl.org/dc/terms/"
                    base="C4"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C4">
      <scm:attributeUse required="true" ref="C75"/>
   </scm:complexType>
   <scm:complexType id="C76" name="AgentOrFreeText"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="C2"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C2">
      <scm:attributeUse required="false" ref="C77"/>
   </scm:complexType>
   <scm:simpleType id="C78" base="C23" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C79" name="DateTimeFormat" targetNamespace="http://purl.org/dc/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="#date #dateTime"/>
   <scm:complexType id="C80" name="TripleType"
                    targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="#string"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#string">
      <scm:attributeUse required="false" ref="C81"/>
      <scm:attributeUse required="false" ref="C82"/>
   </scm:complexType>
   <scm:simpleType id="C32" name="OverheidBestuursOrgaanScheme"
                   targetNamespace="http://purl.org/dc/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanWaterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanRegionaalSamenwerkingsorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanProvincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanMinisterie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanGemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanDeelgemeente"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:complexType id="C83" name="InformationObject"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="#string"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#string">
      <scm:attributeUse required="true" ref="C84"/>
   </scm:complexType>
   <scm:simpleType id="C85" name="URI"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#anyURI"
                   variety="atomic"
                   primitiveType="#anyURI">
      <scm:minLength value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C43" name="OverheidAgentScheme" targetNamespace="http://purl.org/dc/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:ZelfstandigBestuursorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Waterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:RegionaalSamenwerkingsorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Regering"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:RechterlijkeMacht"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Provincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Politiekorps"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:OpenbaarLichaamVoorBedrijfEnBeroep"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Ministerie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:KoninklijkHuis"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Koepelorganisatie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:HoogCollegeVanStaat"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Gemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:DienstAgentschapInstellingOfProject"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:EersteOfTweedeKamer"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Deelgemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Adviescollege"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C17" name="OverheidLocationScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Waterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Provincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Gemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:PostcodeHuisnummer"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:complexType id="C86" name="InformationType"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="C2"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C2">
      <scm:attributeUse required="true" ref="C87"/>
   </scm:complexType>
   <scm:complexType id="C88" name="AgentOrFreeText"
                    targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="C6"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C6">
      <scm:attributeUse required="false" ref="C89"/>
   </scm:complexType>
   <scm:simpleType id="C90" name="DateTimeFormat"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="#date #dateTime"/>
   <scm:complexType id="C91" name="PeriodOfTime" targetNamespace="http://purl.org/dc/terms/"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C92"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C93"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C92" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C93" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C41" name="OverheidBestuursOrgaanScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanWaterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanRegionaalSamenwerkingsorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanProvincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanMinisterie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanGemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanDeelgemeente"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C94" name="InformationTypeScheme"
                   targetNamespace="http://purl.org/dc/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C8 C78"/>
   <scm:complexType id="C95" name="InformationObject"
                    targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="#string"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="#string">
      <scm:attributeUse required="true" ref="C96"/>
   </scm:complexType>
   <scm:simpleType id="C57" base="C25" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C97" name="URI" targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#anyURI"
                   variety="atomic"
                   primitiveType="#anyURI">
      <scm:minLength value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C56" name="OverheidAgentScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:ZelfstandigBestuursorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Waterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:RegionaalSamenwerkingsorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Regering"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:RechterlijkeMacht"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Provincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Politiekorps"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:OpenbaarLichaamVoorBedrijfEnBeroep"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Ministerie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:KoninklijkHuis"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Koepelorganisatie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:HoogCollegeVanStaat"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Gemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:DienstAgentschapInstellingOfProject"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:EersteOfTweedeKamer"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Deelgemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Adviescollege"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C23" name="SchemeName" targetNamespace="http://purl.org/dc/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C46" name="OverheidLocationScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Waterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Provincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Gemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:PostcodeHuisnummer"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C33" base="C23" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:complexType id="C98" name="FreeText" targetNamespace="http://purl.org/dc/terms/" base="C4"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C4">
      <scm:attributeUse required="false" ref="C7"/>
   </scm:complexType>
   <scm:simpleType id="C99" base="C25" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C100" name="DateTimeFormat"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="#date #dateTime"/>
   <scm:simpleType id="C101" base="C0" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:complexType id="C102" name="PeriodOfTime"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C103"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C104"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C103" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C104" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:simpleType id="C55" name="AgentScheme_25109548"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C56 C57"/>
   <scm:simpleType id="C105" base="#string" variety="atomic" primitiveType="#string">
      <scm:enumeration value="non-actief"/>
      <scm:enumeration value="actief"/>
   </scm:simpleType>
   <scm:simpleType id="C106" name="InformationTypeScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C30 C99"/>
   <scm:simpleType id="C69" name="OverheidBestuursOrgaanScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanWaterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanRegionaalSamenwerkingsorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanProvincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanMinisterie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanGemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:BestuursorgaanDeelgemeente"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C21" base="C0" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C107" name="AgentClassScheme" targetNamespace="http://purl.org/dc/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C108 C22"/>
   <scm:simpleType id="C73" name="OverheidAgentScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:ZelfstandigBestuursorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Waterschap"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:RegionaalSamenwerkingsorgaan"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Regering"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:RechterlijkeMacht"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Provincie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Politiekorps"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:OpenbaarLichaamVoorBedrijfEnBeroep"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Ministerie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:KoninklijkHuis"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Koepelorganisatie"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:HoogCollegeVanStaat"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Gemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:DienstAgentschapInstellingOfProject"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:EersteOfTweedeKamer"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Deelgemeente"
                       namespaceSensitive="true"/>
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Adviescollege"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:simpleType id="C25" name="SchemeName"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:complexType id="C109" name="Location" targetNamespace="http://purl.org/dc/terms/" base="C4"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C4">
      <scm:attributeUse required="true" ref="C110"/>
   </scm:complexType>
   <scm:simpleType id="C111" base="#string" variety="atomic" primitiveType="#string">
      <scm:enumeration value="Werkplein"/>
      <scm:enumeration value="Antwoord"/>
   </scm:simpleType>
   <scm:complexType id="C112" name="FreeText"
                    targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                    base="C2"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C2">
      <scm:attributeUse required="false" ref="C7"/>
   </scm:complexType>
   <scm:complexType id="C113" name="InformationType"
                    targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="C6"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C6">
      <scm:attributeUse required="true" ref="C114"/>
   </scm:complexType>
   <scm:simpleType id="C115" base="C23" variety="atomic" primitiveType="#QName">
      <scm:pattern value="(overheid|\p{Ll}+):\p{Lu}\p{L}+"/>
   </scm:simpleType>
   <scm:simpleType id="C108" name="OverheidAgentClassScheme"
                   targetNamespace="http://purl.org/dc/terms/"
                   base="#QName"
                   variety="atomic"
                   primitiveType="#QName">
      <scm:enumeration xmlns:overheid="http://standaarden.overheid.nl/owms/terms/"
                       value="overheid:Doelgroep"
                       namespaceSensitive="true"/>
   </scm:simpleType>
   <scm:complexType id="C116" name="BeslissendOrgaan" targetNamespace="http://purl.org/dc/terms/"
                    base="C4"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C4">
      <scm:attributeUse required="true" ref="C117"/>
   </scm:complexType>
   <scm:complexType id="C118" name="PeriodOfTime"
                    targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C119"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C120"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C119" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C120" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C121" name="AgentClassOrFreeText"
                    targetNamespace="http://purl.org/dc/terms/"
                    base="C4"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C4">
      <scm:attributeUse required="false" ref="C122"/>
   </scm:complexType>
   <scm:simpleType id="C123" name="InformationTypeScheme"
                   targetNamespace="http://standaarden.overheid.nl/vac/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C51 C101"/>
   <scm:complexType id="C124" name="Agent" targetNamespace="http://purl.org/dc/terms/" base="C4"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C4">
      <scm:attributeUse required="true" ref="C125"/>
   </scm:complexType>
   <scm:simpleType id="C126" base="#NCName" variety="atomic" primitiveType="#string">
      <scm:enumeration value="preserve"/>
      <scm:enumeration value="default"/>
   </scm:simpleType>
   <scm:complexType id="C127" name="AgentLeverancierSysteem"
                    targetNamespace="http://standaarden.overheid.nl/vac/"
                    base="C6"
                    derivationMethod="extension"
                    abstract="false"
                    variety="simple"
                    simpleType="C6">
      <scm:attributeUse required="false" ref="C128"/>
   </scm:complexType>
   <scm:simpleType id="C129" name="LocationScheme" targetNamespace="http://purl.org/dc/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C71 C115"/>
   <scm:simpleType id="C130" name="AgentClassScheme"
                   targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                   base="#anySimpleType"
                   variety="union"
                   memberTypes="C9 C58"/>
   <scm:element id="C131" name="accessRights" targetNamespace="http://purl.org/dc/terms/"
                type="C48"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C132" name="creator" targetNamespace="http://purl.org/dc/terms/"
                type="C124"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C133" name="tableOfContents" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C134" name="relation" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C135" name="issued" targetNamespace="http://purl.org/dc/terms/" type="C79"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C136" name="extent" targetNamespace="http://purl.org/dc/terms/" type="C98"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C137" name="date" targetNamespace="http://purl.org/dc/terms/" type="C79"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C138" name="vacs" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C139"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C140" name="isRatifiedBy"
                targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                type="C10"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C141" name="hasVersion" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C142" name="vac" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C143"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C144" name="hasPart" targetNamespace="http://purl.org/dc/terms/" type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C145" name="dateSubmitted" targetNamespace="http://purl.org/dc/terms/"
                type="C79"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C146" name="requires" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C147" name="references" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C148" name="replaces" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C149" name="dateCopyrighted" targetNamespace="http://purl.org/dc/terms/"
                type="C79"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C150" name="mediator" targetNamespace="http://purl.org/dc/terms/"
                type="C121"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C151" name="coverage" targetNamespace="http://purl.org/dc/terms/"
                type="C98"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C152" name="rights" targetNamespace="http://purl.org/dc/terms/" type="C48"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C153" name="abbreviation"
                targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                type="C112"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C154" name="abstract" targetNamespace="http://purl.org/dc/terms/"
                type="C98"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C155" name="audience" targetNamespace="http://purl.org/dc/terms/"
                type="C121"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C156" name="provenance" targetNamespace="http://purl.org/dc/terms/"
                type="C48"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C157" name="temporal" targetNamespace="http://purl.org/dc/terms/"
                type="C91"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C158" name="dateAccepted" targetNamespace="http://purl.org/dc/terms/"
                type="C79"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C159" name="rightsHolder" targetNamespace="http://purl.org/dc/terms/"
                type="C59"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C160" name="isFormatOf" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C161" name="hasFormat" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C162" name="identifier" targetNamespace="http://purl.org/dc/terms/"
                type="C70"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C163" name="alternative" targetNamespace="http://purl.org/dc/terms/"
                type="C98"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C164" name="license" targetNamespace="http://purl.org/dc/terms/" type="C48"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C165" name="language" targetNamespace="http://purl.org/dc/terms/"
                type="#language"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C166" name="body" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C167"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C168" name="description" targetNamespace="http://purl.org/dc/terms/"
                type="C98"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C169" name="valid" targetNamespace="http://purl.org/dc/terms/" type="C91"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C170" name="source" targetNamespace="http://purl.org/dc/terms/" type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C171" name="isVersionOf" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C172" name="available" targetNamespace="http://purl.org/dc/terms/"
                type="C91"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C173" name="publisher" targetNamespace="http://purl.org/dc/terms/"
                type="C59"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C174" name="title" targetNamespace="http://purl.org/dc/terms/" type="C98"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C175" name="conformsTo" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C176" name="created" targetNamespace="http://purl.org/dc/terms/" type="C79"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C177" name="isRequiredBy" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C178" name="type" targetNamespace="http://purl.org/dc/terms/" type="C74"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C179" name="contributor" targetNamespace="http://purl.org/dc/terms/"
                type="C59"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C180" name="isReplacedBy" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C181" name="spatial" targetNamespace="http://purl.org/dc/terms/"
                type="C109"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C182" name="instructionalMethod"
                targetNamespace="http://purl.org/dc/terms/"
                type="C98"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C183" name="isPartOf" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C184" name="authority"
                targetNamespace="http://standaarden.overheid.nl/owms/terms/"
                type="C14"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C185" name="subject" targetNamespace="http://purl.org/dc/terms/" type="C48"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C186" name="isReferencedBy" targetNamespace="http://purl.org/dc/terms/"
                type="C66"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C187" name="educationLevel" targetNamespace="http://purl.org/dc/terms/"
                type="C121"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C188" name="modified" targetNamespace="http://purl.org/dc/terms/"
                type="C79"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C189" name="format" targetNamespace="http://purl.org/dc/terms/" type="C98"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C190" name="meta" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C191"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C7" name="lang" targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="#language"
                  global="true"/>
   <scm:attribute id="C192" name="space" targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="C126"
                  global="true"
                  default="preserve"/>
   <scm:attribute id="C193" name="base" targetNamespace="http://www.w3.org/XML/1998/namespace"
                  type="#anyURI"
                  global="true"/>
   <scm:modelGroupDefinition id="C194" name="owmsmantel"
                             targetNamespace="http://standaarden.overheid.nl/owms/terms/">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C153"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C140"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C184"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C154"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C131"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C163"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C155"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C172"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C175"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C179"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C151"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C176"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C137"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C158"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C149"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C145"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C168"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C187"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C136"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C189"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C161"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C144"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C141"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C182"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C160"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C183"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C186"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C180"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C177"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C171"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C135"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C164"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C150"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C156"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C173"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C147"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C134"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C148"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C146"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C152"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C159"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C170"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C185"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C133"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C169"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:modelGroupDefinition id="C195" name="owmskern"
                             targetNamespace="http://standaarden.overheid.nl/owms/terms/">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C162"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C174"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C165"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C178"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C132"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C188"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C181"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C157"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:modelGroupDefinition>
   <scm:attributeGroup id="C196" name="specialAttrs"
                       targetNamespace="http://www.w3.org/XML/1998/namespace">
      <scm:attributeUse required="false" ref="C193"/>
      <scm:attributeUse required="false" ref="C7"/>
      <scm:attributeUse required="false" ref="C192"/>
   </scm:attributeGroup>
   <scm:attribute id="C37" name="scheme" type="C19" global="false" containingComplexType="C36"/>
   <scm:attribute id="C35" name="scheme" type="C68" global="false" containingComplexType="C34"/>
   <scm:attribute id="C89" name="scheme" type="C72" global="false" containingComplexType="C88"/>
   <scm:element id="C93" name="end" type="C79" global="false" containingComplexType="C91"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C128" name="scheme" type="#QName" global="false"
                  containingComplexType="C127">
      <scm:fixed>
         <scm:item type="#QName" value="overheidvac:LeverancierSysteem"
                   namespace="http://standaarden.overheid.nl/vac/terms/"/>
      </scm:fixed>
   </scm:attribute>
   <scm:attribute id="C3" name="scheme" type="C16" global="false" containingComplexType="C1"/>
   <scm:attribute id="C65" name="resourceIdentifier" type="C85" global="false"
                  containingComplexType="C63"/>
   <scm:attribute id="C82" name="resourceIdentifier" type="C97" global="false"
                  containingComplexType="C80"/>
   <scm:attribute id="C75" name="scheme" type="C94" global="false" containingComplexType="C74"/>
   <scm:attribute id="C87" name="scheme" type="C106" global="false" containingComplexType="C86"/>
   <scm:attribute id="C15" name="scheme" type="C54" global="false" containingComplexType="C14"/>
   <scm:element id="C103" name="start" type="C90" global="false" containingComplexType="C102"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C81" name="scheme" type="C0" global="false" containingComplexType="C80"/>
   <scm:attribute id="C84" name="resourceIdentifier" type="#anyURI" global="false"
                  containingComplexType="C83"/>
   <scm:attribute id="C125" name="scheme" type="C42" global="false" containingComplexType="C124"/>
   <scm:attribute id="C64" name="scheme" type="C25" global="false" containingComplexType="C63"/>
   <scm:attribute id="C67" name="resourceIdentifier" type="#anyURI" global="false"
                  containingComplexType="C66"/>
   <scm:element id="C119" name="start" type="C100" global="false" containingComplexType="C118"
                nillable="false"
                abstract="false"/>
   <scm:element id="C120" name="end" type="C100" global="false" containingComplexType="C118"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C27" name="scheme" type="C45" global="false" containingComplexType="C26"/>
   <scm:complexType id="C191" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C197"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C198"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C199"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C197" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C198" to="2"/>
         </scm:state>
         <scm:state nr="2">
            <scm:edge term="C199" to="3"/>
         </scm:state>
         <scm:state nr="3" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C198" name="owmsmantel"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C200"
                global="false"
                containingComplexType="C191"
                nillable="false"
                abstract="false"/>
   <scm:element id="C199" name="vacmeta" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C201"
                global="false"
                containingComplexType="C191"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C201" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C202"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C203"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C204"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C205"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C206"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C207"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C202" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C203" to="2"/>
         </scm:state>
         <scm:state nr="2">
            <scm:edge term="C204" to="3"/>
         </scm:state>
         <scm:state nr="3">
            <scm:edge term="C205" to="4"/>
         </scm:state>
         <scm:state nr="4">
            <scm:edge term="C206" to="5"/>
         </scm:state>
         <scm:state nr="5">
            <scm:edge term="C207" to="6"/>
         </scm:state>
         <scm:state nr="6" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C202" name="bestemming"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C111"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:element id="C204" name="leverancierSysteem"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C127"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C200" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C184"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C179"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C172"/>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C155"/>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C185"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C184" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C179" to="2"/>
            <scm:edge term="C155" to="3"/>
            <scm:edge term="C172" to="4"/>
         </scm:state>
         <scm:state nr="2">
            <scm:edge term="C179" to="2"/>
            <scm:edge term="C155" to="3"/>
            <scm:edge term="C172" to="4"/>
         </scm:state>
         <scm:state nr="3">
            <scm:edge term="C155" to="5"/>
            <scm:edge term="C185" to="6"/>
         </scm:state>
         <scm:state nr="4">
            <scm:edge term="C155" to="3"/>
         </scm:state>
         <scm:state nr="5">
            <scm:edge term="C155" to="5"/>
            <scm:edge term="C185" to="6"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C185" to="7"/>
         </scm:state>
         <scm:state nr="7" final="true">
            <scm:edge term="C185" to="7"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C206" name="datumControle"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C100"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:element id="C203" name="prioriteit"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C62"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C139" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C142"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C142" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C142" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C142" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C122" name="scheme" type="C107" global="false" containingComplexType="C121"/>
   <scm:attribute id="C117" name="scheme" type="C31" global="false" containingComplexType="C116"/>
   <scm:attribute id="C13" name="scheme" type="C130" global="false" containingComplexType="C12"/>
   <scm:element id="C205" name="statusRedactie"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C105"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:element id="C207" name="revisionHistory"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C208"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C208" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C209"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C209" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C209" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C209" to="2"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C209" name="revision" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C210"
                global="false"
                containingComplexType="C208"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C210" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C211"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C212"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C213"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C214"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C215"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C211" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C212" to="2"/>
         </scm:state>
         <scm:state nr="2">
            <scm:edge term="C213" to="3"/>
         </scm:state>
         <scm:state nr="3">
            <scm:edge term="C214" to="4"/>
         </scm:state>
         <scm:state nr="4">
            <scm:edge term="C215" to="5"/>
         </scm:state>
         <scm:state nr="5" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C211" name="revisionType"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C29"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:element id="C212" name="version" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="#string"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:element id="C213" name="dateTime" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C100"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:element id="C214" name="author" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="#string"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:element id="C215" name="comment" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="#string"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C167" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C216"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C217"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C218"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C216" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C218" to="2"/>
            <scm:edge term="C216" to="3"/>
            <scm:edge term="C217" to="4"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C218" to="2"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C218" to="2"/>
            <scm:edge term="C216" to="3"/>
            <scm:edge term="C217" to="4"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C218" to="2"/>
            <scm:edge term="C217" to="4"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C218" name="verwijzingVac"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C95"
                global="false"
                containingComplexType="C167"
                nillable="false"
                abstract="false"/>
   <scm:element id="C216" name="kanaal" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C219"
                global="false"
                containingComplexType="C167"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C219" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C220"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C221"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C222"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C223"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C224"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C225"/>
            <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C226"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C221" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C222" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true">
            <scm:edge term="C225" to="3"/>
            <scm:edge term="C224" to="4"/>
            <scm:edge term="C226" to="5"/>
            <scm:edge term="C223" to="6"/>
         </scm:state>
         <scm:state nr="3" final="true">
            <scm:edge term="C225" to="3"/>
            <scm:edge term="C226" to="5"/>
         </scm:state>
         <scm:state nr="4" final="true">
            <scm:edge term="C225" to="3"/>
            <scm:edge term="C226" to="5"/>
         </scm:state>
         <scm:state nr="5" final="true">
            <scm:edge term="C226" to="5"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C225" to="3"/>
            <scm:edge term="C224" to="4"/>
            <scm:edge term="C226" to="5"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C222" name="kortAntwoord"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C227"
                global="false"
                containingComplexType="C219"
                nillable="false"
                abstract="false"/>
   <scm:element id="C225" name="verwijzingOverigeInfo"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C95"
                global="false"
                containingComplexType="C219"
                nillable="false"
                abstract="false"/>
   <scm:element id="C224" name="onderwaterantwoord"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C228"
                global="false"
                containingComplexType="C219"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C228" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="mixed">
      <scm:elementWildcard minOccurs="0" maxOccurs="unbounded" ref="C229"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C229" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C229" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:wildcard id="C229" processContents="lax" constraint="enumeration"
                 namespaces="http://www.w3.org/1999/xhtml"/>
   <scm:element id="C221" name="vraag" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="#string"
                global="false"
                containingComplexType="C219"
                nillable="false"
                abstract="false"/>
   <scm:element id="C226" name="verwijzingContact"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C95"
                global="false"
                containingComplexType="C219"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C227" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="mixed">
      <scm:elementWildcard minOccurs="0" maxOccurs="unbounded" ref="C230"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C230" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C230" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C220" name="type" type="C53" global="false" containingComplexType="C219"/>
   <scm:element id="C223" name="antwoord" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C231"
                global="false"
                containingComplexType="C219"
                nillable="false"
                abstract="false"/>
   <scm:wildcard id="C230" processContents="lax" constraint="enumeration"
                 namespaces="http://www.w3.org/1999/xhtml"/>
   <scm:element id="C197" name="owmskern" targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C232"
                global="false"
                containingComplexType="C191"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C232" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C162"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C174"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C165"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C178"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C132"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C188"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C181"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C157"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C162" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C174" to="2"/>
         </scm:state>
         <scm:state nr="2">
            <scm:edge term="C165" to="3"/>
         </scm:state>
         <scm:state nr="3">
            <scm:edge term="C178" to="4"/>
         </scm:state>
         <scm:state nr="4">
            <scm:edge term="C132" to="5"/>
         </scm:state>
         <scm:state nr="5">
            <scm:edge term="C188" to="6"/>
         </scm:state>
         <scm:state nr="6" final="true">
            <scm:edge term="C181" to="7"/>
            <scm:edge term="C157" to="8"/>
         </scm:state>
         <scm:state nr="7" final="true">
            <scm:edge term="C157" to="8"/>
         </scm:state>
         <scm:state nr="8" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:complexType id="C143" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C233"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C190"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C166"/>
         </scm:sequence>
      </scm:modelGroupParticle>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0">
            <scm:edge term="C190" to="1"/>
         </scm:state>
         <scm:state nr="1">
            <scm:edge term="C166" to="2"/>
         </scm:state>
         <scm:state nr="2" final="true"/>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:attribute id="C233" name="owms-version" type="#token" global="false"
                  containingComplexType="C143">
      <scm:fixed>
         <scm:item type="#string" value="3.5"/>
      </scm:fixed>
   </scm:attribute>
   <scm:attribute id="C49" name="scheme" type="C23" global="false" containingComplexType="C48"/>
   <scm:complexType id="C231" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="mixed">
      <scm:elementWildcard minOccurs="0" maxOccurs="unbounded" ref="C234"/>
      <scm:finiteStateMachine initialState="0">
         <scm:state nr="0" final="true">
            <scm:edge term="C234" to="1"/>
         </scm:state>
         <scm:state nr="1" final="true">
            <scm:edge term="C234" to="1"/>
         </scm:state>
      </scm:finiteStateMachine>
   </scm:complexType>
   <scm:element id="C104" name="end" type="C90" global="false" containingComplexType="C102"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C50" name="resourceIdentifier" type="C70" global="false"
                  containingComplexType="C48"/>
   <scm:attribute id="C11" name="scheme" type="C52" global="false" containingComplexType="C10"/>
   <scm:attribute id="C60" name="scheme" type="C42" global="false" containingComplexType="C59"/>
   <scm:element id="C92" name="start" type="C79" global="false" containingComplexType="C91"
                nillable="false"
                abstract="false"/>
   <scm:wildcard id="C234" processContents="lax" constraint="enumeration"
                 namespaces="http://www.w3.org/1999/xhtml"/>
   <scm:attribute id="C114" name="scheme" type="C123" global="false" containingComplexType="C113"/>
   <scm:attribute id="C77" name="scheme" type="C54" global="false" containingComplexType="C76"/>
   <scm:element id="C217" name="verwijzingProduct"
                targetNamespace="http://standaarden.overheid.nl/vac/"
                type="C95"
                global="false"
                containingComplexType="C167"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C96" name="resourceIdentifier" type="#anyURI" global="false"
                  containingComplexType="C95"/>
   <scm:attribute id="C110" name="scheme" type="C129" global="false" containingComplexType="C109"/>
   <scm:attribute id="C39" name="scheme" type="C72" global="false" containingComplexType="C38"/>
</scm:schema>