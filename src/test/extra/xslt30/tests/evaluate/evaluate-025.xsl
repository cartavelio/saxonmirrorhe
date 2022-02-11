<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns:f="http://local-functions/"
				xmlns="http://www.w3.org/1999/xhtml"
				exclude-result-prefixes="saxon xs f">
				
				<!-- test saxon:sort with two arguments -->
				
	<xsl:output method="xml" indent="yes"/>


<xsl:variable name="data" as="element()*">
  <a>$92.00</a>
  <b>$103.22</b>
  <c>$127.35 </c>
  <d>$34.00</d>
</xsl:variable>

	<xsl:template name="main">
	<xsl:variable name="sk">number(translate(., '$', ''))</xsl:variable>
	<z>
    <out><xsl:copy-of select="saxon:sort($data, f:strip#1)"/></out>
  </z>  
	</xsl:template>
	
    <xsl:function name="f:strip" as="xs:double">
      <xsl:param name="node" as="xs:string"/>
      <xsl:sequence select="number(translate($node, '$', ''))"/>
    </xsl:function>	
	
</xsl:stylesheet>
