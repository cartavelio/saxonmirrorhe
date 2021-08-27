<?xml version="1.0" encoding="utf-8"?> 

<?dependency saxon?>
<?spec xslt#variables?>



<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="xs"
>


                                                     
<xsl:output method="xml" omit-xml-declaration="no" encoding="utf-8" indent="no"/>
  
  <xsl:template match="/">
    <out>
      <saxon:assign name="counter">12.5</saxon:assign>
      <xsl:value-of select="$counter * 2 = 25.0"/>
    </out>
  </xsl:template>
  
  <xsl:variable name="counter" saxon:assignable="yes" select="1" as="xs:decimal"/>
  
  


</xsl:stylesheet>
