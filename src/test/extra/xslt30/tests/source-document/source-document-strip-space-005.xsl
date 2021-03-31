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
  
  <xsl:import-schema schema-location="../../docs/loans.xsd"/>
  
  <xsl:param name="strip" as="xs:string" required="yes" static="yes"/>
  
  <!-- xsl:source-document/@saxon:strip-space -->

  <xsl:template name="xsl:initial-template">
    <xsl:source-document href="../../docs/loans.xml" saxon:_strip-space="{$strip}" validation="strict">
      <xsl:copy-of select="."/>
    </xsl:source-document>
  </xsl:template>
  

</xsl:stylesheet>
