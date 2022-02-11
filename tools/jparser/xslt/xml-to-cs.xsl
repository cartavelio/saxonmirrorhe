<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   version="3.0"
   expand-text="yes"
   default-mode="xml-to-cs">
   
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
      'java.lang.System#out' : 'Out',
      'java.lang.System#in' : 'In' (:,
      'java.util.Map#get' : 'Get',
      'java.util.Map#put' : 'Put' :)}"/>
   
   <xsl:variable name="specialTypes" as="map(xs:string, xs:string)" select="map{
      'java.lang.CharSequence': 'string',
      'java.lang.Boolean': 'System.Boolean',
      'java.lang.Class': 'System.Type',
      'java.lang.Exception': 'System.Exception',
      'java.lang.Integer': 'System.Int32',
      'java.lang.Long': 'System.Int64',
      'java.lang.Object': 'System.Object',
      'java.lang.RuntimeException': 'System.Exception',
      'java.lang.System': 'System.Console',
      'java.lang.Throwable': 'System.Exception',
      'java.util.function.Function': 'System.Func',
      'java.util.function.Predicate': 'System.Predicate'
      }"/>
   
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
   
   <xsl:variable name="refactoredMethods" as="map(*)">
      <!-- Patterns for rewriting X.do() as something(X) -->
      <!-- Currently only handles zero-argument methods -->
      <!-- Currently insensitive to the type of the target of the method -->
      <xsl:variable name="json" as="xs:string" expand-text="no">
         { "printStackTrace": "Console.Write($0.ToString() + \" -- \" + $0.StackTrace)",
           "newInstance": "System.Activator.CreateInstance($0)"
         }    
      </xsl:variable>
      <xsl:sequence select="parse-json($json)"/>
   </xsl:variable>
   
   <xsl:variable name="refactoredProperties" as="map(*)">
      <!-- Not actually used: the "class" property is parsed as a ClassExpr -->
      <xsl:variable name="json" as="xs:string" expand-text="no">
         { "class": "typeof($0)" 
         }    
      </xsl:variable>
      <xsl:sequence select="parse-json($json)"/>
   </xsl:variable>
   
   
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
      <!-- Given a MethodDeclaration node, return the full method name in the form package.class[.class]*.method -->
      <xsl:param name="decl" as="element()"/>
      <!--<xsl:assert test="$decl/@nodeType = 'MethodDeclaration'"/>
      <xsl:assert test="exists($decl/ancestor::root/packageDeclaration/name)">Method: {path($decl)} in class: {$decl/ancestor::type[1]/name/@identifier}</xsl:assert>
      -->
      <xsl:for-each select="$decl">
         <xsl:choose>
            <xsl:when test="parent::default">
               <!-- this is a default method declaration in the digest -->
               <xsl:text>{ancestor::module[1]/@package}.{ancestor::interface[1]/@name}.{ancestor::method[1]/@name}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:sequence select="string-join((f:qualified-name(ancestor::root/packageDeclaration/name), ancestor::*[@nodeType='ClassOrInterfaceDeclaration']/name/@identifier, name/@identifier), '.')"/>
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
      <xsl:if test="$degen = 'java.util.function.Predicate'">
         <xsl:message>Renaming {$degen} to {$specialTypes($degen)}</xsl:message>
         <xsl:message>$specialTypes = {serialize($specialTypes, map{'method':'json'})}</xsl:message>
      </xsl:if>
      <xsl:sequence select="($specialTypes($degen), $degen)[1]"/>
   </xsl:function>
   
   <xsl:function name="f:degenerify" as="xs:string?">
      <xsl:param name="nameOfType" as="xs:string?"/>
      <xsl:sequence select="replace($nameOfType, '&lt;.*$', '') => f:escapeDottedName()"/>
   </xsl:function>
   
   <xsl:function name="f:escapeSimpleName" as="xs:string">
      <xsl:param name="simpleName" as="xs:string"/>
      <xsl:sequence select="if ($simpleName = $reservedWords) then ('@' || $simpleName) else $simpleName"/>
   </xsl:function>
   
   <xsl:function name="f:escapeDottedName" as="xs:string">
      <xsl:param name="nameOfType" as="xs:string?"/>
      <xsl:sequence select="tokenize($nameOfType, '\.') ! f:escapeSimpleName(.) => string-join('.')"/>
   </xsl:function>
   
   <xsl:function name="f:refactor-method-call" as="xs:string">
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
   
   <xsl:function name="f:refactor-property-reference" as="xs:string">
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
   </xsl:function>
   
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
      <xsl:apply-templates select="*"/>
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
<!--         <xsl:variable name="implementsIterator" 
            select="implementedTypes/implementedType[starts-with(@RESOLVED_TYPE, 'java.lang.Iterable')]"/>
         <xsl:if test="$implementsIterator">
            <!-\- A Java Iterable becomes a C# IEnumerable, so it needs to implement GetEnumerator -\->
            <xsl:text>{$NL}</xsl:text>
            <xsl:call-template name="indent"/>
            <xsl:text>public {replace($implementsIterator/@RESOLVED_TYPE, 'java.lang.Iterable', 'System.Collections.Generic.IEnumerator')} GetEnumerator() {{{$NL}</xsl:text>
            <xsl:text>    return new com.saxonica.glue.EnumeratorFromIterator(this.iterator());</xsl:text>
            <xsl:text>}}{$NL}</xsl:text>
         </xsl:if>-->
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
      <xsl:sequence select="exists($interfaceDecl[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='true']
         [annotations/annotation/name/@identifier='FunctionalInterface'][count(members/member)=1])"/>
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
      <xsl:variable name="isConstant" select="count(variables/variable)=1 
         and variables/variable/initializer and f:isConstantExpression(variables/variable/initializer)"/>
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
      <xsl:text>// Extra method to make the class IEnumerable as well as Iterable{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>public {replace(type/@RESOLVED_TYPE, 'java\.util\.Iterator', 'System.Collections.Generic.IEnumerator')} GetEnumerator() {{{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>return new com.saxonica.glue.EnumeratorFromIterator{replace(type/@RESOLVED_TYPE, '^[^&lt;]+', '')}(iterator());{$NL}</xsl:text>
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
      <xsl:if test="count(key('digestMethodKey', $full-name, $digest)) gt 1">
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
         <xsl:when test="body and modifiers/modifier/@keyword[. = 'SYNCHRONIZED']">
            <xsl:text> {{ lock(this) {{{$NL}</xsl:text>
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
         <xsl:apply-templates select="type"/>
         <xsl:if test="@isVarArgs='true'">[] </xsl:if>
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
      <xsl:text>com.saxonica.glue.Utils.unsignedRightShift(</xsl:text>
      <xsl:apply-templates select="left"/>
      <xsl:text>, </xsl:text>
      <xsl:apply-templates select="right"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ConditionalExpr']">
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
            <!--<xsl:text>(</xsl:text>-->
            <xsl:apply-templates select="body/expression"/>
            <!--<xsl:text>)</xsl:text>-->
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>

   <xsl:template match="iterable[@nodeType='MethodCallExpr'][name/@identifier='entrySet']" priority="30">
      <xsl:apply-templates select="scope/*"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Convert first character of a string to upper-case -->
   <xsl:function name="f:capitalize" as="xs:string">
      <xsl:param name="in" as="xs:string"/>
      <xsl:sequence select="upper-case(substring($in, 1, 1)) || substring($in, 2)"/>
   </xsl:function>
   
   <!-- Convert calls on delegate methods -->
   <!-- For example 'consumer.accept(item)' becomes 'consumer(item)' if consumer is a functional interface
        that converts to a C# delegate -->
   <xsl:template match="*[@nodeType='MethodCallExpr'][key('digestClassKey', f:degenerify(@RESOLVED_TYPE), $digest)/@isDelegate='1']" priority="30">      
      <xsl:apply-templates select="scope"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Convert method calls applied to java.lang.String -->
   <xsl:template match="*[@nodeType='MethodCallExpr'][@RESOLVED_TYPE=('java.lang.String','java.lang.CharSequence')]" priority="30">
      <xsl:variable name="renamedMethods" as="map(*)" select="map{
         'toLowerCase': 'ToLower',
         'toUpperCase': 'ToUpper',
         'hashCode':    'GetHashCode'}"/>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'charAt'">
            <xsl:apply-templates select="scope"/>
            <xsl:text>[</xsl:text>
            <xsl:apply-templates select="arguments"/>
            <xsl:text>]</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'length'">
            <xsl:apply-templates select="scope"/>
            <xsl:text>.Length</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'isEmpty'">
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="scope"/>
            <xsl:text>.Length==0)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'matches'">
            <xsl:text>System.Text.RegularExpressions.Regex.IsMatch(</xsl:text>
            <xsl:apply-templates select="scope"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'getChars'">
            <!-- Java str.getChars(begin, end, array, dest) => C# str.CopyTo(begin, array, dest, count) -->
            <xsl:text>com.saxonica.glue.Utils.stringToCharArray(</xsl:text>
            <xsl:apply-templates select="scope"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'equalsIgnoreCase'">
            <!-- Java str.getChars(begin, end, array, dest) => C# str.CopyTo(begin, array, dest, count) -->
            <xsl:text>com.saxonica.glue.Utils.stringEqualsIgnoreCase(</xsl:text>
            <xsl:apply-templates select="scope"/>
            <xsl:text>, </xsl:text>
            <xsl:apply-templates select="arguments"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="map:contains($renamedMethods, name/@identifier)">
            <xsl:apply-templates select="scope"/>
            <xsl:text>.</xsl:text>
            <xsl:value-of select="$renamedMethods(name/@identifier)"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="scope"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodCallExpr']" priority="10">
      <xsl:variable name="digestMethod" select="key('digestMethodKey', @RESOLVED_TYPE||'.'||name/@identifier, $digest)"/>
      <!-- Be prepared to cast the result to the required type in the case where the C# method returns a supertype of 
           the return type of the corresponding Java method -->
      <xsl:variable name="optionalCast" 
         select="if (not(parent::statement) and count($digestMethod)=1 and $digestMethod[@returns != @csReturns]) 
                 then $digestMethod/@returns 
                 else ''"/>
      <xsl:if test="$optionalCast">(({$optionalCast})</xsl:if>
      <xsl:if test="scope">
         <xsl:choose>
           <xsl:when test="scope/name/@nodeType='SimpleName' and ends-with(f:degenerify(@RESOLVED_TYPE), '.'||scope/name/@identifier) and not(scope/scope)">
               <!-- A simple static method call -->
               <xsl:text>{f:escapeDottedName(@RESOLVED_TYPE)}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:apply-templates select="scope"/>
            </xsl:otherwise>
         </xsl:choose>         
         <xsl:text>.</xsl:text>         
      </xsl:if>
      <xsl:apply-templates select="typeArguments"/>
      <xsl:variable name="property" select="f:method-as-property(.)"/>
        
      <xsl:choose>
         <xsl:when test="$property">{$property}</xsl:when>     
         <xsl:otherwise>           
            <xsl:apply-templates select="name"/>
            <xsl:text>(</xsl:text> 
            <xsl:apply-templates select="arguments"/>
            <xsl:text>)</xsl:text>            
         </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="$optionalCast">)</xsl:if>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodCallExpr'][$refactoredMethods(name/@identifier)]" priority="20">
      <xsl:value-of select="f:refactor-method-call(., name/@identifier)"/>
   </xsl:template>
   
   
   <!-- special-case getKey() and getValue() in KeyValuePair -->
   <xsl:function name="f:method-as-property" as="xs:string?">
      <xsl:param name="expr" as="element(*)"/>
      <xsl:variable name="simpleName" select="f:degenerify($expr/@RESOLVED_TYPE)"/>
      <xsl:sequence select="$specialProperties($simpleName)?($expr/name/@identifier)"/>
      <!--<xsl:if test="starts-with($expr/@RESOLVED_TYPE, 'java.util.Map.Entry')">
         <xsl:variable name="name" select="$expr/name/@identifier"/>
         <xsl:choose>
            <xsl:when test="$name = 'getKey'">Key</xsl:when>
            <xsl:when test="$name = 'getValue'">Value</xsl:when>
         </xsl:choose>
      </xsl:if> -->
   </xsl:function>
   
  <!-- <xsl:template match="*[@nodeType='MethodCallExpr'][starts-with(@RESOLVED_TYPE, 'java.util.Map.Entry')]" priority="15">
      <xsl:message>#674 MethodCallExpr {name/@identifier}</xsl:message>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'getValue'">Value</xsl:when>
         <xsl:when test="name/@identifier = 'getKey'">Key</xsl:when>
         <xsl:otherwise><xsl:next-match/></xsl:otherwise>
      </xsl:choose>
   </xsl:template>-->
   
   <!--<xsl:template match="*[@nodeType='MethodCallExpr'][typeArguments]" priority="20">
      <!-\- for example Collections.<AtomicValue>emptyList() -\->
      <!-\- this is rare and we special-case it; we're probably not handling the full generality -\->
      <xsl:if test="scope">
         <xsl:apply-templates select="scope"/>
         <xsl:text>.</xsl:text>
      </xsl:if>
      <xsl:apply-templates select="typeArguments"/>
      <xsl:apply-templates select="name"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>-->
   
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
         
   <xsl:template match="*[@nodeType='FieldDeclaration'][variables/variable/initializer[contains(@nodeType, 'Literal')]]/modifiers/modifier[@keyword = 'STATIC']" priority="20"/>  
   
   <xsl:template match="*[@nodeType=('MethodDeclaration')]/modifiers/modifier[@keyword = 'FINAL']" priority="20"/>

   
  <!-- <xsl:template match="type | typeArgument[name] | thrownException | componentType">
      <xsl:apply-templates select="name, typeArguments"/>
   </xsl:template>-->
   
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
   
   <!--<xsl:template match="*[@nodeType = 'ClassOrInterfaceType'][name/@identifier='boolean']" priority="30">
      <xsl:text>bool</xsl:text>
   </xsl:template>-->
   
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
   
   <!-- Handle a variable initialisation of the form List<X> var = new ArrayList<>(); -->
   <!-- This becomes List<X> var = new ArrayList<X>(); -->
   
   <xsl:template match="variable[initializer[@nodeType='ObjectCreationExpr']/type/typeArguments[not(*)]][type/typeArguments/typeArgument]">
      <xsl:next-match>
         <xsl:with-param name="implicit-type-arguments" select="type/typeArguments" tunnel="yes"/>
      </xsl:next-match>
   </xsl:template>

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
      <xsl:param name="implicit-type-arguments" tunnel="yes" 
         as="element(typeArguments)?" select="()"/>
      <xsl:text>new </xsl:text>
      <xsl:apply-templates select="type">
         <xsl:with-param name="implicit-type-arguments" tunnel="yes"
            select="if (exists($implicit-type-arguments)) 
                     then $implicit-type-arguments 
                     else if (@REQUIRED_TYPE) 
                           then f:make-implicit-type-arguments(@REQUIRED_TYPE) 
                           else ()"/>

      </xsl:apply-templates>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ObjectCreationExpr'][anonymousClassBody]" priority="20">
      <xsl:text>new {f:allocateInnerClassName(.)}(</xsl:text>
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
      <xsl:text>public {$allocatedName}({$containingClass} outerInstance /*...*/)</xsl:text>
      <xsl:text> : base(outerInstance /*, ...*/) {{{$NL}</xsl:text>
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
   
   <xsl:template match="*[@nodeType=('IntegerLiteralExpr', 'BooleanLiteralExpr', 'DoubleLiteralExpr', 'LongLiteralExpr')]">
      <xsl:text>{@value}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='InstanceOfExpr']">
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
   
   <xsl:template match="*[@nodeType='FieldAccessExpr']">
      <xsl:apply-templates select="scope"/>
      <xsl:text>.</xsl:text>
      <xsl:apply-templates select="name"/>
   </xsl:template>
   
   <!-- length accessor on arrays -->
   <xsl:template match="*[@nodeType='FieldAccessExpr'][name/@identifier='length'][ends-with(@RESOLVED_TYPE, '[]')]" priority="50">
      <xsl:apply-templates select="scope"/>
      <xsl:text>.Length</xsl:text>
   </xsl:template>
   
   <!-- Identify a static field accessor such as BooleanValue.TRUE -->
   <xsl:template match="*[@nodeType='FieldAccessExpr'][scope/name/@nodeType='SimpleName'][ends-with(f:degenerify(@RESOLVED_TYPE), '.'||scope/name/@identifier)][not(scope/scope)]" priority="18">
      <xsl:text>{f:escapeDottedName(@RESOLVED_TYPE)}.{name/@identifier}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='FieldAccessExpr'][$refactoredProperties(name/@identifier)]" priority="20">
      <xsl:value-of select="f:refactor-property-reference(., name/@identifier)"/>
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
   
   <!-- Attempt to identify generic type for an assignment like x = new HashMap<>() -->
   <!-- We have to resort to parsing the resolved type of the variable x -->
   <xsl:template match="*[@nodeType='AssignExpr'][value[@nodeType='ObjectCreationExpr' and type/typeArguments[not(*)]]]" priority="20">
      <xsl:next-match>
         <xsl:with-param name="implicit-type-arguments" 
            select="if (target/@RESOLVED_TYPE) then f:make-implicit-type-arguments(target/@RESOLVED_TYPE) else ()" 
            as="element(typeArguments)?" 
            tunnel="yes"/>
      </xsl:next-match>
   </xsl:template>
   
   <xsl:function name="f:make-implicit-type-arguments" as="element(typeArguments)">
      <xsl:param name="genericType" as="xs:string"/>
      <typeArguments>
         <xsl:for-each select="$genericType => substring-after('&lt;') => substring-before('>') => tokenize(',')">
            <typeArgument nodeType="ClassOrInterfaceType" RESOLVED_TYPE="{.}">
               <name nodeType="SimpleName" identifier="{tokenize(., '\.')[last()]}"/>
            </typeArgument>
         </xsl:for-each>               
      </typeArguments>
   </xsl:function>
      
   
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
   
   <xsl:template match="*[@nodeType='NameExpr']">
      <xsl:apply-templates select="name"/>
   </xsl:template>
   
   <xsl:key name="digestClassKey" match="class | interface | enum" 
      use="string-join((ancestor::module/@package, ancestor-or-self::*[self::class or self::interface or self::enum]/@name), '.')"/>
   
   <xsl:key name="digestMethodKey" match="method"
      use="string-join((ancestor::module/@package, ancestor-or-self::*[self::class or self::interface]/@name, ./@name), '.')"/>
   
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
            <!-- This shouldn't happen, but it does, with strange uses of nested inner classes -->
            <xsl:message>No digest found for {$thisClassName}</xsl:message>
         </xsl:when>
         <xsl:otherwise>
            <xsl:variable name="defaults" select="f:gather-default-methods($thisDigest)|()" as="element(*)*"/>
            <!--<xsl:message>Found {count($defaults)} default methods {string-join($defaults/(@name || '-' || generate-id()), ';')}</xsl:message>-->
            <xsl:for-each-group select="$defaults" group-by="@name, @arity" composite="yes">
               <!--<xsl:message>Considering default for {@name}#{@arity}</xsl:message>
               <xsl:message>Has concrete method? {f:has-concrete-method($thisDigest, current()/@name, xs:integer(current()/@arity))}</xsl:message>
               <xsl:message>Superclass implements? {$thisDigest/extends ! key('digestClassKey', @name, $digest) ! f:get-implemented-interfaces(.) ! @name}</xsl:message>-->
               <xsl:if test="not(f:has-concrete-method($thisDigest, current()/@name, xs:integer(current()/@arity)))
                  and (empty(($thisDigest/extends ! key('digestClassKey', @name, $digest) ! f:get-implemented-interfaces(.)) intersect current()/..))">
                  <xsl:text>{$NL}</xsl:text>
                  <xsl:call-template name="indent-extra"/>
                  <xsl:text>/* Expanding default method {@name} from interfaces */{$NL}</xsl:text>
                  <xsl:apply-templates select="current-group()[last()]/default/*">
                     <xsl:with-param name="expanding-defaults" as="xs:boolean" select="true()" tunnel="yes"/>
                     <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
                  </xsl:apply-templates>
               </xsl:if>        
            </xsl:for-each-group>
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
   
   <xsl:function name="f:has-concrete-method" as="xs:boolean">
      <xsl:param name="thisClass" as="element(class)"/>
      <xsl:param name="method" as="xs:string"/>
      <xsl:param name="arity" as="xs:integer"/>
      <!--<xsl:message>Has concrete method {$thisClass/@name} . {$method}#{$arity}</xsl:message>-->
      <xsl:sequence select="exists($thisClass/method[@name=$method and xs:integer(@arity)=$arity and not(@abstract='1')])"/>
   </xsl:function>
   
 
</xsl:stylesheet>