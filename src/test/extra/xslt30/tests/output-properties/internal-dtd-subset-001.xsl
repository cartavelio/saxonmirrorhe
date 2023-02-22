<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

  <!-- PURPOSE:  Test saxon:internal-dtd-subset with XML output -->
  <?dependency saxon?>

  <xsl:output method="xml" saxon:internal-dtd-subset="
    &lt;!ELEMENT out (a)*>&#xa;
    &lt;!ELEMENT a EMPTY>&#xa;
    &lt;!ATTLIST a id ID #REQUIRED>"/>

  <xsl:template name="main">
    <out>
      <a id="a01"/>
      <a id="a02"/>
    </out>
  </xsl:template>
</xsl:stylesheet>
