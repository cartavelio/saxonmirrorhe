<?xml version="1.0" encoding="utf-8"?> 

<?dependency saxon?>

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:saxon="http://saxon.sf.net/"
>

<xsl:template name="xsl:initial-template">
  <out>
    <a/>
    <xsl:message saxon:time="yes">One</xsl:message>
    <a/>
    <xsl:message saxon:time="yes">Two</xsl:message>
    <a/>
    <xsl:message saxon:time="yes">Three</xsl:message>
    <a/>
    <xsl:message saxon:time="yes">Four</xsl:message>
  </out>
</xsl:template>

</xsl:stylesheet>
