<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
	xmlns:c="http://www.w3.org/ns/xproc-step"
	xmlns:m="http://xylarium.org/ns/xml/documentation"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	expand-text="true"
	exclude-result-prefixes="#all"
	version="3.0">
	
	<xsl:strip-space elements="*"/>
	<xsl:output method="xml" indent="true" />
	
	<m:documentation>
		<m:note>
			<m:ol>
				<m:ingress>Expected inputs:</m:ingress>
				<m:items>
					<m:li>the output from <m:code>p:directory-list</m:code>.</m:li>
				</m:items>
			</m:ol>
		</m:note>
	</m:documentation>

	<xsl:template match="*[parent::*]/@xml:base">
		<xsl:attribute name="{name()}" select="replace(string-join(parent::*/ancestor::*/@xml:base, '/'), '//', '/')" />
	</xsl:template>
	
	<xsl:template match="node() | attribute()">
		<xsl:copy>
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>