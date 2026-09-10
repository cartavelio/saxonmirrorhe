<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon xs">

<!-- PURPOSE:  Test data uri schema returning JSON, read using json-doc -->
<?dependency saxon?>

  <xsl:variable name="content"><![CDATA[{"first":"Michael", "last":"Kay", "age":21}]]></xsl:variable>

<xsl:template name="main">
<out>
  <xsl:copy-of select="json-doc('data:application/json,' || encode-for-uri($content))?last"/>
</out>
</xsl:template>
</xsl:stylesheet>