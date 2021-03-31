<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns:t="http://www.typeannotation003.uri/"
				xmlns:f="http://www.typeannotation003.uri/"
				exclude-result-prefixes="saxon xs t f">
				
				<!-- test type annotation on schema-validated node -->
	
<xsl:output indent="yes"/>

<xsl:import-schema>
  <xs:schema targetNamespace="http://www.typeannotation003.uri/" >
  <xs:element name="ele" type="t:eletype"/>
  <xs:complexType name="eletype">
    <xs:simpleContent>
      <xs:extension base="xs:integer">
        <xs:attribute name="att" type="t:atttype"/>
      </xs:extension>     
    </xs:simpleContent>    
  </xs:complexType>
  <xs:simpleType name="atttype">
    <xs:restriction base="xs:double"/>
  </xs:simpleType>
  </xs:schema>
</xsl:import-schema>				
				
<xsl:template name="main">
 <xsl:variable name="data" as="document-node(element(*, t:eletype))">
   <xsl:document validation="strict">
     <t:ele att="93.7">42</t:ele>
   </xsl:document>
 </xsl:variable>
<out>
<nodes>
 <doc v="{f:annotation($data)}"/>
  <el v="{f:annotation($data/t:ele)}"/>
  <at v="{f:annotation($data/t:ele/@att)}"/>
</nodes>
<atomics>
  <doc v="{f:annotation(data($data))}"/>
  <el v="{f:annotation(data($data/t:ele))}"/>
  <at v="{f:annotation(data($data/t:ele/@att))}"/>
</atomics>
</out>
</xsl:template>
  
  <xsl:function name="f:annotation" as="xs:string">
    <xsl:param name="arg"/>
    <xsl:variable name="v" select="saxon:type-annotation($arg)"/>
    <xsl:sequence select="'Q{' || namespace-uri-from-QName($v) || '}' || local-name-from-QName($v)"/>
  </xsl:function>
	
</xsl:stylesheet>
