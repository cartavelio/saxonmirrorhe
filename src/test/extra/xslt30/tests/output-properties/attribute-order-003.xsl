<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

<!-- PURPOSE:  Test saxon:attribute-order with XML output -->
<?dependency saxon?>

<xsl:output method="xml" saxon:attribute-order="id name alpha beta * xml:space" indent='yes'/>

<xsl:template name="main">
<out>
  <a id="x" name="y" alpha="z" beta="w" xml:space="preserve" gamma="g" />
  <a xs:extra="4" gamma="g" alpha="z" beta="w" id="x" name="y" xml:space="preserve" java:jam="j"/>
  <a xml:space="preserve" alpha="z" xs:extra="4" gamma="g" beta="w" id="x" name="y"/>
  <a b="3" c="4"/>
</out>  
</xsl:template>
</xsl:stylesheet>