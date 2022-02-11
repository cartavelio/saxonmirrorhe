<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

  <!-- PURPOSE:  Test saxon:single-quotes with XML output -->
  <?dependency saxon?>

  <xsl:output method="xml" saxon:single-quotes='yes'/>

  <xsl:template name="main">
    <out>
      <a id="x" name="y" alpha="O'Connor" beta='w' xml:space="preserve" gamma="g" xmlns="http://saxon.sf.net"/>
    </out>
  </xsl:template>
</xsl:stylesheet>
