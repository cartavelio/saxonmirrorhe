from saxoncee import *

optimizer_flags = '-'

class Environment:




    def __init__(self):
        ...
        self.proc = PySaxonProcessor(license=True)
        self.proc.set_configuration_property("http://saxon.sf.net/feature/licenseFileLocation",
                                               "/Users/ond1/work/development/git/private/saxon-license.lic")
        self.lang = None
        self.proc.set_configuration_property('http://saxon.sf.net/feature/optimizationLevel', optimizer_flags)

        self.xpath_proc = self.proc.new_xpath_processor()
        self.base_uri = ""
        self.spec = None
        self.xquery_proc = self.proc.new_xquery_processor()

        self.xslt_proc = self.proc.new_xslt30_processor()

        self.source_docs = {}








