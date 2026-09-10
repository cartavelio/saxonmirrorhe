<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs">
				
				<!-- test for saxon:query supplying parameters -->
				
				
	<xsl:template name="main">
	    <xsl:variable name="q1"><![CDATA[declare variable $x external; declare variable $y external; <z>{$x + $y + .}</z>]]></xsl:variable>
	    <xsl:variable name="query" as="java:net.sf.saxon.query.XQueryExpression" xmlns:java="http://saxon.sf.net/java-type"
	        select="saxon:compile-query($q1)"/>
	    <xsl:variable name="params" as="element()*">
	        <x>11</x>
	        <y>22</y>
	    </xsl:variable>    

        <result>
           <xsl:sequence select="saxon:query($query, 5, $params)"/>
        </result>
	</xsl:template>
	
</xsl:stylesheet>
