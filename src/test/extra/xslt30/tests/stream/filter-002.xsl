<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:saxon="http://saxon.sf.net/"
	exclude-result-prefixes="#all"
	expand-text="yes"
	version="3.0">
	
	<!-- Same as -001 but with an inline function that can be inferred to have streamability=inspection -->
	
	<xsl:mode streamable="yes"/>
	<xsl:template match="/">
		<out xmlns:s="http://copycorp/">
			<xsl:copy-of select="/*/customer => filter(function($e){$e/@id = 'ccc'})"/>
		</out>	
	</xsl:template>
	
	
</xsl:stylesheet>