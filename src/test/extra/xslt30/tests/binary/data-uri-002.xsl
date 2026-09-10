<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon xs">

<!-- PURPOSE:  Test data uri schema returning XML encoded in base64 -->
<?dependency saxon?>

  <xsl:variable name="content"><![CDATA[<a><b id='b1'/></a>]]></xsl:variable>

<xsl:template name="main">
<out>
  <xsl:copy-of select="doc('data:application/xml;encoding=utf-8;base64,' || saxon:string-to-base64Binary($content, 'utf-8'))"/>
</out>
</xsl:template>
</xsl:stylesheet>