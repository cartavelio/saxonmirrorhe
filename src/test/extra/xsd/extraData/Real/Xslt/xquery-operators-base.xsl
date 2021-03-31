<?xml version="1.0"?>

<!-- Norm hacked this severely on 7 July. This used to be a direct copy of      -->
<!-- the base stylesheet. It isn't anymore. That's not going to break anything, -->
<!-- is it? -->

<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
               version="1.0">

  <xsl:import href="xsl-query.xsl"/>

  <xsl:param name="spec" select="'FO'"/>

  <xsl:template match="span">
    <xsl:copy>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>

</xsl:transform>
