<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:date-time -->

<xsl:template match="/">
<out>
   <xsl:value-of select="date:date-time() eq string(current-dateTime())"/>
</out>

</xsl:template>
</xsl:stylesheet>
