<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs">
				
				<!-- test for saxon:transform supplying a parameter -->
				
				
	<xsl:template name="main">
	  <xsl:variable name="config" select="doc('t-100/config.xml')" />
	  <xsl:variable as="map(*)" name="options"
	    select="
	    map {
	    'stylesheet-location': 'transform-100-target.xsl',
	    'initial-template': xs:QName('xsl:initial-template'),
	    'vendor-options':
	    map {
	    QName('http://saxon.sf.net/', 'configuration'): $config
	    }
	    }" />
	  <xsl:sequence select="transform($options)?output" />
	</xsl:template>
	
</xsl:stylesheet>
