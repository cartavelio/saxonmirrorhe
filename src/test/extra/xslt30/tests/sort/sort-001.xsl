<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns="http://www.w3.org/1999/xhtml"
				exclude-result-prefixes="saxon xs">
				
				<!-- test saxon:sort with one argument on atomic values -->
				
	<xsl:output method="xhtml" encoding="UTF-8" omit-xml-declaration="yes" indent="yes" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/" name="main">
	<z>
    <out><xsl:value-of select="saxon:sort((8, 4, 5, 2, 0, 9))"/></out>
    <out><xsl:value-of select="saxon:sort((8.1, 4.5, 5.6, 2.2, 0.1, 9.7))"/></out>
    <out><xsl:value-of select="saxon:sort(('x', 'ert', 'yh', 'wgh'))"/></out>
  </z>  
	</xsl:template>
	
	<xsl:function name="saxon:sort">
		<xsl:param name="in" as="item()*"/>
		<xsl:sequence select="sort($in)"/>
	</xsl:function>
	
	<xsl:function name="saxon:sort">
		<xsl:param name="in" as="item()*"/>
		<xsl:param name="by" as="function(*)"/>
		<xsl:sequence select="sort($in, (), $by)"/>
	</xsl:function>
	
</xsl:stylesheet>
