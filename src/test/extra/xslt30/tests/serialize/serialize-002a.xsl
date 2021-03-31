<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:o="http://o/"
  exclude-result-prefixes="o xsl" expand-text="yes">

  <!-- PURPOSE:  Test saxon:serialize with xsl:output declaration as second arg -->
  <?dependency saxon?>
  
  <xsl:namespace-alias stylesheet-prefix="o" result-prefix="xsl"/>

  <xsl:variable name="one" as="element(*)">
    <o:output method="text"/>
  </xsl:variable>
  <xsl:variable name="two" as="element(*)">
    <o:output method="html"/>
  </xsl:variable>
  <xsl:variable name="three" as="element(*)">
    <o:output method="xml"/>
  </xsl:variable>
  <xsl:variable name="four" as="element(*)">
    <o:output method="xml" omit-xml-declaration="yes"/>
  </xsl:variable>

</xsl:stylesheet>
