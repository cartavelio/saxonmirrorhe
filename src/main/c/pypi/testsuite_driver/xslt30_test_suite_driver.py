from saxoncee import *
import os
from Environment import Environment
import sys
from os.path import isfile
from datetime import datetime as date

dir = "/Users/ond1/work/development/git/XSLT30"
'''dir = os.getcwd()'''
path = dir + "/xslt30-test/"
catalog = path + "catalog.xml"
saxon_version = ""

driver_proc = PySaxonProcessor(license=True)
driver_proc.set_configuration_property("http://saxon.sf.net/feature/licenseFileLocation", "/Users/ond1/work/development/git/private/saxon-license.lic")
driver_proc.set_cwd(os.getcwd())
cat_builder = None

xsltproc = driver_proc.new_xslt30_processor()

xpathProcForEnv = driver_proc.new_xpath_processor()

xpathProcForCompareNodes = driver_proc.new_xpath_processor()

xpathProcForCompareNodes.declare_namespace("fn","http://www.w3.org/2005/xpath-functions")


schemaValidator = driver_proc.new_schema_validator()


globalEnvironment = {}
localEnvironment = {}
environments = {}
xpathProcForEnv.declare_namespace("", "http://www.w3.org/2012/10/xslt-test-catalog")
edition = "EE"
passVar = 0
fail = 0
error = 0
not_run = 0
run_test = None
'''"streamable"'''
run_test_case_name = None
'''result-document-0206'''
run_assertion = None
'''serialization-matches'''
total_tests = 0
exceptionList = []
'''number-5018 number-2806 number-5014 number-5029'''

def process_catalog(catalogi):
    att_name = None
    att_file = None
    global cat_builder
    cat_builder = driver_proc.new_document_builder()
    xpc = driver_proc.new_xpath_processor()
    '''xpc.set_lanaguage("3.1")'''
    if os.path.exists(catalogi):
      '''catalog_node = cat_builder.parse_xml(xml_file_name=catalogi)'''
      catalog_node = driver_proc.parse_xml(xml_file_name=catalogi)

      if run_test is None:
          create_global_environment(catalog_node, xpc)

      '''TODO read_exceptions_file()'''
      xpathProcForEnv.set_context(xdm_item=catalog_node)
      xml = xpathProcForEnv.evaluate("//*:test-set")
      if xml is not None:
          print("<b>Test sets: " + str(xml.size) + "<b/> See <a href='#results'> Results here </a>")

          for i in range(xml.size):
            value = xml.item_at(i)
            if value is not None and value.is_node:
              node = value.get_node_value()
              if node is not None:
                att_name = node.get_attribute_value("name")
                att_file = node.get_attribute_value("file")
                process_test_set(cat_builder, driver_proc, path + att_file)
              else:
                print("Node for test-set is None")
            else:
              print("Value for test-set is NULL")
          print("Total number of tests: #" + str(total_tests))
          '''write resultpostamble'''
      else:
        print('Failed to open ' + catalogi)

def process_test_set(doc_builderi: PyDocumentBuilder, proci: PySaxonProcessor, test_set_file: str):

    global total_tests

    if os.path.exists(test_set_file):


        test_set_xml = doc_builderi.parse_xml(xml_file_name=test_set_file)
        if test_set_xml is not None:
            create_local_environments(test_set_xml, xpathProcForEnv)
            child0 = test_set_xml.children[0]
            xpathProcForEnv.set_context(xdm_item=child0)
            envs = xpathProcForEnv.evaluate("//environment[@name]")

            if envs is not None:
                for i in range(envs.size):
                    env = envs.item_at(i)
                    process_environment(xpathProcForEnv, env.get_node_value(), "spec", localEnvironment, localEnvironment['default'])
            if child0 is None:
                print("Cannot find child node of test " + test_set_file)
                return None
            else:
                test_set_name = child0.get_attribute_value("name")
                '''if run_test is not None and test_set_name is not run_test:
                    return None'''
                ''' get environment'''

                '''check dependencies'''

                xpathProcForEnv.set_context(xdm_item=test_set_xml)
                test_cases = xpathProcForEnv.evaluate("//*:test-case")

                if test_cases is not None:
                    tcs_count = test_cases.size
                    total_tests += tcs_count


                    print(test_set_file)
                    print("Test cases count = " + str(tcs_count))
                    result = "NA"
                    for x, test_case in enumerate(test_cases):
                        if test_case.is_node:
                            test_casei = test_case.get_node_value()
                            test_case_name = test_casei.get_attribute_value("name")
                            if (run_test_case_name is None or test_case_name is run_test_case_name) and test_set_name not in exceptionList:
                                result = run_test_case(test_case, xpathProcForEnv)
                            else:
                                result = "Not run"
                            print("Test case:" + test_case_name + " result:= " + result)

                        else:
                            print("Error test_case is not a node")




        else:
            print("Cannot find test " + test_set_file)



''', catalog_xpath_proci'''


def process_environment(xpc: PyXPathProcessor, env: PyXdmNode, spec: str, environment: dict, default_environment: Environment):

    global environments

    environment = Environment()
    xpc.set_context(xdm_item=env)
    name = env.get_attribute_value("name")
    if name is not None:
        print("Loading environment " + name)
        environments[name] = environment
    else:
        print("Loading environment without name")



    '''set the base URI if specified'''
    base_uri = str(env.base_uri).split("file://", 1)[1]
    environment.proc.set_cwd(base_uri)
    '''TODO There is a bug with the DocumentBuilder base_uri and cwd for the processor'''
    builder = environment.proc.new_document_builder()
    base = xpc.evaluate("//static-base-uri")
    if base is not None:
        for i in range(base.size):
            basei = base.item_at(i)
            uri = basei.get_node_value().get_attribute_value("uri")
            if uri is None or uri == "UNDEFINED":
                environment.xquery_proc.set_query_base_uri(None)
            else:
                environment.xquery_proc.set_query_base_uri(uri)
                environment.base_uri = uri
                print("base-uri = " + uri)

    '''set any requested collations - TODO'''



    '''declare the requested namespaces - TODO'''

    ns_nodes = xpc.evaluate("//namespace")
    if ns_nodes is not None:
        for i in range(ns_nodes.size):
            ns_node = ns_nodes.item_at(i)
            prefix = ns_node.get_node_value().get_attribute_value("prefix")
            uri = ns_node.get_node_value().get_attribute_value("uri")
            environment.xquery_proc.declare_namespace(prefix=prefix, uri=uri)
            environment.xpath_proc.declare_namespace(prefix=prefix, uri=uri)

    '''load the requested schema documents'''

    schema_validator = environment.proc.new_schema_validator()
    ''' PyXdmNode.base_uri returns file:// - this will cause problems with resolving later.'''
    '''Create bug issue to remove the file:// internally TODO create bug issue '''

    schema_validator.set_cwd(base_uri)

    schemas = xpc.evaluate("schema")
    validate_sources = False
    if schemas is not None:
        print("schema found count=" + str(schemas.size))
        for i in range(schemas.size):
            schema = schemas.item_at(i).get_node_value()
            role = schema.get_attribute_value("role")
            xsd_version = schema.get_attribute_value("xsd-version")

            if xsd_version is not None:
                schema_validator.set_property("xsdversion", xsd_version)
            else:
                schema_validator.set_property("xsdversion", "1.0")

            if role != "secondary":
                href = schema.get_attribute_value("file")
                ns = schema.get_attribute_value("uri")

                if ns == "http://www.w3.org/2005/xpath-functions":
                    schema_validator.set_property("xsdversion", "1.1")

                if href is None:
                    try:
                        schema_validator.register_schema(xsd_file=ns)
                    except PySaxonApiError as ex:
                        print("*** Failed to load schema by URI: " + ns + " - " + str(ex))


                else:
                    try:
                        schema_validator.register_schema(xsd_file=href)
                    except PySaxonApiError as ex:
                        print("*** Failed to load schema by URI: " + href + " - " + str(ex))

                xpc.import_schema_namespace(str(ns))
                '''TODO create bug issue byte type require str for import_schema_namespace'''
                environment.xpath_proc.import_schema_namespace(str(ns))

                if role == "source-reference":
                    validate_sources = True
                '''if role == "stylesheet-import": '''









    '''SchemaManager manager = environment.processor.getSchemaManager()
    boolean validateSources = loadSchemaDocuments(driver, xpc, env, environment, manager)'''

    '''load the requested source documents'''

    load_source_documents(xpc, env, environment, builder, schemaValidator, validate_sources)
    return environment

def load_source_documents(xpc: PyXPathProcessor, env: PyXdmNode, environment: Environment, builder: PyDocumentBuilder, schema_validator: PySchemaValidator, validate_sources: bool):
    sources = xpc.evaluate("source")

    if sources is not None:
        for i in range(sources.size):

            source = sources.item_at(i).get_node_value()
            raw_uri = source.get_attribute_value("uri")
            role = source.get_attribute_value("role")
            media_type = source.get_attribute_value("media-type")
            validation = source.get_attribute_value("validation")
            if validation is None:
                validation = "skip"
            streaming = source.get_attribute_value("streaming")

            if not validate_sources and validation == "skip":
                print('validation is skipped')
                '''builder.set_schema_validator(None)
                builder.set_schema_validator(None) 
                TODO this is a bug cannot set validator to None'''
            else :
                schema_validatori = environment.proc.new_schema_validator()
                schema_validatori.set_lax(validation == "lax")
                builder.set_schema_validator(schema_validatori)
            href = source.get_attribute_value("file")
            select = source.get_attribute_value("select")
            xinc = source.get_attribute_value("xinclude")
            content = None
            builder.set_line_numbering(True)

            try:
                if href is not None:
                    base_uri = source.base_uri
                    builder.set_base_uri(base_uri)
                    content = builder.parse_xml(xml_file_name=href)
                    environment.source_docs[href] = content
                else:
                    print(source)
                    xpc.set_context(xdm_item=source)
                    contenti = xpc.evaluate_single("string(content)")
                    content = builder.parse_xml(xml_text=contenti.string_value)
                    if href is None:
                        environment.source_docs["default"] = content
                    else:
                        environment.source_docs[href] = content
            except PySaxonApiError as ex:
                print("*** failed to build source document " + str(ex))





def create_global_environment(catalog: PyXdmNode, xpath_proc: PyXPathProcessor):
    xpath_proc.set_context(xdm_item=catalog)

    nodes = xpath_proc.evaluate("//environment")
    if nodes is not None:
        for env in nodes:
            process_environment(xpath_proc, env, globalEnvironment, localEnvironment['default'])

def create_local_environments(test_set_node: PyXdmNode, xpath_proc: PyXPathProcessor):
    localEnvironment.clear()
    environmenti = Environment()
    localEnvironment.update({"default" : environmenti})

def run_test_case(test_case_node: PyXdmNode, xpath_proc: PyXPathProcessor):

    global environments

    xpath_proc.set_context(xdm_item=test_case_node)
    test_name  = xpath_proc.evaluate_single("@name")
    dependencies = xpath_proc.evaluate_single("//dependencies")
    env_node = xpath_proc.evaluate_single("environment")
    spec_att = 'XSLT10+'
    if dependencies is not None:
        xpath_proc.set_context(xdm_item=dependencies)
        spec_att = xpath_proc.evaluate_single("spec/@value")
        if(spec_att is not None):
            print("spec value = " + spec_att.string_value)

        ''' get environment for this testcase'''

    if env_node is not None:
        print(env_node)
        env_ref = env_node.get_node_value().get_attribute_value("ref")
        '''environment = None
        if env_ref is None:
            environment = process_environment(xpath_proc, env_node.get_node_value(), "spec", globalEnvironment, localEnvironment['default'])
        else:
            print('env_Ref = ' + env_ref)
            environment = environments[env_ref]'''
    print("checkpoint 1")
    test_input = xpath_proc.evaluate_single("test")
    print("checkpoint 2")
    stylesheet = xpath_proc.evaluate_single("stylesheet[not(@role = 'secondary')]")
    print("checkpoint 3")
    if stylesheet is not None:
        print('stylesheet found XXX')
        print(str(stylesheet))




    return "Not run"


saxon_version = driver_proc.version

print("W3 XSLT 3.0 Test Suite")
print("(Python Test Harness. Version: 1.0. SaxonC Python API, Saxon product version:" + saxon_version + ")")

process_catalog(catalog)
