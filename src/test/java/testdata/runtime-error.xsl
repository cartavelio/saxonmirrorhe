<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
xsi:schemaLocation="http://www.w3.org/1999/XSL/Transform xslt20.xsd"
version="2.0">
<xsl:template match="/">
  <xsl:variable name="attribute-item" as="attribute()">
    <xsl:attribute name="foo" select="'bar'" />
  </xsl:variable>
  <xsl:variable name="attribute-var" as="item()*" select="$attribute-item,root-element,$attribute-item"/> 
  <result>
    <child-node/>
    <!--  Produce a runtime error (two references to the variable to prevent inlining) -->
    <xsl:sequence select="$attribute-var, $attribute-var"/>
  </result>
</xsl:template>
</xsl:stylesheet>
