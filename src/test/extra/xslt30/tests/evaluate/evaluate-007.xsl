<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon xs">

<!-- PURPOSE:  Test saxon:evaluate() with variables -->
<!-- Formerly test saxon070 -->



<xsl:template match="/">
<out>
<xsl:value-of select="saxon:evaluate(add, 6, 7)" />
</out>
</xsl:template>
</xsl:stylesheet>