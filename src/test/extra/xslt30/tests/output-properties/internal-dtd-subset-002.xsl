<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:f="f"
  exclude-result-prefixes="saxon xs f">

  <!-- PURPOSE:  Test saxon:internal-dtd-subset with fn:serialize -->
  <?dependency saxon?>
  
  <xsl:function name="f:elementDecl" as="xs:string">
    <xsl:param name="name" as="xs:string"/>
    <xsl:param name="model" as="xs:string"/>
    <xsl:sequence select="'&lt;!ELEMENT ' || $name || ' ' || $model || '>&#xa;'"/>
  </xsl:function>
  
  <xsl:function name="f:attListDecl" as="xs:string">
    <xsl:param name="elemName" as="xs:string"/>
    <xsl:param name="attName" as="xs:string"/>
    <xsl:param name="attType" as="xs:string"/>
    <xsl:param name="attQualifier" as="xs:string"/>
    <xsl:sequence select="'&lt;!ATTLIST ' || $elemName || ' ' || $attName || ' ' || $attType || ' ' || $attQualifier || '>&#xa;'"/>
  </xsl:function>
  

  <xsl:variable name="serParams" as="map(*)">
    <xsl:map>
      <xsl:map-entry key="QName('http://saxon.sf.net/', 'internal-dtd-subset')">
        <xsl:sequence select="f:elementDecl('out', '(a)*')"/>
        <xsl:sequence select="f:elementDecl('a', 'EMPTY')"/>
        <xsl:sequence select="f:attListDecl('a', 'id', 'ID', '#REQUIRED')"/>
      </xsl:map-entry>
    </xsl:map>
  </xsl:variable>

  <xsl:template name="main">
    <xsl:variable name="doc">
      <out>
        <a id="a01"/>
        <a id="a02"/>
      </out>
    </xsl:variable>
    <xsl:variable name="ser" select="serialize($doc, $serParams)"/>
    <result>
      <xsl:value-of select="deep-equal($doc, parse-xml($ser))"/>
    </result>
  </xsl:template>
</xsl:stylesheet>
