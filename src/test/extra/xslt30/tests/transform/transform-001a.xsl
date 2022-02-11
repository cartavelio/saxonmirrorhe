<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns:f="http://ns.com/f"
				exclude-result-prefixes="saxon xs f">
				
				<!-- simple stylesheet for use by transform002 -->
				
	<xsl:param name="p" as="xs:integer" select="0"/>
	<xsl:param name="f:q" as="xs:integer" select="1"/>			
				
	<xsl:template match="/">
        <out>
           <xsl:value-of select="(in + $p) * $f:q"/>
        </out>
	</xsl:template>
	
</xsl:stylesheet>
