<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

  <!-- PURPOSE:  Test saxon:single-quotes with saxon:serialize -->
  <?dependency saxon?>

  <xsl:output method="text"/>

  <xsl:template name="main">
    <xsl:variable name="v">
      <out>
        <a id="x" name="y" alpha="O'Connor" beta='w' xml:space="preserve" gamma="g" xmlns="http://saxon.sf.net"/>
      </out>
    </xsl:variable>
    <xsl:value-of select="serialize($v, map{'method':'xml', xs:QName('saxon:single-quotes'): true()})"/>
  </xsl:template>
</xsl:stylesheet>
