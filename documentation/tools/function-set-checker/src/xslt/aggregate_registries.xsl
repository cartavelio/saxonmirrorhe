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
					<m:li>the output from <m:code>p:directory-list</m:code> with registry XML embedded in <m:code>c:file</m:code> elements.</m:li>
				</m:items>
			</m:ol>
		</m:note>
	</m:documentation>

	<xsl:template match="/">
		<registry id="aggregated">
			<xsl:apply-templates select="descendant::function">
				<xsl:sort select="(if (contains(@name, ':')) then substring-after(@name, ':') else @name) ! lower-case(.)" data-type="text" order="ascending" />
			</xsl:apply-templates>
		</registry>
	</xsl:template>
	
	<xsl:template match="function">
		<xsl:copy>
			<xsl:apply-templates select="attribute()" />
			<xsl:apply-templates select="parent::registry" />
			<xsl:apply-templates select="node()" />
		</xsl:copy>
	</xsl:template>
	
	<xsl:template match="registry">
		<xsl:attribute name="registry" select="@id" />
		<xsl:apply-templates select="@* except @id" />
	</xsl:template>
		
	<xsl:template match="registry/@*">
		<xsl:attribute name="registry-{local-name()}" select="." />
	</xsl:template>		
	
	<xsl:template match="node() | attribute()">
		<xsl:copy>
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>