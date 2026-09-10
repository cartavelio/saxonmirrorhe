<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns:q="urn:q01"
				exclude-result-prefixes="saxon xs q">
				
				<!-- test for saxon:import-query -->
	<!-- Reference to private variable fails -->
	
	<saxon:import-query namespace="urn:q01" href="q01.xq"/>
								
	<xsl:template name="main">
	    <out>
	    	<var><xsl:copy-of select="$q:VAR2"/></var>
	    </out>
	</xsl:template>
	
</xsl:stylesheet>
