<?xml version="1.0"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:my="http://www.example.com/my-functions" exclude-result-prefixes="my">

  <!-- PURPOSE test xsl:evaluate allowing reference to schema namespaces not imported by the stylesheet -->


  <xsl:param name="p">"BrokerPriceOpinion" castable as PropertyValuationMethodType</xsl:param>
  
  <xsl:mode on-no-match="shallow-copy"/>
  
  <xsl:template match="/">
    <xsl:apply-templates select="//ACERequest"/>
  </xsl:template>
  
  <xsl:template match="ACERequest" xmlns:saxon="http://saxon.sf.net/">
    <out>
      <xsl:evaluate xpath="$p" schema-aware="true" saxon:options="map{'allow-any-namespace' : false()}"/>
    </out>
  </xsl:template>

 
</xsl:stylesheet>
