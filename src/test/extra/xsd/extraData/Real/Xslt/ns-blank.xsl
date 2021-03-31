<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:exsl="http://exslt.org/common"
                exclude-result-prefixes="exsl"
                version="1.0">

<xsl:import href="../style/data-model.xsl"/>

<xsl:param name="specdoc" select="'SomeNS'"/>

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

<xsl:template match="rddl:resource"
	      xmlns:xlink="http://www.w3.org/1999/xlink"
	      xmlns:rddl="http://www.rddl.org/">
  <rddl:resource>
    <xsl:copy-of select="@*"/>
    <xsl:apply-templates/>
  </rddl:resource>
</xsl:template>

</xsl:stylesheet>
