<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs">
				
				<!-- test for saxon:transform supplying a parameter in the form of a map -->
				
				
	<xsl:template name="main">
	    <xsl:variable name="params" as="map(*)">
	      <xsl:map>
	        <xsl:map-entry key="'p'" select="19"/>
	        <xsl:map-entry key="QName('http://ns.com/f', 'q')" select="2"/>
	      </xsl:map>  
	    </xsl:variable>
        <result>
           <xsl:variable name="stylesheet" select="saxon:compile-stylesheet(doc('transform-001a.xsl'))"/>
           <xsl:copy-of select="saxon:transform($stylesheet, 
                                                doc('transform-001a.xml'),
                                                $params)"/>
        </result>
	</xsl:template>
	
</xsl:stylesheet>
