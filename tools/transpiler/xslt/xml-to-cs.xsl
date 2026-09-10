<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math"
   exclude-result-prefixes="#all"
   version="3.0"
   expand-text="yes"
   default-mode="xml-to-cs">
   
   <xsl:include href="functions.xsl"/>
   <xsl:include href="diamond.xsl"/>
   <xsl:include href="methods.xsl"/>
   <xsl:include href="fields.xsl"/>
   <xsl:include href="constructors.xsl"/>
   
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
   
   <xsl:mode name="xml-to-cs" saxon:trace="no" xmlns:saxon="http://saxon.sf.net/"/>
   
   <xsl:template name="indent">
      <xsl:param name="indent" as="xs:integer" tunnel="yes" select="0"/>
      <xsl:value-of select="string-join((1 to $indent*4)!' ', '')"/>
   </xsl:template>
   
   <xsl:function name="f:indent">
      <xsl:param name="indent" as="xs:integer"/>
      <xsl:value-of select="string-join((1 to $indent*4)!' ', '')"/>
   </xsl:function>
   
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
      <xsl:text>using System.Linq;{$NL}</xsl:text>
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
      <xsl:text>namespace {f:qualifiedName(name) => replace('^com\.saxonica', 'Saxon.Eej') => replace('^net\.sf\.saxon', 'Saxon.Hej')} </xsl:text>
   </xsl:template>
   
   <xsl:template match="import"/>
   
   <xsl:template match=".[. instance of xs:string]">
      <xsl:value-of select="."/>
   </xsl:template>
   
   <xsl:template match="name | *[@nodeType='Name']">
      <xsl:value-of select="f:renameMethod(@identifier, ..)"/>
   </xsl:template>
   
   <xsl:template match="scope">
      <xsl:apply-templates select="*"/>
   </xsl:template>
   
   <!-- This rule compensates for an inadequacy in the JavaParser symbol solver -->
   <xsl:template match="scope[@UNRESOLVED and ends-with(../@RESOLVED_TYPE, @UNRESOLVED)]" priority="10">
      <xsl:choose>
         <xsl:when test="matches(../@RESOLVED_TYPE, '^(java\.|javax\.|net\.sf\.saxon|com.\saxonica)')">
            <xsl:text>{f:renameGenericType(../@RESOLVED_TYPE)}</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>{f:escapeDottedName(../@RESOLVED_TYPE) => f:adjustTypeNamespace()}</xsl:text>
         </xsl:otherwise>
      </xsl:choose>     
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
      <xsl:variable name="renamed" as="xs:string" select="f:renameType(@RESOLVED_TYPE)"/>
      <!--<xsl:message>Type {@RESOLVED_TYPE} renamed as {$renamed}</xsl:message>-->
      <xsl:value-of select="$renamed"/>
      <xsl:if test="not($dropTypeParameters = $renamed) and typeArguments/typeArgument"> <!-- exclude diamond operator -->
         <xsl:apply-templates select="typeArguments"/>
      </xsl:if>
   </xsl:template>
   
   <xsl:template match="extendedType[@nodeType = 'ClassOrInterfaceType']
      [@RESOLVED_TYPE=('java.lang.Exception', 'java.lang.RuntimeException')]" priority="25">
      <xsl:text>{@RESOLVED_TYPE => replace('java\.lang', 'Saxon.Ejava.lang')}</xsl:text>
   </xsl:template>
   
   <xsl:template name="classOrInterface">
      <xsl:param name="indent" as="xs:integer" tunnel="yes"/>
      <xsl:param name="include-nested-interfaces" select="false()" tunnel="yes"/>
      <xsl:assert test="@nodeType='ClassOrInterfaceDeclaration'"/>
      <xsl:variable name="kind" select="if (@isInterface='true') then 'interface' else 'class'" as="xs:string"/>
      <xsl:variable name="containing-interface" select="ancestor::*[@isInterface='true'][1]" as="element(*)?"/>
      <xsl:if test="$kind = 'class' or $include-nested-interfaces or empty($containing-interface)">
         <!-- that is, process everything other than a nested inner interface -->
         <xsl:variable name="className" select="($renamedClasses?(name/@identifier),  name/@identifier)[1]"/>
         <xsl:text>{$kind} {if (exists($containing-interface)) then ($containing-interface/name/@identifier || '_') else ''}{$className}</xsl:text>
         <xsl:apply-templates select="typeParameters"/>
         <!-- If the class implements java.lang.Iterable, then make it also implement IEnumerable so it
              can be used in a foreach loop -->
         <xsl:variable name="additionalSuperTypes" as="element(*)*">
            <!--<xsl:for-each select="extendedTypes/extendedType, implementedTypes/implementedType">
               <xsl:if test="starts-with(@RESOLVED_TYPE, 'java.lang.Iterable')">
                  <implementedType 
                     nodeType="ClassOrInterfaceType" 
                     RESOLVED_TYPE="{replace(@RESOLVED_TYPE, 'java\.lang\.Iterable', 'System.Collections.Generic.IEnumerable')}">
                     <name nodeType="SimpleName" identifier="IEnumerable"/>
                     <xsl:copy-of select="typeArguments"/>
                  </implementedType>
               </xsl:if>              
            </xsl:for-each>-->
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
         
         <!-- Declare type bounds. If no type bound is given explicitly, generate "where T : class" to indicate that it must be
              a "reference type" (otherwise "null" isn't allowed) -->
         <xsl:variable name="thisDeclaration" select="."/>
         <xsl:for-each select="typeParameters/typeParameter">
            <xsl:variable name="parameterName" select="name/@identifier"/>
            <xsl:choose>
               <xsl:when test="typeBound/typeBoun/@RESOLVED_TYPE">                  
                  <xsl:text> where {name/@identifier} : {f:renameType(typeBound/typeBoun/@RESOLVED_TYPE)}</xsl:text>
               </xsl:when>
               <xsl:when test="$thisDeclaration/annotations/annotation[name/@identifier='CSharpTypeBounds']">
                  <xsl:variable name="bound" select="$thisDeclaration/annotations/annotation[name/@identifier='CSharpTypeBounds']
                     //*[@nodeType='StringLiteralExpr'][normalize-space(substring-before(@value, ':')) eq string($parameterName)]"/>
                  <xsl:if test="normalize-space($bound/@value)">
                     <xsl:text> where {$bound/@value}</xsl:text>   
                  </xsl:if>
               </xsl:when>
               <xsl:otherwise/>
            </xsl:choose>
         </xsl:for-each>
         
 
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
            <xsl:text>        {f:unescapeJava(@value)}{$NL}</xsl:text>                    
         </xsl:for-each>
         <xsl:if test="exists(annotations/annotation[name/@identifier='CSharpInjectMembers'])">    // End of injected C# code{$NL}</xsl:if>
         
         <xsl:apply-templates select="members/member">
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
         </xsl:apply-templates>
         
         <!-- Anonymous inner classes anywhere in the body are now generated as named inner classes -->
         <xsl:call-template name="indent"/>
         <xsl:apply-templates select=".//anonymousClassBody">
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
         </xsl:apply-templates>
         
         <!-- Expand default methods inherited from interfaces -->
         <xsl:if test="not(@isInterface='true')">
            <xsl:call-template name="expand-defaulted-methods"/>
         </xsl:if>
         <xsl:text>{$NL}</xsl:text>
         <xsl:call-template name="indent"/>
         <xsl:text>}}{$NL}</xsl:text>
      </xsl:if>
      
      <!-- Outside the class, create definitions of nested interfaces -->
      
      <!--<xsl:message>Looking for nested interfaces: {$include-nested-interfaces}</xsl:message>-->
      <xsl:if test="$kind = 'interface' and not($include-nested-interfaces)">
         <!-- interfaces nested within other interfaces are pushed up a level, renaming Push.Document as Push_Document -->
         <xsl:apply-templates select="members/member[@isInterface='true']">
            <xsl:with-param name="include-nested-interfaces" select="true()" tunnel="yes"/>
         </xsl:apply-templates>
      </xsl:if>
   </xsl:template>
   
   <!-- Code to inject an implementation of IEnumerable to a class that already supports Iterable -->
   <xsl:template name="addEnumeratorMethods">
      <xsl:param name="deliveredType" as="xs:string"/>
      <xsl:param name="indent" as="xs:integer" tunnel="yes"/>
      <xsl:variable name="nl" select="f:indent($indent)"/>
      <xsl:text>
{$nl}object System.Collections.IEnumerator.Current => Current;
{$nl}public {$deliveredType} Current {{ get; private set; }}
{$nl}public bool MoveNext() {{
{$nl}    if (hasNext()) {{Current = next(); return true;}} else return false;
{$nl}}}
{$nl}public void Reset() {{throw new NotImplementedException();}}
{$nl}public void Dispose() {{}}
      </xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ClassOrInterfaceDeclaration']">
      <xsl:call-template name="indent"/>
      <xsl:if test="exists(annotations/annotation[name/@identifier='CSharpNullable'])">
         <xsl:text>#nullable {annotations/annotation[name/@identifier='CSharpNullable']//@value}{$NL}</xsl:text>
         <xsl:call-template name="indent"/>
      </xsl:if>
      <xsl:if test="exists(annotations/annotation[name/@identifier='CSharpSuppressCompilerWarnings'])">
         <xsl:text>#pragma warning disable {annotations/annotation[name/@identifier='CSharpSuppressCompilerWarnings']//@value}{$NL}</xsl:text>
         <xsl:call-template name="indent"/>
      </xsl:if>
      <xsl:if test="members/member[@nodeType='MethodDeclaration']/name/@identifier='computeHashCode'">
         <!-- Subclasses of Expression have a method computeHashCode(), called by hashCode() on the superclass,
              which computes the hash code and causes it to be cached. This means that the C# warning CS0659 indicating
              that GetHashCode() is not implemented is in fact spurious, so we suppress the warning -->
         <xsl:text>#pragma warning disable 659{$NL}</xsl:text>
         <xsl:call-template name="indent"/>
      </xsl:if>
      <xsl:choose>
         <xsl:when test="annotations/annotation[name/@identifier='CSharpModifiers']">
            <xsl:value-of select="annotations/annotation[name/@identifier='CSharpModifiers']//@value ! string()" separator=" "/>
            <xsl:text> </xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:call-template name="modifiers"/>
         </xsl:otherwise>
      </xsl:choose>     
      <xsl:call-template name="classOrInterface"/>
   </xsl:template>
   
  
   
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
      
      <xsl:choose>
         <xsl:when test="annotations/annotation[name/@identifier='CSharpModifiers']">
            <xsl:value-of select="annotations/annotation[name/@identifier='CSharpModifiers']//@value ! string()" separator=" "/>
            <xsl:text> </xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:variable name="isConstant" select="
               count(variables/variable)=1 
               and variables/variable/initializer 
               and f:isConstantExpression(variables/variable/initializer)
               and modifiers/modifier[@keyword='STATIC']
               and modifiers/modifier[@keyword='FINAL']"/>
            <xsl:call-template name="modifiers">
               <xsl:with-param name="isConstant" select="$isConstant" tunnel="yes"/>
            </xsl:call-template>
         </xsl:otherwise>
      </xsl:choose>    
      <xsl:apply-templates select="variables"/>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ConstructorDeclaration']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:choose>
         <xsl:when test="annotations/annotation[name/@identifier='CSharpModifiers']">
            <xsl:value-of select="annotations/annotation[name/@identifier='CSharpModifiers']//@value ! string()" separator=" "/>
            <xsl:text> </xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:call-template name="modifiers"/>
         </xsl:otherwise>
      </xsl:choose>   
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
         [f:isIteratorType(type/@RESOLVED_TYPE)]
         [empty(parameters/parameter)]" priority="20">
      <!-- If there's a method to implement java.lang.Iterable, it gets converted to implement
           return System.Collections.Generic.IEnumerator, but we need another method to implement
           a non-generic System.Collections.IEnumerator as well  -->
      <xsl:next-match/>
      <xsl:call-template name="indent"/>
      <xsl:text>// Extra method to make the class IEnumerable as well as IEnumerable&lt;T&gt;{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {{{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>return GetEnumerator();{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodDeclaration']
         [name/@identifier = 'compare']
         [count(parameters/parameter) = 2]" priority="20">
      <!-- If there's a compare(a, b) method, add a Compare(a, b) method  -->
      <xsl:next-match/>
      <xsl:call-template name="indent"/>
      <xsl:text>// Extra method to make the class a C# Comparer{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>public override int Compare(</xsl:text>
      <xsl:apply-templates select="parameters/parameter[1]/type"/>
      <xsl:text> a, </xsl:text>
      <xsl:apply-templates select="parameters/parameter[2]/type"/>
      <xsl:text> b) {{{$NL}</xsl:text>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>return compare(a, b);{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <!-- Don't process a method declaration if (a) it's on an interface, (b) it's marked @Override,
        and (c) it defines a default implementation. -->
   <xsl:template match="*[@nodeType='MethodDeclaration']
                         [../../@isInterface='true']
                         [modifiers/modifier/@keyword='DEFAULT']
                         [annotations/annotation/name/@identifier='Override']" priority="20"/>
   
   <xsl:template match="*[@nodeType='MethodDeclaration']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:param name="expandingDefaults" select="false()" tunnel="yes" as="xs:boolean"/>
      <xsl:call-template name="indent"/>
      <xsl:variable name="full-name" select="f:full-method-name(.)"/>
      <xsl:variable name="arity" select="count(parameters/parameter)"/>
      <!--<xsl:message>Method {name/@identifier} = {$full-name}</xsl:message>-->
      <xsl:variable name="digestMethod" select="key('digestMethodKey', $full-name, $digest)[1]"/>
      
      <xsl:if test="$digestMethod/@hidden='1' and not($expandingDefaults)">        
         <xsl:text>/* implicit ====={$NL}</xsl:text>
         <xsl:call-template name="indent"/>
      </xsl:if>
      
      <!-- Modifiers -->
      
      <xsl:choose>
         
         <xsl:when test="annotations/annotation[name/@identifier='CSharpModifiers']">
            <xsl:value-of select="annotations/annotation[name/@identifier='CSharpModifiers']//@value ! string()" separator=" "/>
            <xsl:text> </xsl:text>
            <xsl:text> /* REPLACING </xsl:text>
            <xsl:if test="body and $expandingDefaults and not(modifiers/modifier/@keyword[. = 'PUBLIC'])">public </xsl:if>
            <xsl:call-template name="modifiers">
               <xsl:with-param name="digestMethod" select="$digestMethod"/>
            </xsl:call-template>
            <xsl:text>*/ </xsl:text>
         </xsl:when>
         <xsl:when test="self::anonymousClassBod">
            <!-- Detect need for "override" modifier on methods in anonymous inner classes (not in digest)-->
            <xsl:variable name="superclass" select="string(../../type/@RESOLVED_TYPE)"/>
            <xsl:variable name="superclassDigest" select="key('digestClassKey', $superclass, $digest)"/>
            <xsl:text>public </xsl:text>
            <xsl:if test="$superclassDigest[self::class] and name/@identifier = $superclassDigest/method/@name">
               <xsl:text>override </xsl:text>
            </xsl:if>
         </xsl:when>
         <xsl:otherwise>
            <xsl:if test="body and $expandingDefaults and not(modifiers/modifier/@keyword[. = 'PUBLIC'])">public </xsl:if>
            <xsl:call-template name="modifiers">
               <xsl:with-param name="digestMethod" select="$digestMethod"/>
            </xsl:call-template>
         </xsl:otherwise>
      </xsl:choose>
      
      <!-- Return type -->
      
      <xsl:choose>
         <xsl:when test="f:isIteratorType(string(type/@RESOLVED_TYPE))">
            <xsl:variable name="typeArgs" select="f:extract-type-arguments(type/@RESOLVED_TYPE)
                                                  => replace('\? extends ', '')"/>           
            <xsl:text>System.Collections.Generic.IEnumerator&lt;{$typeArgs}&gt;</xsl:text>
         </xsl:when>
         <xsl:when test="string($digestMethod/@csReturns)">{f:escapeDottedName(f:renameType($digestMethod/@csReturns))}</xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="type"/> <!-- TODO: take return type from digest? -->
         </xsl:otherwise>
      </xsl:choose>
      
      <!-- Method name -->
      
      <xsl:choose>
         <xsl:when test="name/@identifier = 'iterator' and f:isIteratorType(string(type/@RESOLVED_TYPE))">
            <xsl:text> GetEnumerator</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'toString' and $arity=0">
            <xsl:text> ToString</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'equals' and $arity=1">
            <xsl:text> Equals</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'hashCode' and $arity=0">
            <xsl:text> GetHashCode</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text> {f:escapeMethodName(name/@identifier)}</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
      
      <!-- Type arguments -->
      
      <xsl:apply-templates select="typeParameters"/>
      
      <!-- Parameter declarations -->
      
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="parameters"/>
      <xsl:text>)</xsl:text>
      
      <!-- Type bounds (where clauses) -->
      
      <xsl:variable name="bounds" select="annotations/annotation[name/@identifier='CSharpTypeBounds']"/>
      <xsl:if test="$bounds and normalize-space($bounds/@value)">
          <xsl:text> where {$bounds/@value}</xsl:text>   
      </xsl:if>
      
      <xsl:for-each select="typeParameters/typeParameter[typeBound/typeBoun]">
         <xsl:text> where {name/@identifier} : {f:renameGenericType(typeBound/typeBoun/@RESOLVED_TYPE)}</xsl:text>
      </xsl:for-each>
      
      <!-- Method body -->
      
      <xsl:choose>
         <xsl:when test="annotations/annotation[name/@identifier='CSharpReplaceBody']">
            <xsl:text> {{{$NL}</xsl:text>
            <xsl:call-template name="indent-extra"/>
            <xsl:text>{annotations/annotation[name/@identifier='CSharpReplaceBody']//*[@nodeType='StringLiteralExpr']/@value ! f:unescapeJava(.)}{$NL}</xsl:text>
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
               <xsl:with-param name="suppress-field-initialization" select="false()" tunnel="yes"/>
            </xsl:apply-templates>
            <xsl:call-template name="indent"/>
            <xsl:text>}}}}{$NL}</xsl:text>
         </xsl:when>
         <xsl:when test="body and ($expandingDefaults or not(modifiers/modifier/@keyword[. = 'DEFAULT']))">
            <xsl:text> {{{$NL}</xsl:text>
            <xsl:apply-templates select="body/statements">
               <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
               <xsl:with-param name="suppress-field-initialization" select="false()" tunnel="yes"/>
            </xsl:apply-templates>
            <xsl:call-template name="indent"/>
            <xsl:text>}}{$NL}</xsl:text>
         </xsl:when>
         <xsl:otherwise>;{$NL}</xsl:otherwise>
      </xsl:choose>
      <xsl:if test="$digestMethod/@hidden='1' and not($expandingDefaults)">
         <xsl:call-template name="indent"/>
         <xsl:text>===== implicit */{$NL}</xsl:text>
      </xsl:if>
   </xsl:template>
   
   <xsl:template match="*[@nodeType=('MethodDeclaration', 'ConstructorDeclaration', 'FieldDeclaration')]
                         [annotations/annotation[name/@identifier='CSharpReplaceMethod']]" priority="30">
      <xsl:param name="indent" tunnel="yes" select="0"/>
      <xsl:call-template name="indent"/>
      <xsl:text>/* C# Replacement Method */{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:value-of select="annotations/annotation[name/@identifier='CSharpReplaceMethod']
         //*[@nodeType='StringLiteralExpr']/@value" separator="{$NL || f:indent($indent)}"/>
      <xsl:text>{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>/* End of C# Replacement Method */{$NL}</xsl:text>
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
   
   <!-- Handle enum classes annotated as CSharpSimpleEnum -->
   <xsl:template match="*[@nodeType='EnumDeclaration'][annotations/annotation/name/@identifier='CSharpSimpleEnum']" priority="10">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:variable name="isFlags" as="xs:boolean"
         select="annotations/annotation[name/@identifier='CSharpSimpleEnum']/pairs/pair[name/@identifier='flags']/value/@value='true'"/>
      <xsl:call-template name="indent"/>
      <xsl:if test="$isFlags">[System.Flags] </xsl:if>
      <xsl:call-template name="modifiers"/>
      <xsl:text>enum {name/@identifier} {{{$NL}</xsl:text>
      <xsl:if test="$isFlags">
         <xsl:call-template name="indent-extra"/>
         <xsl:text>NONE = 0,{$NL}</xsl:text>
      </xsl:if>
      <xsl:for-each select="entries/entrie">
         <xsl:call-template name="indent-extra"/>
         <xsl:text>{name/@identifier}</xsl:text>
         <xsl:if test="$isFlags">
            <xsl:text> = {math:pow(2, position()-1)}</xsl:text>
         </xsl:if>
         <xsl:text>,{$NL}</xsl:text>
      </xsl:for-each>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='EnumDeclaration']">
      <!-- Code for complex enum classes is inspired by the Tangible converter -->
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:variable name="enumDecl" select="."/>
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
      <!-- The Java language spec says that `values()` returns an array, but returning a list works well enough -->
      <xsl:text>public static IList&lt;{$className}> values() {{{$NL}</xsl:text>
      <xsl:call-template name="indent-plus-two"/>
      <xsl:text>return valueList;{$NL}</xsl:text>
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
      <xsl:text>) {{ {$NL}</xsl:text>
      <xsl:for-each select="members/member[@nodeType='ConstructorDeclaration']/parameters/parameter">
         <xsl:call-template name="indent-plus-two"/>
         <xsl:text>this.{name/@identifier} = {name/@identifier};{$NL}</xsl:text>
      </xsl:for-each>
      
      <xsl:call-template name="indent-plus-two"/>
      <xsl:text>nameValue = name;{$NL}</xsl:text> 
      <xsl:call-template name="indent-plus-two"/>
      <xsl:text>ordinalValue = nextOrdinal++;{$NL}</xsl:text>     
      <xsl:call-template name="indent-plus-two"/>
      <xsl:text>innerEnumValue = innerEnum;{$NL}</xsl:text>      
      <xsl:apply-templates select="members/member[@nodeType='ConstructorDeclaration']/body">
         <xsl:with-param name="indent" select="$indent+4"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent-extra"/>
      <xsl:text>}} {$NL}</xsl:text>
      
      <xsl:for-each select="members/member[@nodeType='ConstructorDeclaration']/parameters/parameter
            [not(name/@identifier = $enumDecl/members/member[@nodeType='FieldDeclaration']/variables/variable/name/@identifier)]">
         <xsl:call-template name="indent-extra"/>
         <xsl:text>private </xsl:text>
         <xsl:apply-templates select="type"/>
         <xsl:text> </xsl:text>
         <xsl:apply-templates select="name"/>
         <xsl:text>;{$NL}</xsl:text>
      </xsl:for-each>
      
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
   
   <xsl:template match="*[@nodeType='ExpressionStmt'][expression[@nodeType='MethodCallExpr'][@RESOLVED_TYPE='net.sf.saxon.transpile.CSharp']]" 
      priority="20">
      <!-- No semicolon for CSharp.emitCode() -->
      <xsl:call-template name="indent"/>
      <xsl:apply-templates select="*"/>
      <xsl:text>{$NL}</xsl:text>
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
      <!--<xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent">
         <xsl:with-param name="indent" select="$indent - 1" tunnel="yes"/>
      </xsl:call-template>
      <xsl:text>{label/@identifier}:{$NL}</xsl:text>-->
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
      <xsl:variable name="firstLabel" select="entries/entrie[1]/labels/label[1]"/>
      <xsl:variable name="enumType" as="element(*)?"
            select="(key('digestClassKey', $selectorElem/@RESOLVED_TYPE, $digest)[self::enum],
                 key('digestClassKey', $firstLabel/@RESOLVED_TYPE, $digest)[self::enum]) [1]"/>
      <xsl:variable name="isSimpleEnum" as="xs:boolean" select="$enumType/@simple='1'"/>
      <xsl:call-template name="indent"/>
      <xsl:text>switch (</xsl:text>
      <xsl:apply-templates select="selector"/>
      <xsl:if test="$enumType and not($isSimpleEnum)">.innerEnumValue</xsl:if>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:for-each select="entries/entrie">
         
         <xsl:choose>
            <xsl:when test="labels/label">
               <xsl:for-each select="labels/label">
                  <xsl:call-template name="indent"/>
                  <xsl:text>  case </xsl:text>
                  <xsl:choose>
                     <xsl:when test="$selectorElem/@RESOLVED_TYPE = 'net.sf.saxon.s9api.HostLanguage'">Saxon.Api.HostLanguage.{name/@identifier}</xsl:when>
                     <xsl:when test="$selectorElem/@RESOLVED_TYPE = 'net.sf.saxon.s9api.UnprefixedElementMatchingPolicy'">Saxon.Api.UnprefixedElementMatchingPolicy.{
                        if (name/@identifier = 'DEFAULT_NAMESPACE') then 'DefaultNamespace' 
                        else if (name/@identifier = 'ANY_NAMESPACE') then 'AnyNamespace' 
                        else if (name/@identifier = 'DEFAULT_NAMESPACE_OR_NONE') then 'DefaultNamespaceOrNone' else '**'}</xsl:when>                   
                     <xsl:when test="$enumType and exists($firstLabel/@RESOLVED_TYPE)">{f:renameType($firstLabel/@RESOLVED_TYPE)}.{if (not($isSimpleEnum)) then 'InnerEnum.' else ''}{name/@identifier}</xsl:when>
                     <xsl:otherwise>
                        <xsl:apply-templates select="."/>
                     </xsl:otherwise>
                  </xsl:choose>
                  <xsl:text>:{$NL}</xsl:text>
               </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
               <xsl:call-template name="indent"/>
               <xsl:text>  default:{$NL}</xsl:text>
            </xsl:otherwise>
         </xsl:choose>
         <xsl:if test="position() = last() and empty(statements/statement)">
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
      <xsl:text>throw</xsl:text>
      <xsl:if test="not(ancestor::catchClause) 
         or ancestor::*[@nodeType='LambdaExpr']/ancestor::catchClause 
         or not(ancestor::catchClause[1]/parameter/name/@identifier = expression/name/@identifier)">
         <!-- Use C# re-throw where possible to preserve stack trace -->
         <xsl:text> </xsl:text>
         <xsl:apply-templates select="*"> 
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
         </xsl:apply-templates>
      </xsl:if>
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
               <xsl:apply-templates select="parameter/type" mode="exception-class"/>
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
   
   <xsl:template match="type" mode="exception-class">
      <xsl:apply-templates select="."/>
   </xsl:template>
   
 <!--  
   <parameter nodeType="Parameter" isVarArgs="false">
      <name nodeType="SimpleName" identifier="e"/>
      <type nodeType="ClassOrInterfaceType"
         RESOLVED_TYPE="java.io.UnsupportedEncodingException">
         <name nodeType="SimpleName"
            identifier="UnsupportedEncodingException"/>
      </type>
   </parameter>
   -->
   
   <!-- Replace exception name based on CSharpReplaceException annotation on containing method -->
   <xsl:template match="type[ancestor::member[1]/annotations/annotation[name/@identifier='CSharpReplaceException']]"
      mode="exception-class">
      <xsl:variable name="class" select="@RESOLVED_TYPE"/>     
      <xsl:variable name="annotations" select="ancestor::member[1]/annotations/annotation[name/@identifier='CSharpReplaceException']"/>
      <xsl:variable name="matching-annotation" select="$annotations[pairs/pair[name/@identifier='from'][value/@value=$class]]"/>
      <xsl:choose>
         <xsl:when test="$matching-annotation">
            <xsl:text>/* substitute exception */ </xsl:text>
            <xsl:value-of select="$matching-annotation/pairs/pair[name/@identifier='to']/value/@value"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>/* no matching annotation */ </xsl:text>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
      
   </xsl:template>
   <!--<xsl:template match="type[@RESOLVED_TYPE = ancestor::member[1]/annotations/annotation
               [name/@identifier='CSharpReplaceException']
               [pairs/pair[name/@identifier='from']]/value/@value]" 
               mode="exception-class">
      <xsl:value-of select="ancestor::member[1]/annotations/annotation
         [name/@identifier='CSharpReplaceException']
         /pairs[pair[name/@identifier='from'][value/@value=current()/@RESOLVED_TYPE]]
         /pair[name/@identifier='from']/value/@value"/>
   </xsl:template>-->
   
   <!--<annotation nodeType="NormalAnnotationExpr">
      <name nodeType="Name" identifier="CSharpReplaceException"/>
      <pairs>
         <pair nodeType="MemberValuePair">
            <name nodeType="SimpleName" identifier="from"/>
            <value nodeType="StringLiteralExpr"
               value="java.io.UnsupportedEncodingException"/>
         </pair>
         <pair nodeType="MemberValuePair">
            <name nodeType="SimpleName" identifier="to"/>
            <value nodeType="StringLiteralExpr"
               value="System.ArgumentException"/>
         </pair>
      </pairs>
   </annotation>-->
   
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
      <xsl:text>Saxon.Impl.Helpers.Utils.unsignedRightShift(</xsl:text>
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
      <xsl:param name="expandingDefaults" as="xs:boolean" select="false()" tunnel="true"/>
      <xsl:param name="classIsFinal" as="xs:boolean" select="false()" tunnel="true"/>
      <xsl:choose>
         <xsl:when test="empty(modifiers) and not(../..[@isInterface='true'])">
            <xsl:text>internal </xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="modifiers/modifier"/>
         </xsl:otherwise>
      </xsl:choose>

      <xsl:choose>
         <xsl:when test="$digestMethod!(@override='1' 
            or (@name='equals' and @sig='java.lang.Object')
            or (@name='hashCode' and not(@sig))
            or (@name='toString' and not(@sig)))">override </xsl:when>
         <xsl:when test="($digestMethod[@virtual='1' and not(@abstract='1')] or $expandingDefaults)
            and not($classIsFinal)">virtual </xsl:when>
         <xsl:when test="($digestMethod[@new='1'])">new </xsl:when>
      </xsl:choose>
      
   </xsl:template>

   
   <xsl:template match="modifier">
      <xsl:apply-templates select="@keyword"/>
   </xsl:template>
   
   <xsl:template match="modifier/@keyword" priority="3">
      <xsl:text>{lower-case(.)} </xsl:text>
   </xsl:template>
   
   <xsl:template match="modifier/@keyword[. = 'PROTECTED']" priority="10">
      <xsl:text>protected internal </xsl:text>
   </xsl:template>
   
   <xsl:template match="modifier/@keyword[. = 'PRIVATE'][ancestor::member[1][@isInterface='true']]" priority="15">
      <xsl:param name="include-nested-interfaces" as="xs:boolean" tunnel="yes" select="false()"/>
      <xsl:choose>
         <xsl:when test="$include-nested-interfaces">public </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
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
  
   
  
  
   <xsl:template match="*[@nodeType=('MethodDeclaration')]/modifiers/modifier[@keyword = 'FINAL']" priority="20"/>


   
   <xsl:template match="typeParameter">
      <xsl:apply-templates select="name"/>
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
   
   <xsl:template match="*[@nodeType = 'ClassOrInterfaceType'][starts-with(@RESOLVED_TYPE, 'java.util.EnumSet')]" priority="30">
      <!-- In C#, if an enum class is declared with [Flags], then a set of values is also an instance of the enum type -->
      <xsl:text>{f:extract-type-arguments(@RESOLVED_TYPE)}</xsl:text>
   </xsl:template>
   
   <!--<xsl:template match="*[@nodeType = 'ClassOrInterfaceType'][starts-with(@RESOLVED_TYPE, 'java.util.Optional')]" priority="30">
      <!-\- Optional<X> translates to `X?` -\->
      <!-\- If used in a module, the module must be annotated with @CSharpNullable("enable") -\->
      <xsl:text>{f:extract-type-arguments(@RESOLVED_TYPE)}?</xsl:text>
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
      <xsl:param name="suppress-field-initialization" select="false()" tunnel="yes"/>
      <xsl:apply-templates select="type"/>
      <xsl:text> </xsl:text>
      <xsl:apply-templates select="name"/>
      <xsl:if test="initializer and not($suppress-field-initialization)">
         <xsl:text> = </xsl:text>
         <xsl:apply-templates select="initializer"/>
      </xsl:if>
   </xsl:template>
   
   <xsl:template match="variable" mode="no-decl">
      <xsl:param name="suppress-field-initialization" select="false()" tunnel="yes"/>
      <xsl:text>, </xsl:text>
      <xsl:apply-templates select="name"/>
      <xsl:if test="initializer and not($suppress-field-initialization)">
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
      <!-- Delegate to another mode/module -->
      <xsl:apply-templates select="." mode="constructors"/>
   </xsl:template>
   
   
   <!-- Handle an inline anonymous class, typically `new SomeInterface(x, y, z) { class body }` -->
   <!-- We generate a named static inner class (in a second pass), and at this point we
        generate a call on its constructor, with an extra argument `this` -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr'][anonymousClassBody]" priority="20">
      <xsl:variable name="directive" select="ancestor::*[@nodeType='MethodDeclaration'][1]/annotations/annotation[name/@identifier='CSharpInnerClass']"
         as="element(*)?"/>
      <!--<xsl:message>Inner class with annotation {serialize($directive)}</xsl:message>-->
      <xsl:text>new {f:allocateInnerClassName(.)}(</xsl:text>
      <xsl:variable name="includeThis" 
         select="not($directive/pairs/pair[name/@identifier='outer']/value/@value='false')
                 and not(ancestor::member[1]/modifiers/modifier[@keyword='STATIC'])"/>
      <xsl:variable name="constructedArguments" as="xs:string*">
         <xsl:if test="$includeThis">
            <xsl:sequence select="'this'"/>
         </xsl:if>
         <xsl:variable name="explicitArguments">
            <xsl:apply-templates select="arguments"/>
         </xsl:variable>
         <xsl:sequence select="string($explicitArguments)[normalize-space()]"/>
         <xsl:for-each select="$directive/pairs/pair[name/@identifier='extra']//@value">
            <xsl:sequence select="substring-after(., ' ')"/>
         </xsl:for-each> 
      </xsl:variable>
      <xsl:value-of select="$constructedArguments" separator=", "/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
  
   <!-- For an anonymous inline inner class, generate a named inner class; in C# this is effectively static,
        so we pass a reference to the outer class as an extra argument in the constructor. The tricky part
        is binding references to methods and fields of the outer class to be scoped on this variable -->
   <xsl:template match="anonymousClassBody">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:variable name="allocatedName" as="xs:string" select="f:allocateInnerClassName(..)"/>
      <xsl:text>{$NL}</xsl:text>
      <xsl:call-template name="indent"/>
      <xsl:text>private class {$allocatedName} : </xsl:text>
      
      <xsl:variable name="superclass" select="f:resolveTypeName(../type)" as="xs:string"/>
      <xsl:variable name="isIterator" select="f:isIteratorType($superclass)" as="xs:boolean"/>
      <xsl:variable name="isComparator" select="f:isComparatorType($superclass)" as="xs:boolean"/>
      
      <!-- The name of the implemented class/interface -->
      
      <xsl:choose>
         <xsl:when test="$isIterator">
            <xsl:text>System.Collections.Generic.IEnumerator&lt;{f:extract-type-arguments($superclass)}&gt;</xsl:text>
         </xsl:when>
         <xsl:when test="$isComparator">
            <xsl:text>System.Collections.Generic.Comparer&lt;{f:extract-type-arguments($superclass)}&gt;</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>{f:escapeDottedName($superclass) => f:adjustTypeNamespace()}</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
      
      <xsl:text> {{{$NL}</xsl:text>
      
      <!-- Possible declaration of variable holding reference to outer class -->
      
      <xsl:variable name="containingMethod" select="ancestor::*[@nodeType='MethodDeclaration'][1]" as="element(*)?"/>
      <xsl:variable name="directive" select="$containingMethod/annotations/annotation[name/@identifier='CSharpInnerClass']"
         as="element(annotation)?"/>
      <xsl:variable name="includeThis" 
         select="not($directive/pairs/pair[name/@identifier='outer']/value/@value='false')
         and not(ancestor::member[1]/modifiers/modifier[@keyword='STATIC'])"/>
      <xsl:variable name="containingClassDecl" select="ancestor::*[@nodeType='ClassOrInterfaceDeclaration'][1]"/>
      <xsl:variable name="containingClass" select="$containingClassDecl/name/@identifier" as="xs:string"/>
      <xsl:variable name="containingClassParams" as="xs:string?">
         <xsl:if test="$containingClassDecl/typeParameters/typeParameter">
            <xsl:text>&lt;{string-join($containingClassDecl/typeParameters/typeParameter/name/@identifier, ',')}&gt;</xsl:text>
         </xsl:if>
      </xsl:variable>
      <xsl:if test="$includeThis">
         <xsl:call-template name="indent-extra"/>
         <xsl:text>private readonly {$containingClass}{$containingClassParams} outerInstance;{$NL}</xsl:text>
      </xsl:if>
      <xsl:call-template name="indent-extra"/>
      
      <!-- Constructor -->
      
      <xsl:text>public {$allocatedName}(</xsl:text>
      <xsl:variable name="extraParams" select="$directive/pairs/pair[name/@identifier='extra']//value/@value"/>
      <xsl:variable name="constructedParams" as="xs:string*">
         <xsl:if test="$includeThis">
            <xsl:text>{$containingClass}{$containingClassParams} outerInstance</xsl:text>
         </xsl:if>
         <xsl:for-each select="../arguments/argument">
            <xsl:variable name="declaredType" as="text()">
               <xsl:choose>
                  <xsl:when test="@RESOLVED_TYPE">{f:escapeDottedName(@RESOLVED_TYPE) => f:adjustTypeNamespace()}</xsl:when>
                  <xsl:when test="@RETURN">{f:escapeDottedName(@RETURN) => f:adjustTypeNamespace()}</xsl:when>
                  <xsl:when test="@nodeType='ThisExpr'">{$containingClass}{$containingClassParams}</xsl:when>
                  <xsl:otherwise>/* UNKNOWN ARG TYPE */ object</xsl:otherwise>
               </xsl:choose>
            </xsl:variable>
            <xsl:sequence select="$declaredType || ' arg' || position()"/>
         </xsl:for-each>
         
         <xsl:sequence select="$extraParams ! string()"/>     
         
      </xsl:variable>
      <xsl:value-of select="$constructedParams" separator=", "/>
      <xsl:text>)</xsl:text>
      
      <!-- Superclass initialization -->
      
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
      
      <!-- Initialize reference to outer class (must be done within the constructor -->
      
      <xsl:if test="$includeThis">
         <xsl:call-template name="indent-plus-two"/>
         <xsl:text>this.outerInstance = outerInstance;{$NL}</xsl:text>
      </xsl:if>
      
      <!-- for some reason the initialisation of fields needs to be brought inside the constructor -->
      
      <xsl:for-each select="$extraParams">
         <xsl:call-template name="indent-plus-two"/>
         <xsl:text>this.{substring-after(., ' ')} = {substring-after(., ' ')};{$NL}</xsl:text>
      </xsl:for-each>
      
      <xsl:for-each select="*[@nodeType='FieldDeclaration']/variables/variable[initializer]">
         <xsl:call-template name="indent-plus-two"/>
         <xsl:text>{name/@identifier} = </xsl:text>
         <xsl:apply-templates select="initializer"/>
         <xsl:text>;{$NL}</xsl:text>
      </xsl:for-each>
      
      <xsl:call-template name="indent-extra"/>
      
      <xsl:text>}}{$NL}</xsl:text>
      
      <xsl:for-each select="$extraParams">
         <xsl:call-template name="indent-extra"/>
         <xsl:text>private {.};{$NL}</xsl:text>
      </xsl:for-each>
      
      <xsl:apply-templates select="*">
         <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
         <xsl:with-param name="suppress-field-initialization" select="true()" tunnel="yes"/>
      </xsl:apply-templates>
      
      <xsl:call-template name="indent"/>
      
      <!-- If the class is an iterator, add methods to implement IEnumerator -->
      
      <xsl:if test="$isIterator">
         <xsl:call-template name="addEnumeratorMethods">
            <xsl:with-param name="deliveredType" select="f:extract-type-arguments($superclass)"/>
            <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
         </xsl:call-template>
      </xsl:if>
      
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <!-- Try to resolve an unscoped name appearing within an anonymous class body -->
   <!-- Note: we don't try to handle anonymous classes nested within anonymous classes -->
   <xsl:template match="anonymousClassBody//name[@identifier][..[not(scope or self::type or self::variable 
         or self::annotation or self::parameter or @nodeType=('MethodDeclaration', 'FieldDeclaration'))]]" priority="40">
      <xsl:variable name="identifier" select="@identifier" as="xs:string"/>
      <xsl:variable name="isMethodCall" select="../@nodeType='MethodCallExpr'"/>
      <xsl:variable name="arity" select="count(../arguments/argument)" as="xs:integer"/>
      <xsl:variable name="containingClass" select="ancestor::anonymousClassBody[1]" as="element(anonymousClassBody)"/>
      <xsl:variable name="implementedClass" select="$containingClass/../type/string(@RESOLVED_TYPE)" as="xs:string"/>
      <xsl:variable name="implementedClassDigest" select="key('digestClassKey', $implementedClass, $digest)"/>
      <xsl:variable name="outerMethod" select="$containingClass/ancestor::*[@nodeType='MethodDeclaration'][1]"/>
      <xsl:variable name="extraParams" select="$outerMethod/annotations/annotation[name/@identifier='CSharpInnerClass']/
                     pairs/pair[name/@identifier='extra']//value/@value" as="xs:string*"/>
      <xsl:variable name="outerClass" select="$containingClass/ancestor::*[@nodeType='ClassOrInterfaceDeclaration'][1]"/>
      <xsl:choose>
         <xsl:when test="ancestor::catchClause//parameter[name/@identifier = $identifier]">
            <!-- The name matches an exception variable in a catch clause -->
            <xsl:text>/*exception*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="ancestor::anonymousClassBod[1]//variable[name/@identifier = $identifier]">
            <!-- The name matches a local variable within the same method -->
            <xsl:text>/*localvar*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="ancestor::anonymousClassBod[1]/parameters/parameter[name/@identifier = $identifier]">
            <!-- The name matches a parameter of the containing method declaration -->
            <xsl:text>/*localparam*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$containingClass/*[@nodeType='FieldDeclaration']/variables/variable/name[@identifier=$identifier]">
            <!-- The name matches a field of the containing anonymous class -->
            <xsl:text>/*field*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$isMethodCall and $containingClass/*[@nodeType='MethodDeclaration'][name/@identifier=$identifier][count(parameters/parameter) = $arity]">
            <!-- The name matches a method of the containing anonymous class -->
            <xsl:text>/*method*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$implementedClassDigest/field[@name=$identifier]">
            <!-- The name matches a field of the interface implemented by the anonymous class -->
            <xsl:text>/*superField*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$isMethodCall and f:hasInheritedMethod($implementedClassDigest, $identifier, $arity)">
            <!-- The name matches a method of the interface implemented by the anonymous class -->
            <xsl:text>/*superMethod*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$extraParams[substring-after(., ' ') = $identifier]">
            <xsl:text>/*extraParam*/{$identifier}</xsl:text>
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
            <xsl:text>/*outerStaticMethod*/{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="$outerClass/members/member[@nodeType='MethodDeclaration']/name[@identifier=$identifier]">
            <!-- The name matches an instance method of the containing anonymous class -->
            <xsl:text>outerInstance.{$identifier}</xsl:text>
         </xsl:when>
         <xsl:when test="matches($identifier, '^[A-Z]$')">
            <!-- Single-character name A-Z - assume a type parameter -->
            <xsl:text>{$identifier}</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <!-- Name not found, but assume it's inherited from a superclass of the outer class? -->
            <xsl:text>/*guess*/outerInstance.{$identifier}</xsl:text>
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
   
   <xsl:template match="*[@nodeType='LongLiteralExpr'][matches(@value, '^0x[89a-fA-F]')]" priority="20">
      <xsl:text>unchecked((long){@value})</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='IntegerLiteralExpr'][matches(@value, '^0x[89a-fA-F]')]" priority="20">
      <xsl:text>unchecked((int){@value})</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='InstanceOfExpr']">
      <!-- Translating "A instanceof B" to "A is B" is OK as a general strategy, but it fails on generics -->
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="expression"/>
      <xsl:text> is </xsl:text>
      <xsl:apply-templates select="type"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Convert a method reference (MyClass::toExportString).
        The problem here is distinguishing static method references (Math::sin becomes Math.sin)
        from instance-level method references (AtomicType::isIdRef becomes (a => a.isIdRef()).
        The convention we adopt is that static method references are recognised only if explicitly
        listed below -->
   
   <xsl:variable name="staticMethodReferences">
      <class name="java.lang.Math">
         <method name="sin"/>
         <method name="cos"/>
         <method name="tan"/>
         <method name="asin"/>
         <method name="acos"/>
         <method name="atan"/>
         <method name="sqrt"/>
         <method name="log"/>
         <method name="log10"/>
         <method name="exp"/>
         <method name="exp10"/>
         <method name="sinh"/>
         <method name="tanh"/>
         <method name="cosh"/>
      </class>
      <class name="net.sf.saxon.value.EmptySequence">
         <method name="getInstance"/>
      </class>
   </xsl:variable>
   
   <xsl:template match="*[@nodeType='MethodReferenceExpr'][scope/type/@RESOLVED_TYPE]" priority="15">
      <xsl:choose>
         <xsl:when test="some $c in $staticMethodReferences/class
                         satisfies ($c/@name = scope/type/@RESOLVED_TYPE
                               and $c/method/@name = @identifier)">
            <xsl:apply-templates select="scope"/>
            <xsl:text>.{f:renameMethod(@identifier, .)} /* known method reference */</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>x => x.{f:renameMethod(@identifier, .)}() /* method reference */</xsl:text>
         </xsl:otherwise>
      </xsl:choose>      
   </xsl:template>
   
   <!-- Try converting a non static method reference (out::append) to (out.append) -->
   <xsl:template match="*[@nodeType='MethodCallExpr'][@RESOLVED_TYPE='net.sf.saxon.transpile.CSharp'][name/@identifier='methodRef']
      /arguments/argument[@nodeType='MethodReferenceExpr']" priority="16">
      <xsl:apply-templates select="scope"/>
      <xsl:text>.{f:renameMethod(@identifier, .)} /* instance method reference */</xsl:text>
   </xsl:template>
   
   <!-- Try converting a static method reference (Thing::append) to (Thing.append) -->
   <xsl:template match="*[@nodeType='MethodCallExpr'][@RESOLVED_TYPE='net.sf.saxon.transpile.CSharp'][name/@identifier='staticRef']
      /arguments/argument[@nodeType='MethodReferenceExpr']" priority="16">
      <xsl:apply-templates select="scope"/>
      <xsl:text>.{f:renameMethod(@identifier, .)} /* static method reference */</xsl:text>
   </xsl:template>
   
   <!-- Try converting a constructor reference (Thing::new) to ((a, b, c) => new Thing(a, b, c)) -->
   <xsl:template match="*[@nodeType='MethodCallExpr'][@RESOLVED_TYPE='net.sf.saxon.transpile.CSharp'][name/@identifier='constructorRef']
      /arguments/argument[@nodeType='MethodReferenceExpr']" priority="25">
      <xsl:variable name="arity" select="xs:integer(../../arguments/argument[2]/@value)"/>
      <xsl:text>/* ctor#{$arity} */ ({string-join((1 to $arity)!format-integer(., 'a'), ', ')}) => new </xsl:text>
      <xsl:apply-templates select="scope"/>
      <xsl:text>({string-join((1 to $arity)!format-integer(., 'a'), ', ')})</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodReferenceExpr']" priority="10">
      <xsl:text>/* Unresolved method reference */</xsl:text>
      <xsl:apply-templates select="scope"/>
      <xsl:text>.{@identifier}</xsl:text>
   </xsl:template>
   
   
   <xsl:template match="*[@nodeType='MethodReferenceExpr'][@identifier='new']" priority="20">
      <!-- Used for classname::new - always invokes zero-argument constructor -->
      <xsl:text>() => new </xsl:text>
      <xsl:apply-templates select="scope"/>
      <xsl:text>()</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodReferenceExpr'][@identifier='isInstance']" priority="20">
      <xsl:text>x => (x is {scope/type/@RESOLVED_TYPE => f:adjustTypeNamespace()})</xsl:text>
   </xsl:template>
   
   <!-- String and character literals -->
   
   <xsl:template match="*[@nodeType='StringLiteralExpr']">
      <xsl:text>"{@value}"</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='CharLiteralExpr']">
      <xsl:text>'{@value}'</xsl:text>
   </xsl:template>
   
   <!-- Character literals appearing within a byte array initializer -->
   <xsl:template match="*[elementType/@type='BYTE']/initializer/values/value[@nodeType='CharLiteralExpr']" priority="10">
      <xsl:text>{string-to-codepoints(@value)}/*{@value}*/</xsl:text>
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
      <!-- Extra parens are needed when the child expression is a lambda expression. 
           Add them anyway, it does no harm -->
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="expression"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- match enumeration constants and qualify them with the enumeration class name -->
   <xsl:template match="*[@nodeType='NameExpr'][@ENUM_TYPE][matches(name/@identifier, '^[A-Z0-9_]+$')]" priority="20">
      <xsl:text>/*explicit enum*/ {f:escapeDottedName(@ENUM_TYPE) => f:adjustTypeNamespace()}.{name/@identifier}</xsl:text>
   </xsl:template>

   <!-- some enumeration constants aren't marked with @ENUM_TYPE and qualify them with the enumeration class name -->
   <xsl:template match="*[not(self::selector)]
                         [@nodeType='NameExpr']
                         [matches(name/@identifier, '^[A-Z0-9_]+$')]
                         [key('digestClassKey', f:expressionType(current()), $digest)/self::enum]" priority="18">
      <xsl:text>/*implicit enum*/ {f:escapeDottedName(f:renameType(f:expressionType(.)))}.{name/@identifier}</xsl:text>
   </xsl:template>
   
   
   <xsl:template match="*[@nodeType='NameExpr']">
      <xsl:apply-templates select="name"/>
   </xsl:template>
 
   <!-- Where an interface defines defaulted implementations of methods, copy this methods into all (immediately)
        implementing classes that do not define their own override of the method. In addition, C# requires that
        an abstract class that implements an interface contains a concrete implementation of every method defined
        in that interface, so we generate stub implementations that exist only to be overridden -->
   <xsl:template name="expand-defaulted-methods">
      <xsl:param name="indent" as="xs:integer" tunnel="yes" select="0"/>
      <xsl:assert test="@nodeType='ClassOrInterfaceDeclaration'"/>
      <xsl:variable name="thisClass" select="."/>
      <xsl:variable name="thisClassName" as="xs:string" select="f:fullClassName(.)"/>
      <!--<xsl:variable name="thisClassName" as="xs:string" 
         select="string-join((f:qualifiedName(ancestor::root/packageDeclaration/name), ancestor-or-self::*[@nodeType='ClassOrInterfaceDeclaration']/name/@identifier), '.')"/>
      -->
      <!--<xsl:message>Expanding default methods for {$thisClassName}</xsl:message>-->
      <xsl:variable name="thisDigest" select="key('digestClassKey', $thisClassName => translate('@', ''), $digest)" as="element(*)?"/>
      <xsl:choose>
         <xsl:when test="empty($thisDigest)">
            <!-- This is happening for methods in inner classes. 
                 May affect whether they are declared as "override", but mostly harmless? -->
            <!--<xsl:message>No digest found for {$thisClassName}</xsl:message>-->
         </xsl:when>
         <xsl:otherwise>
            <xsl:variable name="defaults" select="f:gatherDefaultMethods($thisDigest)|()" as="element(*)*"/>
            <!--<xsl:message>Found {count($defaults)} default methods {string-join($defaults/(@name || '-' || generate-id()), ';')}</xsl:message>-->
            <xsl:for-each-group select="$defaults" group-by="@name, string(@sig)" composite="yes">
               <!--<xsl:message>Considering default for {@name}#{@sig}</xsl:message>-->
               <!--<xsl:message>Has concrete method? {f:has-concrete-method($thisDigest, current()/@name, xs:integer(current()/@arity))}</xsl:message>
               <xsl:message>Superclass implements? {$thisDigest/extends ! key('digestClassKey', @name, $digest) ! f:getImplementedInterfaces(.) ! @name}</xsl:message>-->
               <xsl:if test="not(f:hasMethod($thisDigest, current()/@name, string(current()/@sig)))
                  and (empty(($thisDigest/extends ! key('digestClassKey', @name, $digest) ! f:getImplementedInterfaces(.)) intersect current()/..))">
                  <xsl:text>{$NL}</xsl:text>
                  <xsl:call-template name="indent-extra"/>
                  <xsl:text>/* Expanding default method {@name} from interface {current-group()[last()]/../@name} */{$NL}</xsl:text>
                  <xsl:apply-templates select="current-group()[last()]/default/*">
                     <xsl:with-param name="expandingDefaults" as="xs:boolean" select="true()" tunnel="yes"/>
                     <xsl:with-param name="classIsFinal" as="xs:boolean" select="$thisClass/modifiers/modifier/@keyword='FINAL'" tunnel="yes"/>
                     <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
                  </xsl:apply-templates>
               </xsl:if>        
            </xsl:for-each-group>
            
            <xsl:if test="$thisDigest/@abstract='1'">
               <!--<xsl:message>Making abstract methods concrete: implements {f:getImplementedInterfaces($thisDigest)/@name}</xsl:message>
               <xsl:message>Defaults: {$defaults/(@name || '=' || @sig)}</xsl:message>-->
               <xsl:variable name="implementees" as="element(method)*">
                  <xsl:for-each select="f:getImplementedInterfaces($thisDigest)/method[not(./default or @static='1')]">
                     <!--<xsl:message>Examining {./@name}</xsl:message>-->
                     <xsl:if test="not(f:hasMethod($thisDigest, current()/@name, string(current()/@sig)))
                        and not(f:hasInheritedConcreteMethod($thisDigest, current()/@name, string(current()/@sig)))
                        and empty(($thisDigest/extends ! key('digestClassKey', @name, $digest) ! f:getImplementedInterfaces(.)) intersect current()/..)
                        and empty($defaults[@name=current()/@name and string(@sig)=string(current()/@sig)])">
                        <!--<xsl:message>Expanding {./@name} on {../@name} with parameters {@sig}</xsl:message>-->
                        <xsl:sequence select="."/>
                     </xsl:if>
                  </xsl:for-each>
               </xsl:variable>
               <xsl:for-each-group select="$implementees" group-by="@name, @sig" composite="yes">
                  <xsl:text>{$NL}</xsl:text>
                  <xsl:call-template name="indent-extra"/>
                  <xsl:text>/* Creating stub method {@name} to satisfy interface {../@name} */{$NL}</xsl:text>
                  <xsl:if test="count(current-group()) gt 1">
                     <xsl:call-template name="indent-extra"/>
                     <xsl:text>/* (Also inherited from {tail(current-group())/../@name}) */{$NL}</xsl:text>   
                  </xsl:if>
                  <xsl:call-template name="indent-extra"/>
                  <xsl:text>public virtual {f:renameGenericType(@returns)} {@name}(</xsl:text>
                  <xsl:for-each select="tokenize(@params,'\|')">
                     <xsl:if test="position() gt 1">, </xsl:if>
                     <xsl:text>{f:renameGenericType(.)} arg{position()}</xsl:text>
                  </xsl:for-each>
                  <xsl:text>) {{{$NL}</xsl:text>
                  <xsl:call-template name="indent-plus-two"/>
                  <xsl:text>throw new System.NotImplementedException();{$NL}</xsl:text>
                  <xsl:call-template name="indent-extra"/>
                  <xsl:text>}}{$NL}</xsl:text>
               </xsl:for-each-group>
            </xsl:if>
         </xsl:otherwise>
      </xsl:choose>
      
      <xsl:variable name="iteratesOver" select="$thisDigest ! f:typeThatThisIteratesOver(.)"/>
      <xsl:if test="$iteratesOver">
         <xsl:call-template name="indent"/>
         <xsl:text>{$NL}</xsl:text>
         <xsl:call-template name="indent"/>
         <xsl:text>/* Add methods to implement IEnumerable */{$NL}</xsl:text>
         <xsl:call-template name="addEnumeratorMethods">
            <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
            <xsl:with-param name="deliveredType" select="f:renameType($iteratesOver)"/>
         </xsl:call-template>
      </xsl:if>
   </xsl:template>
   
  
</xsl:stylesheet>