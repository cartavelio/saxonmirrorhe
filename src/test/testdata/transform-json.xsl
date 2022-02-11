<?xml version="1.0"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	
	<xsl:template match=".">
		<data id="{?id}" name="{?name}" price="{?price}" tags="{?tags}"/>
	</xsl:template>
</xsl:stylesheet>