from setuptools import setup, Extension
from Cython.Build import cythonize
import os
from pathlib import Path

# This file is copied to build/pypi before running; internal
# paths are relative to that location

# include platform specific dynamic lib in the wheel.
from platform import system

extra_link_args = []

libsaxonc_path = Path(os.environ.get('LIBSAXONC_PATH'))
saxonc_include_location = libsaxonc_path / 'include'
saxonc_dll_location = libsaxonc_path / 'bin'
saxonc_built_libs_location = libsaxonc_path / 'lib'

saxonc_edition = os.environ.get('SAXONC_BUILD_EDITION')
saxonc_version = os.environ.get('SAXONC_BUILD_VERSION')
saxonc_core_libname = "saxonc-core-%s" % saxonc_edition
saxonc_libname = "saxonc-%s" % saxonc_edition

if system() == 'Windows':
    extra_link_args.append("/LIBPATH:%s" % saxonc_built_libs_location)
    extra_link_args.append("/LIBPATH:%s" % saxonc_dll_location)
else:
    extra_link_args.append("-L%s" % saxonc_built_libs_location)

saxonc_api_sources = []

all_sources = ["src/saxonc/saxonc.pyx"] + [str(os.path.join("src/cpp/lib", src)) for src in saxonc_api_sources]

saxonc_ext = Extension(
    "saxonc%s" % saxonc_edition,
    sources = all_sources,
    language = "c++",
    include_dirs = ["src/cpp/include", str(saxonc_include_location)],
    extra_link_args = extra_link_args,
    libraries = [saxonc_core_libname, saxonc_libname]
)

ext_modules = [saxonc_ext]
setup(
    ext_modules=cythonize(ext_modules, compiler_directives={'language_level': 3},),
)
