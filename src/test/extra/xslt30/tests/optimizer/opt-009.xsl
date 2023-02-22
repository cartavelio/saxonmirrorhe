<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test promotion of key() subexpression in a filter for 
       a single-document node-set -->

  <xsl:key name="k" match="para" use="@code"/>

  <xsl:template match="/">
    <out>
      <xsl:copy-of select="//section[@nr = key('k', 'C')/@ref]"/>
    </out>
  </xsl:template>

</xsl:stylesheet>