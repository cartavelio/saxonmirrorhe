<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet 
	version="3.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:saxon="http://saxon.sf.net/"
	exclude-result-prefixes="#all">

	<xsl:template match="/">
	<xsl:variable name="language_fr" select="'fr'" />
	<xsl:variable name="language_en" select="'en'" />
	<html>
		<head></head>
		<body>
			<div>
				<xsl:apply-templates select="items">
					<xsl:with-param name="language" select="$language_fr" />
				</xsl:apply-templates>	
				<xsl:apply-templates select="items">
					<xsl:with-param name="language" select="$language_en" />
				</xsl:apply-templates>
			</div>
		</body>
	</html>
</xsl:template>

<xsl:template match="items">
	<xsl:param name="language" />
	<xsl:result-document href="{$language}/index.html" saxon:asynchronous="false">
	<html>
		<head></head>
		<body>
			<div>
				<ul>
					<xsl:apply-templates select="./item">
						<xsl:with-param name="language" select="$language" />
					</xsl:apply-templates>
				</ul>	
			</div>	
		</body>
	</html>
	</xsl:result-document>
</xsl:template>

<xsl:template match="item">
	<xsl:param name="language" />
	<li><a href="item_{./@id}.html"><xsl:value-of select="./title[@language = $language]" /></a></li>

	<xsl:result-document href="{$language}/item_{./@id}.html" saxon:asynchronous="true">
	<html>
		<head></head>
		<body>
			<div>
				<ul class="nav" pos="{position()}" of="{count(../item)}" last="{last()}">
					<xsl:variable name="pos" select="position()"/>
					<xsl:if test="$pos > 1">
						<li><a href="item_{../item[$pos - 1]/@id}.html">prev</a></li>
					</xsl:if>
					<xsl:if test="$pos &lt; last()">
						<li><a href="item_{../item[$pos + 1]/@id}.html">next</a></li>
					</xsl:if>	
				</ul>
			</div>
		</body>
	</html>
	</xsl:result-document>
</xsl:template>

</xsl:stylesheet>