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
	
	<xsl:param name="catalog-dir" select="resolve-uri('../../data/catalogs/')" as="xs:anyURI" />
	
	<xsl:key name="function" match="function" use="@name" />

	<m:documentation>
		<m:note>
			<m:ol>
				<m:ingress>Expected inputs:</m:ingress>
				<m:items>
					<m:li>qtspecs/specifications/xpath-functions-40/src/function-catalog.xml</m:li>
					<m:li>data/implemented/*/XPath40FunctionSet.xml</m:li>
				</m:items>
			</m:ol>
		</m:note>
	</m:documentation>

	<xsl:variable name="line-feed" select="codepoints-to-string(10)" />

	<xsl:template match="/">		
		
		<xsl:choose>
			<xsl:when test="not(doc-available(resolve-uri(concat($catalog-dir, /registry/@catalog, '.function-catalog.simplified.xml'))))">
				<xsl:text>Catalog not found.</xsl:text>
			</xsl:when>
			<xsl:otherwise>

				<xsl:variable name="catalog" select="document(resolve-uri(concat($catalog-dir, /registry/@catalog, '.function-catalog.simplified.xml')))" as="document-node()" />		
				<xsl:variable name="functions-implemented-and-still-in-spec" select="registry/function[@name = $catalog/catalog/function/@name]" as="element()*" />		
				<xsl:variable name="functions-implemented-but-no-longer-in-spec" select="registry/function[@name[not(. = $catalog/catalog/function/@name)]]" as="element()*" />
				
				
				<xsl:text># {registry/@name} Checklist{$line-feed}{$line-feed}</xsl:text>
				<xsl:text>Below is a list of the functions registered in `{registry/@id}`{$line-feed}{$line-feed}</xsl:text>
				<xsl:text>_Last checked: {registry/@pulled}_{$line-feed}{$line-feed}</xsl:text>
				<xsl:text>## Still in 4.0{$line-feed}{$line-feed}</xsl:text>
				<xsl:for-each select="distinct-values($functions-implemented-and-still-in-spec/@name)">
					<xsl:sort select="." data-type="text" order="ascending" />
					<xsl:text>* `</xsl:text><xsl:value-of select="." /><xsl:text>`{$line-feed}</xsl:text>
				</xsl:for-each>
				<xsl:text>{$line-feed}</xsl:text>
				<xsl:text>## Not in 4.0{$line-feed}{$line-feed}</xsl:text>
				<xsl:for-each select="distinct-values($functions-implemented-but-no-longer-in-spec/@name)">
					<xsl:sort select="." data-type="text" order="ascending" />
					<xsl:text>* `</xsl:text><xsl:value-of select="." /><xsl:text>`{$line-feed}</xsl:text>
				</xsl:for-each>
				
			</xsl:otherwise>
		</xsl:choose>		
		
	</xsl:template>
	
	<xsl:template match="node() | attribute()">
		<xsl:copy copy-namespaces="no">
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>