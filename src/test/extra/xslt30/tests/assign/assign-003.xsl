<?xml version="1.0" encoding="utf-8"?> 

<?dependency saxon?>
<?spec xslt#variables?>
<!-- A stylesheet that combines saxon:assign with saxon:while -->


<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
   xmlns:saxon="http://saxon.sf.net/"
   extension-element-prefixes="saxon"
>

  <xsl:variable name="string" saxon:assignable="yes" select="''"/>
  <xsl:variable name="counter" saxon:assignable="yes" select="1"/>
  

                                                     
<xsl:output method="xml" omit-xml-declaration="no" encoding="utf-8" indent="no"/>
  
  <xsl:template match="/">
    <out>
      <saxon:while test="string-length($string) lt 100">
        <saxon:assign name="counter" select="$counter*2"/>
        <saxon:assign name="string" select="concat($string, '|', $counter)"/>
      </saxon:while>
      <xsl:value-of select="$string"/>
    </out>
  </xsl:template>


</xsl:stylesheet>
