<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="myfunctions"
    xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:l="LINENUMBER"
    exclude-result-prefixes="xs l" version="2.0">

    <xsl:function name="f:truncate-string" as="xs:string">
        <xsl:param name="in" as="xs:string"/>
        <xsl:param name="truncate" as="xs:integer"/>
        <xsl:value-of
            select="if(string-length($in) gt $truncate) then concat(substring($in,1,$truncate),'...') else $in"
        />
    </xsl:function>

    <xsl:variable name="no-LineNo" as="attribute()" doc:doc="Blank line number">
        <xsl:attribute name="l:no">0</xsl:attribute>
    </xsl:variable>
    <xsl:template match="*" mode="serialize">
        <xsl:param name="text-on-newline" as="xs:boolean" select="false()" tunnel="yes"/>
        <xsl:param name="indent" select="''" tunnel="yes"/>
        <xsl:param name="indent.by" select="'  '" tunnel="yes"/>
        <xsl:param name="indent.newline" as="xs:string" select="'    '" tunnel="yes"/>
        <xsl:param name="truncate" as="xs:integer" select="70" tunnel="yes"/>
        <xsl:param name="element.class">XMLelem</xsl:param>
        <xsl:param name="body.class">XMLbody</xsl:param>
        <xsl:param name="name" select="concat('&lt;',name())" as="xs:string"/>
        <xsl:param name="folder" as="element()?" select="()" doc:doc="Any information for folding/expanding/collapsing"/>
        <xsl:param name="href" as="attribute()*" select="()" tunnel="yes"/>
        <xsl:param name="after-head" as="item()*" select="()" tunnel="yes"
            doc:doc="Information to place after the element head"/>
        <xsl:param name="line-no" as="element()?" select="()" tunnel="yes"/>

        <xsl:variable name="parts" as="item()*" expand-text="yes">
            <xsl:apply-templates select="(@l:no,$no-LineNo)[1]" mode="#current"/><xsl:sequence
                select="$folder"/>{$indent}<span class="{$element.class}"><xsl:choose><xsl:when
                        test="$href"><a><xsl:sequence select="$href,$name"
                        /></a></xsl:when><xsl:otherwise>{$name}</xsl:otherwise></xsl:choose></span>
            <xsl:apply-templates select="@* except @l:no" mode="#current">
                <xsl:with-param name="href" select="()" tunnel="yes"/>
            </xsl:apply-templates>
        </xsl:variable>
        <xsl:iterate select="$parts"
            use-when="contains(system-property('xsl:product-version'),'9.')">
            <xsl:param name="column" select="string-length($indent)"/>
            <xsl:variable name="next-column" select="$column + string-length(.)"/>
            <xsl:variable name="new-line" select="$next-column gt $truncate"/>
            <!--<xsl:value-of select="'nc',$next-column,$new-line"/>-->
            <xsl:if test="$new-line">
                <xsl:value-of select="'&#xA;',$indent,$indent.newline"/>
            </xsl:if>
            <xsl:sequence select="."/>
            <xsl:next-iteration>
                <xsl:with-param name="column"
                    select="if($new-line) then string-length($indent) + string-length($indent.newline) + string-length(.) else $next-column"
                />
            </xsl:next-iteration>
        </xsl:iterate>
        <xsl:sequence select="$parts" use-when="system-property('xsl:product-name')='Saxon-CE'"/>
        <xsl:sequence select="$after-head"/>
        <xsl:text>&gt;</xsl:text>
        <!--<xsl:value-of select="concat(string-join($attributes,''),'&gt;')"/>-->
        <xsl:variable name="just-text" select="text()[matches(.,'\S')] and empty(*)"/>
        <xsl:if test="$just-text">
            <xsl:if test="$text-on-newline">
                <xsl:value-of select="'&#xA;',$indent"/>
            </xsl:if>
            <xsl:apply-templates select="text()" mode="#current"/>
        </xsl:if>
        <xsl:value-of select="'&#xA;'"/>
        <xsl:if test="exists(*|text()[not($just-text)])">
            <span class="{$body.class}">
                <xsl:apply-templates select="*|text()[not($just-text)]" mode="#current">
                    <xsl:with-param name="indent" select="concat($indent,$indent.by)" tunnel="yes"/>
                    <xsl:with-param name="href" select="()" tunnel="yes"/>
                    <xsl:with-param name="after-head" select="()" tunnel="yes"/>
                    <xsl:with-param name="line-no" select="()" tunnel="yes"/>
                </xsl:apply-templates>
            </span>
        </xsl:if>
    </xsl:template>

    <xsl:template match="@l:no" mode="serialize" expand-text="yes">
        <span class="lineNumber">
            <xsl:analyze-string select="format-number(.,'000')" regex="^0+">
                <xsl:matching-substring>
                    <xsl:value-of select="for $i in 1 to string-length(.) return ' '" separator=""/>
                </xsl:matching-substring>
                <xsl:non-matching-substring>{.}</xsl:non-matching-substring>
            </xsl:analyze-string>
        </span>
    </xsl:template>

    <xsl:template match="text()" mode="serialize">
        <xsl:param name="truncate.before" as="xs:integer" select="300" tunnel="yes"/>
        <xsl:if test="matches(.,'\S')">
            <span class="XMLtext">
                <xsl:value-of select="f:truncate-string(.,$truncate.before)"/>
            </span>
        </xsl:if>
    </xsl:template>

    <xsl:template match="@*" mode="serialize">
        <xsl:param name="truncate.attributes" select="20" tunnel="yes"/>
        <xsl:param name="normalize.attributes" select="false()" as="xs:boolean" tunnel="yes"/>
        <xsl:param name="attribute.class">XMLatt</xsl:param>
        <xsl:param name="indent" select="''" tunnel="yes"/>
        <xsl:param name="attributes" as="attribute()*" select="()"/>
        <xsl:variable name="s"
            select="f:truncate-string(if($normalize.attributes) then normalize-space(.) else .,
                    $truncate.attributes)"/>
        <xsl:variable name="name" select="concat(' ',name(),'=')"/>
        <span class="{$attribute.class}">
            <xsl:sequence select="$attributes"/>
            <xsl:value-of select="$name"/>
            <span class="XMLquot">
                <xsl:text>"</xsl:text>
                <xsl:analyze-string select="$s" regex="\s+/\s*|\s*/\s+">
                    <xsl:matching-substring>/&#xA;</xsl:matching-substring>
                    <xsl:non-matching-substring>
                        <xsl:value-of
                            select="if(position() eq 1) then ()
              else ($indent,string-join(for $i in 1 to string-length($name) return ' ','')),
              ."
                            separator=""/>
                    </xsl:non-matching-substring>
                </xsl:analyze-string>
                <xsl:text>"</xsl:text>
            </span>
        </span>
    </xsl:template>
</xsl:transform>
