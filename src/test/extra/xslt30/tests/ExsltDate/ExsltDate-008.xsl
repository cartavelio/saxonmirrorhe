<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:day-in-month (with argument) -->

<xsl:template match="/">
<out>
VALID FORMATS
'<xsl:value-of select="date:day-in-month('2001-02-03T00:00:00Z')"/>'
'<xsl:value-of select="date:day-in-month('2001-02-04T00:00:00')"/>'
'<xsl:value-of select="date:day-in-month('2001-02-05Z')"/>'
'<xsl:value-of select="date:day-in-month('--02-06Z')"/>'
'<xsl:value-of select="date:day-in-month('--04-30Z')"/>'
'<xsl:value-of select="date:day-in-month('---07Z')"/>'
INVALID FORMATS
'<xsl:value-of select="date:day-in-month('00:00:00Z')"/>'
'<xsl:value-of select="date:day-in-month('2001-02Z')"/>'
'<xsl:value-of select="date:day-in-month('2001Z')"/>'
INVALID DATES
'<xsl:value-of select="date:day-in-month('2001-01-01T00:00:61Z')"/>'
'<xsl:value-of select="date:day-in-month('2001-01-01Txx:xx:00Z')"/>'
'<xsl:value-of select="date:day-in-month('2001-02-29T00:00:00Z')"/>'
'<xsl:value-of select="date:day-in-month('2001-02-29Z')"/>'
'<xsl:value-of select="date:day-in-month('--04-31Z')"/>'
'<xsl:value-of select="date:day-in-month('---32Z')"/>'
</out>

</xsl:template>
</xsl:stylesheet>
