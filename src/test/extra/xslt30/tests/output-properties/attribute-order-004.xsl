<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">
  
  <!-- PURPOSE:  Test saxon:attribute-order with saxon:serialize -->
  <?dependency saxon?>
  
  <xsl:output method="text"/>
  
  <xsl:template name="main">
    <xsl:variable name="v">
      <out>
        <a id="x" name="y" alpha="z" beta="w" xml:space="preserve" gamma="g" />
        <a xs:extra="4" gamma="g" alpha="z" beta="w" id="x" name="y" xml:space="preserve" java:jam="j"/>
        <a xml:space="preserve" alpha="z" xs:extra="4" gamma="g" beta="w" id="x" name="y"/>
        <a b="3" c="4"/>
      </out>
    </xsl:variable>
    <xsl:value-of select="serialize($v, map{'method':'xml', 'indent': true(),
      xs:QName('saxon:attribute-order'): (xs:QName('id'), xs:QName('name'), xs:QName('alpha'),
      xs:QName('beta'), '*', xs:QName('xml:space'))})"/>
  </xsl:template>
</xsl:stylesheet>