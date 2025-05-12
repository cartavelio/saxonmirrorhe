<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0"
   xmlns="http://www.w3.org/2010/09/qt-fots-catalog"
   xpath-default-namespace="http://www.w3.org/xpath-functions/spec/namespace"
   expand-text="yes">
   
   <!-- Generate Java code to generate a data structure holding argument keywords for built-in functions -->
   
   <!-- Run against the function-catalog.xml source document -->
   
   <xsl:output method="text" indent="yes"/>
   <xsl:strip-space elements="*"/>
   
   <xsl:template match="/" expand-text="no">
    public final static Map&lt;String, String> fnParamNames = new HashMap&lt;>();
    public final static Map&lt;String, String> mathParamNames = new HashMap&lt;>();
    public final static Map&lt;String, String> mapParamNames = new HashMap&lt;>();
    public final static Map&lt;String, String> arrayParamNames = new HashMap&lt;>();
    
    // ==== DO NOT EDIT ====
    // Following code is generated from the function catalog using generate-keyword-declarations.xsl.
    // ==== DO NOT EDIT ====
    
    static {
         <xsl:apply-templates select="//function[not(@prefix='op')]">
            <xsl:sort select="(@prefix, 'fn')[1]"/>
            <xsl:sort select="@name"/>
         </xsl:apply-templates>
    }      
   </xsl:template>
   
   <xsl:template match="function">
        {(@prefix,'fn')[1]}ParamNames.put("{@name}", "{string-join(signatures/proto[last()]/arg/@name, '|')}");</xsl:template>
   
</xsl:stylesheet>