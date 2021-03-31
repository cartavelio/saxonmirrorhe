<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="1.0">

<xsl:output method="text"/>

<xsl:param name="status" select="'open'"/>

<xsl:template match="/">
  <xsl:choose>
    <xsl:when test="$status = ''">
      <xsl:apply-templates select="//issue"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:apply-templates select="//issue[@status = $status]"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template match="/test">
  <xsl:apply-templates select="//issue[@id='Issue-0033']"/>
</xsl:template>

<xsl:template match="issue">
  <xsl:text>--------------------------------------------------------------</xsl:text>
  <xsl:text>&#10;</xsl:text>

  <xsl:value-of select="@id"/>
  <xsl:text>: </xsl:text>
  <xsl:value-of select="head"/>
  <xsl:text>&#10;</xsl:text>
  <xsl:text>            Raised on: </xsl:text>
  <xsl:value-of select="@date"/>

  <xsl:if test="@raisedby">
    <xsl:text>   Raised by: </xsl:text>
    <xsl:value-of select="@raisedby"/>
  </xsl:if>

  <xsl:if test="$status = ''">
    <xsl:text>   Status: </xsl:text>
    <xsl:value-of select="@status"/>
  </xsl:if>

  <xsl:text>&#10;</xsl:text>

  <xsl:apply-templates select="*[not(self::head) and not(self::resolution)]"/>

  <xsl:text>&#10;</xsl:text>

  <xsl:if test="resolution">
    <xsl:text>Resolution:&#10;</xsl:text>
    <xsl:apply-templates select="resolution"/>
  </xsl:if>

  <xsl:text>&#10;</xsl:text>
</xsl:template>

<xsl:template match="p">
  <xsl:call-template name="wrap-text">
    <xsl:with-param name="text" select="normalize-space(.)"/>
  </xsl:call-template>
  <xsl:text>&#10;</xsl:text>
</xsl:template>

<xsl:template match="*">
  <xsl:apply-templates/>
</xsl:template>

<xsl:template name="wrap-text">
  <xsl:param name="text" select="''"/>
  <xsl:param name="width" select="60"/>

  <xsl:text>&#10;</xsl:text>
  <xsl:choose>
    <xsl:when test="string-length($text) &lt; $width">
      <xsl:value-of select="$text"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:variable name="line">
        <xsl:call-template name="leading-text">
          <xsl:with-param name="text" select="$text"/>
          <xsl:with-param name="width" select="$width"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:value-of select="$line"/>

      <xsl:call-template name="wrap-text">
        <xsl:with-param name="text" select="substring($text, string-length($line)+1)"/>
        <xsl:with-param name="width" select="$width"/>
      </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="leading-text">
  <xsl:param name="text" select="''"/>
  <xsl:param name="width" select="60"/>
  <xsl:param name="line" select="''"/>

  <xsl:variable name="pre-blank" select="substring-before($text, ' ')"/>
  <xsl:variable name="non-blank">
    <xsl:choose>
      <xsl:when test="$pre-blank = ''">
        <xsl:value-of select="$text"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pre-blank"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:choose>
    <xsl:when test="$non-blank = ''">
      <xsl:value-of select="$line"/>
    </xsl:when>
    <xsl:when test="string-length($line) + string-length($non-blank) &gt; $width">
      <xsl:choose>
        <xsl:when test="$line = ''">
          <xsl:value-of select="concat($non-blank,' ')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$line"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:otherwise>
      <xsl:call-template name="leading-text">
        <xsl:with-param name="text" select="substring-after($text, ' ')"/>
        <xsl:with-param name="width" select="$width"/>
        <xsl:with-param name="line" select="concat($line, $non-blank, ' ')"/>
      </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="copy-string">
  <!-- returns 'count' copies of 'string' -->
  <xsl:param name="string"/>
  <xsl:param name="count" select="0"/>
  <xsl:param name="result"/>

  <xsl:choose>
    <xsl:when test="$count&gt;0">
      <xsl:call-template name="copy-string">
        <xsl:with-param name="string" select="$string"/>
        <xsl:with-param name="count" select="$count - 1"/>
        <xsl:with-param name="result">
          <xsl:value-of select="$result"/>
          <xsl:value-of select="$string"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$result"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


</xsl:stylesheet>
