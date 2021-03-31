<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test promotion of document-dependent subexpressions in a filter for 
       a single-document node-set -->

  <xsl:template match="/">
    <out>
      <xsl:copy-of select="//section[number(/doc/@nr)]"/>
    </out>
  </xsl:template>

</xsl:stylesheet>