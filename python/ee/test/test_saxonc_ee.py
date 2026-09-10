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

def testXquery_40_invisible_xml(saxonproc):
    xquery_processor = saxonproc.new_xquery_processor()
    result = xquery_processor.run_query_to_value(
        query_text="xquery version '4.0'; invisible-xml(\"S=A. A='a'.\")('a')", lang='4.0')
    assert "<A>a</A>" in str(result)

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

def user_function2(value, **kwargs):
    ''' values: PyXdmValue '''
    print("In python user function2 value=", value)
    n_value = int(value) + 4
    print("In python value=",n_value)
    return n_value

def float_value_test(value, **kwargs):

    try:
        saxonproc = kwargs['processor']
        valuex= saxonproc.make_float_value(value/100)
        print('print Value=', valuex)
        return valuex
    except Exception as err:
        print(err)
        return None


def merge_iterators(*iters):
    for it in iters:
        for item in it:
            yield item

def check_none(**kwargs):
    return None

def testUserFunction():
    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.example.com", user_function, signature_text="{ \"myFunc\": {"
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
    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.float.com", float_value_test, signature_text="{ \"float_value_test\": {"
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
    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.fruits.com", list_example, signature_text="{ \"list_example\": {"
                                                                           "      \"as\": \"array(*)\","
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
        assert str(result.get_array_value().as_list()[0]) == 'apple'
        assert str(result.get_array_value().as_list()[1]) == 'strawberry'
    except Exception as err:
        print(err)
        assert False

def testUserFunctionList2():

    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.example.com", user_function2, signature_text="{ \"myFunc2\": {"
                                                                              "      \"as\": \"xs:integer?\","
                                                                              "      \"param\": ["
                                                                              "        \"xs:integer\""
                                                                              "      ],"
                                                                              "      \"arity\": [1]"
                                                                              "    }"
                                                                              "}", use_python_types=True)

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("dy", "http://www.example.com")
        result2 = xpath_processor.evaluate_single("dy:myFunc2(4)")

        assert result2 is not None

        assert result2.get_atomic_value().integer_value == 8

    except Exception as err:
        print(err)
        assert False

def list_example(value_str, **kwargs):
    fruits = ["apple", "banana", "cherry"]
    fruits.insert(1, str(value_str))
    return fruits

def test_user_function_with_list():

    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.fruits.com", list_example, signature_text="{ \"list_example\": {"
                                                                           "      \"as\": \"array(*)\","
                                                                           "      \"param\": ["
                                                                           "        \"xs:string\""
                                                                           "      ],"
                                                                           "      \"arity\": [1]"
                                                                           "    }"
                                                                           "}", use_python_types=False)

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("fr", "http://www.fruits.com")

        result_list = xpath_processor.evaluate_single("fr:list_example('strawberry')")

        assert result_list.is_array
        assert str(result_list.get_array_value().as_list()[0]) == 'apple'
        assert str(result_list.get_array_value().as_list()[1]) == 'strawberry'

    except Exception as err:
        print(err)
        assert False


def list_example2(value_str, **kwargs):
    fruits = ["apple", "banana", "cherry"]
    fruits.insert(1, str(value_str))

def test_list2_return_none():
    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.fruits.com", list_example2, signature_text="{ \"list_example2\": {"
                                                                            "      \"as\": \"xs:string?\","
                                                                            "      \"param\": ["
                                                                            "        \"xs:string\""
                                                                            "      ],"
                                                                            "      \"arity\": [1]"
                                                                            "    }"
                                                                            "}", use_python_types=False)

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("fr", "http://www.fruits.com")

        result_list = xpath_processor.evaluate_single("fr:list_example2('strawberry')")

        assert result_list is None

    except Exception as err:
        print(err)
        assert False


def returning_dict_example(keyi, valuei, **kwargs):
    mymap = {"a": 1, "b": 2,"c": 3}
    mymap[keyi] = valuei
    return mymap

def returning_set_example(fruit, **kwargs):

    fruits = {"apple", "banana", "cherry"}
    fruits.add(fruit)
    return fruits


def test_dict_and_set():
    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.example.com", returning_dict_example, signature_text="{ \"returning_dict_example\": {"
                                                                                      "      \"as\": \"map(*)\","
                                                                                      "      \"param\": ["
                                                                                      "        \"xs:string\", \"xs:int\""
                                                                                      "      ],"
                                                                                      "      \"arity\": [2]"
                                                                                      "    }"
                                                                                      "}", use_python_types=True)


        proc.register_user_function("http://www.fruits.com", returning_set_example, signature_text="{ \"returning_set_example\": {"
                                                                                    "      \"as\": \"xs:string*\","
                                                                                    "      \"param\": ["
                                                                                    "        \"xs:string\""
                                                                                    "      ],"
                                                                                    "      \"arity\": [1]"
                                                                                    "    }"
                                                                                    "}", use_python_types=True)
        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("dy", "http://www.example.com")

        xpath_processor.declare_namespace("fr", "http://www.fruits.com")

        result_map = xpath_processor.evaluate_single("dy:returning_dict_example('d', 4)")

        assert result_map.is_map
        map_item1 = result_map.get_map_value().get('b').head
        assert map_item1.get_atomic_value().integer_value == 2
        assert str(result_map.get_map_value().get('c')) == '3'

        '''Sets in python are unordered'''
        result_val = xpath_processor.evaluate("fr:returning_set_example('pear')")

        assert 'pear' in str(result_val)



    except Exception as err:
        print(err)
        assert False



def tuple_example(itemi, **kwargs):
    print("tuple_example called\n")
    if isinstance(itemi, int):
        t = (1, 2, 3)
        t = t + (itemi,)
        print(type(t))
        return t
    else:
        return None

def test_user_function_with_tuple():

    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.example.com", tuple_example, signature_text="{ \"tuple_example\": {"
                                                                             "      \"as\": \"xs:integer*\","
                                                                             "      \"param\": ["
                                                                             "        \"xs:integer\""
                                                                             "      ],"
                                                                             "      \"arity\": [1]"
                                                                             "    }"
                                                                             "}", use_python_types=True)

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("dy", "http://www.example.com")

        value1 = xpath_processor.evaluate("dy:tuple_example(9)")

        assert value1.item_at(2).integer_value == 3

    except Exception as err:
        print(err)
        assert False


def sum_as_sequence(value, **kwargs):
    print("sumASequence called\n")
    if isinstance(value, (list, tuple)):
        # Check that all items are numbers
        if all(isinstance(x, (int, float)) for x in value):
            return sum(value)
        else:
            raise TypeError("Array contains non-numeric values.")
    else:
        raise TypeError("Value is not an array (list/tuple).")

def two_sequence_param2(values1, values2, **kwargs):
    print("two_sequence_param2 called\n")
    if(values1 is None or values2 is None):
        return None
    merged1 = iter(list(values1) + list(values2))
    print(merged1)
    return merged1

def merge_iterators(*iters):
    for it in iters:
        for item in it:
            yield item

def two_sequence_param3(values1, values2, **kwargs):
    print("two_sequence_param2 called\n")
    if(values1 is None or values2 is None):
        return set()
    merged1 = merge_iterators(values1, values2)
    print(type(merged1))
    return merged1

def test_sequence():

    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        proc.register_user_function("http://www.example.com", sum_as_sequence, signature_text="{ \"sum_as_sequence\": {"
                                                                                    "      \"as\": \"xs:integer?\","
                                                                                    "      \"param\": ["
                                                                                    "        \"array(xs:integer*)\""
                                                                                    "      ],"
                                                                                    "      \"arity\": [1]"
                                                                                    "    }"
                                                                                    "}", use_python_types=True)


        proc.register_user_function("http://www.example.com", two_sequence_param2, signature_text="{ \"two_sequence_param2\": {"
                                                                                    "      \"as\": \"item()*\","
                                                                                    "      \"param\": ["
                                                                                    "        \"xs:string*\", \"xs:integer*\""
                                                                                    "      ],"
                                                                                    "      \"arity\": [1]"
                                                                                    "    }"
                                                                                    "}", use_python_types=True)

        proc.register_user_function("http://www.example.com", two_sequence_param3, signature_text="{ \"two_sequence_param3\": {"
                                                                                    "      \"as\": \"item()*\","
                                                                                    "      \"param\": ["
                                                                                    "        \"xs:string*\", \"xs:integer*\""
                                                                                    "      ],"
                                                                                    "      \"arity\": [1]"
                                                                                    "    }"
                                                                                    "}", use_python_types=True)

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("dy", "http://www.example.com")


        result_sum = xpath_processor.evaluate_single("dy:sum_as_sequence([1,2,3,4,5])")

        assert result_sum is not None
        assert result_sum.is_atomic
        assert result_sum.get_atomic_value().integer_value == 15

        merged_seq = xpath_processor.evaluate("dy:two_sequence_param2(('a','b','c'), (1,2,3))")

        assert merged_seq is not None

        assert merged_seq.item_at(0).get_string_value() == 'a'

        assert merged_seq.item_at(4).get_string_value() == '2'

        merged_seq2 = xpath_processor.evaluate("dy:two_sequence_param3(('a','b','d'), (1,2,4))")

        assert merged_seq2.item_at(0).get_string_value() == 'a'

        val2 = merged_seq2.item_at(4)

        assert val2.integer_value == 2


    except Exception as err:
        print(err)
        assert False

def test_user_function_signature_dict():

    try:
        proc = saxoncee.PySaxonProcessor(license=True)
        signature = {'tuple_example' : { 'as': 'xs:integer*', 'param': ['xs:integer'], 'arity':1}}
        proc.register_user_function("http://www.example.com", tuple_example, signature_map= signature, use_python_types=True)

        xpath_processor = proc.new_xpath_processor()
        xpath_processor.declare_namespace("dy", "http://www.example.com")

        value1 = xpath_processor.evaluate("dy:tuple_example(9)")

        assert value1.item_at(2).integer_value == 3

    except Exception as err:
        print(err)
        assert False


