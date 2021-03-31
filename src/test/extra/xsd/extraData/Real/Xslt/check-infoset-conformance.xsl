<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:xs="http://www.w3.org/2001/XMLSchema"
                version="2.0">

<xsl:output method="text"/>

<xsl:param name="id" select="'infoset-conformance'"/>

<xsl:param name="missing-items"
	   select="('information item',
                    'unexpanded entity reference information item',
		    'document type declaration information item',
		    'processing instruction')"/>

<xsl:param name="missing-props"
	   select="('infoset property',
		    'declaration base uri')"/>

<xsl:template match="/">
  <xsl:variable name="appendix" select="//*[@id=$id]"/>
  <xsl:variable name="items" as="xs:string*">
    <xsl:for-each select="//emph[@role='info-item']">
      <xsl:choose>
	<xsl:when test="ends-with(.,'s')">
	  <xsl:value-of select="normalize-space(lower-case(substring(.,1,string-length(.)-1)))"/>
	</xsl:when>
	<xsl:otherwise>
	  <xsl:value-of select="normalize-space(lower-case(.))"/>
	</xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>

  <xsl:variable name="props" as="xs:string*">
    <xsl:for-each select="//emph[@role='infoset-property' and not(ancestor::div3/head='Infoset Mapping')]">
      <xsl:choose>
	<xsl:when test="ends-with(.,'s')">
	  <xsl:value-of select="normalize-space(lower-case(substring(.,1,string-length(.)-1)))"/>
	</xsl:when>
	<xsl:otherwise>
	  <xsl:value-of select="normalize-space(lower-case(.))"/>
	</xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>

  <xsl:for-each select="distinct-values($items)">
    <xsl:variable name="item" select="."/>
    <xsl:choose>
      <xsl:when test="$appendix//emph[@role='info-item'
		                      and normalize-space(lower-case(.)) = $item]"/>
      <xsl:when test="$appendix//emph[@role='info-item'
		                      and normalize-space(lower-case(.)) = concat($item,'s')]"/>
      <xsl:when test="$item = $missing-items"/>
      <xsl:otherwise>
	<xsl:text>Missing info-item: </xsl:text>
	<xsl:value-of select="$item"/>
	<xsl:text>&#10;</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:for-each>

  <xsl:for-each select="distinct-values($props)">
    <xsl:variable name="prop" select="."/>
    <xsl:choose>
      <xsl:when test="$appendix//emph[@role='infoset-property'
		                      and normalize-space(lower-case(.)) = $prop]"/>
      <xsl:when test="$appendix//emph[@role='infoset-property'
		                      and normalize-space(lower-case(.)) = concat($prop,'s')]"/>
      <xsl:when test="$prop = $missing-props"/>
      <xsl:otherwise>
	<xsl:text>Missing infoset-property: </xsl:text>
	<xsl:value-of select="$prop"/>
	<xsl:text>&#10;</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:for-each>

</xsl:template>

</xsl:stylesheet>
