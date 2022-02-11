<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:year (with argument) -->

<xsl:template match="/">
<out>
VALID FORMATS
'<xsl:value-of select="date:year('2001-02-03T00:00:00Z')"/>'
'<xsl:value-of select="date:year('2002-02-04T00:00:00')"/>'
'<xsl:value-of select="date:year('2003-02-05Z')"/>'
'<xsl:value-of select="date:year('2004-02Z')"/>'
'<xsl:value-of select="date:year('2005Z')"/>'
'<xsl:value-of select="date:year('-2005Z')"/>'
'<xsl:value-of select="date:year('0001Z')"/>'
INVALID FORMATS
'<xsl:value-of select="date:year('00:00:00Z')"/>'
'<xsl:value-of select="date:year('--02-06Z')"/>'
'<xsl:value-of select="date:year('---07Z')"/>'
INVALID DATES
'<xsl:value-of select="date:year('2001-01-01T00:00:61Z')"/>'
'<xsl:value-of select="date:year('2001-01-01Txx:xx:00Z')"/>'
'<xsl:value-of select="date:year('2001-02-29T00:00:00Z')"/>'
'<xsl:value-of select="date:year('2001-02-29Z')"/>'
'<xsl:value-of select="date:year('2004-13Z')"/>'
'<xsl:value-of select="date:year('2x04Z')"/>'
'<xsl:value-of select="date:year('1Z')"/>'
'<xsl:value-of select="date:year('0000Z')"/>'
</out>

</xsl:template>
</xsl:stylesheet>
