<!-- Stylesheet to be executed client-side to supply the navigation menu in each page -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:variable name="homepage" select="'../welcome/welcome.xml'"/>
  
  <xsl:variable name="contents-doc" select="document('../src/userdoc/contents.xml')"/>

  <xsl:template match="*">
    <xsl:copy>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>

  <!-- Used to insert Saxon version information - e.g. current-maintenance-release on download_page -->
  <xsl:template match="insert">
    <xsl:value-of select="document('../src/userdoc/contents.xml')
                            //constant[@name=current()/@name]/@value"/>
  </xsl:template>

  <!-- Used on Client testimonials page -->
  <xsl:template match="div[@style='multi-column']">
    <table class="client-table">
      <tr>
        <xsl:apply-templates/>
      </tr>
    </table>
  </xsl:template>
  <xsl:template match="div[@style='multi-column']/div">
    <td>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </td>
  </xsl:template>

  <!-- Don't copy over 'this-is' elements -->
  <xsl:template match="this-is"/>

  <!-- Ignore certain sections unless in correct mode -->
  <xsl:template match="div[@id='aside']"/>
  <xsl:template match="div[@id='fixed-aside']"/>
  <xsl:template match="div[@id='fullpage']"/>

  <!-- Make mail link - on Evaluation License page -->
  <xsl:template match="email">
    <a class="bodylink" href="mailto:{@address}">
      <xsl:value-of select="@title"/>
    </a>
  </xsl:template>


  <!-- Page formating - if window is smaller than fixed page width, then ensure whole page is scrollable
  i.e. no fixed top nav bar, etc. -->
  <xsl:template match="head">
    <head>
      <xsl:copy-of select="node()"/>
      <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js"/>
      <link href="//maxcdn.bootstrapcdn.com/font-awesome/4.2.0/css/font-awesome.min.css"
        rel="stylesheet"/>      
      <link rel="stylesheet" href="../saxondocs.css?v=1.7" type="text/css"></link>  

      <script type="text/javascript">
        function getWidth() {
        if (self.innerHeight) {
        return self.innerWidth;
        }
        
        if (document.documentElement &amp;&amp; document.documentElement.clientHeight) {
        return document.documentElement.clientWidth;
        }
        
        if (document.body) {
        return document.body.clientWidth;
        }
        }
       
       function header_fixed_or_not(){
        //window.alert(getWidth());       
        if (getWidth() &lt; $("#wrapper").width()){
        //document.getElementById("header-container").style.position = "static";
        //document.getElementById("content-container").style.padding = "12px 0 0 0";
        $("#header-container").css("position", "static");
        $("#header-container").css("height", "auto");
        $("#content-container").css("padding", "0 0 0 0");
        $("a.anchor").css("top", "-20px");
        //window.alert("window sizing - static");
        }
        else{
        //document.getElementById("header-container").style.position = "fixed";
        //document.getElementById("content-container").style.padding = "112px 0 0 0";
        $("#header-container").css("position", "fixed");
        $("#header-container").css("height", "110px");
        $("#content-container").css("padding", "110px 0 0 0");
        $("a.anchor").css("top", "-120px");
        //window.alert("window sizing - fixed");
        }        
        }
        
        function header_size(){
        var headerHeight = document.getElementById("header-nav-container").clientHeight;
        if (headerHeight &gt; 110 &amp;&amp; getWidth() &gt; $("#wrapper").width()){
        $("#content-container").css("padding-top", headerHeight);
        $("a.anchor").css("top", -headerHeight);
        //window.alert("header nav sizing");
        }
        }
      </script>

      <script type="text/javascript">
  var _gaq = _gaq || [];
  _gaq.push(["_setAccount", "UA-38837694-1"]);
  _gaq.push(["_trackPageview"]);

  (function() {
    var ga = document.createElement("script"); ga.type = "text/javascript"; ga.async = true;
    ga.src = ("https:" == document.location.protocol ? "https://ssl" : "http://www") +
    ".google-analytics.com/ga.js";
    var s = document.getElementsByTagName("script")[0]; s.parentNode.insertBefore(ga, s);
  })();
      </script>

    </head>
  </xsl:template>

  <xsl:template match="body">
    <body onload="header_fixed_or_not(), header_size()" onresize="header_fixed_or_not(), header_size()">
      <xsl:copy-of select="@*"/>
      <div id="wrapper">
        
          <div id="header-container">
            <div id="header-nav-container">
          <div id="header">
            <div class="header-saxonica-logo">
              <a href="{$homepage}">
                <img src="../img/logo_crop-mid-blue-background.gif" width="260" height="55"
                  border="0" hspace="10px" alt="saxonica.com"/>
              </a>
            </div>
            <div class="social-media">
              <a href="https://saxonica.plan.io/" title="Saxonica forum">
                <i class="fa fa-comments-o fa-2x"/>
              </a>
              <a href="mailto:saxon-orders@saxonica.com" title="Email us">
                <i class="fa fa-envelope-square fa-2x"/>
              </a>
              <a href="https://www.linkedin.com/company/saxonica-ltd" title="LinkedIn">
                <i class="fa fa-linkedin-square fa-2x"/>
              </a>
              <a href="https://twitter.com/michaelhkay" title="Michael Kay's twitter">
                <i class="fa fa-twitter-square fa-2x"/>
              </a>
              <a href="http://dev.saxonica.com/blog/" title="Saxonica blogs">
                <i class="fa fa-bullhorn fa-2x"/>
              </a>
            </div>            
          </div>
          <xsl:apply-templates mode="menu-area" select="."/>
          <!--<div style="clear:both"/>   -->         
            </div>
           <xsl:apply-templates mode="fixed-aside-area" select="./div[@id='fixed-aside']"/>
        </div>

        <div class="content-cont" id="content-container">
          <a id="top" class="anchor"/>
          <xsl:if test="not(./div[@id='fullpage']) and not(./div[@id='section'])">
            <div id="section">
              <xsl:apply-templates/>
            </div>
          </xsl:if>
          <xsl:apply-templates mode="page-content" select="./div[@id='fullpage']"/>
          <xsl:apply-templates mode="page-content" select="./div[@id='section']"/>
          <xsl:apply-templates mode="aside-content" select="./div[@id='aside']"/>
          <div id="footer">
            <p>&#169; 2020 Saxonica</p>
          </div>
        </div>

      </div>
    </body>
  </xsl:template>

  <xsl:template match="div[@id='section' or @id='fullpage']" mode="page-content">
    <xsl:copy>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="div[@id='aside']" mode="aside-content">
    <div id="aside" class="aside">
      <xsl:apply-templates/>
    </div>
  </xsl:template>

  <xsl:template match="div[@id='fixed-aside']" mode="fixed-aside-area">
    <div id="fixed-aside" class="aside">
      <xsl:apply-templates/>
    </div>
  </xsl:template> 
  
  <xsl:template match="div[@id='sitemap']">
    <xsl:param name="extension" select="'xml'"/>
    <table class="sitemap-table">
      <xsl:for-each select="$contents-doc/*/section | $contents-doc/*/home">
        <tr>
          <td class="col1">
            <ul>
              <li><a href="../{@folder}/{@name}.{$extension}" class="block-link"><xsl:value-of
              select="@title"/></a></li>
            </ul>
          </td>
          <td>
            <table>
              <tr>
                <xsl:for-each select="page">
                  <td>
                    <ul>
                      <li>
                        <xsl:if test="@name">
                          <a href="../{@folder}/{@name}.{$extension}">
                            <xsl:value-of select="@title"/>
                          </a>
                        </xsl:if>
                        <xsl:if test="@href">
                          <a href="{@href}">
                            <xsl:value-of select="@title"/>
                          </a>
                        </xsl:if>
                      </li>
                      <xsl:for-each select="subpage[@name != 'download_asp' and @name != 'php_api']">
                        <li>
                          <xsl:if test="@name">
                            <a href="../{@folder}/{@name}.{$extension}">
                              <xsl:value-of select="@title"/>
                            </a>
                          </xsl:if>
                          <xsl:if test="@href">
                            <a href="{@href}">
                              <xsl:value-of select="@title"/>
                            </a>
                          </xsl:if>
                        </li>
                      </xsl:for-each>
                    </ul>
                  </td>
                </xsl:for-each>
              </tr>
            </table>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>
  
  <xsl:template mode="menu-area" match="*">
    <xsl:param name="extension" select="'xml'"/>
    <xsl:variable name="this-page" select="(//this-is)[1]"/>
    <xsl:variable name="this-folder">
      <xsl:choose>
        <xsl:when test="$this-page/@folder">
          <xsl:value-of select="$this-page/@folder"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$this-page/@section"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <ul class="nav">
      <xsl:for-each select="$contents-doc/*/section">
        <xsl:choose>
          <xsl:when test="./@folder = 'sitemap'">
            <li class="float-right">
              <a href="../{@folder}/{@name}.{$extension}">
                <xsl:if test="./@folder = $this-page/@section">
                  <xsl:attribute name="id">thispage</xsl:attribute>
                </xsl:if>
                <xsl:attribute name="class">sitemap</xsl:attribute>
                <xsl:value-of select="@title"/>
              </a>
            </li>
          </xsl:when>
          <xsl:otherwise>
            <li>
              <a href="../{@folder}/{@name}.{$extension}">
                <xsl:if test="./@folder = $this-page/@section">
                  <xsl:attribute name="id">thispage</xsl:attribute>
                </xsl:if>
                <xsl:value-of select="@title"/>
              </a>
              <!-- drop down menus -->
              <ul>
                <xsl:for-each select="page">
                  <xsl:if test="@name">
                    <li>
                      <a href="../{@folder}/{@name}.{$extension}">
                        <xsl:value-of select="@title"/>
                      </a>
                    </li>
                  </xsl:if>
                  <xsl:if test="@href">
                    <li>
                      <a href="{@href}">
                        <xsl:value-of select="@title"/>
                      </a>
                    </li>
                  </xsl:if>
                </xsl:for-each>
              </ul>
            </li>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </ul>
    
    <div id="breadcrumbs-submenu-container">
    <div id="breadcrumbs">
      <xsl:variable name="place-in-contents" 
        select="$contents-doc//section[(@folder = $this-folder and @name = $this-page/@page and $this-page/@subpage='')]|
        $contents-doc//page[(@folder = $this-folder and @name = $this-page/@page and $this-page/@subpage='')]|        
        $contents-doc//subpage[(@folder = $this-folder and @name = $this-page/@subpage)]"/>
      <p>
        <a href="{$homepage}">SAXONICA</a>
        <xsl:text> > </xsl:text>
        <xsl:if test="$this-page/@section = 'welcome'">          
          <a href="{$homepage}">Welcome</a>
        </xsl:if>      
        <xsl:for-each select="$place-in-contents/ancestor-or-self::*[not(self::contents)]">
          <xsl:if test="self::section | self::page | self::subpage[not(@name = 'download_asp')]">
            <a href="../{@folder}/{@name}.{$extension}">
              <xsl:value-of select="@title"/></a>
          </xsl:if>
          <xsl:if test="self::subpage[@name = 'download_asp']">
            <xsl:value-of select="@title"/>
          </xsl:if>
          <xsl:if test="not(position()=last())">
            <xsl:text> > </xsl:text>
          </xsl:if>
        </xsl:for-each>      
      </p>
    </div>        
   
    <xsl:variable name="section-in-contents"
      select="$contents-doc/*/section[@folder = $this-page/@section]"/>  
    
    <div id="submenu">
      <xsl:choose>
        <xsl:when test="$this-page/@section = 'welcome'">
          <p>&#160;</p>
        </xsl:when>
        <xsl:otherwise>
          <p class="align-right">
            <a href="../{$section-in-contents/@folder}/{$section-in-contents/@name}.{$extension}">
              <xsl:if
                test="$section-in-contents/@name = $this-page/@page and $section-in-contents/@folder = $this-folder">
                <xsl:attribute name="id">thispage</xsl:attribute>
              </xsl:if>
              <xsl:value-of select="$section-in-contents/@title"/>
            </a>
            <xsl:for-each select="$section-in-contents/page">
              <xsl:value-of select="' | '"/>
              <xsl:if test="@name">
                <a href="../{@folder}/{@name}.{$extension}">
                  <xsl:if test="./@name = $this-page/@page and ./@folder = $this-folder">
                    <xsl:attribute name="id">thispage</xsl:attribute>
                  </xsl:if>
                  <xsl:value-of select="@title"/>
                </a>
              </xsl:if>
              <xsl:if test="@href">
                <a href="{@href}">
                  <xsl:value-of select="@title"/>
                </a>
              </xsl:if>
            </xsl:for-each>
          </p>
        </xsl:otherwise>
      </xsl:choose>      
    </div>
      <div style="clear:both;"/>
    </div>
  </xsl:template>

</xsl:stylesheet>
