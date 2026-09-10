<?xml version="1.0" encoding="utf-8"?> 

<?dependency saxon?>

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   version="3.0"
   xmlns:saxon="http://saxon.sf.net/"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="#all"
   expand-text="yes"
>
  
  <xsl:param name="line-numbering" as="xs:boolean" required="yes" static="yes"/>
  
  <!-- xsl:source-document/@saxon:line-numbering="yes" -->

  <xsl:template name="xsl:initial-template">
    <xsl:source-document href="../../docs/books.xml" saxon:_line-numbering="{$line-numbering}">
      <out>
        <xsl:apply-templates select="//PRICE"/>
      </out>
    </xsl:source-document>
  </xsl:template>
  
  <xsl:template match="PRICE">
    <xsl:variable name="line" select="saxon:line-number()"/>
    <price line-number="{$line}">{.}</price>
  </xsl:template>


</xsl:stylesheet>
