from setuptools import setup, Extension
from Cython.Build import cythonize
import shutil

# read the long_description from a file.
from pathlib import Path
this_directory = Path(__file__).parent
long_description = (this_directory / "README.md").read_text()

# include platform specific dynamic lib in the wheel.
from platform import system
from distutils.dir_util import copy_tree

extra_link_args = []

if system() == 'Darwin':
    saxon_home = 'libs/darwin'
    #extra_link_args.append('-Wl,-rpath,' + saxon_home)
else:
    saxon_home = 'libs/nix'
    #extra_link_args.append('-Wl,-rpath,' + saxon_home)




copy_tree(saxon_home, 'src/saxonc_home')
copy_tree("../../graalvm", 'Saxon.C.API/graalvm')
shutil.copyfile("../saxonc.pyx","python_saxon/saxonc.pyx")
shutil.copyfile("../saxoncClasses.pxd","python_saxon/saxoncClasses.pxd")
shutil.copyfile("../nodekind.py","python_saxon/nodekind.py")
shutil.copyfile("../readme.txt","python_saxon/readme.txt")
shutil.copyfile("../../SaxonProcessor.cpp", "Saxon.C.API/SaxonProcessor.cpp")
shutil.copyfile("../../SaxonCGlue.c","Saxon.C.API/SaxonCGlue.c")
shutil.copyfile("../../SaxonCProcessor.c","Saxon.C.API/SaxonCProcessor.c")
shutil.copyfile("../../SaxonCXPath.c","Saxon.C.API/SaxonCXPath.c")
shutil.copyfile("../../XdmValue.cpp","Saxon.C.API/XdmValue.cpp")
shutil.copyfile("../../XdmItem.cpp","Saxon.C.API/XdmItem.cpp")
shutil.copyfile("../../XdmNode.cpp","Saxon.C.API/XdmNode.cpp")
shutil.copyfile("../../XdmAtomicValue.cpp","Saxon.C.API/XdmAtomicValue.cpp")
shutil.copyfile("../../XdmFunctionItem.cpp","Saxon.C.API/XdmFunctionItem.cpp")
shutil.copyfile("../../XdmMap.cpp","Saxon.C.API/XdmMap.cpp")
shutil.copyfile("../../XdmArray.cpp","Saxon.C.API/XdmArray.cpp")
shutil.copyfile("../../DocumentBuilder.cpp","Saxon.C.API/DocumentBuilder.cpp")
shutil.copyfile("../../Xslt30Processor.cpp","Saxon.C.API/Xslt30Processor.cpp")
shutil.copyfile("../../XsltExecutable.cpp","Saxon.C.API/XsltExecutable.cpp")
shutil.copyfile("../../XQueryProcessor.cpp","Saxon.C.API/XQueryProcessor.cpp")
shutil.copyfile("../../XPathProcessor.cpp","Saxon.C.API/XPathProcessor.cpp")
shutil.copyfile("../../SchemaValidator.cpp","Saxon.C.API/SchemaValidator.cpp")
shutil.copyfile("../../SaxonApiException.cpp","Saxon.C.API/SaxonApiException.cpp")

shutil.copyfile("../../SaxonProcessor.h", "Saxon.C.API/SaxonProcessor.h")
shutil.copyfile("../../SaxonCGlue.h","Saxon.C.API/SaxonCGlue.h")
shutil.copyfile("../../SaxonCProcessor.h","Saxon.C.API/SaxonCProcessor.h")
shutil.copyfile("../../SaxonCXPath.h","Saxon.C.API/SaxonCXPath.h")
shutil.copyfile("../../XdmValue.h","Saxon.C.API/XdmValue.h")
shutil.copyfile("../../XdmItem.h","Saxon.C.API/XdmItem.h")
shutil.copyfile("../../XdmNode.h","Saxon.C.API/XdmNode.h")
shutil.copyfile("../../XdmAtomicValue.h","Saxon.C.API/XdmAtomicValue.h")
shutil.copyfile("../../XdmFunctionItem.h","Saxon.C.API/XdmFunctionItem.h")
shutil.copyfile("../../XdmMap.h","Saxon.C.API/XdmMap.h")
shutil.copyfile("../../XdmArray.h","Saxon.C.API/XdmArray.h")
shutil.copyfile("../../DocumentBuilder.h","Saxon.C.API/DocumentBuilder.h")
shutil.copyfile("../../Xslt30Processor.h","Saxon.C.API/Xslt30Processor.h")
shutil.copyfile("../../XsltExecutable.h","Saxon.C.API/XsltExecutable.h")
shutil.copyfile("../../XQueryProcessor.h","Saxon.C.API/XQueryProcessor.h")
shutil.copyfile("../../XPathProcessor.h","Saxon.C.API/XPathProcessor.h")
shutil.copyfile("../../SchemaValidator.h","Saxon.C.API/SchemaValidator.h")
shutil.copyfile("../../SaxonApiException.h","Saxon.C.API/SaxonApiException.h")

# extented modules
ext_modules = [Extension(
                        "saxonc", 
                        sources=[
                            "python_saxon/saxonc.pyx", 
                            "Saxon.C.API/SaxonProcessor.cpp",
                            "Saxon.C.API/SaxonCGlue.c",
                            "Saxon.C.API/SaxonCXPath.c",
                            "Saxon.C.API/XdmValue.cpp",
                            "Saxon.C.API/XdmItem.cpp",
                            "Saxon.C.API/XdmNode.cpp",
                            "Saxon.C.API/XdmAtomicValue.cpp",
                            "Saxon.C.API/XdmFunctionItem.cpp",
                            "Saxon.C.API/XdmMap.cpp",
                            "Saxon.C.API/XdmArray.cpp",
                            "Saxon.C.API/DocumentBuilder.cpp",
                            "Saxon.C.API/Xslt30Processor.cpp",
                            "Saxon.C.API/XsltExecutable.cpp",
                            "Saxon.C.API/XQueryProcessor.cpp",
                            "Saxon.C.API/XPathProcessor.cpp",
                            "Saxon.C.API/SchemaValidator.cpp",
                            "Saxon.C.API/SaxonApiException.cpp"],
                        language="c++",
                        include_dirs = ['Saxon.C.API/graalvm',],
                         extra_link_args = extra_link_args,
                       libraries=['saxon-eec-12.1']
                        ),
                    "python_saxon/nodekind.py",
                ]
setup(
    name='saxoncee',
    version='0.0.1',
    description='A python package for the SaxonC-EE 12.1, an XSLT 3.0, XQuery 3.1, XPath 3.1 and Schema Validator processor by Saxonica.',
    long_description=long_description,
    long_description_content_type='text/markdown',
    author='ONeil Delpratt',
    author_email='oneil@saxonica.com',
    include_package_data=True,
    url='https://github.com/tennom/saxonpy',     # TODO change github project
    # packages=find_packages(),
    package_dir={'saxoncee':'src'},
    packages=['saxoncee'],
    python_requires='>=3.6',                # Minimum version requirement of the package
    ext_modules=cythonize(ext_modules,
                          compiler_directives={'language_level': 3},
                          ),

)
