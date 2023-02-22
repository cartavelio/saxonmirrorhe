<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:seconds with duration argument -->

<xsl:template match="/">
<out>
The duration 'PT123S' in seconds is: <xsl:value-of select="date:seconds('PT123S')"/>
The duration 'PT1.04S' in seconds is: <xsl:value-of select="date:seconds('PT1.04S')"/>
The duration 'PT0.04S' in seconds is: <xsl:value-of select="date:seconds('PT0.04S')"/>
The duration '-PT1S' in seconds is: <xsl:value-of select="date:seconds('-PT1S')"/>
The duration 'P0M' in seconds is: <xsl:value-of select="date:seconds('P0M')"/>
The duration 'PT0H' in seconds is: <xsl:value-of select="date:seconds('PT0H')"/>
The duration 'PT0S' in seconds is: <xsl:value-of select="date:seconds('PT0S')"/>
The duration 'P1DT1H1M1S' in seconds is: <xsl:value-of select="date:seconds('P1DT1H1M1S')"/>
The duration 'P0Y0M1DT1H1M1S' in seconds is: <xsl:value-of select="date:seconds('P0Y0M1DT1H1M1S')"/>
The duration 'P0Y0M1DT25H61M61S' in seconds is: <xsl:value-of select="date:seconds('P0Y0M1DT25H61M61S')"/>
The duration 'P1M' in seconds is: <xsl:value-of select="date:seconds('P1M')"/>
The duration 'P1Y' in seconds is: <xsl:value-of select="date:seconds('P1Y')"/>
The invalid duration 'P1x' in seconds is: <xsl:value-of select="date:seconds('P1x')"/>
<xsl:text>
</xsl:text>
</out>

</xsl:template>
</xsl:stylesheet>
