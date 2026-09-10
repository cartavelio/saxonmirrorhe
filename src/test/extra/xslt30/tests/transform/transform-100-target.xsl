<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs">
				
				<!-- test for saxon:transform supplying a parameter -->
				
  <xsl:use-package name="http://example.org/package.xsl" version="1.0"/>		
		
	<xsl:template name="xsl:initial-template">
	  <xsl:call-template name="foo"/>
	</xsl:template>
	
</xsl:stylesheet>
