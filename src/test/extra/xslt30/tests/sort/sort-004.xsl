<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns:my="http://www.w3.org/1999/my"
				exclude-result-prefixes="saxon xs my">
				
				<!-- test saxon:sort with two arguments using a user-defined function -->
				
	<xsl:output method="xml" indent="yes"/>


<xsl:variable name="data" as="element()*">
  <a>92</a>
  <b>103</b>
  <c>africa</c>
  <d>europe</d>
  <d>asia</d>
</xsl:variable>

 <xsl:template match="/" name="main">
   <z>
    <out><xsl:copy-of select="saxon:sort($data, my:function#1)"/></out>
   </z>  
 </xsl:template>
 
 <xsl:function name="my:function">
   <xsl:param name="in"/>
   <xsl:sequence select="codepoints-to-string(reverse(string-to-codepoints($in)))"/>
 </xsl:function>
  
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
