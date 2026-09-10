<xsl:transform
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:saxon="http://saxon.sf.net/"
   exclude-result-prefixes="#all"
   version="3.0"
   expand-text="yes"
   >
   <xsl:param name="doc"/>
   
   <xsl:template name="a">
      <a>{saxon:line-number((.//ITEM)[1])}</a>
   </xsl:template>
   
   <xsl:template name="b">
      <b>{saxon:line-number(($doc//ITEM)[1])}</b>
   </xsl:template>
   
   <xsl:template name="c">
      <c>{saxon:line-number((doc('books.xml')//ITEM)[1])}</c>
   </xsl:template>
   
   
   
</xsl:transform>	
