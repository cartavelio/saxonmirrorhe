import pytest
import saxoncee
import os

@pytest.fixture
def saxonproc():
    return saxoncee.PySaxonProcessor(license=True)

@pytest.fixture
def data_dir(pytestconfig):
    return pytestconfig.getoption("--data-dir")

def test_version(saxonproc):
    """SaxonProcessor version string content"""
    assert saxonproc is not None
    ver = saxonproc.version

    assert ver.startswith('SaxonC-EE')
    assert ver.endswith('from Saxonica')

def test_saxon_edition_licensed(saxonproc):
    assert saxonproc.edition == 'EE'

def test_saxon_edition_unlicensed():
    saxonproc = saxoncee.PySaxonProcessor()
    assert saxonproc.edition == 'HE'

def test_icu_lib():
    saxonproc = saxoncee.PySaxonProcessor(license=True)
    print(saxonproc.version)
    xqc = saxonproc.new_xquery_processor()
    xqc.set_query_content("format-date(current-date(),'[FNn]','de',(),())")
    result = xqc.run_query_to_value()
    print("format-date(current-date(),'[FNn]','de',(),()) => " + str(result.head))
    assert str(result.head) in "Montag, Dienstag, Mittwoch, Donnerstag, Freitag, Samstag, Sonntag"

    xqc.set_query_content("format-integer(33,'[Ww]')")
    result2 = xqc.run_query_to_value()
    print("format-integer(33,'[Ww]') => " + str(result2))
    assert '0033' in str(result2)

    xqc.set_query_content("format-integer(33,'Ww')")
    result5 = xqc.run_query_to_value()
    print("format-integer(33,'Ww') => " + str(result5))
    assert 'Thirty Three' in str(result5)

    xqc.set_query_content("format-integer(33,'Ww','de')")
    result6 = xqc.run_query_to_value()
    print("format-integer(33,'Ww','de') => " + str(result6))
    assert 'Drei\xadund\xaddreißig' in str(result6)

    xqc.set_query_content("format-integer(33,'Ww','cs')")
    result7 = xqc.run_query_to_value()
    print("format-integer(33,'Ww','cs') => " + str(result7))
    assert 'Třicet Tři' in str(result7)

def test_schema_aware():
    ''' This unit test requires a valid license - SaxonCEE '''
    try:
        sp = saxoncee.PySaxonProcessor(license=True)
        assert sp.is_schema_aware == True
    except Exception as err:
        print("Error: ", err)
        assert False

def test_packages(tmp_path, data_dir):
    sp = saxoncee.PySaxonProcessor(license=True)
    xsl = """\
    <xsl:package name = \"package-002.xsl\" package-version = \"2.1.0.5\"
    version = \"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">
    <xsl:param name='c' select='2'/>
    <xsl:mode/>
    <xsl:template match='/'>
    <out><xsl:apply-templates/></out>
    </xsl:template>
    <xsl:template match='/*'>
    <in><xsl:value-of select='name()'/></in>
    </xsl:template>
    </xsl:package>
    """
    package_file = tmp_path / 'package02.xsltpack'
    xsltproc = sp.new_xslt30_processor()
    xsltproc.compile_stylesheet(stylesheet_text=xsl, save=True, output_file=str(package_file))
    xsltproc = None
    xsltproc2 = sp.new_xslt30_processor()
    xsltproc2.transform_to_string(source_file=os.path.join(data_dir, "books.xml"), stylesheet_file=str(package_file))

def testXquery_40_functions(saxonproc):
    saxonproc.set_configuration_property('http://saxon.sf.net/feature/allowSyntaxExtensions', 'on')
    xquery_processor = saxonproc.new_xquery_processor()
    result = xquery_processor.run_query_to_value(
        query_text='xquery version "4.0"; parse-html("<p>This is paragraph 1.<p>This is paragraph 2.")', lang='4.0')
    assert "paragraph" in str(result)

def testXquery_40_functions_error(saxonproc):
    try:
        saxonproc.set_configuration_property('http://saxon.sf.net/feature/allowSyntaxExtensions', 'on')
        xquery_processor = saxonproc.new_xquery_processor()
        result = xquery_processor.run_query_to_value(
            query_text='xquery version "4.0"; parse-html("<p>This is paragraph 1.<p>This is paragraph 2.")', lang='3.0')
        assert False
    except Exception:
        assert True



def user_function(values, **kwargs):
    ''' values: PyXdmValue '''
    print("In python user function")

    saxonproc = kwargs['processor']
    print(values)
    n_value  = values[0].head.integer_value + 2
    print(n_value)
    try:
        valuex= saxonproc.make_integer_value(n_value)
        print('print XdmAtomicValue=')
        print(valuex)
        return valuex
    except Exception as err:
        print(err)
        return None

def float_value_test(value, **kwargs):

    try:
        saxonproc = kwargs['processor']
        valuex= saxonproc.make_float_value(value/100)
        print('print Value=', valuex)
        return valuex
    except Exception as err:
        print(err)
        return None


def list_example(value_str, **kwargs):
    fruits = ["apple", "banana", "cherry"]
    fruits.insert(1, str(value_str))
    return fruits

def testUserFunction():
    proc = saxoncee.PySaxonProcessor(license=True)
    try:
        proc.register_user_function("http://www.example.com", user_function, "{ \"myFunc\": {"
                                                                             "      \"as\": \"xs:integer?\","
                                                                             "      \"param\": ["
                                                                             "        \"xs:integer\""
                                                                             "      ],"
                                                                             "      \"arity\": [1]"
                                                                             "    }"
                                                                             "}", use_python_types=False)
        #add flag to prevent auto converted values

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("dy", "http://www.example.com")

        result = xpath_processor.evaluate_single("dy:myFunc(3)")

        assert result is not None
        assert result == 5
    except Exception as err:
        print(err)
        assert False

def testUserFunctionWithFloat():
    proc = saxoncee.PySaxonProcessor(license=True)
    try:
        proc.register_user_function("http://www.float.com", float_value_test, "{ \"float_value_test\": {"
                                                                            "      \"as\": \"xs:float\","
                                                                            "      \"param\": ["
                                                                            "        \"xs:float\""
                                                                            "      ],"
                                                                            "      \"arity\": [1]"
                                                                            "    }"
                                                                            "}", use_python_types=True)
        #add flag to prevent auto converted values

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("fr", "http://www.float.com")

        result = xpath_processor.evaluate_single("fr:float_value_test(55.2)")

        assert result is not None
        print(result.head.double_value)
        assert result.head.double_value == 0.5519999861717224
    except saxoncee.PySaxonApiError as err:
        print(err.args)
        raise err


def testUserFunctionWithLists():
    proc = saxoncee.PySaxonProcessor(license=True)
    try:
        proc.register_user_function("http://www.fruits.com", list_example, "{ \"list_example\": {"
                                                                             "      \"as\": \"xs:string\","
                                                                             "      \"param\": ["
                                                                             "        \"xs:string\""
                                                                             "      ],"
                                                                             "      \"arity\": [1]"
                                                                             "    }"
                                                                             "}", use_python_types=False)

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("fr", "http://www.fruits.com")

        result = xpath_processor.evaluate_single("fr:list_example('strawberry')")

        assert result is not None
        assert result.is_array
        assert result.get_array_value().as_list()[0] == 'apple'
        assert result.get_array_value().as_list()[1] == 'strawberry'
    except Exception as err:
        print(err)
        assert False

