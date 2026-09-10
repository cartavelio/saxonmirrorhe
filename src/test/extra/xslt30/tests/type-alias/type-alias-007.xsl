<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:c="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon c"
   expand-text="yes"
>
  
  <!-- Static type checking against tuple types -->

  <saxon:item-type name="c:complex" as="tuple(r as xs:double, i as xs:double)"/>
  
  <xsl:function name="c:complex" as="type(c:complex)">
    <xsl:param name="r" as="xs:double"/>
    <xsl:param name="i" as="xs:double"/>
    <xsl:sequence select="map{'r':$r, 'i':$i}"/>
  </xsl:function>
  
  <xsl:function name="c:wrong" as="xs:string">
    <xsl:param name="x" as="type(c:complex)"/>
    <xsl:sequence select="substring($x?r, 2)"/>
  </xsl:function>
  
  <xsl:template name="xsl:initial-template">
    <out>{c:wrong(c:complex(1.0, 2.0))}</out>
  </xsl:template>  

</xsl:stylesheet>
