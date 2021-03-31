<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:f="http://localhost/functions"
  exclude-result-prefixes="f">

  <!-- test of serial copy retrieving attributes -->
  
  <xsl:output indent="yes"/>
  <xsl:strip-space elements="*"/>
  
  <xsl:template name="main">
    <out>
      <xsl:for-each select="f:atts()">
        <att name="{name()}" value="{.}"/>
      </xsl:for-each>
    </out>
  </xsl:template>
  
  <xsl:function name="f:atts">
    <xsl:sequence select="saxon:stream(doc('xserial002.xml')//@*)" xmlns:saxon="http://saxon.sf.net/"/>
  </xsl:function>

</xsl:stylesheet>