<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

<!-- PURPOSE:  Test saxon:double-space with XML output -->
<?dependency saxon?>

<xsl:output method="xml" saxon:double-space="test-case java:comment" indent='yes'/>

<xsl:template name="main">
<out><test-case name="attribute-order-004b">
    <description>Test saxon:attribute-order with fn:serialize() (Bug #4458)</description>
    <created by="Debbie Lockett" on="2020-03-10"/>
    <dependencies>
      <spec value="XSLT20+"/>
      <feature value="Saxon-PE" satisfied="false"/>
    </dependencies>
  </test-case><test-case name="double-space-001">
    <description>Test saxon:double-space with XML output method</description>
    <created by="Michael Kay" on="2014-04-13"/>
    <modified by="Michael Kay" on="2020-02-25" change="use serialization-matches to avoid indentation problems"/>
    <dependencies>
      <spec value="XSLT20+"/>
      <feature value="Saxon-PE"/>
    </dependencies>
  </test-case><java:comment/><test-case name="property-order-001">
    <description>Test saxon:property-order with JSON output method</description>
    <created by="Michael Kay" on="2017-10-06"/>
    <dependencies>
      <spec value="XSLT20+"/>
      <feature value="Saxon-PE"/>
    </dependencies>
  </test-case>
</out>  
</xsl:template>
</xsl:stylesheet>