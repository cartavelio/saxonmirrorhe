<xsl:stylesheet exclude-result-prefixes="#all" version="3.0" 
  xmlns:JarURLConnection="java:java.net.JarURLConnection" xmlns:URL="java:java.net.URL" 
  xmlns:jt="http://saxon.sf.net/java-type" xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output indent="yes" omit-xml-declaration="yes" />
  <xsl:template name="main">
    <xsl:variable as="jt:java.net.URL" name="url" 
      select=" 
      URL:new('jar:file:/test!/')
      => URL:openConnection()
      => JarURLConnection:getURL()" />
    <test>
      <xsl:value-of select="URL:getProtocol($url)" />
    </test>
  </xsl:template>
</xsl:stylesheet>