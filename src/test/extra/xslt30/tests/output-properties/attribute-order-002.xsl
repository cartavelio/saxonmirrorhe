<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

<!-- PURPOSE:  Test saxon:attribute-order with XML output -->
<?dependency saxon?>

<xsl:output saxon:attribute-order="baz" />
  
  <xsl:template name="main">
    <e foo="" baz="" bar="" />
  </xsl:template>
</xsl:stylesheet>