<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                exclude-result-prefixes="xs"
                version="3.0">

<xsl:param name="use-sef" select="'false'"/>

<xsl:template match="/" name="xsl:initial-template">
  <xsl:variable name="style"
                select="if ($use-sef != 'false')
                        then 'src/test/java/iss5701/style.sef.xml'
                        else 'src/test/java/iss5701/style.xsl'"/>

  <xsl:sequence select="transform(map {
      'stylesheet-location': $style,
      'source-node': .
      })?output"/>
</xsl:template>

</xsl:stylesheet>
