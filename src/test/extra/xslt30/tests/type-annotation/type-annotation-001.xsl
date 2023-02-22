<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs"
				saxon:allow-all-built-in-types="yes">
				
				<!-- test type annotation on atomic values -->
				
<xsl:output indent="yes"/>				
<xsl:template name="main">
<out>
 <int v="{saxon:type-annotation(3)}"/>
 <bool v="{saxon:type-annotation(true())}"/>
 <string v="{saxon:type-annotation('banana')}"/>
 <qname v="{saxon:type-annotation(xs:QName('saxon:giraffe'))}"/>
 <id v="{saxon:type-annotation(xs:ID('unique'))}"/>
</out>
</xsl:template>
	
</xsl:stylesheet>
