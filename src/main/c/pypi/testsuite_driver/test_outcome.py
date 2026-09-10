from saxoncee import *
import os
from lxml import etree

class PyTestOutcome:

    processor: PySaxonProcessor
    wrong_error: str

    def __init__(self):
        self.wrong_error = None

    def test_assertion(self, assertion: PyXdmNode, result, result_as_error, proc: PySaxonProcessor, assert_xpc: PyXPathProcessor,
                       catalog_xpc: PyXPathProcessor, assert_set: set):
        self.processor = proc
        tag = assertion.name
        success = self.test_assertion2(assertion, result, result_as_error, assert_xpc, catalog_xpc, assert_set)
        return success

    def test_assertion_error(self, assertion: PyXdmNode, result: PySaxonApiError, assert_xpc: PyXPathProcessor,
                             catalog_xpc: PyXPathProcessor):
        return self.compare_expected_error(assertion, result, assert_xpc, catalog_xpc)

    def test_assertion2(self, assertion: PyXdmNode, result, result_as_error, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor, assert_set: set):

        tag = assertion.name.split("}")[1]
        assert_set.add(tag)

        if "assert-result-document" == tag:
            print(assertion)


        if "error" != tag and result is None:
            print(result_as_error)
            return False
        print(assert_set)
        if "assert" == tag:
            return self.assert_xpath(assertion, result, assert_xpc, catalog_xpc)
        elif "assert-xml" == tag:
            return self.assert_xml(assertion, result, assert_xpc, catalog_xpc)
        elif "assert-eq" == tag:
            return self.assert_eq(assertion, result, assert_xpc, catalog_xpc)
        elif "assert-empty" == tag:
            return result.size == 0
        elif "assert-true" == tag:
            return result.size == 1 and result.item_at(0).is_atomic and result.item_at(0).get_atomic_value().primitive_type_name
        elif "error" == tag:

            return self.compare_expected_error(assertion, result_as_error, assert_xpc, catalog_xpc)
        elif "assert-result-document" == tag:
            catalog_xpc.set_context(xdm_item=assertion)
            sub_assertion = catalog_xpc.evaluate_single("*")
            print(result.base_uri)
            uri = assertion.get_attribute_value("uri")
            print("assertion uri=" + uri)
            exit()
            return False
        elif "all-of" == tag:
            catalog_xpc.set_context(xdm_item=assertion)
            children = catalog_xpc.evaluate("*")
            for i in range(children.size):
                child = children.item_at(i)
                if not self.test_assertion(child.get_node_value(), result, result_as_error, self.processor, assert_xpc, catalog_xpc, assert_set):
                    return False
            return True

        elif "any-of" == tag:
            partial_success = False
            catalog_xpc.set_context(xdm_item=assertion)
            children = catalog_xpc.evaluate("*")
            for i in range(children.size):
                child = children.item_at(i)
                if self.test_assertion(child.get_node_value(), result, result_as_error, self.processor, assert_xpc, catalog_xpc, assert_set):
                    if self.wrong_error is not None:
                        partial_success = True
                        continue
                    return True
            return partial_success

        return False

    def compare_expected_error(self, assertion: PyXdmNode, result: PySaxonApiError, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        if result is None:
            print("Error in PytestOutCome PySaxonApiError should not be None")
            return False
        code = assertion.get_attribute_value("code")
        resulti = code in str(result)
        if resulti == False:
            self.wrong_error = str(result)
        return resulti


    def assert_xpath(self, assertion: PyXdmNode, result: PyXdmValue, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        catalog_xpc.set_context(xdm_item=assertion)
        namespaceValues = catalog_xpc.evaluate("for $i in 1 to count(//namespace::*) return if (empty(index-of((//namespace::*)[position() = (1 to ($i - 1))][name() = name((//namespace::*)[$i])], (//namespace::*)[$i]))) then (//namespace::*)[$i] else ()")
        if namespaceValues != None and namespaceValues.size > 0:
            ns_size = namespaceValues.size
            for x in range(ns_size):
                itemx = namespaceValues.item_at(x)
                if itemx is not None and isinstance(itemx, PyXdmNode):
                    itemn = itemx.get_node_value()
                    ns_name = itemn.name
                    ns_value = itemn.get_node_value()
                    if ns_name is not None and ns_value is not None:
                        print("Namespace=" + ns_name + " = " + str(ns_value))
                        assert_xpc.declare_namespace(ns_name, str(ns_value))
            assert_xpc.declare_variable("result")
            print("Testing " + assertion.string_value)
            print(result)
            assert_xpc.declare_namespace("fn", "http://www.w3.org/2005/xpath-functions")
            assert_xpc.declare_namespace("xs", "http://www.w3.org/2001/XMLSchema")
            assert_xpc.declare_namespace("math", "http://www.w3.org/2005/xpath-functions/math")
            assert_xpc.declare_namespace("map", "http://www.w3.org/2005/xpath-functions/map")
            assert_xpc.declare_namespace("array", "http://www.w3.org/2005/xpath-functions/array")
            assert_xpc.declare_namespace("j", "http://www.w3.org/2005/xpath-functions")
            assert_xpc.declare_namespace("file", "http://expath.org/ns/file")
            assert_xpc.declare_namespace("bin", "http://expath.org/ns/binary")
            assert_xpc.set_parameter("result", result)
            assert_xpc.set_context(xdm_item=result.item_at(0))
            b = assert_xpc.effective_boolean_value(assertion.string_value)
            if not b:
                print("XPath assertion " + assertion.string_value + " failed")
            return b
        return False

    def elements_equal(self, e1, e2):
        if e1.tag != e2.tag: return False
        if e1.text != e2.text: return False
        if e1.tail != e2.tail: return False
        if e1.attrib != e2.attrib: return False
        if len(e1) != len(e2): return False
        return all(self.elements_equal(c1, c2) for c1, c2 in zip(e1, e2))

    def assert_xml(self, assertion: PyXdmNode, result: PyXdmValue, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        normalizeAtt = assertion.get_attribute_value("normalize-space")
        normalize = normalizeAtt is not None and ("true" == normalizeAtt.strip() or "1" == normalizeAtt.strip())
        ignoreAtt = assertion.get_attribute_value("ignore-prefixes")
        ignorePrefixes = ignoreAtt is not None and ("true" == ignoreAtt.strip() or "1" == ignoreAtt.strip())
        xmlVersion = assertion.get_attribute_value("xml-version")
        xml11 = "1.1" == xmlVersion

        catalog_xpc.set_context(xdm_item=assertion)
        comparand = str(catalog_xpc.evaluate("if (@file) then unparsed-text(resolve-uri(@file, base-uri(.))) else string(.)"))
        if comparand.startswith("<?xml"):
            indexi = comparand.index("?>")+2
            comparand = comparand[indexi:]
        comparand = comparand.strip().replace("\r\n", "\n").replace("\n", "")

        result_str = str(result).strip().replace("\r\n", "\n").replace("\n", "")

        if result_str == comparand:
            return True



        parser = etree.XMLParser(remove_blank_text=True)
        if os.getenv("SAXONC_DEBUG_FLAG") is not None:
            print("====== assertion - assert-xml:=")
            print(comparand)
            print("====== result:=")
            print(result_str)

        try:
            comparand_xml = etree.XML(comparand, parser)
            resulti = etree.XML(result_str, parser)
            return self.elements_equal(comparand_xml, resulti)
        except Exception as ex:
            print(ex)
        return False

    def assert_eq(self, assertion: PyXdmNode, result: PyXdmValue, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):

        assert_xpc.declare_variable("{}result")
        assert_xpc.set_parameter("{}result", result)
        try:
            item = assert_xpc.evaluate_single("$result eq " + assertion.string_value)
            print(item)
            return item is not None and item.get_atomic_value().boolean_value

        except PySaxonApiError as ex:
            print("assert-eq failed - " + str(ex))
            return False
