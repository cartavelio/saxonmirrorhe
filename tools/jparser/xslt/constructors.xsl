<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   expand-text="yes"
   default-mode="constructors"
   version="3.0">
   
   <xsl:mode name="constructors" on-no-match="fail" saxon:trace="no" xmlns:saxon="http://saxon.sf.net/"/>
   
   <!-- This module is concerned with constructors (object creation expressions), 
      especially rules for 
      properties that are specific to particular classes. All the rules in this 
      module are in mode "constructors", and all pertain to expressions satisfying
      the pattern `*[@nodeType='ObjectCreationExpr']`, so this condition is
      not re-stated as part of each pattern.
   -->
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr']">
      <xsl:text>new </xsl:text>
      <xsl:apply-templates select="type" mode="xml-to-cs"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- new AtomicInteger(x) creates a plain int -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
                         [type/@RESOLVED_TYPE='java.util.concurrent.atomic.AtomicInteger']" priority="30">
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
   </xsl:template>
   
   <!-- Special-case 'new BigInteger(string)' ==> Numerics.BigInteger.Parse(string) -->
   <!-- Detecting the type of the argument is a bit beyond our capabilities, but we do our best -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.math.BigInteger']
      [arguments/argument[1][@RESOLVED_TYPE='java.lang.String' 
                             or @nodeType='StringLiteralExpr'
                             or (@nodeType='MethodCallExpr' and name/@identifier='toString')]]" priority="30">
      <xsl:text>System.Numerics.BigInteger.Parse</xsl:text>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
                         [type/@RESOLVED_TYPE='java.math.BigDecimal']
                         [arguments/argument[1][@RESOLVED_TYPE='java.lang.String' 
      or @nodeType='StringLiteralExpr'
      or (@nodeType='MethodCallExpr' and name/@identifier='toString')]]" priority="30">
      <xsl:text>Singulink.Numerics.BigDecimal.Parse</xsl:text>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.math.BigDecimal']
      [count(arguments/argument) eq 1]
      [arguments/argument[1][@RESOLVED_TYPE='java.math.BigInteger']]" priority="30">
      <xsl:text>new Singulink.Numerics.BigDecimal(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>, 0)</xsl:text>
   </xsl:template>
   
   <!-- DateTimeParseException -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.time.format.DateTimeParseException']
      [count(arguments/argument) eq 1]
      [arguments/argument[1][@RESOLVED_TYPE='java.math.BigInteger']]" priority="30">
      <xsl:text>new System.FormatException(</xsl:text>
      <xsl:apply-templates select="arguments[0]" mode="xml-to-cs"/>
      <xsl:text> + " : " + </xsl:text>
      <xsl:apply-templates select="arguments[1]" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
 
   
</xsl:stylesheet>
