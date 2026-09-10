<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  exclude-result-prefixes="xs math" version="3.0">

  <doc:meta>
    <dc:title>Pattern classifications - S19.8.9</dc:title>
  </doc:meta>
  
  <xsl:mode streamable="yes"/>
  <xsl:variable name="doc" as="document-node()?"/>

  
<!-- Motionless -->
  <xsl:template match="/"/>
  <xsl:template match="*"/>
  <xsl:template match="/*"/>
  <xsl:template match="p"/>
  <xsl:template match="p|q"/>
  <xsl:template match="p/q"/>
  <xsl:template match="p[@status='red']"/>
  <xsl:template match="p[base-uri()]"/>
  <xsl:template match="p[@class or @style]"/>
  <xsl:template match="p[@status]"/>
  <xsl:template match="p[@class | @style]"/>
  <xsl:template match="p[contains(@class,':')]"/>
  <xsl:template match="p[substring-after(@class,':')]"/>
  <xsl:template match="p[ancestor::*[@xml:lang]]"/>
  <xsl:template match="p[ancestor::*]"/>
  <xsl:template match="text()[starts-with(.,'$')]"/>
  <xsl:template match="@price"/>
  <xsl:template match="@price[starts-with(.,'$')]"/>
  <xsl:template match="//p/text()[. = 'Introduction']"/>
  
  <!-- Not motionless -->
  <xsl:template match="id('abc')" doc:doc="contains RootedPath"/>
  <xsl:template match="$doc//p" doc:doc="contains RootedPath"/>
  <xsl:template match="p[b]" doc:doc="predicate not motionless"/>
  <xsl:template match="p[. = 'Introduction']" doc:doc="predicate not motionless"/>
  <xsl:template match="p[starts-with(.,'$')]" doc:doc="predicate not motionless"/>
  <xsl:template match="p[preceding-sibling::p[1] = '']" doc:doc="contains a positional predicate: return type is numeric"/>
  <xsl:template match="p[data(@status)]" doc:doc="contains a positional predicate: return type is potentially numeric"/>
  <xsl:template match="p[position() gt 2]" doc:doc="contains a positional predicate: calls position()"/>
  <xsl:template match="p[last()]" doc:doc="contains a positional predicate: calls last()"/>
  

</xsl:stylesheet>
