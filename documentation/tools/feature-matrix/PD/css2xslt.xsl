<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="svg"
    version="3.0" xmlns:jwl="http://jwlresearch.net/" xmlns:pdf="java:net.jwl.Test" xmlns:svg="http://www.w3.org/2000/svg"
    xmlns:eg="http://example.com/saxon-extension" xmlns:sp="http://jwl/extension" xmlns:doc="http://jwlresearch.net/2012/doc"
    xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:lay="http://jwlresearch.net/2012/layout" xmlns:X="http://jwlresearch.net/2012/XSL"
    xmlns:mat="http://jwlresearch.net/2012/matrix" xmlns:mat2="http://jwlresearch.net/2012/matrix2" xmlns:vec="http://jwlresearch.net/2012/vector"
    xmlns:test="http://jwlresearch.net/2012/TEST" xmlns:math="http://exslt.org/math" xmlns:xlink="http://www.w3.org/1999/xlink"
    xmlns:H="http://www.w3.org/1999/xhtml" xmlns:saxon="http://saxon.sf.net/">

    <xsl:output method="xml" indent="yes"/>
    <xsl:namespace-alias stylesheet-prefix="X" result-prefix="xsl"/>

    <doc:synopsis title="CSS to XSLT conversion" xmlns="http://www.w3.org/1999/xhtml">
        <p>Produce an XSLT transform that should perform a lot of the CSS style generation on HTML.</p>
        <p>This should also be generalisable to support CSS on SVG as well.</p>
        <p>Note: this does not like comments in the CSS!</p>
    </doc:synopsis>

    <xsl:template name="CSS2XSLT1" doc:doc="Convert a CSS stylesheet into sort-of equivalent XSLT">
        <!--<xsl:param name="URI" as="xs:string"/>-->
        <xsl:call-template name="CSS2XSLT">
            <xsl:with-param name="URI" select="resolve-uri('saxondocs_new.css')"/>
        </xsl:call-template>
    </xsl:template>

    <xsl:variable name="apos" as="xs:string">'</xsl:variable>
    <xsl:function name="jwl:classPredicates" as="xs:string">
        <xsl:param name="in" as="xs:string"/>
        <xsl:variable name="classes" select="tokenize(substring-after($in, '.'), '\.')"/>
        <xsl:value-of select="string-join($classes ! ('[contains(@class,' || $apos || . || $apos || ')]'))"/>
    </xsl:function>

    <xsl:template name="CSS2XSLT" doc:doc="Convert a CSS stylesheet into sort-of equivalent XSLT">
        <xsl:param name="URI" as="xs:anyURI"/>
        <xsl:param name="stylesheet.attributes" as="attribute()*" doc:doc="additional attributes to add to the stylesheet"/>
        <xsl:variable name="src" select="replace(unparsed-text($URI), '/\*[^*]+\*/|&lt;!--[^-]*--&gt;', '')"/>


        <xsl:variable name="elementIdP">\w+#[\w\-]+</xsl:variable>
        <xsl:variable name="idP">#[\w\-]+</xsl:variable>
        <xsl:variable name="elementClassP">\w+\.[\w\-_\.]+</xsl:variable>
        <xsl:variable name="classP">\.[\w\-_\.]+</xsl:variable>
        <xsl:variable name="childP">\s*&gt;\s*</xsl:variable>
        <xsl:variable name="descendantP">\s+</xsl:variable>

        <xsl:variable name="nthChildEvenP">\w+:nth-child\(even\)</xsl:variable>
        <xsl:variable name="nthChildDigitP">\w+:nth-child\(\d+\)</xsl:variable>
        <xsl:variable name="actionP">\w+:(after|before)</xsl:variable>

        <xsl:variable name="regex"
            select="
                string-join((
                $elementIdP, $idP,
                $elementClassP, $classP,
                $nthChildEvenP, $nthChildDigitP, $actionP,
                $childP, $descendantP), '|')"/>



        <xsl:variable name="patterns" as="element()*">
            <xsl:for-each select="tokenize($src, '\}\s*')[matches(., '\S')]">
                <xsl:variable name="matches" select="normalize-space(substring-before(., '{'))"/>
                <xsl:variable name="properties" as="element()*">
                    <xsl:for-each select="tokenize(normalize-space(substring-after(., '{')), '\s*;\s*')[matches(., '\S')]">
                        <xsl:variable name="parts" select="tokenize(., '\s*:\s*')"/>
                        <X:attribute name="{$parts[1]}">
                            <xsl:value-of select="$parts[2]"/>
                            <!--<xsl:value-of select="if($parts[1] = 'content') then replace($parts[2],'&quot;','') else $parts[2]"/>-->
                        </X:attribute>
                    </xsl:for-each>
                </xsl:variable>


                <xsl:for-each select="tokenize($matches, '\s*,\s*')">
                    <xsl:variable name="steps" select="tokenize(., '\s+')"/>
                    <pattern match="{.}">
                        <xsl:analyze-string select="." regex="{$actionP}$">
                            <xsl:matching-substring>
                                <xsl:attribute name="action" select="substring-after(., ':')"/>
                            </xsl:matching-substring>
                        </xsl:analyze-string>
                        <xsl:attribute name="match">
                            <xsl:analyze-string select="." regex="{$regex}">
                                <xsl:matching-substring>
                                    <xsl:choose>
                                        <xsl:when test="matches(., $elementIdP)">
                                            <xsl:value-of
                                                select="substring-before(., '#') || '[@id=' || $apos || substring-after(., '#') || $apos || ']'"/>
                                        </xsl:when>
                                        <xsl:when test="matches(., $idP)">
                                            <xsl:value-of select="'*[@id=' || $apos || substring-after(., '#') || $apos || ']'"/>
                                        </xsl:when>
                                        <xsl:when test="matches(., $elementClassP)">
                                            <xsl:value-of select="substring-before(., '.') || jwl:classPredicates(.)"/>
                                        </xsl:when>
                                        <xsl:when test="matches(., $classP)">
                                            <xsl:value-of select="'*' || jwl:classPredicates(.)"/>
                                        </xsl:when>
                                        <xsl:when test="matches(., $nthChildEvenP)">
                                            <xsl:value-of select="substring-before(., ':') || '[position() mod 2 = 0]'"/>
                                        </xsl:when>
                                        <xsl:when test="matches(., $nthChildDigitP)">
                                            <xsl:value-of select="substring-before(., ':') || '[position() =' || substring-after(., 'child') || ']'"/>
                                        </xsl:when>
                                        <xsl:when test="matches(., $actionP)">
                                            <xsl:value-of select="substring-before(., ':')"/>
                                        </xsl:when>
                                        <xsl:when test="matches(., $childP)">/</xsl:when>
                                        <xsl:when test="matches(., $descendantP)">//</xsl:when>
                                    </xsl:choose>
                                </xsl:matching-substring>
                                <xsl:non-matching-substring>
                                    <xsl:value-of select="."/>
                                </xsl:non-matching-substring>
                            </xsl:analyze-string>
                        </xsl:attribute>
                        <xsl:sequence select="$properties"/>
                    </pattern>
                </xsl:for-each>
            </xsl:for-each>
        </xsl:variable>



        <!-- <X:stylesheet version="3.0" xpath-default-namespace="http://www.w3.org/1999/xhtml"-->
        <X:stylesheet version="3.0" xmlns:xs="http://www.w3.org/2001/XMLSchema">
            <!--  xpath-default-namespace="http://www.w3.org/2000/svg">-->
            <xsl:sequence select="$stylesheet.attributes"/>
            <X:template match="/">
                <X:apply-templates select="*" mode="body"/>
            </X:template>
            <X:template match="*">
                <X:apply-templates select="." mode="body"/>
            </X:template>

            <X:template match="*" mode="body">
                <X:param name="before"/>
                <X:param name="after"/>
                <X:copy>
                    <X:apply-templates select="@*" mode="#current"/>
                    <X:variable name="style-att" as="attribute()*">
                        <X:apply-templates select="." mode="css"/>
                    </X:variable>
                    <X:sequence select="$style-att"/>
                    <!-- By writing onto an element, we ensure that same-name attributes overwrite -->
                    <X:variable name="h" as="element()">
                        <H>
                            <X:sequence select="$style-att"/>
                        </H>
                    </X:variable>
                    <X:if test="exists($style-att)">
                        <X:attribute name="style" select="string-join(($h/@* ! (name() || ':' || .)), ';')"/>
                    </X:if>
                    <X:value-of select="$before"/>
                    <X:apply-templates select="* | text()" mode="#current">
                        <X:with-param name="text-transform" select="$h/@text-transform"/>
                    </X:apply-templates>
                    <X:value-of select="$after"/>
                </X:copy>
            </X:template>
            <X:template match="@*" mode="body">
                <X:copy-of select="."/>
            </X:template>
            <X:template match="text()" mode="body">
                <X:param name="text-transform"/>
                <X:choose>
                    <X:when test="$text-transform = 'uppercase'">
                        <X:value-of select="upper-case(.)"/>
                    </X:when>
                    <!--<X:when test="$text-transform = 'capitalizeXX'">
                        <X:value-of
                            select="string-join(tokenize(.,'\s+') ! 
                            (if(position() ne 1 and (matches(.,'^[A-Z]+$') or . = ('a','an','and','the','of','with'))) then . else (upper-case(substring(.,1,1))|| lower-case(substring(.,2))),
                            ' '))"
                        />
                    </X:when>-->
                    <X:when test="$text-transform = 'capitalize'">
                        <X:variable name="t" as="xs:string*">
                            <X:for-each select="tokenize(., '\s+')">
                                <X:choose>
                                    <X:when test="position() eq 1">
                                        <X:value-of select="."/>
                                    </X:when>
                                    <X:when test=". = ('a', 'an', 'and', 'the', 'of', 'with')">
                                        <X:value-of select="."/>
                                    </X:when>
                                    <X:otherwise>
                                        <X:variable name="parts" as="xs:string*">
                                            <X:analyze-string select="." regex="^([A-Z]+|.)">
                                                <X:matching-substring>
                                                    <X:value-of select="upper-case(.)"/>
                                                </X:matching-substring>
                                                <X:non-matching-substring>
                                                    <X:value-of select="lower-case(.)"/>
                                                </X:non-matching-substring>
                                            </X:analyze-string>
                                        </X:variable>
                                        <X:value-of select="string-join($parts, '')"/>
                                    </X:otherwise>
                                </X:choose>
                            </X:for-each>
                        </X:variable>
                        <X:value-of select="string-join($t, ' ')"/>
                        <!--<X:value-of
                            select="string-join(tokenize(.,'\s+') ! 
                            (if(position() ne 1 and (matches(.,'^[A-Z]+$') or . = ('a','an','and','the','of','with'))) then . else (upper-case(substring(.,1,1))|| lower-case(substring(.,2))),
                            ' '))"
                        />-->
                    </X:when>
                    <X:otherwise>
                        <X:copy-of select="."/>
                    </X:otherwise>
                </X:choose>

            </X:template>
            <X:template match="html | body" mode="body">
                <X:copy>
                    <X:sequence select="@*"/>
                    <X:apply-templates select="*" mode="#current"/>
                </X:copy>
            </X:template>
            <X:template match="head" mode="body">
                <X:copy>
                    <X:sequence select="* except link[@href]"/>
                </X:copy>
            </X:template>


            <X:mode name="css" on-no-match="deep-skip"/>
            <xsl:variable name="base-priority" select="1.5"/>

            <xsl:variable name="counter.resets" as="element()*"
                doc:doc="Collect all the patterns that invoke a counter reset. Note that this approach relies on the name of the counter being that of the element being tracked.">
                <xsl:for-each select="$patterns[@action = ('after', 'before')][xsl:attribute[@name = 'counter-reset']]">
                    <reset match="{@match}" counter="{xsl:attribute[@name='counter-reset']}"/>
                </xsl:for-each>
            </xsl:variable>
            <doc:patterns>
                <xsl:copy-of select="$patterns" copy-namespaces="no"/>
            </doc:patterns>
            <doc:counter.resets>
                <xsl:copy-of select="$counter.resets" copy-namespaces="no"/>
            </doc:counter.resets>

            <xsl:for-each select="$patterns[@action = ('after', 'before')]">
                <X:template priority="{$base-priority +0.001*position()}" mode="body">
                    <xsl:variable name="real-match" select="replace(@match, ':(after|before)$', '')"/>
                    <xsl:attribute name="match" select="$real-match"/>
                    <X:param name="{@action}" as="item()?" select="()"/>
                    <X:variable name="content" as="item()*">
                        <xsl:variable name="counterP">\s*counter\((\w+)\)\s*</xsl:variable>
                        <xsl:variable name="quotedSP">"[^"]*"</xsl:variable>
                        <xsl:analyze-string select="xsl:attribute[@name = 'content']" regex="{string-join(($counterP,$quotedSP),'|')}">
                            <xsl:matching-substring>
                                <xsl:choose>
                                    <xsl:when test="matches(., $counterP)">
                                        <xsl:variable name="counter" select="regex-group(1)"/>
                                        <!--<xsl:variable name="valid.reset.patterns"
                                            select="string-join($counter.resets[@counter=$counter]!('[preceding::'||string(@match)||']'),'')"/>
                                        <xsl:message select="'Counter reset for ',$counter,' :', $valid.reset.patterns"/>-->
                                        <xsl:variable name="resets.xpath"
                                            select="'(' || string-join($counter.resets[@counter = $counter] ! ('(preceding::' || string(@match) || ')'), '|') || ')[last()]'"/>
                                        <!-- <xsl:message select="'Counter pattern for ',$counter,' :', $resets.xpath"/>-->
                                        <!-- <X:variable name="reset"
                                            select="{string-join($counter.resets[@counter=$counter]!('(preceding::'||string(@match)||')'),'|')}"/>-->
                                        <X:variable name="last.reset" select="{$resets.xpath}"/>
                                        <X:value-of
                                            select="count(if(exists($last.reset)) then preceding::{$counter}[. >> $last.reset] else preceding::{$counter}) + {if($counter eq $real-match) then '1' else '0'}"/>

                                        <!--<X:number count="{$counter}{$valid.reset.patterns}" level="any"/>-->
                                    </xsl:when>
                                    <xsl:when test="matches(., $quotedSP)">
                                        <X:text>
                                            <xsl:value-of select="replace(., '&quot;', '')"/>
                                        </X:text>
                                    </xsl:when>
                                </xsl:choose>
                            </xsl:matching-substring>
                            <xsl:non-matching-substring>
                                <xsl:value-of select="."/>
                            </xsl:non-matching-substring>
                        </xsl:analyze-string>
                    </X:variable>
                    <X:next-match>
                        <X:with-param name="{@action}" select="(${@action},string-join($content))[1]"/>
                    </X:next-match>
                </X:template>
            </xsl:for-each>

            <X:template match="*" priority="{$base-priority}" mode="css"/>
            <xsl:for-each select="$patterns[not(@action)]">
                <X:template priority="{$base-priority+0.001*position()}" mode="css">
                    <xsl:sequence select="@match"/>
                    <X:next-match/>
                    <xsl:sequence select="* except xsl:attribute[@name = 'counter-reset']"/>
                </X:template>
            </xsl:for-each>
        </X:stylesheet>

    </xsl:template>

    <doc:doc xmlns="http://www.w3.org/1999/xhtml">
        <p>Apply a CSS stylesheet to a document using an XSLT intermediate.</p>
    </doc:doc>
    <xsl:function name="jwl:applyCSS" as="element()*">
        <xsl:param name="css.style" as="xs:anyURI" doc:doc="The location of the CSS stylesheet"/>
        <xsl:param name="doc" as="element()" doc:doc="The document to apply it to."/>
        <xsl:param name="stylesheet.attributes" as="attribute()*" doc:doc="Additional stylesheet attributes"/>
        <xsl:variable name="transform">
            <xsl:call-template name="CSS2XSLT">
                <xsl:with-param name="URI" select="$css.style"/>
                <xsl:with-param name="stylesheet.attributes" select="$stylesheet.attributes"/>
            </xsl:call-template>
        </xsl:variable>
        <!--<xsl:message select="serialize($transform)"/>-->
        <!--<xsl:variable name="compiled" select="saxon:compile-stylesheet($transform)"/>
        <xsl:sequence select="saxon:transform($compiled,$doc)/*"/>-->
        <xsl:variable name="result" select="transform(map{'stylesheet-node':$transform,'source-node': $doc})"/>
        <xsl:sequence select="$result?output/*"/>
    </xsl:function>
    <xsl:function name="jwl:applyCSS" as="element()*">
        <xsl:param name="css.style" as="xs:anyURI" doc:doc="The location of the CSS stylesheet"/>
        <xsl:param name="doc" as="element()" doc:doc="The document to apply it to."/>
        <xsl:sequence select="jwl:applyCSS($css.style, $doc, ())"/>
    </xsl:function>

    <xsl:function name="jwl:compileCSS">
        <xsl:param name="css.style" as="xs:anyURI" doc:doc="The location of the CSS stylesheet"/>
        <xsl:sequence select="jwl:compileCSS($css.style, ())"/>
    </xsl:function>
    <xsl:function name="jwl:compileCSS">
        <xsl:param name="css.style" as="xs:anyURI" doc:doc="The location of the CSS stylesheet"/>
        <xsl:param name="stylesheet.attributes" as="attribute()*" doc:doc="Additional stylesheet attributes"/>
        <xsl:variable name="transform">
            <xsl:call-template name="CSS2XSLT">
                <xsl:with-param name="URI" select="$css.style"/>
                <xsl:with-param name="stylesheet.attributes" select="$stylesheet.attributes"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:sequence select="saxon:compile-stylesheet($transform)"/>
    </xsl:function>
</xsl:stylesheet>
