<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:seconds with datetime argument -->

<xsl:template match="/">
<out>
'<xsl:value-of select="date:seconds('2008-04-15Z')"/>'
'<xsl:value-of select="date:seconds('1970-01-01Z')"/>'
'<xsl:value-of select="date:seconds('1970-01-01T00:00:00.001Z')"/>'
'<xsl:value-of select="date:seconds('1969-12-31T23:59:59Z')"/>'
'<xsl:value-of select="date:seconds('1969-12-31Z')"/>'
'<xsl:value-of select="date:seconds('1970-01-02+06:00')"/>'
'<xsl:value-of select="date:seconds('1969-12-31T23:00:00-02:00')"/>'
'<xsl:value-of select="date:seconds('2008-04-15x')"/>'
'<xsl:value-of select="date:seconds('2007-02-29Z')"/>'
</out>

</xsl:template>
</xsl:stylesheet>
