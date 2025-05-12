<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   version="3.0"
   expand-text="yes"
   default-mode="xml-to-java">
   
   <xsl:output method="text"/>
   <xsl:strip-space elements="*"/>
   <xsl:variable name="NL" select="'&#xa;'" as="xs:string"/>
   
   <xsl:mode on-no-match="fail"/>
   <!--<xsl:mode on-no-match="text-only-copy"/>-->
   <!--<xsl:template match="*">
      <xsl:text>*** {name()}/{@nodeType} ***</xsl:text>
   </xsl:template>-->
   
   <xsl:function name="f:qualifiedName" as="xs:string">
      <xsl:param name="in" as="element(*)"/>
      <xsl:sequence select="(if ($in/qualifier) then f:qualifiedName($in/qualifier) || '.' else '') || $in/@identifier"/>
   </xsl:function>
   
   <xsl:template name="indent">
      <xsl:param name="indent" as="xs:integer" tunnel="yes" select="0"/>
      <xsl:value-of select="string-join((1 to $indent*4)!' ', '')"/>
   </xsl:template>
   
   <xsl:template match="root">
      <xsl:apply-templates>
         <xsl:with-param name="indent" select="0" tunnel="yes"/>
      </xsl:apply-templates>
   </xsl:template>
   
   <xsl:template match="packageDeclaration">
      <xsl:text>package {f:qualifiedName(name)};{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="import">
      <xsl:text>import {if (@isStatic='true') then 'static ' else ''}{f:qualifiedName(name)}{if (@isAsterisk='true') then '.*' else ''};{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="name | *[@nodeType='Name']">
      <xsl:if test="../scope">
         <xsl:apply-templates select="../scope"/>
         <xsl:text>.</xsl:text>
      </xsl:if>
      <xsl:value-of select="f:qualifiedName(.)"/>
   </xsl:template>
   
   <xsl:template match="name | *[@nodeType='Name']" mode="sans-scope">
      <xsl:value-of select="f:qualifiedName(.)"/>
   </xsl:template>
   
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
      <xsl:text>super</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'ClassOrInterfaceType']" priority="20">
      <xsl:apply-templates select="name, typeArguments"/>
   </xsl:template>
   
   <xsl:template name="classOrInterface">
      <xsl:param name="indent" as="xs:integer" tunnel="yes"/>
      <xsl:text>{if (@isInterface='true') then 'interface' else 'class'} {name/@identifier}</xsl:text>
      <xsl:apply-templates select="typeParameters"/>
      <xsl:if test="extendedTypes/extendedType">
         <xsl:text> extends </xsl:text>
         <xsl:for-each select="extendedTypes/extendedType">
            <xsl:if test="position() ne 1">, </xsl:if>
            <xsl:apply-templates select="."/>
         </xsl:for-each>
      </xsl:if>
      <xsl:if test="implementedTypes/implementedType">
         <xsl:text> implements </xsl:text>
         <xsl:for-each select="implementedTypes/implementedType">
            <xsl:if test="position() ne 1">, </xsl:if>
            <xsl:apply-templates select="."/>
         </xsl:for-each>
      </xsl:if>
      <xsl:text> {{{$NL}</xsl:text>
      <xsl:apply-templates select="members/member">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ClassOrInterfaceDeclaration']">
      <xsl:call-template name="indent"/>
      <xsl:apply-templates select="modifiers/modifier"/>
      <xsl:call-template name="classOrInterface"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='FieldDeclaration']">
      <xsl:call-template name="indent"/>
      <xsl:apply-templates select="modifiers/modifier, variables"/>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ConstructorDeclaration']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:apply-templates select="modifiers/modifier"/>
      <xsl:text>{name/@identifier}(</xsl:text>
      <xsl:apply-templates select="parameters"/>
      <xsl:text>) </xsl:text>
      <xsl:apply-templates select="thrownExceptions"/>
      <xsl:text> {{{$NL}</xsl:text>
      <xsl:apply-templates select="body/statements">
         <xsl:with-param name="indent" select="$indent + 1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodDeclaration']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:apply-templates select="modifiers/modifier, typeParameters, type"/>
      <xsl:text> {name/@identifier}(</xsl:text>
      <xsl:apply-templates select="parameters"/>
      <xsl:text>) </xsl:text>
      <xsl:apply-templates select="thrownExceptions"/>
      <xsl:choose>
         <xsl:when test="body">
            <xsl:text>{{{$NL}</xsl:text>
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
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:apply-templates select="modifiers/modifier"/>
      <xsl:text>enum </xsl:text>
      <xsl:text> {name/@identifier} </xsl:text>
      <xsl:if test="extendedTypes/extendedType">
         <xsl:text> extends </xsl:text>
         <xsl:for-each select="extendedTypes/extendedType">
            <xsl:if test="position() ne 1">, </xsl:if>
            <xsl:apply-templates select="."/>
         </xsl:for-each>
      </xsl:if>
      <xsl:if test="implementedTypes/implementedType">
         <xsl:text> implements </xsl:text>
         <xsl:for-each select="implementedTypes/implementedType">
            <xsl:if test="position() ne 1">, </xsl:if>
            <xsl:apply-templates select="."/>
         </xsl:for-each>
      </xsl:if>
      <xsl:text> {{</xsl:text>
      <xsl:for-each select="entries/entrie">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="name"/>
         <xsl:if test="arguments">
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments"/>
            <xsl:text>)</xsl:text>
         </xsl:if>
      </xsl:for-each>
      <xsl:text>;</xsl:text>     
      <xsl:apply-templates select="members/member">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'InitializerDeclaration']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>{if (@isStatic = 'true') then 'static' else ''}</xsl:text>
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
      <xsl:text>throws </xsl:text>
      <xsl:for-each select="thrownException">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
      <xsl:text> </xsl:text>
   </xsl:template>
   
   <xsl:template match="parameters">
      <xsl:for-each select="parameter">
         <xsl:if test="position() ne 1">, </xsl:if>
         <xsl:apply-templates select="type"/>
         <xsl:if test="@isVarArgs='true'">...</xsl:if>
         <xsl:text> </xsl:text>
         <xsl:apply-templates select="name"/>
      </xsl:for-each>
   </xsl:template>
   
   <xsl:template match="statements">
      <xsl:apply-templates select="statement"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='AssertStmt']">
      <xsl:call-template name="indent"/>
      <xsl:text>assert </xsl:text>
      <xsl:apply-templates select="*"/>
      <xsl:text>;{$NL}</xsl:text>
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
      <xsl:text>for (</xsl:text>
      <xsl:apply-templates select="variable"/>
      <xsl:text> : </xsl:text>
      <xsl:apply-templates select="iterable"/>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:apply-templates select="body">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:call-template name="indent"/>
      <xsl:text>}}{$NL}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ExplicitConstructorInvocationStmt']">
      <xsl:call-template name="indent"/>
      <xsl:text>{if (@isThis = 'true') then 'this' else 'super'}</xsl:text>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
   
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
      <xsl:call-template name="indent"/>
      <xsl:text>switch (</xsl:text>
      <xsl:apply-templates select="selector"/>
      <xsl:text>) {{{$NL}</xsl:text>
      <xsl:for-each select="entries/entrie">
         <xsl:call-template name="indent"/>
         <xsl:choose>
            <xsl:when test="labels/label">
               <xsl:text>  case </xsl:text>
               <xsl:apply-templates select="labels/label"/>
               <xsl:text>:{$NL}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:text>  default:{$NL}</xsl:text>
            </xsl:otherwise>
         </xsl:choose>        
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
      <xsl:text>synchronized (</xsl:text>
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
      <xsl:call-template name="indent"/>
      <xsl:text>;{$NL}</xsl:text>
   </xsl:template>
 
   <xsl:template match="*[@nodeType='TryStmt']">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:call-template name="indent"/>
      <xsl:text>try </xsl:text>
      <xsl:if test="resources">
         <xsl:text>(</xsl:text>
         <xsl:apply-templates select="resources/*"/>
         <xsl:text>) </xsl:text>
      </xsl:if>
      <xsl:text>{{{$NL}</xsl:text>
      <xsl:apply-templates select="tryBlock/statements">
         <xsl:with-param name="indent" select="$indent+1" tunnel="yes"/>
      </xsl:apply-templates>
      <xsl:for-each select="catchClauses/catchClause">
         <xsl:call-template name="indent"/>
         <xsl:text>}} catch (</xsl:text>
         <xsl:apply-templates select="parameter/type"/>
         <xsl:text> </xsl:text>
         <xsl:apply-templates select="parameter/name"/>
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
   
   <xsl:template match="*[@nodeType='ConditionalExpr']">
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="condition"/>
      <xsl:text> ? </xsl:text>
      <xsl:apply-templates select="thenExpr"/>
      <xsl:text> : </xsl:text>
      <xsl:apply-templates select="elseExpr"/>
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
      <xsl:text> -&gt; </xsl:text>
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

   <xsl:template match="*[@nodeType='MethodCallExpr']" priority="10">
      <xsl:apply-templates select="name"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodCallExpr'][typeArguments]" priority="20">
      <!-- for example Collections.<AtomicValue>emptyList() -->
      <!-- this is rare and we special-case it; we're probably not handling the full generality -->
      <xsl:if test="scope">
         <xsl:apply-templates select="scope"/>
         <xsl:text>.</xsl:text>
      </xsl:if>
      <xsl:apply-templates select="typeArguments"/>
      <xsl:apply-templates select="name" mode="sans-scope"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="modifier">
      <xsl:text>{lower-case(@keyword)} </xsl:text>
   </xsl:template>

   
   <xsl:template match="type | typeArgument[name] | thrownException | componentType">
      <xsl:apply-templates select="name, typeArguments"/>
   </xsl:template>
   
   <xsl:template match="typeParameter">
      <xsl:apply-templates select="name"/>
      <xsl:if test="typeBound">
         <xsl:text> extends </xsl:text>
         <xsl:apply-templates select="typeBound/typeBoun"/>
      </xsl:if>
   </xsl:template>
   
   
   <xsl:template match="type[@nodeType = 'VoidType']">
      <xsl:text>void</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType = 'PrimitiveType']">
      <xsl:text>{lower-case(@type)}</xsl:text>
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
      <xsl:text>&lt;</xsl:text>
      <xsl:for-each select="typeArgument">
         <xsl:if test="position() ne 1">,</xsl:if>
         <xsl:apply-templates select="."/>
      </xsl:for-each>
      <xsl:text>&gt;</xsl:text>
   </xsl:template>
   
   <xsl:template match="typeArgument[extendedType]" priority="20">
      <xsl:text>? extends </xsl:text>
      <xsl:apply-templates select="extendedType"/>
   </xsl:template>
   
   <xsl:template match="typeArgument[superType]" priority="20">
      <xsl:text>? super </xsl:text>
      <xsl:apply-templates select="superType"/>
   </xsl:template>
   
   <xsl:template match="typeArgument[@nodeType = 'WildcardType']" priority="10">
      <xsl:text>?</xsl:text>
   </xsl:template>
   
   <xsl:template match="variables">
      <xsl:apply-templates select="head(variable)"/>
      <xsl:apply-templates select="tail(variable)" mode="no-decl"/>
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
      <xsl:text>new </xsl:text>
      <xsl:apply-templates select="type"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments"/>
      <xsl:text>)</xsl:text>
      <xsl:apply-templates select="anonymousClassBody"/>
   </xsl:template>
   
   <xsl:template match="anonymousClassBody">
      <xsl:param name="indent" tunnel="yes" as="xs:integer"/>
      <xsl:text> {{{$NL}</xsl:text>
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
      <xsl:text> instanceof </xsl:text>
      <xsl:apply-templates select="type"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='MethodReferenceExpr']">
      <xsl:apply-templates select="scope"/>
      <xsl:text>::{@identifier}</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='StringLiteralExpr']">
      <xsl:text>"{@value}"</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='CharLiteralExpr']">
      <xsl:text>'{@value}'</xsl:text>
   </xsl:template>

   <xsl:template match="*[@nodeType='ThisExpr']">
      <xsl:if test="typeName">
         <xsl:apply-templates select="typeName"/>
         <xsl:text>.</xsl:text>
      </xsl:if>
      <xsl:text>this</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='TypeExpr']">
      <xsl:apply-templates select="type"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='FieldAccessExpr']">
      <xsl:apply-templates select="scope"/>
      <xsl:text>.{name/@identifier}</xsl:text>
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
   
   <!-- AssignExpr is tricky because sometimes it needs parens e.g in `while ((a=iter.next())!=null)`, and
      sometimes parens are not allowed, e.g. in `for(int i=0; i<3; i+=2)` -->
   
   <!--<xsl:template match="*[@nodeType='AssignExpr']" priority="10">
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="target"/>
      <xsl:choose>
         <xsl:when test="@operator"> {$operators(@operator)}= </xsl:when>
         <xsl:otherwise> = </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates select="value"/>
      <xsl:text>)</xsl:text>
   </xsl:template>-->
   
   <xsl:template match="*[@nodeType='AssignExpr']" priority="20">
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
      <!--<xsl:text>{if (type/name) then f:qualifiedName(type/name) else "**unknown class name**"}.class</xsl:text>-->
      <xsl:apply-templates select="type"/>
      <xsl:text>.class</xsl:text>
      <!--<xsl:apply-templates select="value"/>-->
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
   
 
</xsl:stylesheet>