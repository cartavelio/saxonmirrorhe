<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:add -->

<xsl:template match="/">
<out>
GENERAL
'<xsl:value-of select="date:add('2000-01-01Z', 'PT0S')"/>'
'<xsl:value-of select="date:add('2000-01-01Z', 'PT1S')"/>'
'<xsl:value-of select="date:add('2000-01-01Z', 'PT23H59M59S')"/>'
'<xsl:value-of select="date:add('2000-01-01T00:00:00Z', 'PT23H59M59S')"/>'
'<xsl:value-of select="date:add('2000-01-01Z', 'PT24H')"/>'
'<xsl:value-of select="date:add('2000-01-01Z', '-PT1H')"/>'
'<xsl:value-of select="date:add('2000-01-01', 'P1D')"/>'
ADDING MONTHS/YEARS
'<xsl:value-of select="date:add('2000-01-01', 'P1M')"/>'
'<xsl:value-of select="date:add('2000-01-01', '-P1M')"/>'
'<xsl:value-of select="date:add('2001-06-30', 'P1M')"/>'
'<xsl:value-of select="date:add('2001-05-31', 'P1M')"/>'
'<xsl:value-of select="date:add('2001-05-31', '-P1M')"/>'
'<xsl:value-of select="date:add('2001-01-31', 'P1M')"/>'
'<xsl:value-of select="date:add('2004-01-31', 'P1M')"/>'
'<xsl:value-of select="date:add('2001-02-28', '-P1M')"/>'
'<xsl:value-of select="date:add('2004-02-29', '-P1M')"/>'
'<xsl:value-of select="date:add('2000-01-01', 'P1Y')"/>'
'<xsl:value-of select="date:add('2004-02-29', 'P1Y')"/>'
EPOCH TESTS
'<xsl:value-of select="date:add('1970-01-01Z', '-PT1H')"/>'
'<xsl:value-of select="date:add('1970-01-01T00:00:00Z', '-PT1H')"/>'
'<xsl:value-of select="date:add('1969-12-31Z', 'P1D')"/>'
LEAP YEARS
'<xsl:value-of select="date:add('1996-02-28Z', 'P1D')"/>'
'<xsl:value-of select="date:add('1996-02-29Z', 'P1D')"/>'
'<xsl:value-of select="date:add('1997-02-28Z', 'P1D')"/>'
'<xsl:value-of select="date:add('1996-03-01Z', '-P1D')"/>'
'<xsl:value-of select="date:add('2000-02-28Z', 'P1D')"/>'
'<xsl:value-of select="date:add('1900-02-28Z', 'P1D')"/>'
OVERLOADED DURATIONS
'<xsl:value-of select="date:add('2000-01-01Z', 'PT48H')"/>'
INVALID INPUT
'<xsl:value-of select="date:add('2000-01-01Z', 'x48H')"/>'
'<xsl:value-of select="date:add('1997-02-29Z', 'P1D')"/>'
</out>

</xsl:template>
</xsl:stylesheet>
