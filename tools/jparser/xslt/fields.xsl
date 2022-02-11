<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   expand-text="yes"
   default-mode="fields"
   version="3.0">
   
   <xsl:mode name="fields" on-no-match="fail"/>
   
   <!-- This module is concerned with field access expressions, especially rules for 
      properties that are specific to particular classes. All the rules in this 
      module are in mode "fields", and all pertain to expressions satisfying
      the pattern `*[@nodeType='FieldAccessExpr']`, so this condition is
      not re-stated as part of each pattern.
   -->
   
   
   <!-- .length accessor on arrays -->
   <xsl:template match="*[name/@identifier='length'][ends-with(@RESOLVED_TYPE, '[]')]" priority="50">
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>.Length</xsl:text>
   </xsl:template>
   
   <!-- .class accessor on any object -->
   <xsl:template match="*[name/@identifier='class']" priority="50">
      <xsl:text>typeof(</xsl:text>
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Identify a static field accessor such as BooleanValue.TRUE -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][ends-with(f:degenerify(@RESOLVED_TYPE), '.'||scope/name/@identifier)][not(scope/scope)]" 
      priority="18">
      <xsl:text>{f:rename-generic-type(@RESOLVED_TYPE)}.{name/@identifier}</xsl:text>
   </xsl:template>
   
   <!-- Identify a static field accessor such as IntegerValue.MAX -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Integer']" priority="19">
      <xsl:text>java.lang.Integer.{name/@identifier}</xsl:text>
   </xsl:template>
   
   <!-- Identify the static field accessor BigInteger.ZERO -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.math.BigInteger'][name/@identifier='ZERO']" priority="19">
      <xsl:text>System.Numerics.BigInteger.Zero</xsl:text>
   </xsl:template>
   
 <!--  <xsl:template match="*[$refactoredProperties(name/@identifier)]" priority="20">
      <xsl:value-of select="f:refactor-property-reference(., name/@identifier)"/>
   </xsl:template>-->
   
   <!-- static field accessor for a field in a generic type such as Feature -->
   <xsl:template match="*[@FIELD_TYPE]" priority="25">
      <xsl:text>{f:rename-generic-type(@FIELD_TYPE)}.{name/@identifier}</xsl:text>
   </xsl:template>
   
   <!-- default rule for field access expressions -->
   <xsl:template match="*">
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>.</xsl:text>
      <xsl:apply-templates select="name" mode="xml-to-cs"/>
   </xsl:template>

</xsl:stylesheet>