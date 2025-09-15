import pytest
import saxoncee

@pytest.fixture
def saxonproc():
    return saxoncee.PySaxonProcessor(license=True)

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
    saxonproc = PySaxonProcessor(license=True)
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
