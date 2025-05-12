<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:saxon="http://saxon.sf.net/"
                xmlns:thread="java:java.lang.Thread"
                xmlns:shared="urn:shared-functions"
                extension-element-prefixes="saxon"
                version="2.0">

   <xsl:function name="shared:sleep">
      <xsl:param name="seconds" as="xs:integer"/>
      <xsl:sequence select="thread:sleep(1000*$seconds)"/>
   </xsl:function>

</xsl:stylesheet>