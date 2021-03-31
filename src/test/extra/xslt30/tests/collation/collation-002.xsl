<?xml version="1.0" encoding="UTF-8"?>
<?spec xslt#keys?>
<?dependency saxon?>
<!-- test xsl:key with collations -->

<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
xmlns:xs="http://www.w3.org/2001/XMLSchema" default-collation="http://saxon.sf.net/collation?lang=sv;strength=primary">



  <xsl:key name="k1" match="item" use="string(.)"  collation="http://saxon.sf.net/collation?lang=en;strength=primary"/>
  <xsl:key name="k2" match="item" use="string(.)"  collation="http://saxon.sf.net/collation?lang=de;strength=primary"/>
  <xsl:key name="k3" match="item" use="string(.)"  collation="http://saxon.sf.net/collation?lang=fr;strength=primary"/>
  <xsl:key name="k4" match="item" use="string(.)"  collation="http://saxon.sf.net/collation?lang=sv;strength=primary"/>

<xsl:template match="/">
    <out>;
     <c1><xsl:value-of select="key('k1', 'Jaeger')" separator=","/></c1>;
     <c2><xsl:value-of select="key('k2', 'Jaeger')" separator=","/></c2>;     
     <c3><xsl:value-of select="key('k3', 'Jaeger')" separator=","/></c3>;
     <c4><xsl:value-of select="key('k4', 'Jaeger')" separator=","/></c4>;          
   </out>
</xsl:template>
</xsl:stylesheet>
