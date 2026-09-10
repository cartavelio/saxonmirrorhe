from setuptools import setup, Extension
from Cython.Build import cythonize
import os
from pathlib import Path

# This file is copied to build/pypi before running; internal
# paths are relative to that location

# include platform specific dynamic lib in the wheel.
from platform import system

extra_link_args = []

class SaxonCLibLocation:
    def __init__(self, base_path):
        self.base_path = base_path

    @property
    def include(self):
        return self.base_path / 'include'

    @property
    def dll(self):
        return self.base_path / 'bin'

    @property
    def lib(self):
        return self.base_path / 'lib'

libsaxonc_core_path = Path(os.environ.get('LIBSAXONCCORE_PATH'))
libsaxonc_path = Path(os.environ.get('LIBSAXONC_PATH'))
saxonc_locations = [SaxonCLibLocation(libsaxonc_core_path), SaxonCLibLocation(libsaxonc_path)]

print([(x.include, x.lib, x.dll) for x in saxonc_locations])

def is_debug_build():
    if 'BUILD_DEBUG' in os.environ:
        return os.environ.get('BUILD_DEBUG') in ['true', '1', 'yes']
    return False

def get_debug_postfix(is_debug_build):
    if is_debug_build:
        if system() == 'Darwin':
            return '_debug'
        elif system() == 'Windows':
            return ''
        return 'd'
    return ''

debug_build = is_debug_build()
debug_postfix = get_debug_postfix(debug_build)
saxonc_edition = os.environ.get('SAXONC_BUILD_EDITION')
saxonc_version = os.environ.get('SAXONC_BUILD_VERSION')

saxonc_core_libname = f'saxonc-core-{saxonc_edition}{debug_postfix}'
saxonc_libname = f'saxonc-{saxonc_edition}{debug_postfix}'

if system() == 'Windows':
    extra_link_args.extend([f'/LIBPATH:{loc.lib}' for loc in saxonc_locations])
    extra_link_args.extend([f'/LIBPATH:{loc.dll}' for loc in saxonc_locations])
else:
    extra_link_args.extend([f'-L{loc.lib}' for loc in saxonc_locations])

saxonc_api_sources = []

all_sources = ['src/saxonc/saxonc.pyx'] + [str(os.path.join('src/cpp/lib', src)) for src in saxonc_api_sources]

saxonc_ext = Extension(
    f'saxonc{saxonc_edition}',
    sources = all_sources,
    language = 'c++',
    include_dirs = ['src/cpp/include'] + [str(loc.include) for loc in saxonc_locations],
    extra_link_args = extra_link_args,
    libraries = [saxonc_core_libname, saxonc_libname]
)

ext_modules = [saxonc_ext]
cythonize_kwargs = {'compiler_directives': {'language_level': 3}}
if debug_build:
    cythonize_kwargs['gdb_debug'] = True
setup(
    ext_modules=cythonize(ext_modules, **cythonize_kwargs),
)
