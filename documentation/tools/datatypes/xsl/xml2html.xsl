<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
	xmlns:int="http://saxon.sf.net/xslt/functions/internal"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:math="http://www.w3.org/2005/xpath-functions/math"
	exclude-result-prefixes="xs math"
	expand-text="true"
	version="3.0">
	
	
	<xsl:strip-space elements="*"/>
	
	<xsl:output indent="true" 
		suppress-indentation="p" 
		omit-xml-declaration="true" />
	
	
	<xsl:key name="datatype" match="datatype" use="@id" />
	<xsl:key name="alter-egos" match="datatype" use="@ref" />
	
	
	<xsl:template match="/*">
		<xsl:variable name="grouped-sources" as="document-node()">
			<xsl:document>
				<sources>
					<xsl:for-each-group select="/datatypes/sources/source" group-by="tokenize(substring-after(@href, 'https://'), '#')[1]">
						<xsl:for-each select="current-group()">
							<xsl:sort select="lower-case(@href)" data-type="text" order="ascending" />
							<xsl:copy-of select="self::source" />
						</xsl:for-each>
					</xsl:for-each-group>
				</sources>
			</xsl:document>
		</xsl:variable>		
		<div>
			<table>
				<thead>
					<tr>
						<td>Name</td>
						<td>Description</td>
						<td>Category</td>
						<td>Parent type</td>
						<td>Sub-types</td>
						<td>Comparison family</td>
						<td>Notes</td>
					</tr>
				</thead>
				<tbody>
					<xsl:apply-templates select="descendant-or-self::datatype[@id]">
						<xsl:sort select="(if (contains(name, ':')) then substring-after(name, ':') else name) ! lower-case(.)" data-type="text" order="ascending" />
						<xsl:with-param name="all-sources" select="$grouped-sources" as="document-node()" tunnel="true" />
					</xsl:apply-templates>
				</tbody>
			</table>		
			<xsl:if test="count($grouped-sources/sources/source) > 0">
				<div>
					<p class="heading">Sources</p>
					<ol>
						<xsl:for-each select="$grouped-sources/sources/source">
							<li id="{int:format-source-id(@id)}"><a href="{@href}">{@href}</a></li>
						</xsl:for-each>
					</ol>
				</div>
			</xsl:if>
		</div>
	</xsl:template>
	
	
	<xsl:template match="datatype">
		<tr id="{int:format-datatype-id(@id)}">
			<xsl:if test="@ref">
				<xsl:attribute name="class" select="'aka'" />
			</xsl:if>
			<td class="name">
				<xsl:apply-templates select="name[1]" />
			</td>
			<td class="description">
				<xsl:apply-templates select="@ref, description" />
			</td>
			<td class="category">
				<xsl:apply-templates select="category" />
			</td>
			<td class="parent-type">
				<xsl:if test="not(@ref)">
					<xsl:apply-templates select="ancestor::datatype[1]" mode="base-type" />
				</xsl:if>
			</td>
			<td class="sub-types">
				<xsl:apply-templates select="sub-types" />
			</td>
			<td class="comparison-family">
				<xsl:apply-templates select="comparison-family" />
			</td>
			<td class="notes">
				<xsl:call-template name="first-introduced" />
				<xsl:apply-templates select="notes" />
				<xsl:apply-templates select="sources" />
			</td>
		</tr>
	</xsl:template>
	
	
	<xsl:template match="datatype/*[not(local-name() = ('category', 'sources'))]" priority="100" mode="#default">
		<xsl:choose>
			<xsl:when test="normalize-space(.) = '' and not(self::notes and following-sibling::sources/source)"> </xsl:when>
			<xsl:when test="self::notes">
				<xsl:next-match />
			</xsl:when>
			<xsl:otherwise>
				<p><xsl:next-match /></p>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	
	<xsl:template match="@ref">
		<p>An alternative name for <a href="#{int:format-datatype-id(.)}" class="bodylink"><code><xsl:value-of select="key('datatype', .)/name" /></code></a>.</p>
	</xsl:template>
	
	
	<xsl:template match="datatype/description">
		<xsl:apply-templates />
		
		<xsl:variable name="alter-egos" select="key('alter-egos', parent::datatype/@id)" as="element()*" />
		<xsl:if test="count($alter-egos) > 0">
			<p class="aka">Also known as: <xsl:for-each select="$alter-egos"><xsl:apply-templates select="self::datatype[@ref]" mode="aka" />{if (position() != last()) then ', ' else ()}</xsl:for-each></p>
		</xsl:if>
	</xsl:template>
	

	
	
	<xsl:template match="datatype/category">
		<ul>
			<xsl:for-each-group select="type" group-by="tokenize(@name, ' \(')[1]">
				<xsl:variable name="specs" select="current-group()/@name/tokenize(., ' \(')[2] ! translate(., ' )', '')" as="xs:string*" />
				<li>{current-grouping-key()}{if (count($specs) > 0) then ' (' else ()}<xsl:for-each select="$specs"><span class="spec">{.}</span>{if (position() != last()) then ', ' else ()}</xsl:for-each>{if (count($specs) > 0) then ')' else ()}</li>
			</xsl:for-each-group>			
		</ul>
	</xsl:template>
	
	
	<xsl:template match="datatype/sub-types">
		<xsl:apply-templates select="datatype[not(@ref)]" mode="sub-type" />
	</xsl:template>
	

	<xsl:template match="datatype" mode="base-type">
		<p><xsl:apply-templates select="name[1]" mode="xref"/></p>
	</xsl:template>
	
	<xsl:template match="datatype" mode="sub-type">
		<xsl:apply-templates select="name[1]" mode="xref" />
		<xsl:if test="following-sibling::datatype">, </xsl:if>
	</xsl:template>
	
	<xsl:template match="datatype" mode="aka">
		<xsl:apply-templates select="name[1]" mode="xref"/>
	</xsl:template>
	
	
	<xsl:template match="datatype/name" mode="xref" priority="150">		
		<a href="#{int:format-datatype-id(parent::datatype/@id)}" class="bodylink{if (int:is-new(parent::datatype) = true()) then ' qt4' else ()}"><xsl:next-match /></a>
	</xsl:template>
	
	
	<xsl:template match="datatype/name" mode="#default" priority="50">
		<xsl:if test="int:is-new(parent::datatype) = true()">
			<xsl:attribute name="class" select="'qt4'" />
		</xsl:if>
		<xsl:next-match />
	</xsl:template>
	
	<xsl:template match="datatype/name" mode="#all">
		<code><xsl:apply-templates /></code>
	</xsl:template>
	
	
	<xsl:template name="first-introduced">		
		<xsl:if test="self::datatype/@first-introduced">
			<p class="intro{if (int:is-new(self::datatype) = true()) then ' qt4' else ()}">Introduced in <xsl:value-of select="self::datatype/@first-introduced" />.</p>
		</xsl:if>
	</xsl:template>
	
	
	<xsl:template match="datatype/comparison-family">
		<xsl:apply-templates />
	</xsl:template>
	
	
	<xsl:template match="datatype/notes">
		<xsl:apply-templates />
	</xsl:template>
	
	<xsl:template match="datatype/sources">
		<xsl:param name="all-sources" as="document-node()" tunnel="true" />
		
		<p class="sources">Sources: <xsl:apply-templates><xsl:sort select="int:get-source-position($all-sources, @ref)" data-type="number" order="ascending" /></xsl:apply-templates></p>
	</xsl:template>
	
	
	<xsl:template match="datatype/sources/source">
		<xsl:param name="all-sources" as="document-node()" tunnel="true" />
		<xsl:variable name="position" select="int:get-source-position($all-sources, @ref)" as="xs:integer" />
		
		<a href="#{int:format-source-id(@ref)}" class="bodylink">[{$position}]</a>
		<xsl:if test="position() != last()">, </xsl:if>
	</xsl:template>
	
	
	<xsl:template match="xref">
		<a href="#{int:format-datatype-id(@ref)}" class="bodylink"><code>{key('datatype', @ref)/name}</code></a>
	</xsl:template>
	
	
	<xsl:template match="processing-instruction()" priority="1" />
	
	
	<xsl:template match="node() | attribute()">
		<xsl:copy>
			<xsl:apply-templates select="@*, node()" />
		</xsl:copy>
	</xsl:template>
	
	
	<xsl:function name="int:is-new" as="xs:boolean">
		<xsl:param name="datatype" as="element()" required="yes" />
		
		<xsl:sequence select="if ($datatype/@first-introduced = 'XDM 4.0') then true() else false()" />
	</xsl:function>
	
	
	<xsl:function name="int:get-source-position" as="xs:integer">
		<xsl:param name="sources" as="document-node()" required="yes" />
		<xsl:param name="id" as="xs:string" required="yes" />
		
		<xsl:sequence select="count($sources/sources/source[@id = $id]/preceding-sibling::source) + 1" />
	</xsl:function>
	
	
	<xsl:function name="int:format-datatype-id" as="xs:string">
		<xsl:param name="id-in" as="xs:string" required="yes" />
		
		<xsl:variable name="id-out" select="translate($id-in, ':', '-') ! lower-case(.)" as="xs:string" />
		
		<xsl:text>datatype-{$id-out}</xsl:text>
	</xsl:function>
		
		
	<xsl:function name="int:format-source-id" as="xs:string">
		<xsl:param name="id-in" as="xs:string" required="yes" />
		
		<xsl:variable name="id-out" select="lower-case($id-in)" as="xs:string" />
		
		<xsl:text>datatype-source-{$id-out}</xsl:text>
	</xsl:function>	
	
</xsl:stylesheet>