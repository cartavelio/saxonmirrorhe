<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs">
				
				<!-- test for saxon:xquery supplying parameters as a document node -->
				
				
	<xsl:template name="main">
	    <xsl:variable name="q1"><![CDATA[declare variable $x external; declare variable $y external; <z>{$x + $y + .}</z>]]></xsl:variable>
		<xsl:variable name="query" as="function(item()?, map(xs:QName, item()*)) as item()*"
	        select="saxon:xquery($q1)"/>
		<xsl:variable name="params" select="map{xs:QName('x'): 11, xs:QName('y'): 22}"/>      

        <result>
           <xsl:sequence select="$query(5, $params)"/>
        </result>
	</xsl:template>
	
</xsl:stylesheet>
