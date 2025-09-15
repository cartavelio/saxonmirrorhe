<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:ex="http://example.com/ns/examples"
                exclude-result-prefixes="xs"
                version="3.0">

  <xsl:character-map name="ex:map1">
    <xsl:output-character character="!" string="?" />
  </xsl:character-map>

  <xsl:character-map name="ex:map2">
    <xsl:output-character character="«" string="&lt;" />
  </xsl:character-map>

<xsl:output method="adaptive" encoding="utf-8" indent="no"
            use-character-maps="ex:map1 ex:map2 ex:map2"/>

<xsl:template match="/">
  <xsl:sequence>
    <doc>Hello, world!</doc>
    <xsl:sequence select="map{'greeting': 'Hello, world!'}"/>
  </xsl:sequence>
</xsl:template>

</xsl:stylesheet>
