<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:saxon="http://saxon.sf.net/"
	exclude-result-prefixes="#all"
	expand-text="yes"
	version="3.0">
	
	<xsl:mode streamable="yes"/>
	<xsl:template match="/">
		<out xmlns:s="http://copycorp/">
			<xsl:copy-of select="/*/customer => filter(Q{f}ccc#1)"/>
		</out>	
	</xsl:template>
	
	<xsl:function name="Q{f}ccc" as="xs:boolean" streamability="inspection">
		<xsl:param name="e" as="element()"/>
		<xsl:sequence select="$e/@id = 'ccc'"/>
	</xsl:function>
	
</xsl:stylesheet>