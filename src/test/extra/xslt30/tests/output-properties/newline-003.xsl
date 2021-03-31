<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

  <!-- PURPOSE:  Test saxon:newline with text output -->
  <?dependency saxon?>
  
  <xsl:output method="text" saxon:newline="&#xd;&#xa;"/>
  
  <xsl:template match="/" name="xsl:initial-template">
      <xsl:value-of select="(1 to 5)!('Line ' || . || '.')" separator="&#xa;"/>
  </xsl:template>
</xsl:stylesheet>
