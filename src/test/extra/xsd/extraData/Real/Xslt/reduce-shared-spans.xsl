<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:output method="html"/>
  <xsl:variable name="strip-all" select="true()"/>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="span[@class='xquery' or @class='xpath']" priority="10">
    <xsl:variable name="class" select="@class"/>
    <xsl:choose>
      <xsl:when test="$strip-all or ancestor::*[@class = $class]">          
        <xsl:apply-templates select="@* | node()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy>
          <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="@class[.='xquery' or .='xpath']" priority="10">
    <xsl:variable name="class" select="."/>
    <xsl:choose>
      <xsl:when test="$strip-all or (($class='xquery' or $class='xpath') and (../ancestor::*[@class = $class]))">          
        <!-- no attribute copy -->
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>

