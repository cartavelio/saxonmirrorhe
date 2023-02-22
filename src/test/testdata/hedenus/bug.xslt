<?xml version="1.0" encoding="UTF-8"?>


<xsl:stylesheet version="2.0"
	xmlns:xsl   = "http://www.w3.org/1999/XSL/Transform"
	xmlns:xs    = "http://www.w3.org/2001/XMLSchema" 
	xmlns:saxon = "http://saxon.sf.net/"
	xmlns:n     = "saxon:bug">
	
	<saxon:import-query href="bug.xq" namespace="saxon:bug"/>
	
	<xsl:param name="myparam"/>
	
	<xsl:output method="xml" version="1.0" encoding="windows-1252" indent="no"/>
	
	<xsl:template match="/">
		<result>
			<xsl:copy-of select="n:get-a($myparam)"/>
		</result>
	</xsl:template>
	
</xsl:stylesheet>