import xml.etree.ElementTree as ET
from saxoncee import *

from datetime import datetime

now = datetime.now()



class PyTestReport:
    result_root: ET.Element
    current_test_set: ET.Element
    def write_result_file_preamble(self, proc: PySaxonProcessor, config: list):
        current_time = now.strftime("%H:%M:%S")
        today = datetime.today()
        ET.register_namespace("", "http://www.w3.org/2012/11/xslt30-test-results")
        self.result_root = ET.Element('test-suite-result', xmlns="http://www.w3.org/2012/11/xslt30-test-results")
        implementation = ET.SubElement(self.result_root, "implementation", name='Saxon-HE', version='12.2')
        ET.SubElement(implementation, "organization",  name="http://www.saxonica.com/", anonymous="false")
        ET.SubElement(implementation, "submitter", name="O'Neil Delpratt", email="oneil@saxonica.com")
        ET.SubElement(implementation, "configuration", timeRun=str(current_time), lang="XT30")
        ET.SubElement(self.result_root, "test-run", dateRun=str(today), testsuiteVersion="3.0.1")

    def start_test_set_element(self, func_set_node: PyXdmNode):
        value = func_set_node.get_attribute_value("name")
        self.current_test_set = ET.SubElement(self.result_root, "test-set", name=value)

    def write_test_case_element(self, namei: str, result: str, comment):
        if self.current_test_set is not None:
            testcase = ET.SubElement(self.current_test_set, "test-case", name=namei, result=result)
            if comment is not None:
                testcase.set("comment", comment)
        else:
            print("Test report error: current test set not found")


    def write_result_file_postamble(self, proc: PySaxonProcessor, cwd: str):

        tree = ET.ElementTree(self.result_root)
        ET.indent(tree, space=" ", level=0)
        tree.write(cwd + '../results/saxon/results_saxonc_XSLT3.0_12.2.xml')

