<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="xs"
    version="2.0">
    
    <!-- Stylesheet to modify the schema for configuration files (config.xsd) with
         documentation from the master FeatureKeys.xml documentation.
         
         The stylesheet also checks the schema for consistency with FeatureKeys.xml;
         the set of attributes defined in both should agree
    -->
        
    <xsl:param name="FeatureKeys" required="yes"/>
    <xsl:param name="AdditionalFeatures" required="yes"/>
    <xsl:param name="ConfigSchema" required="yes"/>
    
    <xsl:template match="*">
        <xsl:copy>
            <xsl:copy-of select="@*"/>
            <xsl:apply-templates/>
        </xsl:copy>
    </xsl:template>
    
    <xsl:template name="main">
        <xsl:apply-templates select="doc($ConfigSchema)"/>
        <xsl:call-template name="consistency-check"/>
    </xsl:template>
    
    <xsl:variable name="schema-exceptions" as="xs:string*" select="('configuration/@edition', 'configuration/@label')"/>
    
    <xsl:template match="xs:attribute">
        <xsl:variable name="key" select="concat(ancestor::xs:element/@name, '/@', @name)"/>
        <xsl:variable name="feature" select="(doc($FeatureKeys)//feature[configfile=$key], doc($AdditionalFeatures)//feature[configfile=$key])"/>
        <xsl:if test="empty($feature) and not($key = $schema-exceptions)">
            <xsl:message>*** Schema entry not found in FeatureKeys <xsl:value-of select="$key"/></xsl:message>
        </xsl:if>
        <xsl:if test="empty($feature) and empty(xs:annotation/xs:documentation)">
            <xsl:message>*** No documentation for attribute <xsl:value-of select="$key"/></xsl:message>
        </xsl:if>
        <xsl:copy>
            <xsl:copy-of select="@*"/>
            <xs:annotation>
                <xs:documentation>
                    <xsl:copy-of select="if (xs:annotation/xs:documentation) then
                        xs:annotation/xs:documentation/node() else 
                        if (exists($feature/configfiledesc)) 
                        then $feature/configfiledesc/* 
                        else $feature/desc/*"/>
                </xs:documentation>
            </xs:annotation>
        </xsl:copy>
    </xsl:template>
    
    <xsl:variable name="exceptions" as="xs:string*" select="('COLLATION_URI_RESOLVER',
        'COLLECTION_FINDER', 'COLLECTION_URI_RESOLVER', 'CONFIGURATION', 'CONFIGURATION_FILE',
        'ENVIRONMENT_VARIABLE_RESOLVER', 'EXPATH_FILE_DELETE_TEMPORARY_FILES',
        'MODULE_URI_RESOLVER', 'NAME_POOL', 'OUTPUT_URI_RESOLVER', 'RECOVERY_POLICY', 'RESOURCE_RESOLVER',
        'RETAIN_DTD_ATTRIBUTE_TYPES', 'SCHEMA_URI_RESOLVER', 'SCHEMA_VALIDATION', 'TRACE_LISTENER',
        'TREE_MODEL', 'UNPARSED_TEXT_URI_RESOLVER', 'XML_PARSER_FEATURE', 'XML_PARSER_PROPERTY',
        'COMMON_RESOURCE_RESOLVER')"/>
    
    <xsl:template name="consistency-check">
        <xsl:for-each select="doc($FeatureKeys)//feature[not(configfile) and not(string(name) = $exceptions)]">
            <xsl:message>*** Feature not definable in configuration file: <xsl:value-of select="name"/></xsl:message>
            <!-- Either the feature should be added to the exceptions list above, or added to the
                configuration file: 
                    * add configfile element in FeatureKeys.xml
                    * update ConfigurationReader.java code
            -->
        </xsl:for-each>
        <xsl:for-each select="doc($FeatureKeys)//feature/configfile">
            <xsl:variable name="key" select="text()"/>
            <xsl:variable name="keyElement" select="substring-before($key, '/@')"/>
            <xsl:variable name="keyAttribute" select="substring-after($key, '/@')"/>
            <xsl:variable name="ConfigAtt" select="doc($ConfigSchema)//xs:element[@name eq $keyElement]//xs:attribute[@name eq $keyAttribute]"/>
            <xsl:if test="empty($ConfigAtt)">
                <xsl:message>*** Feature not included in schema: <xsl:value-of select="$key"/></xsl:message>
                <!-- Update the schema config.xsd -->
            </xsl:if>
        </xsl:for-each>
    </xsl:template>
    
    <xsl:strip-space elements="*"/>
    <xsl:output indent="yes"/>
    
</xsl:stylesheet>
