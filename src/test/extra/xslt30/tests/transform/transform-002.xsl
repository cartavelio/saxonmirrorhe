<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs">
				
				<!-- test for saxon:transform not supplying a parameter -->
				
				
	<xsl:template name="main">
        <result>
           <xsl:variable name="stylesheet" select="saxon:compile-stylesheet(doc('transform-001a.xsl'))"/>
           <xsl:copy-of select="saxon:transform($stylesheet, 
                                                doc('transform-001a.xml'))"/>
        </result>
	</xsl:template>
	
</xsl:stylesheet>
