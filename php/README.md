README document for SaxonC PHP extension:

The PHP extension is in the form of a C/C++ PHP8 extension to SaxonC created using the Zend module API.

SaxonC provides processing in XSLT 3.0, XQuery 3.1 and
XPath 3.1, and Schema validation 1.0/1.1.

This is a library with APIs for PHP. The main
classes in the SaxonC PHP API are as follows: SaxonProcessor, Xslt30Processor,
XsltExecutable, XQueryProcessor, XPathProcessor, SchemaValidator and
DocumentBuilder.

The following classes provide a subset of the XDM model: XdmValue,
XdmItem, XdmAtomicValue, XdmNode, XdmFunctionItem, XdmArray, and
XdmMap.



Full documentation for SaxonC 12 PHP is available. The [SaxonC PHP 12
documentation](https://www.saxonica.com/saxon-c/documentation12/index.html#!api/saxon_c_php_api)
contains information specific to SaxonC - including instructions for
installing and configuring the product, details of the SaxonC C/C++
API, PHP API and Python3 API, and sample code. Meanwhile, the main
[Saxon 12 documentation](https://www.saxonica.com/documentation12/index.html)
contains further information relevant to SaxonC.

For further information, see the [SaxonC web
page](https://www.saxonica.com/saxon-c/index.xml). For bug reporting,
lists of known bugs, and help forums, please visit the SaxonC bug issue project:
[https://saxonica.plan.io/projects/saxon-c](https://saxonica.plan.io/projects/saxon-c).

To install, open a shell window and navigate to the `php` directory inside
the location where you unpacked the zip file. Run these commands:

- phpize
  
User should install the SaxonC libraries (i.e. directory 'SaxonC[EDITION]') in a suitable location. 
The search paths are '/opt/saxonica' '/usr/local' '/usr'. Then run the command

- ./configure --with-saxon

Alternatively:
- ./configure --with-saxon=path/SaxonC[EDITION]

- make
- sudo make install

Next create a module conf file:

---
nano /etc/php/8.X/mods-available/20-saxon.ini
with content:

; configuration for php Saxon HE/PE/EE module
extension=saxon.so
---
and save the file. Now enable the module for PHP using the command:

phpenmod saxon
Alternatively, you can update the php.ini file or add the 20-saxon.ini file under the conf.d directory (if using Ubuntu it is usually in the location /etc/php/8.X/apache2/) to contain the PHP extension. Insert the code extension=saxon.so in the Dynamic Extensions section, then run the following commands:

For some Linux distributions using php-fpm the file 20-saxon.ini should be installed within the directory /etc/php.d

sudo service apache2 restart
php -m | grep saxon -i