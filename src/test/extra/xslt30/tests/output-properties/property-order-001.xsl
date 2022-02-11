<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

<!-- PURPOSE:  Test saxon:property-order with JSON output -->
<?dependency saxon?>

<xsl:output method="json" saxon:property-order="Monday Tuesday * Saturday Sunday"/>

<xsl:template name="main">
  <xsl:sequence select="map{'Friday':5, 'Saturday':6, 'Sunday':7, 'Monday':1, 'Wednesday':3}"/>
</xsl:template>
</xsl:stylesheet>