from saxoncee import *



class PyTestOutcome:

    def test_assertion(self, assertion: PyXdmNode, result: PyXdmNode, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        print(assertion)
        tag = assertion.name
        success = self.test_assertion2(assertion, result, assert_xpc, catalog_xpc)
        return success


    def test_assertion_error(self, assertion: PyXdmNode, result: PySaxonApiError, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        return False

    def test_assertion2(self, assertion: PyXdmNode, result: PyXdmNode, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        tag = assertion.name.split("}")[1]
        resulti = result.item_at(0)
        if "assert" == tag:
            return self.assert_xpath(assertion, result, assert_xpc, catalog_xpc)

        return False

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
            assert_xpc.set_parameter("result", result)
            assert_xpc.set_context(xdm_item=result.item_at(0))
            b = assert_xpc.effective_boolean_value(assertion.string_value)
            if not b:
                print("XPath assertion " + assertion.string_value + " failed")
            return b
        return False
