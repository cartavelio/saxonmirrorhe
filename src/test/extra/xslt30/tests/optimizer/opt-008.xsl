<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test non-promotion of document-dependent subexpressions in a filter for 
       a multi-document node-set -->

  <xsl:param name="p" select="//section"/>

  <xsl:template match="/">
    <out>
      <xsl:copy-of select="$p[number(/doc/@nr)]"/>
    </out>
  </xsl:template>

</xsl:stylesheet>