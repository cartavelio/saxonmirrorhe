<?xml version="1.0" encoding="utf-8"?> 

<?dependency saxon?>

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   version="3.0"
   xmlns:saxon="http://saxon.sf.net/"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="#all"
   xpath-default-namespace="http://loan-shark.com/with-defaults"
   expand-text="yes"
>
  
  <xsl:import-schema namespace="http://loan-shark.com/with-defaults" schema-location="../../docs/loans-with-defaults.xsd"/>
  
  <xsl:param name="expand" as="xs:boolean" required="yes" static="yes"/>
  
  <!-- xsl:source-document/@saxon:expand-attribute-defaults -->

  <xsl:template name="xsl:initial-template">
    <xsl:source-document href="../../docs/loans-ns.xml" validation="strict" saxon:_expand-attribute-defaults="{$expand}">
      <xsl:copy-of select="."/>
    </xsl:source-document>
  </xsl:template>
  

</xsl:stylesheet>
