<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:exsl="http://exslt.org/common"
                exclude-result-prefixes="exsl"
                version="1.0">

<xsl:import href="../style/xquery-operators.xsl"/>

<xsl:param name="specdoc" select="'ERRNS'"/>

<xsl:variable name="fando" select="document('xpath-functions.xml',.)"/>
<xsl:variable name="xslt" select="document('../xslt/xslt20.xml',.)"/>
<xsl:variable name="lang" select="document('../lang/XPathXQuery.xml',.)"/>

<xsl:template match="header">
  <div class="head">
    <p>
      <a href="http://www.w3.org/">
	<img src="http://www.w3.org/Icons/w3c_home" alt="W3C"
	     height="48" width="72"/>
      </a>
    </p>

    <h1>
      <a name="title" id="title"/>
      <xsl:value-of select="title"/>
    </h1>
  </div>
</xsl:template>

<xsl:template name="css">
  <style type="text/css">
    <xsl:value-of select="$additional.css"/>
  </style>
  <link rel="stylesheet" type="text/css">
    <xsl:attribute name="href">
      <xsl:text>http://www.w3.org/StyleSheets/TR/base.css</xsl:text>
    </xsl:attribute>
  </link>
</xsl:template>

<xsl:template match="code">
  <code>
    <xsl:apply-templates/>
  </code>
</xsl:template>

<xsl:template match="processing-instruction('error-summary')">
  <!-- collate them better -->
  <dl>
    <xsl:apply-templates select="$fando//error" mode="error-list">
      <xsl:with-param name="spec" select="'FO'"/>
    </xsl:apply-templates>

    <xsl:apply-templates select="$xslt//error" mode="error-list">
      <xsl:with-param name="spec" select="'XT'"/>
    </xsl:apply-templates>

    <xsl:for-each select="$lang//error">
      <xsl:variable name="role"
		    select="ancestor-or-self::*[(@role='xpath')
			                        or (@role='xquery')]"/>
      <xsl:choose>
	<xsl:when test="$role/@role = 'xquery'">
	  <xsl:apply-templates select="." mode="error-list">
	    <xsl:with-param name="spec" select="'XQ'"/>
	  </xsl:apply-templates>
	</xsl:when>
	<xsl:otherwise>
	  <xsl:apply-templates select="." mode="error-list">
	    <xsl:with-param name="spec" select="'XP'"/>
	  </xsl:apply-templates>
	</xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </dl>
</xsl:template>

<xsl:template match="error" mode="error-list">
  <xsl:param name="spec"/>

  <xsl:variable name="class">
    <xsl:choose>
      <xsl:when test="@class"><xsl:value-of select="@class"/></xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="code">
    <xsl:choose>
      <xsl:when test="@code"><xsl:value-of select="@code"/></xsl:when>
      <xsl:otherwise>??</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="type">
    <xsl:choose>
      <xsl:when test="@type"><xsl:value-of select="@type"/></xsl:when>
      <xsl:otherwise>??</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="label">
    <xsl:choose>
      <xsl:when test="@label"><xsl:value-of select="@label"/></xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$spec"/>
        <xsl:value-of select="$class"/>
        <xsl:value-of select="$code"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <dt>
    <a>
      <xsl:attribute name="href">
	<xsl:choose>
	  <xsl:when test="$spec = 'FO'">
	    <xsl:text>http://www.w3.org/TR/xpath-functions/#ERR</xsl:text>
	  </xsl:when>
	  <xsl:when test="$spec = 'XT'">
	    <xsl:text>http://www.w3.org/TR/xslt20/#err-</xsl:text>
	  </xsl:when>
	  <xsl:when test="$spec = 'XQ'">
	    <xsl:text>http://www.w3.org/TR/xquery/#ERR</xsl:text>
	  </xsl:when>
	  <xsl:otherwise>
	    <xsl:text>http://www.w3.org/TR/xpath20/#ERR</xsl:text>
	  </xsl:otherwise>
	</xsl:choose>
	<xsl:value-of select="$spec"/>
	<xsl:value-of select="$class"/>
	<xsl:value-of select="$code"/>
      </xsl:attribute>
      <xsl:text>err:</xsl:text>
      <xsl:value-of select="concat($spec, $class, $code)"/>
    </a>
  </dt>
</xsl:template>

</xsl:stylesheet>
