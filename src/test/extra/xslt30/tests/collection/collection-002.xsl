<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="xs">

<!-- test of directory-based collection with whitespace stripping -->
  
  <xsl:strip-space elements="white"/>

<xsl:template name="main">
<out>
  <xsl:copy-of select="collection('.?select=*.xml')/*"/>
</out>     
</xsl:template>

</xsl:stylesheet>
  