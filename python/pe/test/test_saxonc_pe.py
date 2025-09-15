import pytest
import saxoncpe

@pytest.fixture
def saxonproc():
    return saxoncpe.PySaxonProcessor(license=True)

def test_version(saxonproc):
    """SaxonProcessor version string content"""
    assert saxonproc is not None
    ver = saxonproc.version

    assert ver.startswith('SaxonC-PE')
    assert ver.endswith('from Saxonica')

def test_saxon_edition_licensed(saxonproc):
    assert saxonproc.edition == 'PE'

def test_saxon_edition_unlicensed():
    saxonproc = saxoncpe.PySaxonProcessor()
    assert saxonproc.edition == 'HE'

def test_icu_lib(saxonproc):
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
