<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
	xmlns:math="http://www.w3.org/2005/xpath-functions/math"
	xmlns:m="http://xylarium.org/ns/xml/documentation"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	expand-text="true"
	exclude-result-prefixes="#all"
	version="3.0">
	
	<xsl:strip-space elements="*"/>
	<xsl:output method="text" />
	
	
	<xsl:key name="function" match="function" use="@name" />

	<m:documentation>
		<m:note>
			<m:ol>
				<m:ingress>Expected inputs:</m:ingress>
				<m:items>
					<m:li>all_registered_functions.xml</m:li>
				</m:items>
			</m:ol>
		</m:note>
	</m:documentation>

	<xsl:variable name="line-feed" select="codepoints-to-string(10)" />

	<xsl:template match="/">		
						
		<xsl:text># All Registered Functions{$line-feed}{$line-feed}</xsl:text>
		<xsl:text>_Sorted in alphabetical order by local-name_{$line-feed}{$line-feed}</xsl:text>
		<xsl:for-each-group select="/*/function" group-by="@name">
			<xsl:sort select="(if (contains(@name, ':')) then substring-after(@name, ':') else @name) ! lower-case(.)" />
			<xsl:sort select="(if (contains(@name, ':')) then substring-before(@name, ':') else '') ! lower-case(.)" />
			<xsl:variable name="functions-in-group" select="count(current-group())" as="xs:integer" />
			<xsl:for-each select="current-group()">
				<xsl:sort select="@registry-name" data-type="text" order="ascending" />
				<xsl:choose>
					<xsl:when test="$functions-in-group > 1">
						<xsl:text>* `</xsl:text><xsl:value-of select="@name" /><xsl:text>` - _({@registry-name})_{$line-feed}</xsl:text>
					</xsl:when>
					<xsl:otherwise><xsl:text>* `</xsl:text><xsl:value-of select="@name" /><xsl:text>`{if (contains(@registry-name, 'Vendor')) then concat(' - _(', tokenize(@registry-name, ' ')[1], ')_') else ()}{$line-feed}</xsl:text></xsl:otherwise>
				</xsl:choose>			
			</xsl:for-each>
		</xsl:for-each-group>
		<xsl:text>{$line-feed}</xsl:text>
		<xsl:text>## Sources{$line-feed}{$line-feed}</xsl:text>
		<xsl:text>_Last pulled: {/*/function[1]/@registry-pulled}_{$line-feed}{$line-feed}</xsl:text>
		<xsl:for-each-group select="/*/function" group-by="@registry">
			<xsl:sort select="current-grouping-key()" data-type="text" order="ascending" />
			<xsl:text>* `{current-grouping-key()}`{$line-feed}</xsl:text>
		</xsl:for-each-group>
		
	</xsl:template>
	
	<xsl:template match="node() | attribute()">
		<xsl:copy copy-namespaces="no">
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>