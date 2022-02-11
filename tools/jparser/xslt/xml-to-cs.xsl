<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   version="3.0"
   expand-text="yes"
   default-mode="xml-to-cs">
   
   <xsl:include href="diamond.xsl"/>
   <xsl:include href="methods.xsl"/>
   <xsl:include href="fields.xsl"/>
   
   <!-- 
      This stylesheet takes the XML file produced as the output of the javaParser
      (representing a single Java source file), and converts it to C# syntax, which
      is output as text in the principal result file.
      
      Some of the conversions needed require access to interface information from
      other modules (for example default methods provided in interfaces), and this
      is made available via a digest file.
   -->
   
   <xsl:param name="digest" as="document-node(element(digest))" required="yes"/>
   
   <xsl:output method="text"/>
   <xsl:strip-space elements="*"/>
   <xsl:variable name="NL" select="'&#xa;'" as="xs:string"/>
   
   <xsl:variable name="reservedWords" as="xs:string*" select="tokenize('
      abstract as base bool break byte case catch char checked class const continue
      decimal default delegate do double else enum event explicit extern false
      finally fixed float for foreach goto if implicit in int interface internal
      is lock long namespace new null object operator out override params private
      protected public readonly ref return sbyte sealed short sizeof stackalloc
      static string struct switch this throw true try typeof uint ulong
      unchecked unsafe ushort using virtual void volatile while')"/>
   
   <xsl:variable name="renamings" as="map(xs:string, xs:string)" select="map{
      'equals':'Equals',
      'hashCode':'GetHashCode',
      'toString':'ToString',
      'getClass':'GetType',     
      'java.lang.System#out' : 'Out',
      'java.lang.System#in' : 'In' (:,
      'java.util.Map#get' : 'Get',
      'java.util.Map#put' : 'Put' :)}"/>
   
   <xsl:variable name="specialTypes" as="map(xs:string, xs:string)" select="map{
      'java.lang.ArrayIndexOutOfBoundsException': 'System.IndexOutOfRangeException',
      'java.lang.Boolean': 'System.Boolean',
      'java.lang.CharSequence': 'string',
      'java.lang.Class': 'System.Type',
      'java.lang.Exception': 'System.Exception',
      'java.lang.IndexOutOfBoundsException': 'System.IndexOutOfRangeException',
      'java.lang.Integer': 'System.Int32',
      'java.lang.Long': 'System.Int64',
      'java.lang.Object': 'System.Object',
      'java.lang.RuntimeException': 'System.Exception',
      'java.lang.Short': 'System.Int16',
      'java.lang.StackOverflowError': 'System.StackOverflowException',
      'java.lang.String': 'string',
      'java.lang.StringIndexOutOfBoundsException': 'System.IndexOutOfRangeException',
      'java.lang.System': 'System.Console',
      'java.lang.InterruptedException': 'System.Threading.ThreadInterruptedException',
      'java.lang.Throwable': 'System.Exception',
      'java.math.BigDecimal': 'System.Numerics.BigDecimal',
      'java.math.BigInteger': 'System.Numerics.BigInteger',
      'java.util.function.Function': 'System.Func',
      'java.util.function.Predicate': 'System.Predicate',
      'java.util.Map.Entry': 'java.util.Map_Entry',
      'net.sf.saxon.ma.arrays.ImmutableArrayItem': 'Saxon.Impl.DataModel.ImmArray'
      }"/>
   
   <xsl:variable name="jdkExceptionTypes" as="xs:string*" select="
      'java.lang.ArrayIndexOutOfBoundsException',
      'java.lang.Exception',
      'java.lang.IndexOutOfBoundsException',
      'java.lang.RuntimeException',
      'java.lang.StackOverflowError',
      'java.lang.StringIndexOutOfBoundsException',
      'java.lang.InterruptedException',
      'java.lang.Throwable'
      "/>
   
   <xsl:variable name="dropTypeParameters" as="xs:string*" select="tokenize('System.Type')"/>

   
   <xsl:variable name="specialProperties" as="map(*)">
      <!--<xsl:variable name="json" as="xs:string" expand-text="no">
         { "java.util.Map.Entry": {"getKey": "Key", "getValue": "Value"},
           "java.util.Map": {"keys": "Keys", "values": "Values"},
           "java.util.HashMap": {"keys": "Keys", "values": "Values"}
         }    
      </xsl:variable>
      <xsl:sequence select="parse-json($json)"/>-->
      <xsl:sequence select="map{}"/>
   </xsl:variable>
   

   
 <!--  <xsl:variable name="refactoredProperties" as="map(*)">
      <!-\- Not actually used: the "class" property is parsed as a ClassExpr -\->
      <xsl:variable name="json" as="xs:string" expand-text="no">
         { "class": "typeof($0)" 
         }    
      </xsl:variable>
      <xsl:sequence select="parse-json($json)"/>
   </xsl:variable>
   -->
   
   <xsl:mode on-no-match="fail"/>
   <!--<xsl:mode on-no-match="text-only-copy"/>-->
   <!--<xsl:template match="*">
      <xsl:text>*** {name()}/{@nodeType} ***</xsl:text>
   </xsl:template>-->
   
   <xsl:function name="f:qualified-name" as="xs:string">
      <xsl:param name="in" as="element(*)"/>
      <xsl:variable name="term" as="xs:string"
         select="$in!(if (@identifier = $reservedWords) then ('@' || @identifier) else ($renamings(@identifier), @identifier)[1])"/>
      <xsl:sequence select="(if ($in/qualifier) then f:qualified-name($in/qualifier) || '.' else '') || $term"/>
   </xsl:function>
   
   <xsl:function name="f:full-method-name" as="xs:string">
      <!-- Given a MethodDeclaration node, return the full method name in the form package.class[.class]*#method!params -->
      <xsl:param name="decl" as="element()"/>
      <!--<xsl:assert test="$decl/@nodeType = 'MethodDeclaration'"/>
      <xsl:assert test="exists($decl/ancestor::root/packageDeclaration/name)">Method: {path($decl)} in class: {$decl/ancestor::type[1]/name/@identifier}</xsl:assert>
      -->
      <xsl:for-each select="$decl">
         <xsl:choose>
            <xsl:when test="parent::default">
               <!-- this is a default method declaration in the digest -->
               <xsl:text>{ancestor::module[1]/@package}.{ancestor::interface[1]/@name}#{ancestor::method[1]/(@name || '!' || @sig)}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:variable name="unescaped" as="xs:string">
                  <xsl:value-of>
                     <xsl:text>{f:qualified-name(ancestor::root/packageDeclaration/name)}.</xsl:text>
                     <xsl:value-of select="ancestor::*[@nodeType='ClassOrInterfaceDeclaration']/name/@identifier" separator="."/>
                     <xsl:text>#{name/@identifier}!</xsl:text>
                     <xsl:value-of select="parameters/parameter/type/(name/@identifier, @type)[1]" separator=","/>
                  </xsl:value-of>
               </xsl:variable>
               <xsl:sequence select="translate($unescaped, '@', '')"/>
            </xsl:otherwise>
         </xsl:choose>
      </xsl:for-each>
   </xsl:function>
   
   <xsl:function name="f:full-class-name" as="xs:string">
      <xsl:param name="classElement" as="element()*"/>
      <xsl:assert test="$classElement/@nodeType = 'ClassOrInterfaceDeclaration'"/>
      <xsl:sequence
         select="$classElement!string-join((f:qualified-name(ancestor::root/packageDeclaration/name), 
         ancestor-or-self::*[@nodeType='ClassOrInterfaceDeclaration']/name/@identifier), '.')"/>
   </xsl:function>
   
   <xsl:function name="f:is-exception-type" as="xs:boolean">
      <xsl:param name="type" as="xs:string?"/>
      <xsl:sequence select="$jdkExceptionTypes = $type or ends-with($type, 'Exception')"/>
   </xsl:function>
   
   <xsl:function name="f:rename-method" as="xs:string">
      <xsl:param name="identifier" as="xs:string"/>
      <xsl:param name="context" as="element(*)"/>
      <xsl:variable name="escaped" select="f:escapeSimpleName($identifier)"/>
      <xsl:variable name="expanded" select="f:degenerify($context/@RESOLVED_TYPE) || '#' || $escaped"/>     
      <xsl:choose>
         <xsl:when test="$renamings($identifier)">{$renamings($identifier)}</xsl:when>
         <xsl:when test="$renamings($expanded)">{$renamings($expanded)}</xsl:when>
         <xsl:otherwise>{$escaped}</xsl:otherwise>
      </xsl:choose>
   </xsl:function>
   
   <xsl:function name="f:rename-type" as="xs:string">
      <xsl:param name="identifier" as="xs:string"/>
      <xsl:variable name="degen" select="f:degenerify($identifier)" as="xs:string"/>
      <!--<xsl:if test="$degen = 'java.util.function.Predicate'">
         <xsl:message>Renaming {$degen} to {$specialTypes($degen)}</xsl:message>
         <xsl:message>$specialTypes = {serialize($specialTypes, map{'method':'json'})}</xsl:message>
      </xsl:if>-->
      <xsl:sequence select="($specialTypes($degen), $degen)[1]"/>
   </xsl:function>
   
   <xsl:function name="f:rename-generic-type" as="xs:string">
      <xsl:param name="identifier" as="xs:string"/>
      <xsl:value-of>
         <xsl:text>{f:rename-type($identifier)}</xsl:text>
         <xsl:if test="contains($identifier, '&lt;')">
            <xsl:text>&lt;</xsl:text>
            <xsl:value-of select="($identifier => substring-after('&lt;') => substring-before('>') => tokenize(',')) ! f:rename-type(.)"
               separator=", "/>
            <xsl:text>&gt;</xsl:text>
         </xsl:if>
      </xsl:value-of>
   </xsl:function>
   
   <xsl:function name="f:degenerify" as="xs:string?">
      <xsl:param name="nameOfType" as="xs:string?"/>
      <xsl:sequence select="replace($nameOfType, '^\? (extends|super) ', '') => replace('&lt;.*$', '') => f:escapeDottedName()"/>
   </xsl:function>
   
   <xsl:function name="f:escapeSimpleName" as="xs:string">
      <xsl:param name="simpleName" as="xs:string"/>
      <xsl:sequence select="if ($simpleName = $reservedWords) then ('@' || $simpleName) else $simpleName"/>
   </xsl:function>
   
   <xsl:function name="f:escapeDottedName" as="xs:string">
      <xsl:param name="nameOfType" as="xs:string?"/>
      <xsl:sequence select="tokenize($nameOfType, '\.') ! f:escapeSimpleName(.) => string-join('.')"/>
   </xsl:function>
   
   <!-- Convert first character of a string to upper-case -->
   <xsl:function name="f:capitalize" as="xs:string">
      <xsl:param name="in" as="xs:string"/>
      <xsl:sequence select="upper-case(substring($in, 1, 1)) || substring($in, 2)"/>
   </xsl:function>
   
   
   
   <!-- special-case getKey() and getValue() in KeyValuePair -->
   <xsl:function name="f:method-as-property" as="xs:string?">
      <xsl:param name="expr" as="element(*)"/>
      <xsl:variable name="simpleName" select="f:degenerify($expr/@RESOLVED_TYPE)"/>
      <xsl:sequence select="$specialProperties($simpleName)?($expr/name/@identifier)"/>
   </xsl:function>
   
   
   
   <!--<xsl:function name="f:refactor-method-call" as="xs:string">
      <xsl:param name="call" as="element(*)"/>
      <xsl:param name="identifier" as="xs:string"/>
      <xsl:value-of>
         <xsl:analyze-string select="$refactoredMethods($identifier)" regex="\$0">
            <xsl:matching-substring>
               <xsl:apply-templates select="$call/scope"/>
            </xsl:matching-substring>
            <xsl:non-matching-substring>{.}</xsl:non-matching-substring>
         </xsl:analyze-string>
      </xsl:value-of>
   </xsl:function>
   -->
<!--   <xsl:function name="f:refactor-property-reference" as="xs:string">
      <xsl:param name="call" as="element(*)"/>
      <xsl:param name="identifier" as="xs:string"/>
      <xsl:value-of>
         <xsl:analyze-string select="$refactoredProperties($identifier)" regex="\$0">
            <xsl:matching-substring>
               <xsl:apply-templates select="$call/scope"/>
            </xsl:matching-substring>
            <xsl:non-matching-substring>{.}</xsl:non-matching-substring>
         </xsl:analyze-string>
      </xsl:value-of>
   </xsl:function>-->
   
   <!-- Ask whether the first class implements or extends the second class.
        Both classes are supplied by fully-qualified name, without type arguments.
        The first class must be present in the digest, i.e. must be a Saxon Java class -->
   <xsl:function name="f:extends" as="xs:boolean">
      <xsl:param name="subclass" as="xs:string"/>
      <xsl:param name="superclass" as="xs:string"/>
      <xsl:variable name="digestClass" as="element(*)?"
         select="key('digestClassKey', $subclass, $digest)"/>
      <xsl:choose>
         <xsl:when test="$subclass eq $superclass">
            <xsl:sequence select="true()"/>
         </xsl:when>
         <xsl:when test="empty($digestClass)">
            <xsl:sequence select="false()"/>
         </xsl:when>
         <xsl:when test="$digestClass/(extends|implements)/@name = $superclass">
            <xsl:sequence select="true()"/>
         </xsl:when>
         <xsl:when test="some $name in $digestClass/(extends|implements)/@name satisfies f:extends($name, $superclass)">
            <xsl:sequence select="true()"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="false()"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>
   
   <xsl:template name="indent">
      <xsl:param name="indent" as="xs:integer" tunnel="yes" select="0"/>
      <xsl:value-of select="string-join((1 to $indent*4)!' ', '')"/>
   </xsl:template>
   
   <xsl:template name="indent-extra">
      <xsl:param name="indent" as="xs:integer" tunnel="yes" select="0"/>
      <xsl:value-of select="string-join((1 to ($indent+1)*4)!' ', '')"/>
   </xsl:template>
   
   <xsl:template name="indent-plus-two">
      <xsl:param name="indent" as="xs:integer" tunnel="yes" select="0"/>
      <xsl:value-of select="string-join((1 to ($indent+2)*4)!' ', '')"/>
   </xsl:template>
   
   <xsl:template match="root">
      <xsl:text>using System;{$NL}</xsl:text>
      <xsl:text>using System.Collections.Generic;{$NL}</xsl:text>
      <xsl:apply-templates select="packageDeclaration"/>
      <xsl:text>{{{$NL}</xsl:text>
      <xsl:apply-templates select="* except packageDeclaration">
         <xsl:with-param name="indent" select="1" tunnel="yes"/>
         <xsl:with-param name="include-nested-interfaces" select="false()" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:text>}}</xsl:text>
      
      <!-- Process any nested interfaces as top-level interfaces -->
      <!-- Note, we're not attempting to handle more than one level of nesting -->
      <xsl:apply-templates select="types/type[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='true']/members/member[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='true']">
         <xsl:with-param name="indent" select="1" tunnel="yes"/>
         <xsl:with-param name="exclude-nested-interfaces" select="true()" tunnel="yes"/>
      </xsl:apply-templates>
   </xsl:template>

   <xsl:template match="packageDeclaration">
      <xsl:text>namespace {f:qualified-name(name)} </xsl:text>
   </xsl:template>
   
   <xsl:template match="import"/>
   
   <xsl:template match=".[. instance of xs:string]">
      <xsl:value-of select="."/>
   </xsl:template>
   
   <!--<xsl:template match="import[@isStatic='true']" priority="20">
      <xsl:text>//*** Can't handle static imports: {f:qualified-name(name)} *** {$NL}</xsl:text>
   </xsl:template>
   -->
   <!--<xsl:template match="import[@isAsterisk='true']" priority="10">
      <xsl:call-template name="indent"/>
      <xsl:text>using {f:qualified-name(name)};{$NL}</xsl:text>
   </xsl:template>-->
   
   <!--<xsl:template match="import">
      <xsl:call-template name="indent"/>
      <xsl:text>using {name/@identifier} = {f:qualified-name(name)};{$NL}</xsl:text>
   </xsl:template>
   -->
   <xsl:template match="name | *[@nodeType='Name']">
      <xsl:value-of select="f:rename-method(@identifier, ..)"/>
   </xsl:template>
   
   <!--<xsl:template match="name | *[@nodeType='Name']" mode="sans-scope">
      <xsl:value-of select="f:qualified-name(.)"/>
   </xsl:template>-->
   
   <xsl:template match="scope">
      <!--<xsl:message>Default scope rule {@RESOLVED_TYPE}</xsl:message>-->
      <xsl:apply-templates select="*"/>
   </xsl:template>
   
   <!-- This rule compensates for an inadequacy in the JavaParser symbol solver -->
   <xsl:template match="scope[@UNRESOLVED and ends-with(../@RESOLVED_TYPE, @UNRESOLVED)]" priority="10">
      <!--<xsl:message>Unresolved scope rule {@UNRESOLVED}</xsl:message>-->
      <xsl:text>{f:escapeDottedName(../@RESOLVED_TYPE)}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'EnclosedExpr']">
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="inner"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'SuperExpr']">
      <xsl:if test="typeName">
         <xsl:apply-templates select="typeName"/>
         <xsl:text>.</xsl:text>
      </xsl:if>
      <xsl:text>base</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'ClassOrInterfaceType']" priority="20">
      <xsl:apply-templates select="name, typeArguments"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'ClassOrInterfaceType'][@RESOLVED_TYPE]" priority="21">
      <xsl:variable name="renamed" as="xs:string" select="f:rename-type(@RESOLVED_TYPE)"/>
      <!--<xsl:message>Type {@RESOLVED_TYPE} renamed as {$renamed}</xsl:message>-->
      <xsl:value-of select="$renamed"/>
      <xsl:if test="not($dropTypeParameters = $renamed)">
         <xsl:apply-templates select="typeArguments"/>
      </xsl:if>
   </xsl:template>
   
   <xsl:template name="classOrInterface">
      <xsl:param name="indent" as="xs:integer" tunnel="yes"/>
      <xsl:param name="include-nested-interfaces" select="false()" tunnel="yes"/>
      <xsl:assert test="@nodeType='ClassOrInterfaceDeclaration'"/>
      <xsl:variable name="kind" select="if (@isInterface='true') then 'interface' else 'class'" as="xs:string"/>
      <xsl:variable name="containing-interface" select="ancestor::*[@isInterface='true'][1]" as="element(*)?"/>
      <xsl:if test="$kind = 'class' or $include-nested-interfaces or empty($containing-interface)">
         <!-- that is, process everything other than a nested inner interface -->
         <xsl:text>{$kind} {if (exists($containing-interface)) then ($containing-interface/name/@identifier || '_') else ''}{name/@identifier}</xsl:text>
         <xsl:apply-templates select="typeParameters"/>
         <!-- If the class implements java.lang.Iterable, then make it also implement IEnumerable so it
              can be used in a foreach loop -->
         <xsl:variable name="additionalSuperTypes" as="element(*)*">
            <xsl:for-each select="extendedTypes/extendedType, implementedTypes/implementedType">
               <xsl:if test="starts-with(@RESOLVED_TYPE, 'java.lang.Iterable')">
                  <implementedType 
                     nodeType="ClassOrInterfaceType" 
                     RESOLVED_TYPE="{replace(@RESOLVED_TYPE, 'java\.lang\.Iterable', 'System.Collections.Generic.IEnumerable')}">
                     <name nodeType="SimpleName" identifier="IEnumerable"/>
                     <xsl:copy-of select="typeArguments"/>
                  </implementedType>
               </xsl:if>              
            </xsl:for-each>
            <xsl:for-each select="annotations/annotation[name/@identifier='CSharpExtendInterface']/pairs/pair/value">
               <!--<xsl:message>Extending with {@value}</xsl:message>-->
               <implementedType nodeType="ClassOrInterfaceType" RESOLVED_TYPE="{@value}"/>                    
            </xsl:for-each>  
         </xsl:variable>
         
         <xsl:if test="extendedTypes/extendedType, implementedTypes/implementedType, $additionalSuperTypes">
            <xsl:text> : </xsl:text>
            <xsl:for-each select="extendedTypes/extendedType, implementedTypes/implementedType, $additionalSuperTypes">
               <xsl:if test="position() ne 1">, </xsl:if>
               <xsl:apply-templates select="."/>
            </xsl:for-each>
         </xsl:if>
         <xsl:text> {{{$NL}</xsl:text>
         <xsl:if test="members/member[@nodeType = 'InitializerDeclaration'][@isStatic = 'true']">
            <xsl:text>{$NL}</xsl:text>
            <xsl:call-template name="indent-extra"/>
            <xsl:text>static {name/@identifier}() {{</xsl:text>
            <xsl:text>{$NL}</xsl:text>
            <xsl:apply-templates select="members/member[@nodeType = 'InitializerDeclaration'][@isStatic = 'true']/*">
               <xsl:with-param name="indent" select="$indent+2" tunnel="yes"/>
            </xsl:apply-templates>
            <xsl:call-template name="indent-extra"/>
            <xsl:text>}}{$NL}</xsl:text>
         </xsl:if>
         <xsl:if test="exists(annotations/annotation[name/@identifier='CSharpInjectMembers'])">    // Injecting C# code{$NL}</xsl:if>
         <xsl:for-each select="annotations/annotation[name/@identifier='CSharpInjectMembers']//*[@nodeType='StringLiteralExpr']">
            <xsl:text>        {@value}{$NL}</xsl:text>                    
         </xsl:for-each>
         <xsl:if test="exists(annotations/annotation[name/@identifier='CSharpInjectMembers'])">    // End of injected C# code{$NL}</xsl:if>
         
         <xsl:apply-templates select="members/member">
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
         </xsl:apply-templates>
         <xsl:call-template name="indent"/>
         <xsl:apply-templates select=".//anonymousClassBody">
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
         </xsl:apply-templates>
         <xsl:if test="not(@isInterface='true')">
            <xsl:call-template name="expand-defaulted-methods"/>
         </xsl:if>
         <!--<xsl:call-template name="indent"/>-->
         <xsl:text>}}{$NL}</xsl:text>
      </xsl:if>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ClassOrInterfaceDeclaration']">
      <xsl:call-template name="indent"/>
      <xsl:call-template name="modifiers"/>
      <xsl:call-template name="classOrInterface"/>
   </xsl:template>
   
   <xsl:function name="f:isDelegate" as="xs:boolean">
      <xsl:param name="interfaceDecl" as="element()"/>
      <xsl:choose>
         <xsl:when test="$interfaceDecl/annotations/annotation/name/@identifier='CSharpDelegate'">
            <xsl:sequence select="xs:boolean($interfaceDecl/annotations/annotation[name/@identifier='CSharpDelegate']/memberValue/@value)"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="exists($interfaceDecl[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='true']
               [annotations/annotation/name/@identifier='FunctionalInterface'][count(members/member)=1])"/>
         </xsl:otherwise>
      </xsl:choose>      
   </xsl:function>
   
   <!-- Convert functional interfaces into C# delegates -->
   <xsl:template match="*[f:isDelegate(.)]" priority="20">
      <!-- see for example net.sf.saxon.om.ItemConsumer -->
      <!-- we generate for example 'public delegate void ItemConsumer<T> (T item)'
           where the name (ItemConsumer<T>) comes from the class/iterface name,
           and the signature (void ... (T item)) comes from the single method -->
      <xsl:call-template name="indent"/>
      <xsl:call-template name="modifiers"/>
      <xsl:text>delegate </xsl:text>
      <xsl:apply-templates select="members/member/type"/>
      <xsl:text> </xsl:text>
      <xsl:apply-templates select="name"/>
      <xsl:apply-templates select="typeParameters"/>
      <xsl:text> (</xsl:text>
      <xsl:apply-templates select="members/member/parameters"/>
      <xsl:text>);{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='FieldDeclaration']">
      <xsl:call-template name="indent"/>
      <xsl:variable name="isConstant" select="
         count(variables/variable)=1 
         and variables/variable/initializer 
         and f:isConstantExpression(variables/variable/initializer)
         and modifiers/modifier[@keyword='STATIC']
         and modifiers/modifier[@keyword='FINAL']"/>
      <xsl:call-template name="modifiers">
         <xsl:with-param name="isConstant" select="$isConstant" tunnel="yes"/>
      </xsl:call-template>
      <xsl:apply-templates select="variables"/>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ConstructorDeclaration']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:call-template name="modifiers"/>
      <xsl:text>{name/@identifier}(</xsl:text>
      <xsl:apply-templates select="parameters"/>
      <xsl:text>) </xsl:text>
      <xsl:apply-templates select="thrownExceptions"/>
      <xsl:variable name="firstStatement" select="body/statements/statement[1]"/>
      <xsl:if test="$firstStatement/@nodeType='ExplicitConstructorInvocationStmt'">
         <xsl:text>: {if ($firstStatement/@isThis = 'false') then 'base' else 'this'} (</xsl:text>
         <xsl:apply-templates select="$firstStatement/arguments"/>
         <xsl:text>)</xsl:text>
      </xsl:if>
      
      
      <xsl:text> {{{$NL}</xsl:text>
      <xsl:apply-templates select="body/statements">
         <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodDeclaration']
      [name/@identifier = 'iterator']
      [starts-with(type/@RESOLVED_TYPE, 'java.util.Iterator')]
      [empty(parameters/parameter)]" priority="20">
      <!-- If there's a method to implement java.lang.Iterable, then add a method to implement IEnumerable as well -->
      <xsl:next-match/>
      <xsl:call-template name="indent"/>
      <xsl:text>// Extra methods to make the class IEnumerable as well as Iterable{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>public {replace(type/@RESOLVED_TYPE, 'java\.util\.Iterator', 'System.Collections.Generic.IEnumerator')} GetEnumerator() {{{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>return new com.saxonica.glue.EnumeratorFromIterator{replace(type/@RESOLVED_TYPE, '^[^&lt;]+', '')}(iterator());{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {{{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>return GetEnumerator();{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodDeclaration']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:param name="expanding-defaults" select="false()" tunnel="yes" as="xs:boolean"/>
      <xsl:call-template name="indent"/>
      <xsl:variable name="full-name" select="f:full-method-name(.)"/>
      <!--<xsl:message>Method {name/@identifier} = {$full-name}</xsl:message>-->
      <xsl:variable name="digestMethod" select="key('digestMethodKey', $full-name, $digest)[1]"/>
      <xsl:if test="empty($digestMethod)">
         <xsl:message>*** Digest not found for method {name/@identifier} full name {$full-name}</xsl:message>
      </xsl:if>
      <xsl:if test="count($digestMethod) gt 1">
         <xsl:message>*** Multiple digests found for method name {$full-name} - using the first</xsl:message>
      </xsl:if>
      <xsl:if test="body and $expanding-defaults and not(modifiers/modifier/@keyword[. = 'PUBLIC'])">public </xsl:if>
      <xsl:call-template name="modifiers">
         <xsl:with-param name="digestMethod" select="$digestMethod"/>
      </xsl:call-template>
      <xsl:choose>
         <xsl:when test="$digestMethod/@csReturns">{f:escapeDottedName($digestMethod/@csReturns)}</xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="type"/> <!-- TODO: take return type from digest? -->
         </xsl:otherwise>
      </xsl:choose>
      
      <xsl:text> {f:escapeSimpleName(name/@identifier)}</xsl:text>
      <xsl:apply-templates select="typeParameters"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="parameters"/>
      <xsl:text>)</xsl:text>
      <!--<xsl:apply-templates select="thrownExceptions"/>-->
      <xsl:choose>
         <xsl:when test="annotations/annotation[name/@identifier='CSharpReplaceBody']">
            <xsl:text> {{{$NL}</xsl:text>
            <xsl:call-template name="indent-extra"/>
            <xsl:text>{annotations/annotation[name/@identifier='CSharpReplaceBody']//value[@nodeType='StringLiteralExpr']}{$NL}</xsl:text>
            <xsl:call-template name="indent"/>
            <xsl:text>}}{$NL}</xsl:text>
         </xsl:when>
         <xsl:when test="body and modifiers/modifier/@keyword[. = 'SYNCHRONIZED']">
            <xsl:text> {{ </xsl:text>
            <xsl:choose>
               <xsl:when test="modifiers/modifier/@keyword[. = 'STATIC']">lock(typeof({ancestor::*[@nodeType='ClassOrInterfaceDeclaration']/name/@identifier}))</xsl:when>
               <xsl:otherwise>lock(this)</xsl:otherwise>
            </xsl:choose>
            <xsl:text> {{{$NL}</xsl:text>
            <xsl:apply-templates select="body/statements">
               <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
            </xsl:apply-templates>
            <xsl:call-template name="indent"/>
            <xsl:text>}}}}{$NL}</xsl:text>
         </xsl:when>
         <xsl:when test="body and ($expanding-defaults or not(modifiers/modifier/@keyword[. = 'DEFAULT']))">
            <xsl:text> {{{$NL}</xsl:text>
            <xsl:apply-templates select="body/statements">
               <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
            </xsl:apply-templates>
            <xsl:call-template name="indent"/>
            <xsl:text>}}{$NL}</xsl:text>
         </xsl:when>
         <xsl:otherwise>;{$NL}</xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodDeclaration'][annotations/annotation[name/@identifier='CSharpReplaceMethod']]" priority="30">
      <xsl:call-template name="indent"/>
      <xsl:text>{annotations/annotation[name/@identifier='CSharpReplaceMethod']//value[@nodeType='StringLiteralExpr']}{$NL}</xsl:text>
      <xsl:text>{$NL}</xsl:text>
   </xsl:template>
   
   <!-- Handle singleton enum classes. These are not true enumerations, it's
      a coding pattern for implementing singleton classes that are never instantiated.
      We generate a regular class with a private constructor -->
     
   <xsl:template match="*[@nodeType='EnumDeclaration'][count(entries/entrie) = 1]" priority="20">
      <xsl:param name="indent" as="xs:integer" tunnel="yes"/>
      <xsl:call-template name="indent"/>
      <xsl:call-template name="modifiers"/>
      <xsl:text>class {name/@identifier}</xsl:text>
      <xsl:if test="extendedTypes/extendedType, implementedTypes/implementedType">
         <xsl:text> : </xsl:text>
         <xsl:for-each select="extendedTypes/extendedType, implementedTypes/implementedType">
            <xsl:if test="position() ne 1">, </xsl:if>
            <xsl:apply-templates select="."/>
         </xsl:for-each>
      </xsl:if>
      <xsl:text> {{{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>// Private constructor (generated for singleton enum class){$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>private {name/@identifier}() {{}}{$NL}{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>// Singleton instance (generated for singleton enum class){$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>public static {name/@identifier} {entries/entrie/name/@identifier} = new {name/@identifier}();{$NL}{$NL}</xsl:text>
      
      <xsl:apply-templates select="members/member">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='EnumDeclaration']">
      <!-- Code for enum classes is inspired by the Tangible converter -->
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:call-template name="modifiers"/>
      <xsl:text>class </xsl:text>
      <xsl:variable name="className" select="name/@identifier" as="xs:string"/>
      <xsl:text> {$className} </xsl:text>
      <xsl:if test="extendedTypes/extendedType">
         <xsl:text>/* extends </xsl:text>
         <xsl:for-each select="extendedTypes/extendedType">
            <xsl:if test="position() ne 1">, </xsl:if>
            <xsl:apply-templates select="."/>
         </xsl:for-each>
         <xsl:text> */</xsl:text>
      </xsl:if>
      <xsl:if test="implementedTypes/implementedType">
         <xsl:text>/* implements </xsl:text>
         <xsl:for-each select="implementedTypes/implementedType">
            <xsl:if test="position() ne 1">, </xsl:if>
            <xsl:apply-templates select="."/>
         </xsl:for-each>
         <xsl:text> */</xsl:text>
      </xsl:if>
      <xsl:text> {{{$NL}</xsl:text>
      <xsl:for-each select="entries/entrie">
         <xsl:variable name="symbol" select="name/@identifier" as="xs:string"/>
         <xsl:call-template name="indent-extra"/>
         <xsl:text>public static readonly {$className} {$symbol}</xsl:text>
         <xsl:text> = new {$className}("{$symbol}", InnerEnum.{$symbol}</xsl:text>
         <xsl:for-each select="arguments/argument">
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="."/>           
         </xsl:for-each>
         <xsl:text>);{$NL}</xsl:text>
      </xsl:for-each>
      
      <xsl:call-template name="indent-extra"/>
      <xsl:text>private static readonly List&lt;{$className}> valueList = new List&lt;{$className}>();{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>static {$className}() {{{$NL}</xsl:text>
      <xsl:for-each select="entries/entrie">
         <xsl:call-template name="indent-plus-two"/>
         <xsl:text>valueList.Add({name/@identifier});{$NL}</xsl:text>
      </xsl:for-each>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>}}{$NL}</xsl:text>
      
      <xsl:call-template name="indent-extra"/>
      <xsl:text>public enum InnerEnum {{{$NL}</xsl:text>
      <xsl:for-each select="entries/entrie">
         <xsl:call-template name="indent-plus-two"/>
         <xsl:text>{name/@identifier},{$NL}</xsl:text>
      </xsl:for-each>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>}}{$NL}</xsl:text>
      
      <xsl:call-template name="indent-extra"/>
      <xsl:text>public readonly InnerEnum innerEnumValue;{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>public readonly string nameValue;{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>public readonly int ordinalValue;{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>public static int nextOrdinal = 0;{$NL}</xsl:text>
           
      <!--<xsl:apply-templates select="members/member">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>-->
      
      <xsl:call-template name="indent-extra"/>
      <xsl:text>internal {$className}(string name, InnerEnum innerEnum</xsl:text>
      <xsl:for-each select="members/member[@nodeType='ConstructorDeclaration']/parameters/parameter">
         <xsl:text>, </xsl:text>
         <xsl:apply-templates select="type"/>
         <xsl:text> </xsl:text>
         <xsl:apply-templates select="name"/>
      </xsl:for-each>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:for-each select="members/member[@nodeType='ConstructorDeclaration']/parameters/parameter">
         <xsl:call-template name="indent-plus-two"/>
         <xsl:text>this.{name/@identifier} = {name/@identifier};{$NL}</xsl:text>
      </xsl:for-each>
      
      <xsl:call-template name="indent-plus-two"/>
      <xsl:text>nameValue = name; ordinalValue = nextOrdinal++; innerEnumValue = innerEnum;{$NL}</xsl:text>      
      <xsl:call-template name="indent-extra"/>
      <xsl:text>}}{$NL}</xsl:text>
      
      <xsl:apply-templates select="members/member[not(@nodeType='ConstructorDeclaration')]">
         <xsl:with-param name="indent" tunnel="yes" select="$indent+1"/>
      </xsl:apply-templates>
      
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'InitializerDeclaration'][@isStatic = 'true']"/>
      
   <xsl:template match="*[@nodeType = 'InitializerDeclaration'][not(@isStatic = 'true')]">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text> {{{$NL}</xsl:text>
      <xsl:apply-templates select="body/*">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="typeParameters">
      <xsl:text>&lt;</xsl:text>
      <xsl:for-each select="typeParameter">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
      <xsl:text>&gt; </xsl:text>
   </xsl:template>
   
   <xsl:template match="thrownExceptions">
      <!--<xsl:text>throws </xsl:text>
      <xsl:for-each select="thrownException">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
      <xsl:text> </xsl:text>-->
   </xsl:template>
   
   <xsl:template match="parameters">
      <xsl:for-each select="parameter">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:if test="@isVarArgs='true'">params </xsl:if>
         <xsl:choose>
            <xsl:when test="annotations/annotation[name/@identifier='CSharpParameterType']">
               <xsl:text>{annotations/annotation[name/@identifier='CSharpParameterType']/pairs/pair[name/@identifier='key']/value/@value}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:apply-templates select="type"/>
               <xsl:if test="@isVarArgs='true'">[] </xsl:if>
            </xsl:otherwise>
         </xsl:choose>         
         <xsl:text> </xsl:text>
         <xsl:apply-templates select="name"/>
      </xsl:for-each>
   </xsl:template>
 
   <xsl:template match="statements">
      <xsl:apply-templates select="statement"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='AssertStmt']">
      <xsl:call-template name="indent"/>
      <xsl:text>System.Diagnostics.Debug.Assert(</xsl:text>
      <xsl:apply-templates select="*"/>
      <xsl:text>);{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="statement[@nodeType='BlockStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>{{{$NL}</xsl:text>
      <xsl:apply-templates select="*">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='BreakStmt']">
      <xsl:call-template name="indent"/>
      <xsl:text>break {label/@identifier};{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ContinueStmt']">
      <xsl:call-template name="indent"/>
      <xsl:text>continue {label/@identifier};{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='DoStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>do {{{$NL}</xsl:text>
      <xsl:apply-templates select="body">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}} while (</xsl:text>
      <xsl:apply-templates select="condition"/>
      <xsl:text>);{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ForEachStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>foreach (</xsl:text>
      <xsl:apply-templates select="variable"/>
      <xsl:text> in </xsl:text>
      <xsl:apply-templates select="iterable"/>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:apply-templates select="body">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>


   <!--<xsl:template match="*[@nodeType='ExplicitConstructorInvocationStmt'][@isThis = 'true']">
      <xsl:call-template name="indent"/>
      <xsl:text>{if (@isThis = 'true') then 'this' else 'super'}</xsl:text>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
   -->
   <xsl:template match="*[@nodeType='ExplicitConstructorInvocationStmt']"/>
   
   <xsl:template match="*[@nodeType='ExpressionStmt']">
      <xsl:call-template name="indent"/>
      <xsl:apply-templates select="*"/>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ForStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>for (</xsl:text>
      <xsl:for-each select="initialization/initializatio">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
      <xsl:text>; </xsl:text>
      <xsl:apply-templates select="compare"/>
      <xsl:text>; </xsl:text>
      <xsl:for-each select="update/updat">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:apply-templates select="body">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='IfStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>if (</xsl:text>
      <xsl:apply-templates select="condition"/>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:apply-templates select="thenStmt">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:if test="elseStmt">
         <xsl:call-template name="indent"/>
         <xsl:text>}} else {{{$NL}</xsl:text>
         <xsl:apply-templates select="elseStmt">
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
         </xsl:apply-templates>
      </xsl:if>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
     
   <xsl:template match="*[@nodeType='LabeledStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent">
         <xsl:with-param name="indent" select="$indent - 1" tunnel="yes"/>
      </xsl:call-template>
      <xsl:text>{label/@identifier}:{$NL}</xsl:text>
      <xsl:apply-templates select="statement"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ReturnStmt']">
      <xsl:call-template name="indent"/>
      <xsl:text>return </xsl:text>
      <xsl:apply-templates select="*"/>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='SwitchStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:variable name="selectorElem" select="selector"/>
      <xsl:variable name="enumType" as="element(*)?"
         select="key('digestClassKey', selector/@RESOLVED_TYPE, $digest)[self::enum]"/>
      <xsl:call-template name="indent"/>
      <xsl:text>switch (</xsl:text>
      <xsl:apply-templates select="selector"/>
      <xsl:if test="$enumType">.innerEnumValue</xsl:if>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:for-each select="entries/entrie">
         <xsl:call-template name="indent"/>
         <xsl:choose>
            <xsl:when test="labels/label">
               <xsl:text>  case </xsl:text>
               <xsl:if test="$enumType">{$selectorElem/@RESOLVED_TYPE}.InnerEnum.</xsl:if>
               <xsl:apply-templates select="labels/label"/>
               <xsl:text>:{$NL}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:text>  default:{$NL}</xsl:text>
            </xsl:otherwise>
         </xsl:choose>
         <xsl:if test="position() = last() and empty(statements)">
            <xsl:call-template name="indent-extra"/>
            <xsl:text>break;{$NL}</xsl:text>
         </xsl:if>
         <xsl:apply-templates select="statements">
            <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
         </xsl:apply-templates>
      </xsl:for-each>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='SynchronizedStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>lock (</xsl:text>
      <xsl:apply-templates select="expression"/>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:apply-templates select="body"> 
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ThrowStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>throw </xsl:text>
      <xsl:apply-templates select="*"> 
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='TryStmt'][resources]" priority="20">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>using (</xsl:text>
      <xsl:apply-templates select="resources/*"/>
      <xsl:text>) {{</xsl:text>
      <xsl:next-match>
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:next-match>
      <xsl:call-template name="indent"/>
      <xsl:text>}}</xsl:text>
   </xsl:template>
 
   <xsl:template match="*[@nodeType='TryStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>try {{{$NL}</xsl:text>
      <xsl:apply-templates select="tryBlock/statements">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:for-each select="catchClauses/catchClause">
         <xsl:call-template name="indent"/>
         <xsl:text>}} catch (</xsl:text>
         <xsl:choose>
            <xsl:when test="parameter/type/@nodeType='UnionType'">
               <xsl:text>Exception </xsl:text>
               <xsl:apply-templates select="parameter/name"/>
               <xsl:text>) when (</xsl:text>
               <xsl:variable name="paramName" select="parameter/name"/>
               <xsl:for-each select="parameter/type/elements/element">
                  <xsl:if test="position() != 1"> || </xsl:if>
                  <xsl:apply-templates select="$paramName"/>
                  <xsl:text> is </xsl:text>
                  <xsl:apply-templates select="."/>         
               </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
               <xsl:apply-templates select="parameter/type"/>
               <xsl:text> </xsl:text>
               <xsl:apply-templates select="parameter/name"/>
            </xsl:otherwise>
         </xsl:choose>
         <xsl:text>) {{{$NL}</xsl:text>
         <xsl:apply-templates select="body">
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
         </xsl:apply-templates>
      </xsl:for-each>
      <xsl:for-each select="finallyBlock">
         <xsl:call-template name="indent"/>
         <xsl:text>}} finally {{{$NL}</xsl:text>
         <xsl:apply-templates select="*">
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
         </xsl:apply-templates>
      </xsl:for-each>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='WhileStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>while (</xsl:text>
      <xsl:apply-templates select="condition"/>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:apply-templates select="body">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:variable name="operators" as="map(xs:string, xs:string)"
      select="map{'AND': '&amp;&amp;',
                  'OR': '||',
                  'BINARY_AND': '&amp;',
                  'BINARY_OR': '|',
                  'BITWISE_COMPLEMENT': '~',
                  'XOR': '^',
                  'EQUALS': '==',
                  'NOT_EQUALS': '!=',
                  'LESS': '&lt;',
                  'GREATER': '>',
                  'LESS_EQUALS': '&lt;=',
                  'GREATER_EQUALS': '>=',
                  'LEFT_SHIFT': '&lt;&lt;',
                  'SIGNED_RIGHT_SHIFT': '&gt;&gt;',
                  'UNSIGNED_RIGHT_SHIFT': '&gt;&gt;&gt;',
                  'PLUS': '+',
                  'MINUS': '-',
                  'MULTIPLY': '*',
                  'DIVIDE': '/',
                  'REMAINDER': '%',
                  'LOGICAL_COMPLEMENT': '!'
                  }"/>
 
   <xsl:template match="*[@nodeType='UnaryExpr']" priority="10">
      <xsl:text>{($operators(@operator), @operator)[1]}</xsl:text>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="expression"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='UnaryExpr'][@operator='MINUS'][expression/@nodeType='IntegerLiteralExpr']" priority="12">
      <xsl:text>-{expression/@value}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='UnaryExpr'][@operator='POSTFIX_INCREMENT']" priority="20">
      <xsl:apply-templates select="expression"/>
      <xsl:text>++</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='UnaryExpr'][@operator='PREFIX_INCREMENT']" priority="20">
      <xsl:text>++</xsl:text>
      <xsl:apply-templates select="expression"/>     
   </xsl:template>
   
   <xsl:template match="*[@nodeType='UnaryExpr'][@operator='POSTFIX_DECREMENT']" priority="20">
      <xsl:apply-templates select="expression"/>
      <xsl:text>--</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='UnaryExpr'][@operator='PREFIX_DECREMENT']" priority="20">
      <xsl:text>--</xsl:text>
      <xsl:apply-templates select="expression"/>     
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ArrayCreationExpr']">
      <xsl:text>new </xsl:text>
      <xsl:apply-templates select="elementType"/>
      <xsl:for-each select="levels/level">
         <xsl:text>[</xsl:text>
         <xsl:apply-templates select="dimension"/>
         <xsl:text>]</xsl:text>
      </xsl:for-each>
      <xsl:apply-templates select="*[@nodeType='ArrayInitializerExpr']"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='BinaryExpr']">
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="left"/>
      <xsl:text>{($operators(@operator), @operator)[1]}</xsl:text>
      <xsl:apply-templates select="right"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='BinaryExpr'][@operator='UNSIGNED_RIGHT_SHIFT']" priority="20">
      <!-- C# has no ">>>" operator, so generate a function call -->
      <xsl:text>com.saxonica.glue.Utils.unsignedRightShift(</xsl:text>
      <xsl:apply-templates select="left"/>
      <xsl:text>, </xsl:text>
      <xsl:apply-templates select="right"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ConditionalExpr']">
      <!-- We're not making any attempt to enforce the C# constraints on ternary
         expressions, whereby the types of the two branches must be balanced. We
         could potentially add a cast to the required type if this is known -->
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="condition"/>
      <xsl:text> ? </xsl:text>
      <xsl:apply-templates select="thenExpr"/>
      <xsl:text> : </xsl:text>
      <xsl:apply-templates select="elseExpr"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='CastExpr']/*[@nodeType='LambdaExpr']" priority="20">
      <xsl:text>(</xsl:text>
      <xsl:next-match/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='LambdaExpr']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:choose>
         <xsl:when test="@isEnclosingParameters='true'">
            <xsl:text>(</xsl:text>
            <xsl:for-each select="parameters/parameter">
               <xsl:if test="position() ne 1">, </xsl:if>
               <xsl:apply-templates select="name"/>
            </xsl:for-each>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="parameters/parameter/name"/>
         </xsl:otherwise>
      </xsl:choose> 
      <xsl:text> =&gt; </xsl:text>
      <xsl:choose>        
         <xsl:when test="body/@nodeType = 'BlockStmt'">
            <xsl:text>{{{$NL}</xsl:text>
            <xsl:apply-templates select="body">
               <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
            </xsl:apply-templates>
            <xsl:call-template name="indent"/>
            <xsl:text>}} </xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="body/expression"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>

 
   
   <xsl:template name="modifiers">
      <xsl:param name="digestMethod" as="element(method)?" select="()"/>
      <xsl:param name="isConstant" as="xs:boolean" select="false()"/>
      <xsl:choose>
         <xsl:when test="empty(modifiers) and not(../..[@isInterface='true'])">
            <xsl:text>internal </xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="modifiers/modifier"/>
         </xsl:otherwise>
      </xsl:choose>
      <!--<xsl:if test="(annotations/annotation/name/@identifier = 'Override' 
         or (../../name/@identifier = ('equals', 'hashCode', 'toString'))) 
         and not(f:isInterfaceMember(.))">override </xsl:if>-->
      <xsl:choose>
         <xsl:when test="$digestMethod/@override='1'">override </xsl:when>
         <xsl:when test="$digestMethod[@virtual='1' and not(@abstract='1')]">virtual </xsl:when>
      </xsl:choose>
      
    
      <!--<xsl:if test="annotations/annotation/name/@identifier = 'Virtual'">virtual </xsl:if>-->
   </xsl:template>
   
   <xsl:function name="f:isInterfaceMember" as="xs:boolean">
      <xsl:param name="expr" as="element(*)"/>
      <xsl:sequence select="$expr/ancestor::type[@nodeType='ClassOrInterfaceDeclaration'][1]/@isInterface='true'"/>
   </xsl:function>
   
   <xsl:template match="modifier">
      <xsl:apply-templates select="@keyword"/>
   </xsl:template>
   
   <xsl:template match="modifier/@keyword" priority="3">
      <xsl:text>{lower-case(.)} </xsl:text>
   </xsl:template>
   
   <xsl:template match="modifier/@keyword[. = 'PROTECTED']" priority="10">
      <xsl:text>protected internal </xsl:text>
   </xsl:template>
   
   <xsl:template match="modifier/@keyword[. = 'STATIC']" priority="5">
      <xsl:param name="isConstant" as="xs:boolean" select="false()" tunnel="yes"/>
      <xsl:text>{if ($isConstant) then '' else 'static '}</xsl:text>
   </xsl:template>
   
   <xsl:template match="modifier/@keyword[. = 'FINAL']" priority="10">
      <xsl:param name="isConstant" as="xs:boolean" select="false()" tunnel="yes"/>
      <xsl:text>{if ($isConstant) then 'const ' else 'readonly '}</xsl:text>
   </xsl:template>
   
   <xsl:template match="modifier[f:isInterfaceMember(../..)]/@keyword[. = 'PUBLIC']" priority="10"/>
   
   <xsl:template match="modifier/@keyword[. = 'TRANSIENT']" priority="10"/>
   <xsl:template match="modifier/@keyword[. = 'SYNCHRONIZED']" priority="10"/>
   <xsl:template match="modifier/@keyword[. = 'DEFAULT']" priority="10"/>
   
   <xsl:template match="*[@nodeType=('ClassOrInterfaceDeclaration')]/modifiers/modifier/@keyword[. = 'STATIC']" priority="10"/>
   
   <xsl:template match="*[@nodeType=('ClassOrInterfaceDeclaration')]/modifiers/modifier[@keyword = 'FINAL']" priority="20">
      <xsl:text>sealed </xsl:text>
   </xsl:template>
  
   
   <!-- determine whether an expression is constant expression. The C# language rules are complex,
      but we allow any unary or binary expression whose operands are constant expressions, literals,
      or the names of variables initialised to constant expressions.
      Declaring the item with a "const" keyword is needed to allow its use in switch/case -->
   <xsl:function name="f:isConstantExpression" as="xs:boolean">
      <xsl:param name="expr" as="element()?"/>
      <xsl:sequence select="contains($expr/@nodeType, 'Literal') 
         or ($expr/@nodeType='BinaryExpr' and f:isConstantExpression($expr/left) and f:isConstantExpression($expr/right)) 
         or ($expr/@nodeType='UnaryExpr' and f:isConstantExpression($expr/expression)
         or ($expr/@nodeType='NameExpr' and $expr/ancestor::type[1]/members/member[@nodeType='FieldDeclaration']
              [variables/variable[name/@identifier=$expr/name/@identifier and f:isConstantExpression(initializer)]]))"/>
   </xsl:function>
         
  
   <xsl:template match="*[@nodeType=('MethodDeclaration')]/modifiers/modifier[@keyword = 'FINAL']" priority="20"/>


   
   <xsl:template match="typeParameter">
      <xsl:apply-templates select="name"/>
      <xsl:if test="typeBound">
         <!--<xsl:text> extends </xsl:text>
         <xsl:apply-templates select="typeBound/typeBoun"/>-->
      </xsl:if>
   </xsl:template>
   
   
   <xsl:template match="type[@nodeType = 'VoidType']">
      <xsl:text>void</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'PrimitiveType']" priority="10">
      <xsl:text>{if (@type='BOOLEAN') then 'bool' else lower-case(@type)}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'ClassOrInterfaceType'][name/@identifier='String']" priority="30">
      <xsl:text>string</xsl:text>
   </xsl:template>
   
   
   <xsl:template match="*[@nodeType = 'ArrayType']">
      <xsl:apply-templates select="componentType"/>
      <xsl:text>[]</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'UnionType']">
      <xsl:for-each select="elements/element">
         <xsl:if test="position() ne 1"> | </xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
   </xsl:template>
   
   <xsl:template match="typeArguments">
      <!-- TODO: implicit-type-arguments mechanism is no longer used? -->
      <xsl:param name="implicit-type-arguments" tunnel="yes" as="element(typeArguments)?"/>
      <xsl:variable name="use-type-arguments" 
         select="if (exists(typeArgument)) then typeArgument else $implicit-type-arguments/typeArgument"/>
      <xsl:text>&lt;</xsl:text>
      <xsl:for-each select="$use-type-arguments">
         <xsl:if test="position() ne 1">,</xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
      <xsl:text>&gt;</xsl:text>
   </xsl:template>
   
   <xsl:template match="typeArgument[extendedType]" priority="20">
      <!--<xsl:text>? extends </xsl:text>-->
      <xsl:apply-templates select="extendedType"/>
   </xsl:template>
   
   <xsl:template match="typeArgument[superType]" priority="20">
      <!--<xsl:text>? super </xsl:text>-->
      <xsl:apply-templates select="superType"/>
   </xsl:template>
   
   <xsl:template match="typeArgument[@nodeType = 'WildcardType']" priority="10">
      <xsl:text>object</xsl:text>
   </xsl:template>
   
   <xsl:template match="variables">
      <xsl:apply-templates select="head(variable)"/>
      <xsl:apply-templates select="tail(variable)" mode="no-decl"/>
   </xsl:template>
   
   
   <!-- A variable declaration of the form "String csharp_fall_through_N = "case 'x'" is recognised as indicating fall-through in a switch -->
   <!--<xsl:template match="variable[starts-with(name/@identifier, 'csharp_fall_through')]" priority="20">
      <xsl:text>goto {initializer/@value}</xsl:text>
   </xsl:template>-->
   

   <xsl:template match="variable">
      <xsl:apply-templates select="type"/>
      <xsl:text> </xsl:text>
      <xsl:apply-templates select="name"/>
      <xsl:if test="initializer">
         <xsl:text> = </xsl:text>
         <xsl:apply-templates select="initializer"/>
      </xsl:if>
   </xsl:template>
   
   <xsl:template match="variable" mode="no-decl">
      <xsl:text>, </xsl:text>
      <xsl:apply-templates select="name"/>
      <xsl:if test="initializer">
         <xsl:text> = </xsl:text>
         <xsl:apply-templates select="initializer"/>
      </xsl:if>
   </xsl:template>
  
   <xsl:template match="arguments">
      <xsl:for-each select="argument">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
   </xsl:template>
 
   <xsl:template match="*[@nodeType='NullLiteralExpr']">
      <xsl:text>null</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr']">
      <xsl:text>new </xsl:text>
      <xsl:apply-templates select="type"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Handle a constructor call with a diamond operator: new ArrayList<>() -->
   <!-- Inference of the specific type is context dependent and is handled in module diamond.xsl -->
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr'][type/typeArguments[not(*)]]" priority="30">
      <!--<xsl:param name="implicit-type-arguments" tunnel="yes" 
         as="element(typeArguments)?" select="()"/>-->
      <xsl:text>new {type/@RESOLVED_TYPE}&lt;</xsl:text>
      <xsl:apply-templates select="." mode="diamond"/>
      <xsl:text>&gt;</xsl:text>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   
   <!-- Handle an inline anonymous class, typically `new SomeInterface(x, y, z) { class body }` -->
   <!-- We generate a named static inner class (in a second pass), and at this point we
        generate a call on its constructor, with an extra argument `this` -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr'][anonymousClassBody]" priority="20">
      <xsl:text>new {f:allocateInnerClassName(.)}(this</xsl:text>
      <xsl:if test="exists(arguments/argument)">, </xsl:if>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:function name="f:allocateInnerClassName" as="xs:string">
      <xsl:param name="creationExpr" as="element(*)"/>
      <xsl:variable name="resolvedName" select="f:resolveTypeName($creationExpr/type)"/>
      <xsl:text>Inner_{tokenize(f:degenerify($resolvedName), '\.')[last()]}_{generate-id($creationExpr)}</xsl:text>
   </xsl:function>
   
   <xsl:function name="f:resolveTypeName">
      <xsl:param name="typeElem" as="element(type)"/>
      <xsl:choose>
         <xsl:when test="$typeElem/@RESOLVED_TYPE">
            <xsl:sequence select="$typeElem/@RESOLVED_TYPE"/>
         </xsl:when>
         <xsl:when test="$typeElem/@UNRESOLVED and exists($typeElem//import[name/@identifier=$typeElem/@UNRESOLVED])">
            <xsl:sequence select="f:qualified-name($typeElem//import[name/@identifier=$typeElem/@UNRESOLVED]/name)"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="$typeElem/name/@identifier"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>
   
   <!-- For an anonymous inline inner class, generate a named inner class; in C# this is effectively static,
        so we pass a reference to the outer class as an extra argument in the constructor. The tricky part
        is binding references to methods and fields of the outer class to be scoped on this variable -->
   <xsl:template match="anonymousClassBody">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:variable name="allocatedName" as="xs:string" select="f:allocateInnerClassName(..)"/>
      <xsl:text>{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>private class {$allocatedName} : {f:escapeDottedName(f:resolveTypeName(../type))} {{{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:variable name="containingClass" select="ancestor::type[@nodeType='ClassOrInterfaceDeclaration'][1]/name/@identifier" as="xs:string"/>
      <xsl:text>private readonly {$containingClass} outerInstance;{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>public {$allocatedName}({$containingClass} outerInstance</xsl:text>
      <xsl:for-each select="../arguments/argument">
         <xsl:choose>
            <xsl:when test="@RESOLVED_TYPE">, {f:escapeDottedName(@RESOLVED_TYPE)}</xsl:when>
            <xsl:when test="@nodeType='ThisExpr'">, {$containingClass}</xsl:when>
            <xsl:otherwise>, /* UNKNOWN ARG TYPE */ object</xsl:otherwise>
         </xsl:choose>
         <xsl:text> arg{position()}</xsl:text>
         <!--<xsl:apply-templates select="."/>-->
      </xsl:for-each>
      <xsl:text>)</xsl:text>
      <xsl:if test="exists(../arguments/argument)">
         <xsl:text> : base(</xsl:text>
         <xsl:for-each select="../arguments/argument">
            <xsl:if test="position() ne 1">, </xsl:if>
            <xsl:text>arg{position()}</xsl:text>
         </xsl:for-each>
         <!--<xsl:value-of select="../arguments/argument/name/@identifier" separator=", "/>-->
         <xsl:text>)</xsl:text>
      </xsl:if>
      <xsl:text> {{{$NL}</xsl:text>
      <xsl:call-template name="indent-plus-two"/>
      <xsl:text>this.outerInstance = outerInstance;{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>}}{$NL}</xsl:text>
      <xsl:apply-templates select="*">
         <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <!-- Try to resolve an unscoped name appearing within an anonymous class body -->
   <!-- Note: we don't try to handle anonymous classes nested within anonymous classes -->
   <xsl:template match="anonymousClassBody//name[@identifier][..[not(scope or self::type or self::variable 
         or self::annotation or self::parameter or @nodeType=('MethodDeclaration', 'FieldDeclaration'))]]" priority="40">
      <xsl:variable name="identifier" select="@identifier" as="xs:string"/>
      <xsl:variable name="containingClass" select="ancestor::anonymousClassBody[1]" as="element(anonymousClassBody)"/>
      <xsl:variable name="outerMethod" select="$containingClass/ancestor::*[@nodeType='MethodDeclaration'][1]"/>
      <xsl:variable name="outerClass" select="$containingClass/ancestor::*[@nodeType='ClassOrInterfaceDeclaration'][1]"/>
      <xsl:choose>
         <xsl:when test="ancestor::catchClause//parameter[name/@identifier = $identifier]">
            <!-- The name matches an exception variable in a catch clause -->
            <xsl:text>/*exception*/ {$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="ancestor::anonymousClassBod[1]//variable[name/@identifier = $identifier]">
            <!-- The name matches a local variable within the same method -->
            <xsl:text>/*localvar*/ {$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="ancestor::anonymousClassBod[1]/parameters/parameter[name/@identifier = $identifier]">
            <!-- The name matches a parameter of the containing method declaration -->
            <xsl:text>/*localparam*/ {$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$containingClass/*[@nodeType='FieldDeclaration']/variables/variable/name[@identifier=$identifier]">
            <!-- The name matches a field of the containing anonymous class -->
            <xsl:text>/*field*/ {$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$containingClass/*[@nodeType='MethodDeclaration']/name[@identifier=$identifier]">
            <!-- The name matches a method of the containing anonymous class -->
            <xsl:text>/*method*/ {$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$outerMethod//variable[name/@identifier = $identifier]">
            <!-- The name matches a local variable within the outer containing method. We can't handle this case -->
            <xsl:text>/* CONVERSION PROBLEM: Reference from anonymous inner class to a local variable outside the class*/ {$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$outerMethod/parameters/parameter[name/@identifier = $identifier]">
            <!-- The name matches a parameter of the containing method declaration -->
            <xsl:text>/* CONVERSION PROBLEM: Reference from anonymous inner class to a local parameter outside the class*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$outerClass/members/member[@nodeType='FieldDeclaration'][modifiers/modifier/@keyword='STATIC']/variables/variable/name[@identifier=$identifier]">
            <!-- The name matches a static field of the outer containing class -->
            <xsl:text>/*outerStaticField*/ {$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$outerClass/members/member[@nodeType='FieldDeclaration']/variables/variable/name[@identifier=$identifier]">
            <!-- The name matches an instance field of the outer containing class -->
            <xsl:text>outerInstance.{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$outerClass/members/member[@nodeType='MethodDeclaration'][modifiers/modifier/@keyword='STATIC']/name[@identifier=$identifier]">
            <!-- The name matches a static method of the containing anonymous class -->
            <xsl:text>/*outerStaticMethod*/ {$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$outerClass/members/member[@nodeType='MethodDeclaration']/name[@identifier=$identifier]">
            <!-- The name matches an instance method of the containing anonymous class -->
            <xsl:text>outerInstance.{$identifier}</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>/*misc*/{$identifier}</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Within an anonymous class body, resolve an expression of the form OuterClass.this -->
   <xsl:template match="anonymousClassBody//*[@nodeType='ThisExpr'][typeName/@identifier=ancestor::*[@nodeType='ClassOrInterfaceDeclaration'][1]/name/@identifier]" priority="40">
      <xsl:text>this.outerInstance</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType=('IntegerLiteralExpr', 'BooleanLiteralExpr', 'DoubleLiteralExpr', 'LongLiteralExpr')]">
      <xsl:text>{@value}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='InstanceOfExpr']">
      <!-- Translating "A instanceof B" to "A is B" is OK as a general strategy, but it fails on generics -->
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="expression"/>
      <xsl:text> is </xsl:text>
      <xsl:apply-templates select="type"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodReferenceExpr']">
      <xsl:apply-templates select="scope"/>
      <xsl:text>.{@identifier}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodReferenceExpr'][@identifier='new']" priority="20">
      <!-- Problem with this is identifying the params to the constructor. Have removed ::new
           construct from the Java code so it doesn't arise -->
      <xsl:text>() => new </xsl:text>
      <xsl:apply-templates select="scope"/>
      <xsl:text>()</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodReferenceExpr'][@identifier='isInstance']" priority="20">
      <xsl:text>x => (x is {scope/type/@RESOLVED_TYPE})</xsl:text>
   </xsl:template>
   
   <!--<argument nodeType="MethodReferenceExpr" identifier="isInstance">
      <scope nodeType="ClassExpr">
         <type nodeType="ClassOrInterfaceType"
            RESOLVED_TYPE="net.sf.saxon.style.StyleElement">
            <name nodeType="SimpleName" identifier="StyleElement"/>
         </type>
      </scope>
   </argument>-->
   
   <xsl:template match="*[@nodeType='StringLiteralExpr']">
      <xsl:text>"{@value}"</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='CharLiteralExpr']">
      <xsl:text>'{@value}'</xsl:text>
   </xsl:template>

   <xsl:template match="*[@nodeType='ThisExpr']">
      <!--<xsl:if test="typeName">
         <xsl:apply-templates select="typeName"/>
         <xsl:text>.</xsl:text>
      </xsl:if>-->
      <xsl:text>this</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='TypeExpr']">
      <xsl:apply-templates select="type"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodCallExpr']">
      <!-- Handled in module methods.xsl -->
      <xsl:apply-templates select="." mode="methods"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='FieldAccessExpr']">
      <!-- Handled in module fields.xsl -->
      <xsl:apply-templates select="." mode="fields"/>
   </xsl:template>
 
   
   <xsl:template match="*[@nodeType='VariableDeclarationExpr']">
      <xsl:apply-templates select="variables"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ArrayInitializerExpr']">
      <xsl:text>{{</xsl:text>
      <xsl:for-each select="values/value">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
      <xsl:text>}}</xsl:text>
   </xsl:template>
   
   <!--<!-\- Attempt to identify generic type for an assignment like x = new HashMap<>() -\->
   <!-\- We have to resort to parsing the resolved type of the variable x -\->
   <xsl:template match="*[@nodeType='AssignExpr'][value[@nodeType='ObjectCreationExpr' and type/typeArguments[not(*)]]]" priority="20">
      <xsl:next-match>
         <xsl:with-param name="implicit-type-arguments" 
            select="if (target/@RESOLVED_TYPE) then f:make-implicit-type-arguments(target/@RESOLVED_TYPE) else ()" 
            as="element(typeArguments)?" 
            tunnel="yes"/>
      </xsl:next-match>
   </xsl:template>-->
   
   <!--<xsl:function name="f:make-implicit-type-arguments" as="element(typeArguments)">
      <xsl:param name="genericType" as="xs:string"/>
      <typeArguments>
         <xsl:for-each select="$genericType => substring-after('&lt;') => substring-before('>') => tokenize(',')">
            <typeArgument nodeType="ClassOrInterfaceType" RESOLVED_TYPE="{.}">
               <name nodeType="SimpleName" identifier="{tokenize(., '\.')[last()]}"/>
            </typeArgument>
         </xsl:for-each>               
      </typeArguments>
   </xsl:function>-->
      
   
   <xsl:template match="*[@nodeType='AssignExpr']">
      <xsl:apply-templates select="target"/>
      <xsl:choose>
         <xsl:when test="@operator"> {$operators(@operator)}= </xsl:when>
         <xsl:otherwise> = </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates select="value"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ArrayAccessExpr']">
      <xsl:apply-templates select="name"/>
      <xsl:text>[</xsl:text>
      <xsl:apply-templates select="index"/>
      <xsl:text>]</xsl:text>
   </xsl:template>
 
   <xsl:template match="*[@nodeType='ClassExpr']">
      <xsl:text>typeof(</xsl:text>
      <xsl:apply-templates select="type"/>
      <xsl:text>)</xsl:text>
   </xsl:template>

   
   <xsl:template match="*[@nodeType='CastExpr']">
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="type"/>
      <xsl:text>)</xsl:text>
      <xsl:apply-templates select="expression"/>
   </xsl:template>
   
   <!-- match enumeration constants and qualify them with the enumeration class name -->
   <xsl:template match="*[@nodeType='NameExpr'][@ENUM_TYPE]" priority="20">
      <xsl:text>{@ENUM_TYPE}.{name/@identifier}</xsl:text>
   </xsl:template>
   
   <!-- match a reference to a field of an outer class -->
   <!--<xsl:template match="*[@nodeType='NameExpr'][@OUTER_FIELD='1']" priority="20">
      <xsl:text>outerInstance.{name/@identifier}</xsl:text>
   </xsl:template>-->
   
   <xsl:template match="*[@nodeType='NameExpr']">
      <xsl:apply-templates select="name"/>
   </xsl:template>
   
   <xsl:key name="digestClassKey" match="class | interface | enum" 
      use="string-join((ancestor::module/@package, ancestor-or-self::*[self::class or self::interface or self::enum]/@name), '.')"/>
   
   <!-- The key for finding a method by name and signature is of the form 'net.sf.saxon.om.NodeInfo#iterateAxis!INT,NodeTest' -->
   <xsl:key name="digestMethodKey" match="method"
      use="concat(ancestor::module/@package, '.', 
               string-join(ancestor-or-self::*[self::class or self::interface]/@name, '.'),
               '#', ./@name, '!', string(@sig))"/>
   
   <!-- The key for finding a method by name and arity is of the form 'net.sf.saxon.om.NodeInfo#iterateAxis!arity' -->
   <xsl:key name="digestMethodArityKey" match="method"
      use="concat(ancestor::module/@package, '.', 
      string-join(ancestor-or-self::*[self::class or self::interface]/@name, '.'),
      '#', ./@name, '!', count(tokenize(string(@sig), ',')))"/>
   
   
   <!-- Where an interface defines defaulted implementations of methods, copy this methods into all (immediately)
        implementing classes that do not define their own override of the method. In addition, C# requires that
        an abstract class that implements an interface contains a concrete implementation of every method defined
        in that interface, so we generate stub implementations that exist only to be overridden -->
   <xsl:template name="expand-defaulted-methods">
      <xsl:param name="indent" as="xs:integer" tunnel="yes" select="0"/>
      <xsl:assert test="@nodeType='ClassOrInterfaceDeclaration'"/>
      <xsl:variable name="thisClassName" as="xs:string" select="f:full-class-name(.)"/>
      <!--<xsl:variable name="thisClassName" as="xs:string" 
         select="string-join((f:qualified-name(ancestor::root/packageDeclaration/name), ancestor-or-self::*[@nodeType='ClassOrInterfaceDeclaration']/name/@identifier), '.')"/>
      --><!--<xsl:message>Expanding default methods for {$thisClassName}</xsl:message>-->
      <xsl:variable name="thisDigest" select="key('digestClassKey', $thisClassName, $digest)" as="element(*)?"/>
      <xsl:choose>
         <xsl:when test="empty($thisDigest)">
            <!-- This is happening for methods in inner classes. 
                 May affect whether they are declared as "override", but mostly harmless? -->
            <!--<xsl:message>No digest found for {$thisClassName}</xsl:message>-->
         </xsl:when>
         <xsl:otherwise>
            <xsl:variable name="defaults" select="f:gather-default-methods($thisDigest)|()" as="element(*)*"/>
            <!--<xsl:message>Found {count($defaults)} default methods {string-join($defaults/(@name || '-' || generate-id()), ';')}</xsl:message>-->
            <xsl:for-each-group select="$defaults" group-by="@name, string(@sig)" composite="yes">
               <!--<xsl:message>Considering default for {@name}#{@sig}</xsl:message>-->
               <!--<xsl:message>Has concrete method? {f:has-concrete-method($thisDigest, current()/@name, xs:integer(current()/@arity))}</xsl:message>
               <xsl:message>Superclass implements? {$thisDigest/extends ! key('digestClassKey', @name, $digest) ! f:get-implemented-interfaces(.) ! @name}</xsl:message>-->
               <xsl:if test="not(f:has-concrete-method($thisDigest, current()/@name, string(current()/@sig)))
                  and (empty(($thisDigest/extends ! key('digestClassKey', @name, $digest) ! f:get-implemented-interfaces(.)) intersect current()/..))">
                  <xsl:text>{$NL}</xsl:text>
                  <xsl:call-template name="indent-extra"/>
                  <xsl:text>/* Expanding default method {@name} from interface {current-group()[last()]/../@name} */{$NL}</xsl:text>
                  <xsl:apply-templates select="current-group()[last()]/default/*">
                     <xsl:with-param name="expanding-defaults" as="xs:boolean" select="true()" tunnel="yes"/>
                     <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
                  </xsl:apply-templates>
               </xsl:if>        
            </xsl:for-each-group>
            
            <xsl:if test="$thisDigest/@abstract='1'">
               <!--<xsl:message>Making abstract methods concrete: implements {f:get-implemented-interfaces($thisDigest)/@name}</xsl:message>
               <xsl:message>Defaults: {$defaults/(@name || '=' || @sig)}</xsl:message>-->
               <xsl:for-each select="f:get-implemented-interfaces($thisDigest)/method[not(./default or @static='1')]">
                  <!--<xsl:message>Examining {./@name}</xsl:message>-->
                  <xsl:if test="not(f:has-method($thisDigest, current()/@name, string(current()/@sig)))
                     and empty(($thisDigest/extends ! key('digestClassKey', @name, $digest) ! f:get-implemented-interfaces(.)) intersect current()/..)
                     and empty($defaults[@name=current()/@name and string(@sig)=string(current()/@sig)])">
                     <!--<xsl:message>Expanding {./@name} on {../@name} with parameters {@sig}</xsl:message>-->
                     <xsl:text>{$NL}</xsl:text>
                     <xsl:call-template name="indent-extra"/>
                     <xsl:text>/* Creating stub method {@name} to satisfy interface {../@name} */{$NL}</xsl:text>
                     <xsl:call-template name="indent-extra"/>
                     <xsl:text>public virtual {f:rename-generic-type(@returns)} {@name}({string-join(tokenize(@params,'\|')!f:rename-generic-type(.), ', ')}</xsl:text>
                     <xsl:for-each select="tokenize(@params,'\|')">
                        <xsl:if test="position() gt 1">, </xsl:if>
                        <xsl:text>{f:rename-generic-type(.)} arg{position()}</xsl:text>
                     </xsl:for-each>
                     <xsl:text>) {{{$NL}</xsl:text>
                     <xsl:call-template name="indent-plus-two"/>
                     <xsl:text>throw new System.NotImplementedException();{$NL}</xsl:text>
                     <xsl:call-template name="indent-extra"/>
                     <xsl:text>}}{$NL}</xsl:text>
                  </xsl:if>
               </xsl:for-each>
            </xsl:if>
         </xsl:otherwise>
      </xsl:choose>     
   </xsl:template>
   
   <xsl:function name="f:get-implemented-interfaces" as="element(interface)*">
      <xsl:param name="base" as="element(*) (: class or interface :)"/>
      <xsl:variable name="all-interfaces" as="element(*)*">
         <xsl:variable name="immediate" select="($base/implements | $base/extends) ! key('digestClassKey', @name, $digest)"/>
         <xsl:sequence select="$immediate, $immediate ! f:get-implemented-interfaces(.)"/>
      </xsl:variable>
      <!-- deduplicate and exclude non-interfaces-->
      <xsl:sequence select="$all-interfaces[self::interface] | ()"/>
   </xsl:function>
   
   <xsl:function name="f:gather-default-methods" as="element(*)*">
      <xsl:param name="thisClass" as="element()?"/>
      <xsl:for-each select="f:get-implemented-interfaces($thisClass)">
         <!--<xsl:message>Looking at interface {@name}</xsl:message>-->
         <xsl:sequence select="method[./default]"/>
      </xsl:for-each>
   </xsl:function>
   
   <xsl:function name="f:has-method" as="xs:boolean">
      <xsl:param name="thisClass" as="element(class)"/>
      <xsl:param name="method" as="xs:string"/>
      <xsl:param name="sig" as="xs:string"/>
      <!--<xsl:message>Has method {$thisClass/@name}#{$method}!{$sig} ? {exists($thisClass/method[@name=$method and string(@sig)=string($sig)])}</xsl:message>-->
      <xsl:sequence select="exists($thisClass/method[@name=$method and string(@sig)=string($sig)])"/>
   </xsl:function>
   
   <xsl:function name="f:has-concrete-method" as="xs:boolean">
      <xsl:param name="thisClass" as="element(class)"/>
      <xsl:param name="method" as="xs:string"/>
      <xsl:param name="sig" as="xs:string"/>
      <!--<xsl:message>Has concrete method {$thisClass/@name}#{$method}!{$sig} ? {exists($thisClass/method[@name=$method and string(@sig)=string($sig) and not(@abstract='1')])}</xsl:message>-->
      <xsl:sequence select="exists($thisClass/method[@name=$method and string(@sig)=string($sig) and not(@abstract='1')])"/>
   </xsl:function>
   
 
</xsl:stylesheet>