<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
	
	<xsl:output omit-xml-declaration="true" />
	
	<xsl:import href="happy_path_dep.xsl" />
	
	<xsl:template match="/">
		<out><xsl:text>Transformation completed successfully using happy-path</xsl:text><xsl:apply-templates /></out>
	</xsl:template>
	
</xsl:stylesheet>