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
   
   
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr']" priority="1000">
      <!--<xsl:text>/* L21 new {type/@RESOLVED_TYPE}({f:expressionType((arguments/argument[1], .)[1])}) */</xsl:text>-->
      <xsl:next-match/>
   </xsl:template>
   
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
   <!-- Detecting the type of the argument is tricky, but we do our best -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.math.BigInteger']
      [f:expressionType(arguments/argument[1])='java.lang.String']" priority="30">
      <xsl:text>System.Numerics.BigInteger.Parse</xsl:text>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
                         [type/@RESOLVED_TYPE='java.math.BigDecimal']
                         [f:expressionType(arguments/argument[1])='java.lang.String']" priority="30">
      <xsl:text>Singulink.Numerics.BigDecimal.Parse</xsl:text>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.math.BigDecimal']
      [count(arguments/argument) eq 1]
      [f:expressionType(arguments/argument[1])='java.math.BigInteger']" priority="30">
      <xsl:text>new Singulink.Numerics.BigDecimal(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>, 0)</xsl:text>
   </xsl:template>
   
   <!-- DateTimeParseException -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.time.format.DateTimeParseException']
      [count(arguments/argument) eq 3]" priority="30">
      <xsl:text>new System.FormatException(</xsl:text>
      <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
      <xsl:text> + " : " + </xsl:text>
      <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- new FileInputStream(java.io.File f) ==> System.IO.File.OpenRead(f.getPath()) -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.io.FileInputStream']
      [count(arguments/argument) eq 1]" priority="30">
      <xsl:variable name="argType" select="f:expressionType(arguments/argument[1])"/>
      <xsl:choose>
         <xsl:when test="$argType = 'java.io.File'">
            <xsl:text>System.IO.File.OpenRead(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>.getPath()</xsl:text>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="$argType = 'java.lang.String'">
            <xsl:text>System.IO.File.OpenRead(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>/* No match for FileInputStream constructor */</xsl:text>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- new FileOutputStream(java.io.File f) ==> new FileStream(f.getPath(), FileMode.OpenOrCreate) -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.io.FileOutputStream']
      [count(arguments/argument) eq 1]
      [f:expressionType(arguments/argument[1])='java.io.File']" priority="30">
      <xsl:variable name="argType" select="f:expressionType(arguments/argument[1])"/>
      <xsl:choose>
         <xsl:when test="$argType = 'java.io.File'">
            <xsl:text>new System.IO.FileStream(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>.getPath(), System.IO.FileMode.OpenOrCreate)</xsl:text>
         </xsl:when>
         <xsl:when test="$argType = 'java.lang.String'">
            <xsl:text>new System.IO.FileStream(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>, System.IO.FileMode.OpenOrCreate)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>/* No match for FileInputStream constructor */</xsl:text>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
      
   </xsl:template>
   
 
   <!-- new InputStreamReader(inputStream, encoding) ==> new StreamReader(inputStream, System.Text.Encoding.GetEncoding(encoding)) -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='java.io.InputStreamReader']
      [count(arguments/argument) eq 2]" priority="30">
      <xsl:text>new System.IO.StreamReader(</xsl:text>
      <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
      <xsl:text>, System.Text.Encoding.GetEncoding(</xsl:text>
      <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
      <xsl:text>))</xsl:text>
   </xsl:template>
   
   <!-- DotNetPlatform -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']
      [type/@RESOLVED_TYPE='net.sf.saxon.dotnet.DotNetPlatform']">
      <xsl:text>new Saxon.Impl.Helpers.DotNetPlatform()</xsl:text>
   </xsl:template>
   
   
 
   
</xsl:stylesheet>
