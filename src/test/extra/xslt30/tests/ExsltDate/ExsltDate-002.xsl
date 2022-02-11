<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:date (no arguments) -->

<xsl:template match="/">
<out>
<xsl:value-of select="date:date() eq string(current-date())"/>
</out>

</xsl:template>
</xsl:stylesheet>
