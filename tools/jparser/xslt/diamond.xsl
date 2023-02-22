<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   expand-text="yes"
   default-mode="diamond"
   version="3.0">
   
   <xsl:mode name="diamond" on-no-match="fail"/>
   
   <!-- This module is concerned with expanding the diamond operator found in Java constructs like
        new ArrayList<>(). When generating the C# equivalent code, we need to add explicit type
        arguments, for example new ArrayList<NodeInfo>(). This requires type inference based on the
        context in which the construct appears.
        
        All the template rules in this module handle nodes satisfying the pattern
        match="*[@nodeType='ObjectCreationExpr'][type/typeArguments[not(*)]"
        (that is, creation of a new object with an empty list of type arguments)
        and since this has already been matched, there is no need to repeat this
        part of the pattern. The templates use the default mode="diamond".
   -->
   
   <xsl:function name="f:extract-type-arguments" as="xs:string?">
      <xsl:param name="type-name" as="xs:string"/>
      <!-- If we start with "List<A, B<C, D>>", we want to extract "A, B<C, D>"-->
      <xsl:variable name="type-param" select="$type-name => replace('^[^&lt;]+&lt;', '') => replace('>$', '')"/>   
      <xsl:value-of>
         <xsl:analyze-string select="$type-param" regex="[a-zA-Z0-9\._]+">
            <xsl:matching-substring>
               <xsl:value-of select="f:rename-type(.)"/>
            </xsl:matching-substring>
            <xsl:non-matching-substring>
               <xsl:value-of select="."/>
            </xsl:non-matching-substring>
         </xsl:analyze-string>
      </xsl:value-of>
   </xsl:function>
   
   <!-- Assignment to "this.field" -->
   <!-- this.fieldname = new XXX<>() -->
   <!-- An example is found in the constructor of SmallAttributeMap -->
   <xsl:template match="*[@nodeType='AssignExpr'][target[@nodeType='FieldAccessExpr'][scope/@nodeType='ThisExpr']]/*">
      <xsl:variable name="fieldRef" select="../target"/>
      <xsl:variable name="fieldDecl" 
         select="ancestor::members/*[@nodeType='FieldDeclaration'][variables/variable/name/@identifier = $fieldRef/name/@identifier]"/>
      <xsl:variable name="type" select="$fieldDecl/variables/variable/type/@RESOLVED_TYPE"/>
      <xsl:value-of select="f:extract-type-arguments($type)"/>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='AssignExpr'][target[@nodeType='FieldAccessExpr'][scope/@nodeType='NameExpr']]/*">
      <xsl:variable name="fieldRef" select="../target"/>
      <xsl:message>Looking for class: {$fieldRef/@RESOLVED_TYPE}</xsl:message>
      <xsl:variable name="targetClassDigest" select="key('digestClassKey', $fieldRef/@RESOLVED_TYPE, $digest)"/>
      <xsl:message>Target class digest: {serialize($targetClassDigest)}</xsl:message>
      <xsl:variable name="fieldType" select="$targetClassDigest/field[@name=$fieldRef/name/@identifier]/@type"/>
      <xsl:value-of select="if ($fieldType) then f:extract-type-arguments($fieldType) else '/*UNIDENTIFIED FIELD*/'"/>
   </xsl:template>
   
  <!-- <statement nodeType="ExpressionStmt">
      <expression nodeType="AssignExpr"
         operator="ASSIGN">
         <target nodeType="FieldAccessExpr"
            RESOLVED_TYPE="net.sf.saxon.expr.flwor.WindowClause.Window">
            <name nodeType="SimpleName" identifier="contents"/>
            <scope nodeType="NameExpr"
               RESOLVED_TYPE="net.sf.saxon.expr.flwor.WindowClause.Window">
               <name nodeType="SimpleName" identifier="window"/>
            </scope>
         </target>
         <value nodeType="ObjectCreationExpr">
            <type nodeType="ClassOrInterfaceType"
               RESOLVED_TYPE="java.util.ArrayList">
               <name nodeType="SimpleName" identifier="ArrayList"/>
               <typeArguments/>
            </type>
         </value>
      </expression>
   </statement>
   
   -->
   <!-- constructor call on RHS of variable declaration -->
   <!-- XXX<T,U> var = new YYY<>() -->
   <!-- An example is in SingletonAttributeMap.put() -->
   <xsl:template match="*[@nodeType='VariableDeclarator']/initializer">
      <!--<xsl:message>Matched VariableDeclarator</xsl:message>-->
      <xsl:variable name="type" select="../type/@RESOLVED_TYPE" as="xs:string?"/>
      <!--<xsl:message>VariableDeclarator: Resolved type {../type/@RESOLVED_TYPE} ==> |{f:extract-type-arguments($type)}|</xsl:message>-->
      <xsl:value-of select="if ($type) then f:extract-type-arguments($type) else '/*CANNOT RESOLVE TYPE OF VARIABLE*/'"/>
   </xsl:template>
   
   
 
   
   <!-- constructor call in "return" statement -->
   <!-- return new XXX<>() -->
   <!-- Example in SingletonAttributeMap.iterator() -->
   <xsl:template match="*[@nodeType='ReturnStmt']/*">
      <xsl:variable name="type" select="ancestor::member[1]/type/@RESOLVED_TYPE" as="xs:string"/>
      <xsl:value-of select="f:extract-type-arguments($type)"/>
   </xsl:template>
   
   <!-- constructor call in assignment statement -->
   <!-- var = new XXX<>() -->
   <!-- Example in DictionaryMap constructor -->
   
   <xsl:template match="*[@nodeType='AssignExpr'][target/@nodeType='NameExpr']/*">
      <xsl:variable name="fieldRef" select="../target"/>
      <xsl:variable name="fieldDecl" 
         select="ancestor::members/*[@nodeType='FieldDeclaration'][variables/variable/name/@identifier = $fieldRef/name/@identifier]"/>
      <xsl:variable name="type" select="../target/@RESOLVED_TYPE"/>
      <!--<xsl:message>AssignExpr: Resolved type {../target/@RESOLVED_TYPE} ==> |{f:extract-type-arguments($type)}|</xsl:message>-->
      <xsl:value-of select="f:extract-type-arguments($type)"/>
   </xsl:template>
   
   
   <!-- constructor call in an argument of a method, generally List.add(new XXX<>()) or Stack.push(new XXX<>())-->
   <!-- We don't try to handle the general case because it requires too much analysis -->
   
   <xsl:template match="*[@nodeType='MethodCallExpr']
                  [starts-with(@RESOLVED_TYPE, 'java.util.')]
                  [name/@identifier=('add', 'push')]
                  /arguments/argument">
      <xsl:variable name="fieldRef" select="../target"/>
      <xsl:variable name="fieldDecl" 
         select="ancestor::members/*[@nodeType='FieldDeclaration'][variables/variable/name/@identifier = $fieldRef/name/@identifier]"/>
      <xsl:variable name="type" select="f:extract-type-arguments(../../@RESOLVED_TYPE)"/>
      <xsl:message>Method Argument: Resolved type {../target/@RESOLVED_TYPE} ==> |{f:extract-type-arguments($type)}|</xsl:message>
      <xsl:value-of select="f:extract-type-arguments($type)"/>
   </xsl:template>  
   
 
 
   <xsl:template match="*">
      <xsl:text>/*FAILED TO RESOLVE DIAMOND OPERATOR*/</xsl:text>
   </xsl:template>   
   
</xsl:stylesheet>