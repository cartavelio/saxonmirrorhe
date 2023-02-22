<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   exclude-result-prefixes="#all"
   expand-text="yes"
   default-mode="methods"
   version="3.0">
   
   <xsl:mode name="methods" on-no-match="fail"/>
   
   <!-- This module is concerned with method calls, especially rules for method
      calls that are specific to particular classes. All the rules in this 
      module are in mode "methods", and all pertain to expressions satisfying
      the pattern `match="*[@nodeType='MethodCallExpr']`, so this condition is
      not re-stated as part of each pattern.
   -->
   
   <xsl:template match="*" priority="1000">
      <!--<xsl:message>METHOD CALL {name/@identifier}() RESOLVED_TYPE={@RESOLVED_TYPE} is-exception={f:is-exception-type(@RESOLVED_TYPE)}</xsl:message>-->
      <xsl:next-match/>
   </xsl:template>
   
   <xsl:template match="iterable[@nodeType='MethodCallExpr']
      [starts-with(@RESOLVED_TYPE, 'java.util.Map')]
      [name/@identifier='entrySet']" priority="30" mode="methods">
      <!--<xsl:message>Iterable.{name/@identifier}()</xsl:message>-->
      <xsl:apply-templates select="scope/*" mode="xml-to-cs"/>
      <!--     <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>-->
   </xsl:template>
   
   <xsl:template match="*[name/@identifier='getClass'][empty(arguments/argument)]" priority="100" mode="methods">
      <xsl:choose>
         <xsl:when test="scope">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>this</xsl:text>
         </xsl:otherwise>
      </xsl:choose>     
      <xsl:text>.GetType()</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[name/@identifier='hashCode'][empty(arguments/argument)]" priority="100" mode="methods">
      <xsl:choose>
         <xsl:when test="scope">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>this</xsl:text>
         </xsl:otherwise>
      </xsl:choose>     
      <xsl:text>.GetHashCode()</xsl:text>
   </xsl:template>
   
   <!-- Special for System.err.println() -->
   
   <xsl:template match="*[@nodeType='MethodCallExpr']
                           [name/@identifier='println']
                           [scope[@RESOLVED_TYPE='java.lang.System' and name/@identifier='err']]" 
                 priority="50">
      <text>System.Console.Error.WriteLine(</text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
 
   
   <!-- Convert calls on delegate methods -->
   <!-- For example 'consumer.accept(item)' becomes 'consumer(item)' if consumer is a functional interface
        that converts to a C# delegate -->
   <xsl:template match="*[key('digestClassKey', f:degenerify(@RESOLVED_TYPE), $digest)/@delegate='1']" 
      priority="30" mode="methods">     
      <!--<xsl:message>Delegate.{name/@identifier}()</xsl:message>-->
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Calls on a system-supplied delegate type such as java.util.functions.Consumer -->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.util.function.')]" 
      priority="28" mode="methods">
      <!--<xsl:message>Function.{name/@identifier}()</xsl:message> -->     
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- A method call of the form CSharp.emitCode("goto case 3") is recognised as indicating fall-through in a switch -->
   <xsl:template match="*[@RESOLVED_TYPE=('net.sf.saxon.transpile.CSharp')]" 
      priority="30" mode="methods">
      <!--<xsl:message>CSharp.{name/@identifier}()</xsl:message>-->
      <xsl:choose>
         <xsl:when test="name/@identifier = 'emitCode'">
         </xsl:when>
         <xsl:otherwise>
            <xsl:message terminate="yes">*** Unknown method CSharp.{name/@identifier} in {base-uri()}***</xsl:message>
         </xsl:otherwise>
      </xsl:choose>
      <xsl:text>{arguments/argument[1]/@value}</xsl:text>
   </xsl:template>
   
   
   <!-- Convert method calls applied to java.lang.String -->
   <xsl:template match="*[@RESOLVED_TYPE=('java.lang.String','java.lang.CharSequence')]" 
      priority="30" mode="methods">
      <!--<xsl:message>String.{name/@identifier}()</xsl:message>-->
      <xsl:variable name="renamedMethods" as="map(*)" select="map{
         'toLowerCase': 'ToLower',
         'toUpperCase': 'ToUpper',
         'hashCode':    'GetHashCode'}"/>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'charAt'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>[</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>]</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'equalsIgnoreCase'">
            <!-- Java str.getChars(begin, end, array, dest) => C# str.CopyTo(begin, array, dest, count) -->
            <xsl:text>com.saxonica.glue.StringUtils.stringEqualsIgnoreCase(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'getChars'">
            <!-- Java str.getChars(begin, end, array, dest) => C# str.CopyTo(begin, array, dest, count) -->
            <xsl:text>com.saxonica.glue.StringUtils.stringToCharArray(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when> 
         <xsl:when test="name/@identifier = 'intern'">
            <xsl:text>System.String.Intern(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when> 
         <xsl:when test="name/@identifier = 'isEmpty'">
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Length==0)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'length'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Length</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'matches'">
            <xsl:text>System.Text.RegularExpressions.Regex.IsMatch(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'replaceAll'">
            <!-- Java str.replaceAll(pattern, replacement) ==> C# Regex.Replace(str, pattern, replacement) -->
            <xsl:text>System.Text.RegularExpressions.Regex(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when> 
         <xsl:when test="name/@identifier = 'split'">
            <xsl:text>System.Text.RegularExpressions.Regex.Split(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>   
         <xsl:when test="map:contains($renamedMethods, name/@identifier)">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:value-of select="$renamedMethods(name/@identifier)"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert method calls applied to built-in numeric types (java.lang.Integer, etc) -->
   <xsl:template match="*[@RESOLVED_TYPE='java.lang.Integer']" 
      priority="30" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'parseInt'">
            <xsl:text>System.Int32.Parse(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert method calls applied to built-in numeric types (java.lang.Integer, etc) -->
   <xsl:template match="*[@RESOLVED_TYPE=('java.lang.Long', 'java.lang.Double')]" 
      priority="30" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'parseLong'">
            <xsl:text>System.Int64.Parse(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'toHexString'">
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>).ToString("X")</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert method calls applied to Exception classes -->
   <xsl:template match="*[exists(@RESOLVED_TYPE) and f:is-exception-type(@RESOLVED_TYPE)]" priority="20">
      <!--<xsl:message>Exception.{name/@identifier}()</xsl:message>-->
      <xsl:choose>
         <xsl:when test="name/@identifier = 'getMessage'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Message</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'getCause'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.InnerException</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'printStackTrace'">
            <xsl:text>Console.Write(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.ToString() + </xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.StackTrace)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="name/@identifier" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert .ordinal() and .values() methods on an enumeration value -->
   
   <xsl:template match="*[name/@identifier=('ordinal', 'values')]
         [empty(arguments/argument)]
         [key('digestClassKey', @RESOLVED_TYPE, $digest)[self::enum]]" 
      priority="30" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier='ordinal'">
            <xsl:text>((int)</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier='values'">
            <xsl:text>System.Enum.GetValues(typeof({@RESOLVED_TYPE}))</xsl:text>
         </xsl:when>
      </xsl:choose>
      
   </xsl:template>

   
   <!-- Convert method calls applied to java.lang.Class (which becomes System.Type) -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.lang.Class' or starts-with(@RESOLVED_TYPE, 'java.lang.Class&lt;')]" 
      priority="30" mode="methods">
      <!--<xsl:message>Class.{name/@identifier}()</xsl:message>-->
      <xsl:variable name="renamedMethods" as="map(*)" select="map{}"/>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'getName'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Name</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'getSimpleName'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Name</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'getSuperClass'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.BaseType</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'isArray'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.IsArray</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'isInterface'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.IsInterface</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'newInstance'">
            <xsl:variable name="classExpr">
               <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            </xsl:variable>
            <xsl:text>System.Activator.CreateInstance({$classExpr})</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert method calls applied to java.lang.Math (which becomes System.Math) -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.lang.Math']" priority="30" mode="methods">
      <!--<xsl:message>Class.{name/@identifier}()</xsl:message>-->
      <xsl:choose>
         <xsl:when test="name/@identifier = 'random' and count(arguments/argument)=0">
            <xsl:text>(new System.Random()).NextDouble()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'round' and count(arguments/argument)=1">
            <xsl:text>(long)(System.Math.Round(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>, System.MidpointRounding.ToPositiveInfinity)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>System.Math.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert method calls applied to java.math.BigInteger (which becomes System.Numerics.BigInteger) -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.math.BigInteger']" 
      priority="20" mode="methods">
    <!--  <xsl:message>BigInteger.{name/@identifier}()</xsl:message>-->
      <xsl:variable name="renamedMethods" as="map(*)" select="map{}"/>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'add'">
            <xsl:text>System.Numerics.BigInteger.Add(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'subtract'">
            <xsl:text>System.Numerics.BigInteger.Subtract(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'multiply'">
            <xsl:text>System.Numerics.BigInteger.Multiply(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'divide'">
            <xsl:text>System.Numerics.BigInteger.Divide(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'negate'">
            <xsl:text>System.Numerics.BigInteger.Negate(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'mod'">
            <xsl:text>com.saxonica.glue.BigIntegerUtils.Mod(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'remainder'">
            <xsl:text>com.saxonica.glue.BigIntegerUtils.Remainder(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'divideAndRemainder'">
            <xsl:text>com.saxonica.glue.BigIntegerUtils.DivideAndRemainder(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'signum'">
            <xsl:text>com.saxonica.glue.BigIntegerUtils.Signum(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'valueOf'">
            <xsl:text>com.saxonica.glue.BigIntegerUtils.ValueOf(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'longValue'">
            <xsl:text>((long)</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'intValue'">
            <xsl:text>((int)</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'valueOf'">
            <xsl:text>new System.Numerics.BigInteger(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>

         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert method calls applied to java.math.BigDecimal (which becomes System.Numerics.BigDecimal) -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.math.BigDecimal']" 
      priority="20" mode="methods">
      <!--<xsl:message>BigDecimal.{name/@identifier}()</xsl:message>-->
      <xsl:variable name="renamedMethods" as="map(*)" select="map{}"/>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'add'">
            <xsl:text>System.Numerics.BigDecimal.Add(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'subtract'">
            <xsl:text>System.Numerics.BigDecimal.Subtract(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'multiply'">
            <xsl:text>System.Numerics.BigDecimal.Multiply(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'divide'">
            <xsl:text>System.Numerics.BigDecimal.Divide(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'negate'">
            <xsl:text>System.Numerics.BigDecimal.Negate(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'mod'">
            <xsl:text>com.saxonica.glue.BigDecimalUtils.Mod(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'remainder'">
            <xsl:text>com.saxonica.glue.BigDecimalUtils.Remainder(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'divideAndRemainder'">
            <xsl:text>com.saxonica.glue.BigDecimalUtils.DivideAndRemainder(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'signum'">
            <xsl:text>com.saxonica.glue.BigDecimalUtils.Signum(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'valueOf'">
            <xsl:text>com.saxonica.glue.BigDecimalUtils.ValueOf(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'intValue'">
            <xsl:text>((int)</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'longValueExact'">
            <xsl:text>((long)</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'setScale'">
            <xsl:text>com.saxonica.glue.BigDecimalUtils.SetScale(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'stripTrailingZeros'">
            <xsl:text>com.saxonica.glue.BigDecimalUtils.StripTrailingZeros(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'toBigInteger'">
            <xsl:text>((System.Numerics.BigInteger)</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'doubleValue'">
            <xsl:text>System.Double.Parse(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.ToString())</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'floatValue'">
            <xsl:text>System.Float.Parse(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'longValue'">
            <xsl:text>((long)</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'valueOf'">
            <xsl:text>new System.Numerics.BigDecimal(</xsl:text>
            <xsl:apply-templates select="arguments"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         
         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Special-case Character.getType() -->
   <!-- Note, the result is of type UnicodeCategory, which will cause problems if it is bound to a variable -->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.lang.Character')]" 
      priority="20" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'getType'">
            <!-- TODO: this assumes .NET 5; earlier versions only accept a char) -->
            <xsl:text>System.Globalization.CharUnicodeInfo.GetUnicodeCategory((char)</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   

   <!-- Special-case EnumSet.noneOf(Class<E> x) and EnumSet.of(Class.ENUM)-->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.util.EnumSet')]" 
      priority="20" mode="methods">
    <!--  <xsl:message>EnumSet.{name/@identifier}()</xsl:message>-->
      <xsl:choose>
         <xsl:when test="name/@identifier = 'noneOf' and arguments/argument[1]/@nodeType='ClassExpr'">
            <xsl:text>java.util.EnumSet&lt;{arguments/argument[1]/type/@RESOLVED_TYPE}>.noneOf()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'of'  and arguments/argument[1]/@nodeType='FieldAccessExpr'">
            <xsl:text>java.util.EnumSet&lt;{arguments/argument[1]/@RESOLVED_TYPE}>.of(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Special-case List.remove, which causes problems because Java has both
      List.remove(int) and List.remove(object), which conflict in C# -->
   <xsl:template match="*[f:degenerify(@RESOLVED_TYPE) = ('java.util.List', 'java.util.ArrayList', 'java.util.LinkedList')]
      [name/@identifier='remove'][count(arguments/argument)=1][arguments/argument[1][@nodeType='IntegerLiteralExpr' or @RESOLVED_TYPE='int' or @operator=('PLUS', 'MINUS')]]" 
      priority="20" mode="methods">
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>.removeAt(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Special-case Collections.emptyIterator() and similar methods -->
   <!-- Reuse the "diamond" logic for determining the required type -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.util.Collections']" 
      priority="20" mode="methods">
     <!-- <xsl:message>Collections.{name/@identifier}()</xsl:message>-->
      <!--<xsl:variable name="targetClass" as="xs:string?">
         <xsl:choose>
            <xsl:when test="../@nodeType='ReturnStmt'">
               <xsl:sequence select="ancestor::member/type/typeArguments/typeArgument[1]/@RESOLVED_TYPE ! string()"/>
            </xsl:when>
         </xsl:choose>
      </xsl:variable>-->
      <xsl:variable name="targetClass" as="xs:string">
         <xsl:apply-templates select="." mode="diamond"/>
      </xsl:variable>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'emptyIterator' and $targetClass">
            <xsl:text>java.util.Collections.emptyIterator&lt;{f:rename-generic-type($targetClass)}>()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'emptyList' and $targetClass">
            <xsl:text>java.util.Collections.emptyList&lt;{f:rename-generic-type($targetClass)}>()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'emptySet' and $targetClass">
            <xsl:text>java.util.Collections.emptySet&lt;{f:rename-generic-type($targetClass)}>()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'emptyMap' and $targetClass">
            <xsl:variable name="targetClass2" select="string(ancestor::member/type/typeArguments/typeArgument[2]/@RESOLVED_TYPE)"/>
            <xsl:text>java.util.Collections.emptyMap&lt;{f:rename-generic-type($targetClass), f:rename-generic-type($targetClass2)}>()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'singleton' and $targetClass">
            <xsl:text>java.util.Collections.singleton&lt;{f:rename-generic-type($targetClass)}>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Special-case Optionial.empty() and Optional.of() -->
   <!-- Currently handled only when appearing in a return statement, the type is taken from the containing method declaration -->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.util.Optional')]" 
      priority="20" mode="methods">
      <!--<xsl:message>Optional.{name/@identifier}()</xsl:message>-->
      <xsl:variable name="targetClass" as="xs:string?">
         <xsl:choose>
            <xsl:when test="../@nodeType='ReturnStmt'">
               <xsl:sequence select="ancestor::member/type/typeArguments/typeArgument[1]/@RESOLVED_TYPE ! string()"/>
            </xsl:when>
         </xsl:choose>
      </xsl:variable>
      <xsl:choose>
         <xsl:when test="name/@identifier = ('empty', 'of') and $targetClass">
            <xsl:text>java.util.Optional&lt;{f:rename-generic-type($targetClass)}>.{name/@identifier}(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <xsl:template match="*" priority="10" mode="methods">
      <xsl:variable name="targetType" select="(@RESOLVED_TYPE,@DECLARING_TYPE)[1]" as="xs:string?"/>
      <xsl:variable name="methodName" select="name/@identifier"/>
      <xsl:variable name="arity" select="count(arguments/argument)"/>
      <xsl:variable name="digestMethod" select="f:candidateMethods($targetType, $methodName, $arity)"/>
      <!--<xsl:message>Method call = {$methodName}#{$arity} digestMethods={count($digestMethod)} 
         returns={$digestMethod/@returns} csReturns={$digestMethod/@csReturns}</xsl:message>-->
      <!-- Be prepared to cast the result to the required type in the case where the C# method returns a supertype of 
           the return type of the corresponding Java method -->
      <xsl:variable name="optionalCast" 
         select="if (count($digestMethod)=1 and $digestMethod[@returns != @csReturns] and not(../@nodeType='ExpressionStmt')) 
                 then $digestMethod/@returns 
                 else ''"/>
      <!--<xsl:message> cast = {$optionalCast}</xsl:message>-->
      <xsl:if test="$optionalCast">(({$optionalCast})</xsl:if>
      <xsl:if test="scope">
         <xsl:choose>
           <xsl:when test="scope/name/@nodeType='SimpleName' and not (scope/@UNRESOLVED) and ends-with(f:degenerify(@RESOLVED_TYPE), '.'||scope/name/@identifier) and not(scope/scope)">
               <!-- A simple static method call -->
               <xsl:text>{f:escapeDottedName(@RESOLVED_TYPE)}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            </xsl:otherwise>
         </xsl:choose>         
         <xsl:text>.</xsl:text>         
      </xsl:if>
      <xsl:apply-templates select="typeArguments" mode="xml-to-cs"/>
      <xsl:variable name="property" select="f:method-as-property(.)"/>
        
      <xsl:choose>
         <xsl:when test="$property">{$property}</xsl:when>     
         <xsl:otherwise>           
            <xsl:apply-templates select="name" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text> 
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>            
         </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="$optionalCast">)</xsl:if>
   </xsl:template>
   


</xsl:stylesheet>