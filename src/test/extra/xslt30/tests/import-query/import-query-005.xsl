<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns:q="urn:q01"
				exclude-result-prefixes="saxon xs q">
				
				<!-- test for saxon:import-query -->
	<!-- Module URI must match -->
	
	<saxon:import-query namespace="urn:q01xyz" href="q01.xq"/>
								
	<xsl:template name="main">
	    <out>
	    	<fun/>
	    </out>
	</xsl:template>
	
</xsl:stylesheet>
