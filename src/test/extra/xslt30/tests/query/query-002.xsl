<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs">
				
				<!-- test for saxon:query defaulting the context item and parameters -->
				
				
	<xsl:template name="main">
	    <xsl:variable name="q1"><![CDATA[declare variable $x external := 1; declare variable $y external := 4; <z>{$x + .}</z>]]></xsl:variable>
	    <xsl:variable name="query" as="java:net.sf.saxon.query.XQueryExpression" xmlns:java="http://saxon.sf.net/java-type"
	        select="saxon:compile-query($q1)"/>

        <result>
           <xsl:for-each select="1 to 5">
             <a><xsl:sequence select="saxon:query($query)"/></a>
           </xsl:for-each>
        </result>
	</xsl:template>
	
</xsl:stylesheet>
