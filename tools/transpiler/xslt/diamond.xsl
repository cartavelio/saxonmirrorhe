<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   expand-text="yes"
   default-mode="diamond"
   version="3.0">
   
   <xsl:mode name="diamond" on-no-match="fail" saxon:trace="no" xmlns:saxon="http://saxon.sf.net/"/>
   
   <!-- This module is concerned with expanding the diamond operator found in Java constructs like
        new ArrayList<>(). When generating the C# equivalent code, we need to add explicit type
        arguments, for example new ArrayList<NodeInfo>(). This requires type inference based on the
        context in which the construct appears.
        
        All the template rules in this module handle nodes satisfying the pattern
        match="*[@nodeType='ObjectCreationExpr'][type/typeArguments[not(*)]"
        (that is, creation of a new object with an empty list of type arguments)
        and since this has already been matched, there is no need to repeat this
        part of the pattern. The templates use the default mode="diamond".
        
        The type inference rules here are also used for constructs such as Collections.emptyList().
        
        A template rule in mode 'diamond' takes an expression as its match selection, and returns
        a string containing the type parameters to be used.
   -->
   
   
   
   <xsl:function name="f:candidateMethods" as="element(method)*">
      <xsl:param name="targetTypeOfCall" as="xs:string?"/>
      <xsl:param name="methodName" as="xs:string"/>
      <xsl:param name="arity" as="xs:integer"/>
      <xsl:variable name="targetClassDigest" select="key('digestClassKey', $targetTypeOfCall, $digest)"/>
      <xsl:choose>
         <xsl:when test="empty($targetClassDigest)"/>
         <xsl:when test="$targetClassDigest/method[@name=$methodName and count(tokenize(@params, '\|')) = $arity]">
            <xsl:sequence select="$targetClassDigest/method[@name=$methodName and count(tokenize(@params, '\|')) = $arity]"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:variable name="superclass" select="($targetClassDigest/extends)[1]/@name"/>
            <xsl:sequence select="f:candidateMethods($superclass, $methodName, $arity)"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>
   
   <!-- Assignment to "this.field" -->
   <!-- this.fieldname = new XXX<>() -->
   <!-- An example is found in the constructor of SmallAttributeMap -->
   <xsl:template match="*[@nodeType='AssignExpr'][target[@nodeType='FieldAccessExpr'][scope/@nodeType='ThisExpr']]/*">
      <xsl:message expand-text="yes">DIAMOND#52 {path(.)}</xsl:message>
      <xsl:variable name="fieldRef" select="../target"/>
      <xsl:variable name="fieldDecl" as="element()"
         select="(ancestor::members/*[@nodeType='FieldDeclaration'][variables/variable/name/@identifier = $fieldRef/name/@identifier])[last()]"/>
      <xsl:variable name="type" select="$fieldDecl/variables/variable/type/@RESOLVED_TYPE"/>
      <xsl:value-of select="f:extract-type-arguments($type)"/>
   </xsl:template>
   
   <!-- Field assignment some.x = new XXX<>() -->
   <xsl:template match="*[@nodeType='AssignExpr'][target[@nodeType='FieldAccessExpr'][scope/@nodeType='NameExpr']]/*">
      <xsl:variable name="fieldRef" select="../target"/>
      <!--<xsl:message>Looking for class: {$fieldRef/@RESOLVED_TYPE}</xsl:message>-->
      <xsl:variable name="targetClassDigest" select="key('digestClassKey', $fieldRef/@RESOLVED_TYPE, $digest)"/>
      <!--<xsl:message>Target class digest: {serialize($targetClassDigest)}</xsl:message>-->
      <xsl:variable name="fieldType" select="$targetClassDigest/field[@name=$fieldRef/name/@identifier]/@type"/>
      <xsl:value-of select="if ($fieldType) then f:extract-type-arguments($fieldType) else '/*UNIDENTIFIED FIELD*/'"/>
   </xsl:template>
   

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
   <xsl:template match="*[@nodeType='ReturnStmt'][ancestor::member[1]/type/@RESOLVED_TYPE]/*">
      <xsl:variable name="type" select="ancestor::member[1]/type/@RESOLVED_TYPE" as="xs:string"/>
      <xsl:value-of select="f:extract-type-arguments($type)"/>
   </xsl:template>
   
   <!-- constructor call in assignment statement -->
   <!-- var = new XXX<>() -->
   <!-- Example in DictionaryMap constructor -->
   
   <xsl:template match="*[@nodeType='AssignExpr'][target[@nodeType='NameExpr' and @RESOLVED_TYPE]]/*">
      <xsl:variable name="fieldRef" select="../target"/>
      <xsl:variable name="fieldDecl" 
         select="ancestor::members/*[@nodeType='FieldDeclaration'][variables/variable/name/@identifier = $fieldRef/name/@identifier]"/>
      <xsl:variable name="type" select="../target/@RESOLVED_TYPE"/>
      <!--<xsl:message>AssignExpr: Resolved type {../target/@RESOLVED_TYPE} ==> |{f:extract-type-arguments($type)}|</xsl:message>-->
      <xsl:value-of select="f:extract-type-arguments($type)"/>
   </xsl:template>
   
   
   <!-- Call appearing in an argument of a method in a system-defined class, 
        generally List.add(new XXX<>()) or Stack.push(new XXX<>())-->
   <!-- We don't try to handle the general case because it requires too much analysis -->
   
   <xsl:template match="*[@nodeType='MethodCallExpr']
                  [starts-with(@RESOLVED_TYPE, 'java.util.')]
                  [name/@identifier=('add', 'push')]
                  /arguments/argument" priority="30">
      <xsl:variable name="fieldRef" select="../target"/>
      <xsl:variable name="fieldDecl" 
         select="ancestor::members/*[@nodeType='FieldDeclaration'][variables/variable/name/@identifier = $fieldRef/name/@identifier]"/>
      <xsl:variable name="type" select="f:extract-type-arguments(../../@RESOLVED_TYPE)"/>
      <xsl:message>Method Argument: Resolved type {$type} ==> |{f:extract-type-arguments($type)}|</xsl:message>
      <xsl:value-of select="f:extract-type-arguments($type)"/>
   </xsl:template> 
   
   <!-- Call appearing in an argument of a method in a user-defined class, 
        for example FuncLibList.bind(x, y, x, new ArrayList<>()) -->
   
   <xsl:template match="*[@nodeType='MethodCallExpr']/arguments/argument">
      <xsl:variable name="targetTypeOfCall" select="../../(@RESOLVED_TYPE,@DECLARING_TYPE)[1]" as="xs:string?"/>
      <xsl:variable name="methodName" select="../../name/@identifier"/>
      <xsl:variable name="arity" select="count(parent::arguments/child::argument)"/>
      <xsl:variable name="targetClassDigest" select="key('digestClassKey', $targetTypeOfCall, $digest)"/>
      <xsl:variable name="selectedMethods" select="f:candidateMethods($targetTypeOfCall, $methodName, $arity)"/>
      <xsl:choose>
         <xsl:when test="count($selectedMethods)=1">
            <xsl:variable name="argPosition" select="count(preceding-sibling::argument)+1" as="xs:integer"/>
            <xsl:variable name="targetClass" select="tokenize($selectedMethods[1]/@params, '\|')[$argPosition]"/>
            <xsl:value-of select="f:extract-type-arguments($targetClass)"/>
         </xsl:when>
         <xsl:otherwise>/*NO UNIQUE METHOD {$targetTypeOfCall}.{$methodName}#{$arity} (FOUND {count($selectedMethods)})*/</xsl:otherwise>
      </xsl:choose>
   </xsl:template>

   
   <!-- argument to a constructor for a user-defined class, as in `new MyClass(new ArrayList<>())`.
        We only solve this if there is a single constructor of the right arity -->
   <xsl:template match="*[@nodeType='ObjectCreationExpr']/arguments/argument">
      <xsl:variable name="typeBeingCreated" select="../../type/(@RESOLVED_TYPE, @UNRESOLVED)[1]" as="xs:string"/>
      <xsl:variable name="constructorArity" select="count(parent::arguments/child::argument)"/>
      <xsl:variable name="targetClassDigest" select="key('digestClassKey', $typeBeingCreated, $digest)"/>
      <xsl:variable name="constructors" select="$targetClassDigest/constructor[count(tokenize(@params, '\|')) = $constructorArity]" 
         as="element(constructor)*"/>
      <xsl:choose>
         <xsl:when test="count($constructors)=1">
            <xsl:variable name="argPosition" select="count(preceding-sibling::argument)+1" as="xs:integer"/>
            <xsl:variable name="targetClass" select="tokenize($constructors[1]/@params, '\|')[$argPosition]"/>
            <xsl:value-of select="f:extract-type-arguments($targetClass)"/>
         </xsl:when>
         <xsl:otherwise>/*NO UNIQUE CONSTRUCTOR FOUND*/</xsl:otherwise>
      </xsl:choose>
   </xsl:template> 
   
   <!-- Special-case `new ObjectValue<>(X)` -->
   
   <xsl:template match="*[type/@RESOLVED_TYPE='net.sf.saxon.value.ObjectValue']
                         [arguments/argument[1]/@RESOLVED_TYPE]" 
      priority="50">
      <xsl:value-of select="arguments/argument[1]/@RESOLVED_TYPE => f:adjustTypeNamespace()"/>
   </xsl:template>
   
   <!-- Special-case `new SequenceExtent.Of<>(List<X>)` -->
   
   <xsl:template match="*[type/@RESOLVED_TYPE='net.sf.saxon.value.SequenceExtent.Of']
                         [arguments/argument[1]/@RESOLVED_TYPE]" 
                         priority="50">
      <xsl:value-of select="f:extract-type-arguments(arguments/argument[1]/@RESOLVED_TYPE) => replace('\[\]', '') => f:adjustTypeNamespace()"/>
   </xsl:template>
   
   <!-- Special-case `new SequenceExtent.Of<>(new X[])` -->
   
   <xsl:template match="*[type/@RESOLVED_TYPE='net.sf.saxon.value.SequenceExtent.Of']
                         [arguments/argument[1]/@RESOLVED_TYPE]
                         [*/@nodeType='ArrayCreationExpr']" priority="60">
      <xsl:value-of select="f:extract-type-arguments(arguments/argument[1]/elementType/@RESOLVED_TYPE) => f:adjustTypeNamespace()"/>
   </xsl:template>
   
   <!-- Special-case `new ListIterator.of<>(List<X>)` -->
   
   <xsl:template match="*[type/@RESOLVED_TYPE='net.sf.saxon.tree.iter.ListIterator.Of'][arguments/argument[1]/@RESOLVED_TYPE]" priority="50">
      <xsl:value-of select="f:extract-type-arguments(arguments/argument[1]/@RESOLVED_TYPE) => replace('\[\]', '') => f:adjustTypeNamespace()"/>
   </xsl:template>
   
   <!-- Special-case `new ArrayIterator.Of<>(X[])`, `new SequenceExtent.Of(X[])` -->
   
   <xsl:template match="*[type/@RESOLVED_TYPE=('net.sf.saxon.tree.iter.ArrayIterator.Of', 'net.sf.saxon.value.SequenceExtent.Of')]
                         [arguments/argument[1]/@RESOLVED_TYPE[ends-with(., '[]')]]" 
                         priority="60">
      <xsl:value-of select="arguments/argument[1]/@RESOLVED_TYPE => replace('\[\]$', '') => f:adjustTypeNamespace()"/>
   </xsl:template>
   
   <!-- Special-case `new ArrayIterator.OfNodes<>(X[])` -->
   
   <xsl:template match="*[type/@RESOLVED_TYPE='net.sf.saxon.tree.iter.ArrayIterator.OfNodes']
      [arguments/argument[1]/@RESOLVED_TYPE[ends-with(., '[]')]]" 
      priority="50">
      <xsl:value-of select="arguments/argument[1]/@RESOLVED_TYPE => replace('\[\]$', '') => f:adjustTypeNamespace()"/>
   </xsl:template>
   
 
 
 
   <xsl:template match="*">
      <xsl:text>/*FAILED TO RESOLVE DIAMOND OPERATOR*/</xsl:text>
   </xsl:template>   
   
</xsl:stylesheet>