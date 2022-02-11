<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:date (with argument) -->

<xsl:template match="/">
<out>
VALID DATES
'<xsl:value-of select="date:date('2001-01-01Z')"/>'
'<xsl:value-of select="date:date('2001-01-01')"/>'
INVALID DATES
'<xsl:value-of select="date:date('2001-02-29Z')"/>'
'<xsl:value-of select="date:date('2001-01-o1Z')"/>'
'<xsl:value-of select="date:date('00:00:00Z')"/>'
'<xsl:value-of select="date:date('2001-02Z')"/>'
'<xsl:value-of select="date:date('2001Z')"/>'
VALID DATETIMES
'<xsl:value-of select="date:date('2001-02-03T00:00:00Z')"/>'
'<xsl:value-of select="date:date('2001-02-03T23:59:59Z')"/>'
'<xsl:value-of select="date:date('2001-02-03T00:00:00')"/>'
INVALID DATETIMES
'<xsl:value-of select="date:date('2001-02-29T00:00:00Z')"/>'
'<xsl:value-of select="date:date('2001-01-01T00:00:61Z')"/>'
'<xsl:value-of select="date:date('2001-01-01T00:0o:00Z')"/>'
</out>

</xsl:template>
</xsl:stylesheet>
