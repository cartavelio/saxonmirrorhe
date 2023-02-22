<xsl:transform
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 version="3.0"
>

<!-- This style sheet displays the books.xml file using the JSON serialization method.  -->

<xsl:output method="json"/>

<xsl:template match="/">

  <xsl:variable name="maps" as="map(*)*">
    <xsl:for-each select="//ITEM">
      <xsl:map>
        <xsl:map-entry key="'title'" select="string(TITLE)"/>
        <xsl:map-entry key="'author'" select="string(AUTHOR)"/>
        <xsl:map-entry key="'price'" select="number(PRICE)"/>
        <xsl:map-entry key="'dimensions'" select="array{tokenize(DIMENSIONS) ! number()}"/>
      </xsl:map>
    </xsl:for-each>
  </xsl:variable>
  <xsl:sequence select="array{$maps}"/>
</xsl:template>

</xsl:transform>