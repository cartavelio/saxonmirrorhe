<?xml version="1.0" encoding="utf-8"?> 

<?dependency saxon?>

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:saxon="http://saxon.sf.net/"
>

<xsl:template name="xsl:initial-template">
  <out>
    <xsl:message error-code="Q{{}}error-001">One</xsl:message>
    <xsl:message error-code="Q{{}}error-001">One</xsl:message>
    <xsl:message error-code="Q{{}}error-002">Two</xsl:message>
    <xsl:message error-code="Q{{}}error-003"/>
    <counts 
      one="{saxon:message-count(QName('', 'error-001'))}"
      two="{saxon:message-count(QName('', 'error-002'))}"
      three="{saxon:message-count(QName('', 'error-003'))}"
      four="{saxon:message-count(QName('', 'error-004'))}"
      all="{saxon:message-count(())}"   
    />
  </out>
</xsl:template>

</xsl:stylesheet>
