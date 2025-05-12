<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:ixsl="http://saxonica.com/ns/interactiveXSLT" 
    xmlns:js="http://saxonica.com/ns/globalJS"
    xmlns:f="urn:viewerapp.function" 
    extension-element-prefixes="ixsl" 
    version="3.0">
    
    <xsl:mode name="jdc" on-no-match="shallow-copy"/>
    <xsl:mode name="detail" on-no-match="deep-skip"/>
    
    <xsl:template match="body|sentence|body/div" mode="jdc">
        <xsl:apply-templates mode="jdc"/>
    </xsl:template>
    
    <xsl:template match="pre" mode="jdc">
        <samp>
            <xsl:apply-templates mode="jdc"/>
        </samp>
    </xsl:template>

    <xsl:template match="tag[@kind='see']" mode="jdc">
        <xsl:variable name="local" select="starts-with(. , '#')" as="xs:boolean"/>
        <xsl:variable name="afterhash"
            select="if (not($local)) then substring-after(., '#') else ''"/>
        <xsl:variable name="isCSlink" select="starts-with(. , 'Saxon.Api')" as="xs:boolean"/>
        <xsl:variable name="isLocalCSlink" select="$local and ancestor::package/@id eq 'Saxon.Api'" as="xs:boolean"/>
        <span class="{if ($isCSlink or $isLocalCSlink) then 'cslink' else 'javalink'}"
            data-href="{if ($local) then concat(ancestor::class[1]/@fulltype, '@', substring(. , 2)) else translate(. , '#', '@')}">
            <xsl:value-of
                select="if ($local) then substring(. , 2)
                        else if ($afterhash ne '') then $afterhash
                        else f:last-substring-after(. , 46)" />
        </span>
    </xsl:template>
    
    <xsl:template match="tag[@kind='code']" mode="jdc">
        <code><xsl:value-of select="."/></code>
    </xsl:template>
    
    <!-- @deprecated  -->
    <xsl:template match="attribute[@name='@deprecated']" mode="jdc">
        <p>Deprecated <xsl:apply-templates select="body" mode="jdc"/></p>
    </xsl:template>
    
    <!-- @see "string" or @see <a/> etc  -->
    <xsl:template match="attribute[@name='@see']" mode="jdc">
        <xsl:sequence select="body/div/node()"/>
    </xsl:template>
    
    <!-- @see package.class#member -->
    <xsl:template match="attribute[@name='@see'][@see='java']" mode="jdc">
        <xsl:variable name="local" select="starts-with(. , '#')" as="xs:boolean"/>
        <xsl:variable name="afterhash"
            select="if (not($local)) then substring-after(., '#') else ''"/>
        <xsl:variable name="isCSlink" select="starts-with(. , 'Saxon.Api')" as="xs:boolean"/>
        <xsl:variable name="isLocalCSlink" select="$local and ancestor::package/@id eq 'Saxon.Api'" as="xs:boolean"/>
        <span class="{if ($isCSlink or $isLocalCSlink) then 'cslink' else 'javalink'}"
            data-href="{if ($local) then concat(ancestor::class[1]/@fulltype, '@', substring(. , 2)) else translate(. , '#', '@')}">
            <xsl:value-of
                select="if ($local) then substring(. , 2)
                else if ($afterhash ne '') then $afterhash
                else f:last-substring-after(. , 46)" />
        </span>
    </xsl:template>

    <xsl:function name="f:last-substring-after" as="xs:string">
        <xsl:param name="text" as="xs:string"/>
        <xsl:param name="char" as="xs:double"/>
        <xsl:variable name="last" as="xs:integer?"
            select="index-of(string-to-codepoints($text), $char)[last()]"/>
        <xsl:value-of select="if (exists($last)) then substring($text, $last + 1) else $text"/>
    </xsl:function>

    <xsl:function name="f:class-parts" as="xs:string*">
        <xsl:param name="class" as="node()?"/>
        <xsl:for-each select="$class">
            <xsl:variable name="isEnum" select="self::enum or @superclass eq 'Enum'"/>
            <xsl:sequence
                select="string(@visibility),
                        if (@abstract and not(@interface)) then 'abstract' else (),
                        if (@static) then 'static' else (),
                        if (@final and not($isEnum)) then 'final' else (),
                        if (@interface) then 'interface' 
                            else if ($isEnum) then 'enum'
                            else if (name(.) eq 'delegate') then 'delegate'
                            else if (name(.) eq 'class') then 'class' else ()" />
        </xsl:for-each>
    </xsl:function>

    <xsl:template match="extends" mode="extends">
        <ul>
            <li>
                <xsl:sequence select="f:showType(interface, true(), false())"/>
                <xsl:choose>
                    <xsl:when test="parent::interface">
                        <xsl:apply-templates select="parent::interface/parent::extends"
                            mode="extends"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <ul>
                            <li>
                                <xsl:value-of select="ancestor::class[1]/concat(@fulltype, f:flatParamTypes(.))"/>
                            </li>
                        </ul>
                    </xsl:otherwise>
                </xsl:choose>
            </li>
        </ul>
    </xsl:template>
    
    <xsl:function name="f:sig" as="element()*">
        <xsl:param name="classid" as="xs:string"/>
        <xsl:param name="method" as="node()"/><!-- method or constructor or delegate element -->
        <xsl:param name="for-detail" as="xs:boolean"/>
        
        <xsl:variable name="isDotNetApiMember" select="starts-with($classid, 'Saxon.Api')" as="xs:boolean"/>
        
        <xsl:choose>
            <xsl:when test="$for-detail">
                <xsl:for-each select="$method">
                    <span class="fullsignature">
                        <span class="fblock">
                            <xsl:value-of select="f:class-parts(.),' '"/>
                            <xsl:if test="not($isDotNetApiMember) and paramtypes">
                                <xsl:sequence select="f:showParamTypes(.)"/>
                                <xsl:text> </xsl:text>
                            </xsl:if>
                            <xsl:sequence select="f:showGenericType(.)"/>
                            <xsl:text> </xsl:text>
                            <xsl:value-of select="@id"/>
                            <xsl:if test="$isDotNetApiMember and paramtypes">
                                <xsl:sequence select="f:showParamTypes(.)"/>
                            </xsl:if>
                        </span>
                        <span class="fblock">
                            <xsl:text> (</xsl:text>
                            <xsl:for-each select="params/param">
                                <xsl:sequence select="f:showGenericType(.)"/>
                                <xsl:text> </xsl:text>
                                <xsl:value-of select="@name"/>
                                <xsl:if test="position() ne last()">
                                    <xsl:text>, </xsl:text>
                                    <br/>
                                </xsl:if>
                            </xsl:for-each>
                            <xsl:text>)</xsl:text>
                            <xsl:if test="exceptions">
                                <br/>
                                <xsl:text>throws </xsl:text>
                                <xsl:for-each select="exceptions/exception">
                                    <br/>
                                    <xsl:sequence select="f:showType(., false(), true())"/>
                                </xsl:for-each>
                            </xsl:if>
                        </span>
                    </span>
                    <div class="fcomment">
                        <xsl:choose>
                            <xsl:when test="comment">
                                <xsl:apply-templates select="comment/attribute[@name='@deprecated']" mode="jdc"/>
                                <xsl:apply-templates select="(comment/body, comment/sentence)[1]" mode="jdc"/>
                            </xsl:when>
                            <xsl:when test="self::method">
                                <!-- Don't bother trying f:find-super for constructor -->
                                <!--<xsl:message>**** no comment: <xsl:value-of select="concat($classid,'@',@id)"/></xsl:message>-->
                                <!-- Attempt to get comment from super method 
                                        - checks all implemented and extended interfaces
                                        - f:find-super now returns only the first super method with a comment -->
                                <xsl:variable name="super" select="f:find-super($method)"/>
                                <!--<xsl:if test="exists($super)">
                                    <xsl:message>BINGO: <xsl:value-of select="concat($classid,'@',@id)"/></xsl:message>
                                </xsl:if>-->
                                <xsl:apply-templates select="($super/comment/body, $super/comment/sentence)[1]" mode="jdc"/>
                            </xsl:when>
                        </xsl:choose>
                    </div>
                    
                    <xsl:if test="override-of">
                        <p class="methodLabel">Overrides:</p>
                        <xsl:for-each select="override-of">
                            <div class="params">
                                <span class="{if ($isDotNetApiMember) then 'cslink' else 'javalink'}" data-href="{concat(@fulltype, '@', @method)}">
                                    <xsl:value-of select="substring-before(@method, '(')"/>
                                </span>
                                <xsl:text> in class </xsl:text>
                                <span class="{if ($isDotNetApiMember) then 'cslink' else 'javalink'}" data-href="{@fulltype}">
                                    <xsl:value-of select="@type"/>
                                </span>
                            </div>
                        </xsl:for-each>
                    </xsl:if>
                    
                    <xsl:if test="paramtags">
                        <p class="methodLabel">Type Parameters:</p>
                        <xsl:for-each select="paramtags/tag">
                            <div class="params">
                                <code><xsl:value-of select="."/></code>
                                <xsl:if test="@comment">
                                    <xsl:text> - </xsl:text>
                                    <xsl:value-of select="@comment"/>
                                </xsl:if>
                            </div>
                        </xsl:for-each>
                    </xsl:if>
                    
                    <xsl:if test="params">
                        <p class="methodLabel">Parameters:</p>
                        <xsl:for-each select="params/param">
                            <div class="params">
                                <code><xsl:value-of select="@name"/></code>
                                <xsl:if test="body/div/node()"><!-- sometimes div is empty -->
                                    <xsl:text> - </xsl:text>
                                    <xsl:apply-templates select="body" mode="jdc"/>
                                </xsl:if>
                            </div>
                        </xsl:for-each>
                    </xsl:if>
                    
                    <xsl:if test="comment/return">
                        <p class="methodLabel">Returns:</p>
                        <div class="params">
                            <xsl:apply-templates select="comment/return/body" mode="jdc"/>
                        </div>
                    </xsl:if>
                    
                    <xsl:if test="exceptions">
                        <p class="methodLabel">Throws:</p>
                        <xsl:for-each select="exceptions/exception">
                            <div class="params">
                                <xsl:sequence select="f:showType(., false(), true())"/>
                                <xsl:if test="exists(body|sentence)">
                                    <xsl:text> - </xsl:text>
                                    <xsl:apply-templates select="(body, sentence)[1]" mode="jdc"/>
                                </xsl:if>
                            </div>
                        </xsl:for-each>
                    </xsl:if>
                    
                    <xsl:for-each select="comment/example">
                        <p class="methodLabel">Example:</p>
                        <samp>
                            <xsl:apply-templates select="code/text()" mode="jdc"/>
                        </samp>
                    </xsl:for-each>
                    
                    <xsl:for-each select="comment/attribute[@name='@since']">
                        <p class="methodLabel">Since:</p>
                        <div class="params">
                            <xsl:apply-templates select="body" mode="jdc"/>
                        </div>
                    </xsl:for-each>
                    
                    <xsl:if test="exists(comment/attribute[@name='@see'])">
                        <p class="methodLabel">See Also:</p>
                        <div class="params">
                            <xsl:for-each select="comment/attribute[@name='@see']">
                                <xsl:apply-templates select="." mode="jdc"/>
                                <xsl:if test="position() ne last()">
                                    <xsl:text>, </xsl:text>
                                </xsl:if>
                            </xsl:for-each>
                        </div>
                    </xsl:if>
                    
                </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
                <xsl:for-each select="$method">
                    <span class="signature">
                        <span class="{if ($isDotNetApiMember) then 'cslink' else 'javalink'}">
                            <xsl:attribute name="data-href" select="concat($classid,'@',@id)"/>
                            <xsl:value-of select="@id"/>
                        </span>
                        <xsl:text> (</xsl:text>
                        <xsl:for-each select="params/param">
                            <xsl:sequence select="f:showGenericType(.)"/>
                            <xsl:text> </xsl:text>
                            <xsl:value-of select="@name"/>
                            <xsl:if test="position() ne last()">
                                <xsl:text>, </xsl:text>
                            </xsl:if>
                        </xsl:for-each>
                        <xsl:text>)</xsl:text>
                    </span>
                    <div class="fnormal">
                        <xsl:choose>
                            <xsl:when test="comment">
                                <xsl:apply-templates select="(comment/attribute[@name='@deprecated'],
                                    comment/sentence)[1]" mode="jdc"/>
                            </xsl:when>
                            <xsl:when test="self::method">
                                <!-- Don't bother trying f:find-super for constructor -->
                                <xsl:variable name="super" select="f:find-super($method)"/>
                                <xsl:apply-templates select="$super/comment/sentence" mode="jdc"/>
                            </xsl:when>
                        </xsl:choose>
                    </div>
                </xsl:for-each>
            </xsl:otherwise>
        </xsl:choose>
        
    </xsl:function>

    <xsl:function name="f:addLink" as="element()">
        <xsl:param name="href"/>
        <xsl:param name="text"/>
        <xsl:param name="style"/>
        <span class="{$style}" data-href="{$href}">
            <xsl:value-of select="$text"/>
        </span>
    </xsl:function>
    
    <xsl:function name="f:isJavaType" as="xs:boolean">
        <xsl:param name="fulltype" as="xs:string"/>
        <xsl:sequence select="starts-with($fulltype, 'net.sf.saxon.') or starts-with($fulltype, 'com.saxonica.')"/>
    </xsl:function>
    
    <xsl:function name="f:isCSType" as="xs:boolean">
        <xsl:param name="fulltype" as="xs:string"/>
        <xsl:sequence select="starts-with($fulltype, 'Saxon.Api.')"/>
    </xsl:function>
    
    <!-- Functions for displaying the types of objects.
         Generally have 2 versions: 'show' to include links; and 'flat' which just returns a string value. -->
    
    <!-- Show the type of any object with @fulltype ($class can be any class, interface, exception, etc.)
         Also used for delegate with @fullname.
         The result is a javalink (or cslink) span element for the main type (named in full if $showFull is true), 
         followed by a span element showing the types of type parameters if the type is generic. -->
    <xsl:function name="f:showType" as="element()*">
        <xsl:param name="class" as="node()"/>
        <xsl:param name="showFull" as="xs:boolean"/>
        <xsl:param name="isClass" as="xs:boolean"/>
        <xsl:variable name="fulltype" select="if ($class/self::delegate) then $class/@fullname else
            $class/@fulltype" as="xs:string"/>
        <xsl:for-each select="$class">
            <span class="{if (f:isJavaType($fulltype)) then 'javalink' else if
                (f:isCSType($fulltype)) then 'cslink' else 'apilink'}" data-href="{$fulltype}">
                <xsl:value-of
                    select="if ($isClass) then @id else if ($showFull) then $fulltype else @type"/>
            </span>
            <xsl:sequence select="f:showParamTypes(.)"/>
        </xsl:for-each>
    </xsl:function>
    
    <!-- Show the types of the type parameters of $class. The result is an operator span element, whose content starts
         with '<' and ends with '>'. $class can be any class, interface, etc.
         If $class has a paramtypes child, then use it (as in javadoc, or in dotnetdoc for generic methods),
         otherwise just use the type children (as in other cases for dotnetdoc).
         Calls to this function usually check for a paramtypes child element first, except for the call from f:showType(). -->
    <xsl:function name="f:showParamTypes" as="element()*">
        <xsl:param name="class"/>
        <xsl:choose>
            <xsl:when test="$class/paramtypes">
                <xsl:for-each select="$class/paramtypes">
                    <span class="operator">
                        <xsl:text>&lt;</xsl:text>
                        <xsl:for-each select="type">
                            <xsl:choose>
                                <xsl:when test="@fulltype ne ''">
                                    <span class="{if (f:isJavaType(@fulltype)) then 'javalink' else if
                                        (f:isCSType(@fulltype)) then 'cslink' else 'apilink'}"
                                        data-href="{@fulltype}">
                                        <xsl:value-of select="@name"/>
                                    </span>
                                </xsl:when>
                                <xsl:otherwise>
                                    <xsl:value-of select="@name"/>
                                </xsl:otherwise>
                            </xsl:choose>
                            <xsl:if test="bounds">
                                <xsl:text> extends </xsl:text>
                                <xsl:for-each select="bounds/limit">
                                    <xsl:sequence select="f:showGenericType(.)"/>
                                </xsl:for-each>
                            </xsl:if>
                            <xsl:if test="position() ne last()">
                                <xsl:text>, </xsl:text>
                            </xsl:if>
                        </xsl:for-each>
                        <xsl:text>&gt;</xsl:text>
                    </span>
                </xsl:for-each>
            </xsl:when>
            <xsl:when test="empty($class/self::delegate) and $class/type">
                <xsl:sequence select="f:showTypeChildren($class)"/>
            </xsl:when>
        </xsl:choose>
    </xsl:function>
    
    <!-- Produce a string (starting with '<' and ending with '>') showing the types of the type parameters of $class.
         If $class has a paramtypes child, then use it (as in javadoc, or in dotnetdoc for generic methods),
         otherwise just use the type children (as in other cases for dotnetdoc).
         Calls to this function do not check for a paramtypes child element first. -->
    <xsl:function name="f:flatParamTypes" as="xs:string?">
        <xsl:param name="class"/>
        <xsl:choose>
            <xsl:when test="$class/paramtypes">
                <xsl:for-each select="$class/paramtypes">
                    <xsl:variable name="ptstr" as="xs:string*">
                        <xsl:for-each select="type">
                            <xsl:value-of select="@name"/>
                            <xsl:if test="bounds">
                                <xsl:text> extends </xsl:text>
                                <xsl:for-each select="bounds/limit">
                                    <xsl:value-of select="f:flatGenericType(.)"/>
                                </xsl:for-each>
                            </xsl:if>
                            <xsl:if test="position() ne last()">
                                <xsl:text>, </xsl:text>
                            </xsl:if>
                        </xsl:for-each>
                    </xsl:variable>
                    <xsl:value-of select="concat('&lt;', string-join($ptstr), '&gt;')"/>
                </xsl:for-each>
            </xsl:when>
            <xsl:when test="$class/type">
                <xsl:sequence select="f:flatTypeChildren($class)"/>
            </xsl:when>
        </xsl:choose>
    </xsl:function>
    
    <!-- Show the type of a class member, which may be generic.
         $member accepts a variety of elements: method, constructor, type, limit, param, enumConstant, field, etc. 
         Handles covariant and contravariant type parameters (with differences for Java and .NET).
         
         This function was previously named f:showMethodType, but now renamed to be more general since the input does
         not have to be a method or constructor element. -->
    <xsl:function name="f:showGenericType" as="node()*">
        <xsl:param name="member" as="node()"/>
        <xsl:variable name="isDotNetApiMember" select="$member/ancestor::package/@id eq 'Saxon.Api'" as="xs:boolean"/>
        <xsl:for-each select="$member">
            <xsl:if test="@extendby">
                <xsl:value-of select="if ($isDotNetApiMember) then 'out ' 
                    else concat(@extendby, ' extends ')"/>
            </xsl:if>
            <xsl:if test="@super">
                <xsl:value-of select="if ($isDotNetApiMember) then 'in ' 
                    else concat(@super, ' super ')"/>
            </xsl:if>
            <xsl:choose>
                <xsl:when test="@preamble"><!-- Note @preamble shouldn't be used anymore -->
                    <xsl:choose>
                        <xsl:when test="not(contains(@fulltype,'.'))">
                            <xsl:value-of select="concat(@preamble, @fulltype)"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="@preamble"/>
                            <span class="{if (f:isJavaType(@fulltype)) then 'javalink' else if
                                (f:isCSType(@fulltype)) then 'cslink' else 'apilink'}"
                                data-href="{@fulltype}">
                                <xsl:value-of select="@type"/>
                            </span>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:when>
                <xsl:when test="not(contains(@fulltype,'.'))">
                    <xsl:value-of select="@type"/>
                </xsl:when>
                <xsl:when test="@type='void'">
                    <xsl:value-of select="@type"/>
                </xsl:when>
                <xsl:when test="contains(@type,'{')"><!-- Note that this shouldn't happen anymore -->
                    <xsl:value-of select="@type"/>
                </xsl:when>
                <xsl:otherwise>
                    <span class="{if (f:isJavaType(@fulltype)) then 'javalink' else if
                        (f:isCSType(@fulltype)) then 'cslink' else 'apilink'}"
                        data-href="{@fulltype}">
                        <xsl:value-of select="@type"/>
                    </span>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="exists(type)">
                <xsl:sequence select="f:showTypeChildren(.)"/>
            </xsl:if>
        </xsl:for-each>
    </xsl:function>
    
    <!-- Produce a string showing the type of a class member, which may be generic.
         $member accepts a variety of elements: method, constructor, type, limit, param, enumConstant, field, etc.
         Handles covariant and contravariant type parameters (with differences for Java and .NET).-->
    <xsl:function name="f:flatGenericType" as="xs:string?">
        <xsl:param name="member" as="node()"/>
        <xsl:variable name="isDotNetApiMember" select="$member/ancestor::package/@id eq 'Saxon.Api'" as="xs:boolean"/>
        <xsl:variable name="flatGenericType" as="xs:string*">
            <xsl:for-each select="$member">
                <xsl:if test="@extendby">
                    <xsl:value-of select="if ($isDotNetApiMember) then concat(@extendby, ' ') 
                        else concat(@extendby, ' extends ')"/>
                </xsl:if>
                <xsl:if test="@super">
                    <xsl:value-of select="if ($isDotNetApiMember) then concat(@super, ' ') 
                        else concat(@super, ' super ')"/>
                </xsl:if>
                <xsl:choose>
                    <xsl:when test="@preamble"><!-- Note @preamble shouldn't be used anymore -->
                        <xsl:choose>
                            <xsl:when test="not(contains(@fulltype,'.'))">
                                <xsl:value-of select="concat(@preamble, @fulltype)"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="concat(@preamble, @type)"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select="@type"/>
                    </xsl:otherwise>
                </xsl:choose>
                <xsl:if test="exists(type)">
                    <xsl:sequence select="f:flatTypeChildren(.)"/>
                </xsl:if>
            </xsl:for-each>
        </xsl:variable>
        <xsl:value-of select="string-join($flatGenericType)"/>
    </xsl:function>
    
    <!-- Show the type (which may be generic) of a property (found only in .NET, not Java).
        Contains processing specific to property elements (which are found in dotnetdoc only). -->
    <xsl:function name="f:showPropertyType" as="node()*">
        <xsl:param name="property" as="element(property)"/>
        <xsl:for-each select="$property">
            <xsl:choose>
                <xsl:when test="f:isJavaType(@fulltype)">
                    <span class="javalink" data-href="{@fulltype}">
                        <xsl:value-of select="tokenize(@type,'\.')[last()]"/>
                    </span>
                </xsl:when>
                <xsl:when test="@type = @fulltype">
                    <code><xsl:value-of select="@type"/></code>
                </xsl:when>
                <xsl:when test="f:isCSType(@fulltype)">
                    <span class="cslink" data-href="{@fulltype}">
                        <xsl:value-of select="@type"/>
                    </span>
                </xsl:when>
                <xsl:otherwise>
                    <span class="apilink" data-href="{@fulltype}">
                        <xsl:value-of select="@type"/>
                    </span>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="exists(type)">
                <xsl:sequence select="f:showTypeChildren(.)"/>
            </xsl:if>
        </xsl:for-each>
    </xsl:function>
    
    <!-- Show the types of the type parameters of $member, with details obtained from the type children. 
         The result is an operator span element, whose content starts with '<' and ends with '>'. 
         $member can be any class, interface, etc. -->
    <xsl:function name="f:showTypeChildren" as="element(span)">
        <xsl:param name="member" as="node()"/>
        <span class="operator">
            <xsl:text>&lt;</xsl:text>
            <xsl:for-each select="$member/type">
                <xsl:sequence select="f:showGenericType(.)"/>
                <xsl:if test="position() ne last()">
                    <xsl:text>, </xsl:text>
                </xsl:if>
            </xsl:for-each>
            <xsl:text>&gt;</xsl:text>
        </span>
    </xsl:function>
    
    <!-- Produce a string (starting with '<' and ending with '>') showing the types of the type parameters of $member,
        with details obtained from the type children. -->
    <xsl:function name="f:flatTypeChildren" as="xs:string">
        <xsl:param name="member" as="node()"/>
        <xsl:variable name="ptstr" as="xs:string*">
            <xsl:for-each select="$member/type">
                <xsl:value-of select="f:flatGenericType(.)"/>
                <xsl:if test="position() ne last()">
                    <xsl:value-of select="', '"/>
                </xsl:if>
            </xsl:for-each>
        </xsl:variable>
        <xsl:value-of select="concat('&lt;', string-join($ptstr), '&gt;')"/>
    </xsl:function>
    
    <!-- Produce a string containing the modifiers for $member -->
    <xsl:function name="f:modifiers" as="xs:string">
        <xsl:param name="member" as="node()"/>
        <xsl:for-each select="$member">
            <xsl:value-of select="string-join((
                if (@visibility eq 'public') then () else string(@visibility),
                if (@abstract) then 'abstract' else (),
                if (@static) then 'static' else ()),
                ' '), ' '" />
        </xsl:for-each>
    </xsl:function>
    
    <!-- The following functions are used to locate comments in a Java superclass or interface if
        missing from a subclass, looking at all implemented or extended interfaces (i.e. all the way
        up the class hierarchy) -->
    <!-- TODO: Only used for method comments. Could extend for other comments (e.g parameter definitions) -->
    
    <xsl:function name="f:get-package-doc" as="document-node()?">
        <xsl:param name="package-name" as="xs:string"/>
        <xsl:variable name="doc-path" select="concat($jclass-path, $package-name, '.xml')"/>
        <xsl:sequence select="if (doc-available($doc-path)) then doc($doc-path) else ()"/>
    </xsl:function>
    
    <xsl:function name="f:get-package-doc-name" as="xs:string?">
        <xsl:param name="full-name" as="xs:string"/>
        <xsl:variable name="tokens" select="tokenize($full-name, '\.')"/>
        <xsl:variable name="package-name" as="xs:string" select="string-join($tokens[position() ne last()], '.')"/>
        <xsl:sequence select="concat($jclass-path, $package-name, '.xml')"/>
    </xsl:function>
    
    <xsl:function name="f:get-class" as="element(class)?">
        <xsl:param name="full-name" as="xs:string"/>
        <xsl:variable name="tokens" select="tokenize($full-name, '\.')"/>
        <xsl:variable name="package" as="xs:string" select="string-join($tokens[position() ne last()], '.')"/>
        <xsl:sequence select="f:get-package-doc($package)/package/class[@fulltype=$full-name]"/>
    </xsl:function>
    
    <xsl:function name="f:get-super" as="element()?">
        <xsl:param name="method" as="element()"/>
        <xsl:param name="superclass" as="element(class)?"/>
        <xsl:variable name="seq" 
            select="$superclass/methods/*[name() = name($method) and @id = $method/@id and count(params/param) = count($method/params/param)][1]"/>
        <!--<xsl:message>get-super exists($seq): <xsl:value-of select="exists($seq)"/></xsl:message>-->
        <xsl:sequence select="$seq"/>
        <!-- TODO: doesn't really handle overloaded methods properly but it's close enough -->
    </xsl:function>
    
    <xsl:function name="f:find-super" as="element()?">
        <!-- Now only returns first matching super method *with a comment* -->
        <xsl:param name="method" as="element()"/>
        <!--<xsl:message>find-super of method: <xsl:value-of select="$method/@id"/> </xsl:message>-->
        <xsl:variable name="seq" 
            select="($method/parent::methods/parent::class/(implements|extends)//interface/@fulltype/f:get-super($method,
            f:get-class(.)))[comment][1]"/>
        <!--<xsl:message>find-super exists($seq): <xsl:value-of select="exists($seq)"/></xsl:message>-->
        <xsl:sequence select="$seq"/>
    </xsl:function>

    <xsl:function name="f:classnotfound" as="element()*"> 
        <xsl:param name="parts" as="xs:string*"/>
        <xsl:variable name="subparts" select="subsequence($parts, 2)"/>
        <xsl:variable name="dotname" select="string-join($subparts,'.')"/>
        <xsl:variable name="path" select="concat(translate($dotname,'.','/'),'.html')"/>
        <h1>Non-Saxon Type</h1>
        <p>Type <code><xsl:value-of select="$dotname"/></code> is not defined here. </p>
        <xsl:if test="starts-with($dotname, 'java')">
            <p>See the Java Specification for <a
                    href="http://docs.oracle.com/javase/1.5.0/docs/api/{$path}" target="_blank"
                        ><xsl:value-of select="$parts[last()]"/></a> for more detail.</p>
        </xsl:if>   
        <xsl:if test="starts-with($dotname, 'System')">
            <p>See the .NET Specification for <a href="{concat('http://msdn.microsoft.com/en-us/library/', lower-case($dotname), 
                '(VS.80).aspx')}" target="_blank"><xsl:value-of select="$dotname"/></a> for more detail.</p>
        </xsl:if>
        <p class="history">Back</p>
    </xsl:function>

    <xsl:function name="f:pkgnotfound" as="element()*">
        <xsl:param name="parts" as="xs:string*"/>
        <xsl:choose>
            <xsl:when test="count($parts) eq 2">
                <h1>Non-Saxon Type</h1>
                <p>Type: <code><xsl:value-of select="$parts[2]"/></code> is not defined within this
                    documentation.</p>
                <p>See the Java Specification <a
                    href="http://docs.oracle.com/javase/specs/jls/se7/html/jls-4.html#jls-4.2"
                    target="_blank">Primitive Types and Values</a> section for more detail.</p>
                <p class="history">Back</p>
            </xsl:when>
            <xsl:otherwise>
                <xsl:sequence select="f:classnotfound($parts)"/>
            </xsl:otherwise>
        </xsl:choose>
        
    </xsl:function>
    
    <xsl:function name="f:eliminate-duplicates" as="element()*">
        <xsl:param name="seq" as="element(interface)*"/>
        <xsl:sequence>
            <xsl:for-each select="distinct-values($seq/@fulltype)">
                <xsl:sequence select="$seq[@fulltype eq current()][1]"/>
            </xsl:for-each>
        </xsl:sequence>
    </xsl:function>

    <xsl:template match="p[@class eq 'history']" mode="ixsl:onclick">
        <xsl:sequence select="js:goback()"/>
    </xsl:template>


    <xsl:template match="class|interface|enum" mode="show-class">
        <!-- Construct main page content for a javadoc class.
            May need additional package documents for superclasses, if comments are missing;
            so load these first. -->
        
        <xsl:variable name="requiredDocs" as="xs:string*">
            <xsl:if test="exists(methods/method[not(comment)])">
                <xsl:sequence select="(implements|extends)//interface/@fulltype/f:get-package-doc-name(.)"/>
            </xsl:if>
        </xsl:variable>
        
        <xsl:variable name="reqDocs" select="distinct-values($requiredDocs)"/>
        <!--<xsl:if test="exists($reqDocs)">
            <xsl:message>$reqDocs <xsl:value-of select="$reqDocs"/></xsl:message>
        </xsl:if>-->
        
        <xsl:choose>
            <xsl:when test="exists($reqDocs)">
                <ixsl:schedule-action document="{string-join($reqDocs, ' ')}">
                    <xsl:call-template name="show-class2"/>
                </ixsl:schedule-action>
            </xsl:when>
            <xsl:otherwise>
                <xsl:call-template name="show-class2"/>
            </xsl:otherwise>
        </xsl:choose>
        
    </xsl:template>
    
    <xsl:template match="delegate" mode="show-class">
        <!-- Delegates only appear in C# API. 
            Process these separately because they are sufficiently different to 
            Classes, Interfaces, etc. -->
        
        <xsl:result-document href="#main" method="ixsl:replace-content">
            
            <h3>
                <xsl:sequence select="f:addLink(ancestor::package/@id, ancestor::package/@id, 
                    'cslink')"/>
            </h3>
            <h1>
                <span class="classLabel">
                    <xsl:text>Delegate </xsl:text>
                </span>
                <xsl:value-of select="@id"/>
            </h1>
            
            <hr style="margin:8px;"/>
            
            <xsl:variable name="classid" select="@fullname"/>
            <xsl:sequence select="f:sig($classid, ., true())"/>
            
        </xsl:result-document>
        
    </xsl:template>
    
    <xsl:template name="show-class2">
        <!-- The context item is the class|interface|enum -->
        <xsl:variable name="isDotNetApiMember" select="/package/@id eq 'Saxon.Api'" as="xs:boolean"/>
        
        <xsl:result-document href="#main" method="ixsl:replace-content">
            
        <h3>
            <xsl:sequence select="f:addLink(ancestor::package/@id, ancestor::package/@id, 
                if ($isDotNetApiMember) then 'cslink' else 'javalink')"/>
        </h3>
        <xsl:variable name="isInterface" select="self::interface or exists(@interface)" as="xs:boolean"/>
        <h1>
            <span class="classLabel">
                <xsl:value-of
                    select="if ($isInterface) then 'Interface ' else if (self::enum or @superclass='Enum') then 'Enum ' else 'Class '"
                />
            </span>
            <xsl:value-of select="@id"/>
            <xsl:sequence select="f:flatParamTypes(.)"/>
        </h1>
        
            <xsl:if test="not($isInterface)">
            <ul class="extends">
                <li>
                    <xsl:choose>
                        <xsl:when test="$isDotNetApiMember">
                            <xsl:text>System.Object</xsl:text>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:text>java.lang.Object</xsl:text>
                        </xsl:otherwise>
                    </xsl:choose>
                    <xsl:text> </xsl:text>
                    <xsl:choose>
                        <xsl:when test="$isDotNetApiMember">
                            <xsl:choose>
                                <xsl:when test="empty(extends) or @superclass='object'">
                                    <ul>
                                        <li>
                                            <xsl:value-of select="concat(@fulltype, f:flatParamTypes(.))"/>
                                        </li>
                                    </ul>
                                </xsl:when>
                                <xsl:otherwise>
                                    <xsl:apply-templates select="(extends|extends//extends)[last()]" mode="extends"/>
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:apply-templates select="(extends|extends//extends)[last()]" mode="extends"/>
                            <xsl:if test="empty(extends)">
                                <ul>
                                    <li>
                                        <xsl:value-of select="concat(@fulltype, f:flatParamTypes(.))"/>
                                    </li>
                                </ul>
                            </xsl:if>
                        </xsl:otherwise>
                    </xsl:choose>
                </li>
            </ul>
        </xsl:if>
            
            <xsl:if test="exists(paramtags)">
                <dl>
                    <dt>Type Parameters</dt>
                    <xsl:for-each select="paramtags/tag">
                        <dd>
                            <code><xsl:value-of select="."/></code>
                            <xsl:if test="@comment">
                                <xsl:text> - </xsl:text>
                                <xsl:value-of select="@comment"/>
                            </xsl:if>
                        </dd>
                    </xsl:for-each>
                </dl>
            </xsl:if>

        <xsl:if test="exists(implements | extends/inherits/interface)">
            <!--<xsl:if test="implements/interface/@fulltype =
                extends/inherits/interface/@fulltype">
                <xsl:message>*** Duplicates to be eliminated </xsl:message>
            </xsl:if>-->
            <xsl:variable name="supers" select="implements/interface | extends/inherits/interface"/>
            <xsl:variable name="distinct-supers" select="if (implements/interface/@fulltype =
                extends/inherits/interface/@fulltype) then f:eliminate-duplicates($supers) else
                $supers"/>
            <dl>
                <dt>All <xsl:value-of
                    select="if ($isInterface) then ' Superinterfaces' else 'Implemented Interfaces '"
                    /></dt>
                <dd>
                    <xsl:for-each select="$distinct-supers">
                        <xsl:sequence select="f:showType(. , false() , false())"/>
                        <xsl:if test="position() ne last()">, </xsl:if>
                    </xsl:for-each>
                </dd>
            </dl>
        </xsl:if>
        
        <xsl:if test="exists(implemented-by)">
            <dl>
                <dt>All Known Implementing Classes</dt>
                <dd>
                    <xsl:for-each select="implemented-by/class">
                        <xsl:sequence select="f:addLink(@fulltype, @type, if ($isDotNetApiMember) then 'cslink' else 'javalink')"/>
                        <xsl:if test="position() ne last()">, </xsl:if>
                    </xsl:for-each>
                </dd>
            </dl>
        </xsl:if>
        
        <xsl:if test="not(parent::package)">
            <dl>
                <dt>Enclosing class</dt>
                <dd>
                    <xsl:sequence select="f:addLink(parent::class/@fulltype, parent::class/@id, if ($isDotNetApiMember) then 'cslink' else 'javalink')"/>
                </dd>
            </dl>
        </xsl:if>
        
        <xsl:if test="exists(subclasses)">
            <dl>
                <dt>Direct Known Subclasses</dt>
                <dd>
                    <xsl:for-each select="subclasses/class">
                        <xsl:sequence select="f:addLink(@fulltype, @type, if ($isDotNetApiMember) then 'cslink' else 'javalink')"/>
                        <xsl:if test="position() ne last()">, </xsl:if>
                    </xsl:for-each>
                </dd>
            </dl>
        </xsl:if>
        
        <hr style="margin:8px;"/>
        <p><code><xsl:value-of select="f:class-parts(.),' '"/></code>&#160;<span
            class="classLabel"><strong><xsl:value-of select="concat(@id, f:flatParamTypes(.))"/></strong></span>
            <xsl:for-each select="extends/interface">
                <br/>extends <xsl:sequence select="f:showType(., false() , false())"/>
            </xsl:for-each>
            <xsl:for-each select="implements/interface">
                <xsl:if test="position() = 1"><br/><xsl:value-of
                    select="if ($isInterface) then 'extends ' else 'implements '"/></xsl:if>
                <xsl:sequence select="f:showType(., false(), false())"/>
                <xsl:if test="position() ne last()">, </xsl:if>
            </xsl:for-each>
        </p>

            <div class="jcomments">
                <xsl:apply-templates select="comment/attribute[@name='@deprecated']" mode="jdc"/>
                <xsl:apply-templates select="(comment/body, comment/sentence)[1]" mode="jdc"/>
                
                <xsl:for-each select="comment/attribute[@name='@since']">
                    <p class="methodLabel">Since:</p>
                    <div class="params">
                        <xsl:apply-templates select="body" mode="jdc"/>
                    </div>
                </xsl:for-each>
                
                <xsl:if test="exists(comment/attribute[@name='@see'])">
                    <p class="methodLabel">See Also:</p>
                    <div class="params">
                        <xsl:for-each select="comment/attribute[@name='@see']">
                            <xsl:apply-templates select="." mode="jdc"/>
                            <xsl:if test="position() ne last()">, </xsl:if>
                        </xsl:for-each>
                    </div>
                </xsl:if>
            </div>
        <xsl:variable name="s-titles"
            select="('Nested Classes',
                     'Enum Constant Summary',
                     'Field Summary',
                     'Constructor Summary',
                     'Property Summary',
                     'Indexer Summary',
                     'Method Summary')"
            as="xs:string+"/>

        <xsl:variable name="this-class" select="."/>
        <xsl:variable name="classid" select="@fulltype"/>
        <xsl:for-each select="1 to 7">
            <xsl:variable name="i" select="." as="xs:integer"/>
            <xsl:variable name="members"
                select="if ($i eq 1) then $this-class/class
                        else if ($i eq 2) then $this-class/enumConstants/enumConstant
                        else if ($i eq 3) then $this-class/fields/field
                        else if ($i eq 4) then $this-class/methods/constructor
                        else if ($i eq 5) then $this-class/property
                        else if ($i eq 6) then $this-class/indexer
                        else $this-class/methods/method"
                        as="node()*"/>
            
            <xsl:variable name="inherited-fields" select="($i eq 3) and $this-class/(extends|implements)/interface//fields"/>
            <xsl:variable name="inherited-methods" select="($i eq 7) and $this-class/(extends|implements)/interface//methods"/>
            
            <xsl:if test="exists($members) or $inherited-fields or $inherited-methods">
                <table border="1" style="width:100%">
                    <thead>
                        <tr>
                            <td colspan="2" style="text-align:center">
                                <h3>
                                    <xsl:value-of select="$s-titles[$i]"/>
                                </h3>
                            </td>
                        </tr>
                    </thead>
                    <tbody>
                        <xsl:for-each select="$members">
                            <tr>
                                <xsl:if test="$i ne 2">
                                <td class="col-left">
                                    <p class="javaclassmember">
                                        <xsl:choose>
                                            <xsl:when test="$i eq 1">
                                                <xsl:value-of select="string-join((
                                                    if (@visibility eq 'public') then '' else string(@visibility),
                                                    if (@static) then 'static' else '',
                                                    if (@interface) then 'interface' else 'class'), ' ')" />
                                            </xsl:when>
                                            <xsl:when test="$i = (2,3)">
                                                <xsl:value-of select="f:modifiers(.)"/>
                                                <xsl:sequence select="f:showGenericType(.)"/>
                                            </xsl:when>
                                            <xsl:when test="$i eq 4">
                                                <xsl:value-of
                                                    select="if (@visibility eq 'public') then () else string(@visibility)"/>
                                            </xsl:when>
                                            <xsl:when test="$i eq 5">
                                                <xsl:value-of select="f:modifiers(.)"/>
                                                <xsl:sequence select="f:showPropertyType(.)"/>
                                            </xsl:when>
                                            <xsl:otherwise>
                                                <xsl:value-of select="f:modifiers(.)"/>
                                                <xsl:if test="paramtypes">
                                                    <xsl:sequence select="f:showParamTypes(.)"/>
                                                    <xsl:text> </xsl:text>
                                                </xsl:if>
                                                <xsl:sequence select="f:showGenericType(.)"/>
                                            </xsl:otherwise>
                                        </xsl:choose>
                                    </p>
                                </td>
                                </xsl:if>
                                <td class="col-right">
                                    <xsl:choose>
                                        <xsl:when test="$i = 1">
                                            <xsl:sequence select="f:showType(., true(), true())"/>
                                            <div class="fnormal">
                                                <xsl:apply-templates select="(comment/attribute[@name='@deprecated'],
                                                    comment/sentence)[1]" mode="jdc"/>
                                            </div>
                                        </xsl:when>
                                        <xsl:when test="$i = (2,3,5)">
                                            <span class="{if ($isDotNetApiMember) then 'cslink' else 'javalink'}" data-href="{$classid}@{@id}">
                                                <xsl:value-of select="@id"/>
                                            </span>
                                            <div class="fnormal">
                                                <xsl:apply-templates select="(comment/attribute[@name='@deprecated'],
                                                    comment/sentence)[1]" mode="jdc"/>
                                            </div>
                                        </xsl:when>
                                        <xsl:when test="$i eq 6">
                                            <span class="{if ($isDotNetApiMember) then 'cslink' else 'javalink'}" data-href="{$classid}@{@id}">
                                                <xsl:value-of select="@id"/>
                                            </span>
                                            <xsl:text> [</xsl:text>
                                            <xsl:for-each select="params/param">
                                                <xsl:sequence select="f:showGenericType(.)"/>
                                                <xsl:text> </xsl:text>
                                                <xsl:value-of select="@name"/>
                                                <xsl:if test="position() ne last()">
                                                    <xsl:text>, </xsl:text>
                                                    <br/>
                                                </xsl:if>
                                            </xsl:for-each>
                                            <xsl:text>]</xsl:text>
                                            <div class="fnormal">
                                                <xsl:apply-templates select="(comment/attribute[@name='@deprecated'],
                                                    comment/sentence)[1]" mode="jdc"/>
                                            </div>
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <xsl:sequence select="f:sig($classid, ., false())"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </td>
                            </tr>
                        </xsl:for-each>
                    </tbody>
                </table>
                
                <xsl:if test="$inherited-fields">
                    <xsl:apply-templates mode="inherited-fields" select="$this-class/extends/interface[//fields]">
                        <xsl:with-param name="isDotNetApiMember" select="$isDotNetApiMember"/>
                    </xsl:apply-templates>
                    <xsl:apply-templates mode="inherited-fields" select="$this-class/implements/interface[//fields]">
                        <xsl:with-param name="isDotNetApiMember" select="$isDotNetApiMember"/>
                    </xsl:apply-templates>
                </xsl:if>
                
                <xsl:if test="$inherited-methods">
                    <xsl:apply-templates mode="inherited-methods" select="$this-class/extends/interface[//methods]">
                        <xsl:with-param name="isDotNetApiMember" select="$isDotNetApiMember"/>
                    </xsl:apply-templates>
                    <xsl:apply-templates mode="inherited-methods" select="$this-class/implements/interface[//methods]">
                        <xsl:with-param name="isDotNetApiMember" select="$isDotNetApiMember"/>
                    </xsl:apply-templates>
                </xsl:if>
                
                <p>&#160;</p>
            </xsl:if>
        </xsl:for-each>
        
        <xsl:if test="exists($this-class/enumConstants/enumConstant)">
            <div class="section">
                <h2>Enum Constant Detail</h2>
                <xsl:apply-templates select="$this-class/enumConstants/enumConstant" mode="detail"/>
            </div>
        </xsl:if>

        <xsl:if test="exists($this-class/fields/field)">
            <div class="section">
                <h2>Field Detail</h2>
                <xsl:apply-templates select="$this-class/fields/field" mode="detail"/>
            </div>
        </xsl:if>

        <xsl:if test="exists($this-class/methods/constructor)">
            <div class="section">
                <h2>Constructor Detail</h2>

                <xsl:for-each select="$this-class/methods/constructor">
                    <div class="method">
                        <h3>
                            <xsl:value-of select="@id"/>
                        </h3>
                        <xsl:sequence select="f:sig($classid, ., true())"/>
                    </div>
                </xsl:for-each>
            </div>
        </xsl:if>
        
        <xsl:if test="exists($this-class/property)"><!-- .NET API only -->
            <div class="section">
                <h2>Property Detail</h2>
                <xsl:apply-templates select="$this-class/property" mode="detail"/>
            </div>
        </xsl:if>
            
            <xsl:if test="exists($this-class/indexer)"><!-- .NET API only -->
                <div class="section">
                    <h2>Indexer Detail</h2>
                    <xsl:apply-templates select="$this-class/indexer" mode="detail"/>
                </div>
            </xsl:if>

        <xsl:if test="exists($this-class/methods/method)">
            <div class="section">
                <h2>Method Detail</h2>
                <xsl:for-each select="$this-class/methods/method">
                    <div class="method">
                        <h3>
                            <xsl:value-of select="@id"/>
                        </h3>
                        <xsl:sequence select="f:sig($classid, ., true())"/>
                    </div>
                </xsl:for-each>
            </div>
        </xsl:if>
            
        </xsl:result-document>
    </xsl:template>
    
    <xsl:template match="interface" mode="inherited-fields">
        <xsl:param name="isDotNetApiMember"/>
        <xsl:variable name="this-superclass" select="@fulltype"/>
        <!--<xsl:message>Inherited fields <xsl:value-of select="$this-superclass"/></xsl:message>-->
        <xsl:if test="fields">
            <div>
                <p class="methodLabel">Fields inherited from <xsl:value-of select="if (name(parent::node()) eq
                    'extends') then ('class ') else 'interface '"/> <xsl:sequence select="f:showType(.,
                    true(), false())"/>:</p>
                <p>
                    <xsl:for-each select="fields/field">
                        <xsl:variable name="this-superclass-field" select="string(.)"/>
                        <xsl:choose>
                            <xsl:when test="f:isJavaType($this-superclass)">
                                <span class="{if ($isDotNetApiMember) then 'cslink' else 'javalink'}"
                                    data-href="{concat($this-superclass, '@', $this-superclass-field)}">
                                    <xsl:value-of select="$this-superclass-field"/>
                                </span>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="$this-superclass-field"/>
                            </xsl:otherwise>
                        </xsl:choose>
                        <xsl:if test="position() ne last()">
                            <xsl:text>, </xsl:text>
                        </xsl:if>
                    </xsl:for-each>
                </p>
            </div>
        </xsl:if>
        <xsl:apply-templates select="extends/interface" mode="inherited-fields"/>
        <xsl:apply-templates select="implements/interface" mode="inherited-fields"/>
    </xsl:template>
    
    <xsl:template match="interface" mode="inherited-methods">
        <xsl:param name="isDotNetApiMember"/>
        <xsl:variable name="this-superclass" select="@fulltype"/>
        <!--<xsl:message>Inherited methods <xsl:value-of select="$this-superclass"/></xsl:message>-->
        <xsl:if test="methods">
            <div>
                <p class="methodLabel">Methods inherited from <xsl:value-of select="if (name(parent::node()) eq
                    'extends') then ('class ') else 'interface '"/> <xsl:sequence select="f:showType(.,
                    true(), false())"/>:</p>
                <p>
                    <xsl:for-each select="methods/method">
                        <xsl:variable name="this-superclass-method"
                            select="substring-before(text(), '(')"/>
                        <xsl:choose>
                            <xsl:when test="f:isJavaType($this-superclass)">
                                <span class="{if ($isDotNetApiMember) then 'cslink' else 'javalink'}"
                                    data-href="{concat($this-superclass, '@', $this-superclass-method)}">
                                    <xsl:value-of select="$this-superclass-method"/>
                                </span>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="$this-superclass-method"/>
                            </xsl:otherwise>
                        </xsl:choose>
                        <xsl:if test="position() ne last()">
                            <xsl:text>, </xsl:text>
                        </xsl:if>
                    </xsl:for-each>
                </p>
            </div>
        </xsl:if>
        <xsl:apply-templates select="extends/interface" mode="inherited-methods"/>
        <xsl:apply-templates select="implements/interface" mode="inherited-methods"/>
    </xsl:template>
    
    <xsl:template match="enumConstant|field|property|indexer" mode="detail">
        <xsl:variable name="isProp" select="self::property"/><!--[@get or @set or @static]-->
        <xsl:variable name="isIndexer" select="self::indexer"/>
        <div class="method">
            <h3>
                <xsl:value-of select="@id"/>
            </h3>
            <p class="signature">
                <xsl:value-of select="f:class-parts(.),' '"/>
                <xsl:sequence select="if ($isProp) then f:showPropertyType(.) else f:showGenericType(.)"/>
                <xsl:text> </xsl:text>
                <xsl:value-of select="if ($isIndexer) then 'this' else @id"/>
                <xsl:if test="$isProp">
                    <xsl:value-of select="' {', if (@get eq 'true') then 'get;' else (), 
                        if (@set eq 'true') then 'set;' else (), 
                        if (@init eq 'true') then 'init;' else (), '}'"/>
                </xsl:if>
                <xsl:if test="$isIndexer">
                    <xsl:text>[</xsl:text>
                    <xsl:for-each select="params/param">
                        <xsl:sequence select="f:showGenericType(.)"/>
                        <xsl:text> </xsl:text>
                        <xsl:value-of select="@name"/>
                        <xsl:if test="position() ne last()">
                            <xsl:text>, </xsl:text>
                            <br/>
                        </xsl:if>
                    </xsl:for-each>
                    <xsl:text>]</xsl:text>
                </xsl:if>
            </p>
            <xsl:if test="@const or @constexpr">
                <p>Constant value: <strong><xsl:value-of select="(@constexpr, @const)[1]"/></strong></p>
            </xsl:if>
            <xsl:if test="exists(comment)">
                <div class="fcomment">
                    <xsl:apply-templates select="comment/attribute[@name='@deprecated']" mode="jdc"/>
                    <xsl:apply-templates select="(comment/body, comment/sentence)[1]" mode="jdc"/>
                </div>
            </xsl:if>
            <xsl:if test="exists(comment/value)">
                <p class="methodLabel">Value:</p>
                <div class="params">
                    <xsl:apply-templates select="comment/value/body" mode="jdc"/>
                </div>
            </xsl:if>
            <xsl:if test="exists(comment/return)">
                <p class="methodLabel">Returns:</p>
                <div class="params">
                    <xsl:apply-templates select="comment/return/body" mode="jdc"/>
                </div>
            </xsl:if>
            <xsl:if test="$isIndexer and exists(params)">
                <p class="methodLabel">Parameters:</p>
                <xsl:for-each select="params/param">
                    <div class="params">
                        <code><xsl:value-of select="@name"/></code>
                        <xsl:if test="body/div/node()">
                            <xsl:text> - </xsl:text>
                            <xsl:apply-templates select="body" mode="jdc"/>
                        </xsl:if>
                    </div>
                </xsl:for-each>
            </xsl:if>
            <xsl:if test="exceptions">
                <p class="methodLabel">Throws:</p>
                <xsl:for-each select="exceptions/exception">
                    <div class="params">
                        <xsl:sequence select="f:showType(., false(), true())"/>
                        <xsl:if test="exists(body|sentence)">
                            <xsl:text> - </xsl:text>
                            <xsl:apply-templates select="(body, sentence)[1]" mode="jdc"/>
                        </xsl:if>
                    </div>
                </xsl:for-each>
            </xsl:if>
            <xsl:for-each select="comment/attribute[@name='@since']">
                <p class="methodLabel">Since:</p>
                <div class="params">
                    <xsl:apply-templates select="body" mode="jdc"/>
                </div>
            </xsl:for-each>
            <xsl:if test="exists(comment/attribute[@name='@see'])">
                <p class="methodLabel">See Also:</p>
                <div class="params">
                    <xsl:for-each select="comment/attribute[@name='@see']">
                        <xsl:apply-templates select="." mode="jdc"/>
                        <xsl:if test="position() ne last()">, </xsl:if>
                    </xsl:for-each>
                </div>
            </xsl:if>
        </div>
    </xsl:template>

</xsl:transform>
