<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:ex="http://example.com/ns/examples"
                exclude-result-prefixes="xs"
                version="3.0">

  <xsl:character-map name="ex:map">
    <xsl:output-character character="!" string="?" />
    <xsl:output-character character="«" string="&lt;" />
  </xsl:character-map>

<xsl:output method="xml" encoding="utf-8" indent="no"
            use-character-maps="ex:map ex:map"/>

<xsl:template match="/">
  <doc>Hello, world!</doc>
</xsl:template>

</xsl:stylesheet>
