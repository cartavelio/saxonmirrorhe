<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon xs">

<!-- PURPOSE:  Test saxon:recognize-binary effect on text output method -->
<?dependency saxon?>

<xsl:output method="xml" encoding="iso-8859-1"/>
  
<xsl:variable name="params" select="map{'method' : 'text', 'encoding' : 'iso-8859-1',
    QName('http://saxon.sf.net/','recognize-binary') : true() }"/>

<xsl:template name="main">
<out>
  <a>
    <xsl:variable name="pi" as="processing-instruction()">
      <xsl:processing-instruction name="hex" select="'f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff'"/>
    </xsl:variable>
    <xsl:value-of select="serialize($pi, $params)"/>
  </a>
  <a>
    <xsl:variable name="pi" as="processing-instruction()">
      <xsl:processing-instruction name="hex.ascii" select="'30313233343536373839'"/>
    </xsl:variable>
    <xsl:value-of select="serialize($pi, $params)"/>
  </a>  
  <a>
    <xsl:variable name="pi" as="processing-instruction()">
      <xsl:processing-instruction name="b64.utf-8" select="saxon:string-to-base64Binary('banana&#xa0;custard', 'utf-8')"/>
    </xsl:variable>
    <xsl:value-of select="serialize($pi, $params)"/>
  </a>    
</out>
</xsl:template>
</xsl:stylesheet>