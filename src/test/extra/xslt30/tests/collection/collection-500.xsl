<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:f="file:///local-functions"
  exclude-result-prefixes="xs f"
  version="2.0">
  
  <!-- link checker for the user documentation. Uses user-doc/catalog.xml as the principal source document. -->
  
  <!-- incomplete, not in use = MHK 2020-03-12 -->
  
  <xsl:template match="/">
    <xsl:variable name="catalog" select="."/>
    <xsl:variable name="docs" select="/*/*:section/@ref[not(. = ('javadoc', 'dotnetdoc'))] ! (. || '.xml') ! document(., $catalog)"/>
    <xsl:variable name="targets" select="$docs//(article|section) ! f:hierarchic-id(.)"/>
    <xsl:variable name="links" select="distinct-values($docs//a[@class='bodylink']/@href ! f:expand())"/>
    <xsl:variable name="internal-links" select="$links[not(starts-with(., 'http'))]"/>
    <out>
      <targets><xsl:value-of select="$targets"/></targets>
      <links><xsl:value-of select="$internal-links[not (. = $targets)]"/></links>
    </out>
  </xsl:template>
  
  <xsl:function name="f:hierarchic-id" as="xs:string">
    <xsl:param name="node" as="node()"/>
    <xsl:sequence select="'/' || string-join($node/ancestor-or-self::*/@id, '/')"/>
  </xsl:function>
  
  <xsl:function name="f:expand" as="xs:string">
    <xsl:param name="href" as="attribute(href)"/>
    <xsl:choose>
      <xsl:when test="starts-with(@href, '/')">
        <xsl:sequence select="@href"/>
      </xsl:when>
      <xsl:when test="starts-with(@href, '..')">
        <xsl:variable name="tokens" select="tokenize(@href, '/')"/>
        <xsl:value-of>
          <xsl:for-each select="$tokens">
            <xsl:choose>
              <xsl:when test=". = '..'">
                <xsl:variable name="pos" select="count($tokens) - position()"/>
              </xsl:when>
            </xsl:choose>
          </xsl:for-each>
        </xsl:value-of>
      </xsl:when>
    </xsl:choose>
  </xsl:function>
  
  
</xsl:stylesheet>