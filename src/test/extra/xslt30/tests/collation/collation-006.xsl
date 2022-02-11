<?xml version="1.0" encoding="UTF-8"?>

<!-- test error when all collations in default-collation attribute are unrecognized -->
<?spec xslt#xsl-key?>
<?dependency saxon?>

<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
xmlns:xs="http://www.w3.org/2001/XMLSchema"
xmlns:saxon="http://saxon.sf.net/"
exclude-result-prefixes="xs saxon" default-collation="http://ibm.com/random-collation swahili collation?lang=sv;strength=tercentenary" xml:base="http://saxon.sf.net/">

<!-- test key matching with collations, using multiple keys with same name -->



  <xsl:key name="k3" match="meti" use="string(.)"/>
  <xsl:key name="k4" match="meti" use="string(.)"  collation="collation?lang=sv;strength=primary"/>
  <xsl:key name="k3" match="item" use="string(.)" />
  <xsl:key name="k4" match="item" use="string(.)"  collation="collation?lang=sv;strength=primary"/>

<xsl:template match="/">
    <out>;
     <c3><xsl:value-of select="key('k3', 'Jaeger')" separator=","/></c3>;
     <c4><xsl:value-of select="key('k4', 'Jaeger')" separator=","/></c4>;          
   </out>
</xsl:template>
</xsl:stylesheet>
