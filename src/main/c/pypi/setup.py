from setuptools import setup, Extension
from Cython.Build import cythonize
import shutil

# This file is copied to build/pypi before running; internal
# paths are relative to that location

# read the long_description from a file.
from pathlib import Path
this_directory = Path(__file__).parent
long_description = (this_directory / "README.md").read_text()

# include platform specific dynamic lib in the wheel.
from platform import system
from distutils.dir_util import copy_tree

extra_link_args = []

if system() == 'Darwin':
    extra_link_args.append('-L@@location@@libs/darwin')
elif system() == 'Windows':
    extra_link_args.append('@@location@@libs\\win\\libsaxon-@@edition@@c-@@VERSION@@.lib')
else:
    extra_link_args.append('-L@@location@@libs/nix')

# extented modules
ext_modules = [Extension(
                         "saxonc@@edition@@", 
                        sources=[
                            "python_saxon/saxonc.pyx",
                            "@@location@@Saxon.C.API/SaxonProcessor.cpp",
                            "@@location@@Saxon.C.API/SaxonCGlue.c",
                            "@@location@@Saxon.C.API/SaxonCXPath.c",
                            "@@location@@Saxon.C.API/XdmValue.cpp",
                            "@@location@@Saxon.C.API/XdmItem.cpp",
                            "@@location@@Saxon.C.API/XdmNode.cpp",
                            "@@location@@Saxon.C.API/XdmAtomicValue.cpp",
                            "@@location@@Saxon.C.API/XdmFunctionItem.cpp",
                            "@@location@@Saxon.C.API/XdmMap.cpp",
                            "@@location@@Saxon.C.API/XdmArray.cpp",
                            "@@location@@Saxon.C.API/DocumentBuilder.cpp",
                            "@@location@@Saxon.C.API/Xslt30Processor.cpp",
                            "@@location@@Saxon.C.API/XsltExecutable.cpp",
                            "@@location@@Saxon.C.API/XQueryProcessor.cpp",
                            "@@location@@Saxon.C.API/XPathProcessor.cpp",
                            "@@location@@Saxon.C.API/SchemaValidator.cpp",
                            "@@location@@Saxon.C.API/CythonExceptionHandler.cpp",
                            "@@location@@Saxon.C.API/SaxonApiException.cpp",],
                        language="c++",
                        include_dirs = ['@@location@@Saxon.C.API/graalvm',],
                         extra_link_args = extra_link_args,
                       libraries=['@@libpath@@saxon-@@edition@@c-@@VERSION@@']
                        ),
                ]
setup(
    name='saxonc@@edition@@',
    version='@@VERSION.3@@',
    description='Official Saxonica python package for the SaxonC-@@EDITION@@ @@VERSION@@ processor: for XSLT 3.0, XQuery 3.1, XPath 3.1 and XML Schema processing.',
    long_description=long_description,
    long_description_content_type='text/markdown',
    author='ONeil Delpratt',
    author_email='oneil@saxonica.com',
    include_package_data=True,
    url='https://www.saxonica.com/saxon-c/index.xml',
    package_dir={'saxonc@@edition@@':'.'},
    packages=['saxonc@@edition@@'],
    python_requires='>=3.8',                # Minimum version requirement of the package
    ext_modules=cythonize(ext_modules,
                          compiler_directives={'language_level': 3},
                          ),

)
