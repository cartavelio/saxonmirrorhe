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

def test_saxon_edition_licensed():
    assert saxonproc.edition == 'pe'

def test_saxon_edition_unlicensed():
    saxonproc = saxoncpe.PySaxonProcessor()
    assert saxonproc.edition == 'pe'
