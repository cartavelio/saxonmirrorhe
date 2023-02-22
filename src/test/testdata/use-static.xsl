<xsl:transform
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema"
 exclude-result-prefixes="xs"
 version="3.0">
  
  <xsl:param name="date" as="xs:string" static="yes" required="yes"/>
  <xsl:param name="now" as="xs:time" static="yes" required="yes"/>
  <xsl:param name="ext" as="document-node()?" required="no"/>

<xsl:template match="*">
  <xsl:copy>
    <xsl:copy-of select="@*"/>
    <xsl:attribute name="date" use-when="xs:date($date) gt xs:date('2000-01-01')" select="$date"/>
    <xsl:attribute name="time" select="$now"/>
    <xsl:attribute name="ext-root" select="$ext/*/name()"/>
    <xsl:apply-templates/>
  </xsl:copy>
</xsl:template>

</xsl:transform>	
