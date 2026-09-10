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
    assert saxonproc.edition == 'ee'

def test_saxon_edition_unlicensed():
    saxonproc = saxoncee.PySaxonProcessor()
    assert saxonproc.edition == 'ee'
