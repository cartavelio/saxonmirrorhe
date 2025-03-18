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
   
   
   
   <xsl:template match="*" priority="1000">
      <!--<xsl:text>/* L21 new {type/@RESOLVED_TYPE}({f:expressionType((arguments/argument[1], .)[1])}) */</xsl:text>-->
      <xsl:next-match/>
   </xsl:template>
   
   <xsl:template match="*">
      <xsl:text>new </xsl:text>
      <xsl:apply-templates select="type" mode="xml-to-cs"/>
      <xsl:call-template name="diamond"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Handle a constructor call with a diamond operator: new ArrayList<>() -->
   <!-- Inference of the specific type is context dependent and is handled in module diamond.xsl -->
   
   <xsl:template name="diamond">
      <xsl:if test="type/typeArguments[not(*)]">
         <xsl:text>&lt;</xsl:text>
         <xsl:apply-templates select="." mode="diamond"/>
         <xsl:text>&gt;</xsl:text>
      </xsl:if>
   </xsl:template>
   
   <!-- new AtomicInteger(x) creates a plain int -->
   <!--<xsl:template match="*[type/@RESOLVED_TYPE='java.util.concurrent.atomic.AtomicInteger']" priority="30">
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
   </xsl:template>-->
   
   <!-- new String(byte[], Charset) => Encoding.XXX.GetString(bytes) -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.lang.String'][count(arguments/argument)=2]" priority="30">
      <!-- Assume this is the only use of a two-argument constructor -->
      <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
      <xsl:text>.GetString(</xsl:text>
      <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Special-case 'new BigInteger(string)' ==> Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(string) -->
   <!-- Detecting the type of the argument is tricky, but we do our best -->
   <!-- The helper method converts the exception so it can be caught -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.math.BigInteger']
      [f:expressionType(arguments/argument[1])='java.lang.String']" priority="30">
      <xsl:text>Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- java.math.BigDecimal -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.math.BigDecimal']" priority="30">
      <xsl:variable name="argType" select="f:expressionType(arguments/argument[1])"/>
      <xsl:variable name="arity" select="count(arguments/argument)"/>
      <xsl:choose>
         <xsl:when test="$argType='java.lang.String' and $arity=1">
            <xsl:text>Singulink.Numerics.BigDecimal.Parse</xsl:text>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture)</xsl:text>
         </xsl:when>
         <xsl:when test="$argType = ('double', 'java.lang.Double') and $arity=1">
            <xsl:text>Singulink.Numerics.BigDecimal.FromDouble</xsl:text>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>, Singulink.Numerics.FloatConversion.Exact)</xsl:text>
         </xsl:when>
         <xsl:when test="$argType = 'java.math.BigInteger' and $arity=1">
            <xsl:text>new Singulink.Numerics.BigDecimal(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>, 0)</xsl:text>
         </xsl:when>
         <xsl:when test="$argType = 'java.math.BigInteger' and $arity=2">
            <xsl:text>new Singulink.Numerics.BigDecimal(</xsl:text>
            <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
            <xsl:text>, -(</xsl:text>
            <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
            <xsl:text>))</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   
   <!-- DateTimeParseException -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.time.format.DateTimeParseException']
      [count(arguments/argument) eq 3]" priority="30">
      <xsl:text>new System.FormatException(</xsl:text>
      <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
      <xsl:text> + " : " + </xsl:text>
      <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- UriSyntaxException -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.net.URISyntaxException']
      [count(arguments/argument) eq 2]" priority="30">
      <xsl:text>new System.UriFormatException(</xsl:text>
      <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
      <xsl:text> + " : " + </xsl:text>
      <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- TopDownStackIterable. Not needed on .NET because stacks naturally iterate top-down -->
   <xsl:template match="*[type/@RESOLVED_TYPE='net.sf.saxon.tree.jiter.TopDownStackIterable']" priority="30">
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
   </xsl:template>
   
   <!-- java.io.File -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.io.FileInputStream']
      [count(arguments/argument) eq 2]" priority="30">
      <xsl:text>java.io.File.Resolve(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- new FileInputStream(java.io.File f) ==> System.IO.File.OpenRead(f.getPath()) -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.io.FileInputStream']
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
   
   <!-- new FileOutputStream(java.io.File f) ==> new FileStream(f.getPath(), FileMode.Create) -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.io.FileOutputStream']" priority="30">
      <xsl:variable name="argType" select="f:expressionType(arguments/argument[1])"/>
      <xsl:variable name="arity" select="count(arguments/argument)"/>
      <xsl:choose>
         <xsl:when test="$arity = 1 and $argType = 'java.io.File'">
            <xsl:text>new System.IO.FileStream(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>.getPath(), System.IO.FileMode.Create)</xsl:text>
         </xsl:when>
         <xsl:when test="$arity = 1 and $argType = 'java.lang.String'">
            <xsl:text>new System.IO.FileStream(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>, System.IO.FileMode.OpenOrCreate)</xsl:text>
         </xsl:when>
         <xsl:when test="$arity = 2 and $argType = 'java.lang.String'">
            <xsl:text>new System.IO.FileStream(</xsl:text>
            <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
            <xsl:text>? System.IO.FileMode.Append : System.IO.FileMode.Create)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>/* No match for FileInputStream constructor */</xsl:text>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
      
   </xsl:template>
   
   <!-- Eliminate java.io.BufferedReader (for now) -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.io.BufferedReader']
      [count(arguments/argument) eq 1]" priority="30">
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
   </xsl:template>
 
   <!-- new InputStreamReader(inputStream, encoding) ==> new StreamReader(inputStream, System.Text.Encoding.GetEncoding(encoding)) -->
   <xsl:template match="*[type/@RESOLVED_TYPE='java.io.InputStreamReader']
      [count(arguments/argument) eq 2]" priority="30">
      <xsl:text>new System.IO.StreamReader(</xsl:text>
      <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
      <xsl:choose>
         <xsl:when test="f:expressionType(arguments/argument[2]) = 'java.nio.charset.Charset'">
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>, System.Text.Encoding.GetEncoding(</xsl:text>
            <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- new ConcurrentHashMap(size) ==> new ConcurrentDictionary(concurrency, size) -->
   <xsl:template match="*[starts-with(type/@RESOLVED_TYPE, 'java.util.concurrent.ConcurrentHashMap')]
      [count(arguments/argument) eq 1]" priority="30">
      <xsl:text>new /* java.util.concurrent.ConcurrentHashMap */ </xsl:text>
      <xsl:apply-templates select="type" mode="xml-to-cs"/>
      <xsl:call-template name="diamond"/>
      <xsl:text>(16, </xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- new java.net.URI() -->
   <xsl:template match="*[starts-with(type/@RESOLVED_TYPE, 'java.net.URI')]
      [count(arguments/argument) gt 3]" priority="30">
      <xsl:text>Saxon.Impl.Helpers.UriUtils.Build(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[starts-with(type/@RESOLVED_TYPE, 'java.net.URI')]
      [count(arguments/argument) eq 1]" priority="30">
      <xsl:text>new System.Uri(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>, System.UriKind.RelativeOrAbsolute)</xsl:text>
   </xsl:template>
   
   
   <!-- DotNetPlatform -->
   <xsl:template match="*[type/@RESOLVED_TYPE='net.sf.saxon.dotnet.DotNetPlatform']">
      <xsl:text>new Saxon.Impl.Helpers.DotNetPlatform()</xsl:text>
   </xsl:template>
   
   
 
   
</xsl:stylesheet>
