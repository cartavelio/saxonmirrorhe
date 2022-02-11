<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon xs">

<!-- PURPOSE:  Test saxon:recognize-binary effect on text output method -->
<?dependency saxon?>

<xsl:output method="text" saxon:recognize-binary="yes" encoding="iso-8859-1"/>



<xsl:template name="main">
<out>
  <a>
      <xsl:processing-instruction name="hex" select="'f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff'"/>
  </a>
  <a>
      <xsl:processing-instruction name="hex.ascii" select="'30313233343536373839'"/>
  </a>  
</out>
</xsl:template>
</xsl:stylesheet>