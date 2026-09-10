from saxoncee import *



class PyTestOutcome:

    def test_assertion(assertion: PyXdmNode, result: PyXdmNode, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        print(assertion)
        tag = assertion.name
        success = testAssertion2(assertion, outcome, result, assertXpc, catalogXpc, debug);
        return True


    def test_assertion_error(assertion: PyXdmNode, result: PySaxonApiError, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        return True

    def test_assertion2(assertion: PyXdmNode, result: PyXdmNode, assert_xpc: PyXPathProcessor, catalog_xpc: PyXPathProcessor):
        tag = assertion.name
        resulti = result.item_at(0)
        if tag is "assert":
            assert_xpc.set_context(xdm_item=result)

        return True
