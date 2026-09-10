from tempfile import mkstemp
import pytest
from saxoncee import *
import os
from os.path import isfile

'''pytest --data-dir=[path/data]  test_saxon_schema.py'''


@pytest.fixture
def saxonproc2():
    try :
        return PySaxonProcessor(license=True)
    except PySaxonApiError as e:
        print(e)
        assert False

@pytest.fixture
def data_dir(pytestconfig):
    return pytestconfig.getoption("--data-dir")

def testValidator2(saxonproc2):
    saxonproc2.set_cwd('.')
    saxonproc2.set_configuration_property("xsdversion", "1.1")
    val = saxonproc2.new_schema_validator()
    assert val is not None
    invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>"
    sch1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:element name='request'><xs:complexType><xs:sequence><xs:element name='a' type='xs:string'/><xs:element name='b' type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = 3'/></xs:complexType></xs:element></xs:schema>"
    try:
        input_ = saxonproc2.parse_xml(xml_text=invalid_xml)
        assert input_ is not None
        print(type(input_))
        val.set_source_node(input_)
        val.register_schema(xsd_text=sch1)
        val.validate()
    except PySaxonApiError as e:
        print(e)
        assert True


def testValdiator3(data_dir, saxonproc2):
    saxonproc2.set_configuration_property("xsdversion", "1.1")
    compiler = saxonproc2.new_xsd_compiler()

    family_ext_xsd = os.path.join(data_dir, "family-ext.xsd")
    family_xsd = os.path.join(data_dir, "family.xsd")
    family_xml = os.path.join(data_dir, "family.xml")

    filename_list = [family_ext_xsd, family_xsd]
    schema = compiler.compile(xsd_list=filename_list)


    try:
        val = schema.new_schema_validator()
        val.validate(file_name=family_xml)
        nodea = val.validation_report
        assert nodea is None
    except PySaxonApiError as e:
        print(e)
        assert False

class TestSchemaExport:
    schema = """<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified' attributeFormDefault='unqualified'>
    <xs:element name='request'>
        <xs:complexType>
            <xs:sequence><xs:element name='a' type='xs:string'/>
                <xs:element name='b' type='xs:string'/>
            </xs:sequence>
            <xs:assert test='count(child::node()) = 3'/>
        </xs:complexType>
    </xs:element>
</xs:schema>
"""

    def export_schema(self, schema_path):
        try:
            processor = PySaxonProcessor(license=True)
            processor.set_configuration_property("xsdversion", "1.1")
            val = processor.new_schema_validator()
            assert val is not None
            print(type(val))
            val.register_schema(xsd_text=self.schema)
            val.export_schema(str(schema_path))
        except PySaxonApiError as e:
            print(e)
            assert False

    def test_schema_export(self, tmp_path, saxonproc2):
        schema_path = tmp_path / "exportedSchema.scm"

        self.export_schema(schema_path)

        assert schema_path.is_file()
        assert schema_path.stat().st_size > 0

    def test_exported_schema_import(self, tmp_path, saxonproc2):
        schema_path = tmp_path / "exportedSchema.scm"
        self.export_schema(schema_path)

        saxonproc2.set_configuration_property("xsdversion", "1.1")
        val = saxonproc2.new_schema_validator()

        val.register_schema(xsd_file=str(schema_path))

        invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>"
        input_ = saxonproc2.parse_xml(xml_text=invalid_xml)
        assert input_ is not None
        print(type(input_))
        val.set_source_node(input_)
        try:
            val.validate()
        except PySaxonApiError as e:
            print(e)
            assert True

def testValidatorReport(saxonproc2):
    saxonproc2.set_cwd('.')
    saxonproc2.set_configuration_property("xsdversion", "1.1")
    val = saxonproc2.new_schema_validator()
    val.set_property('report-node', "true")
    assert val is not None
    
    invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>"
    sch1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:element name='request'><xs:complexType><xs:sequence><xs:element name='a' type='xs:string'/><xs:element name='b' type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = 3'/></xs:complexType></xs:element></xs:schema>"
    input_ = saxonproc2.parse_xml(xml_text=invalid_xml)
    assert input_ is not None

    val.set_source_node(input_)
    val.register_schema(xsd_text=sch1)
    try:
        val.validate()
    except PySaxonApiError as e:
        print(e)
        assert True

    report = val.validation_report
    assert 'validation-report' in str(report)



def release(saxonproc):
   saxonproc.release()
