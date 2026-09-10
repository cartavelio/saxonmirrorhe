cmake_minimum_required(VERSION 3.27)
project(SaxonCPE VERSION ${SAXON_VERSION})
enable_testing()

set(SaxonC_EDITION "pe" CACHE STRING "Saxon edition")
set(SaxonC_VERSION_MAJOR "${SaxonCPE_VERSION_MAJOR}")
set(SaxonC_VERSION "${SaxonCPE_VERSION}")
include("../cmake/common-vars.cmake")

configure_file(${SAXONC_SRC_DIR}/lib/saxonc_version.c.in saxonc_version.c @ONLY)
include("../cmake/libsaxonc.cmake")
include("../cmake/export-header.cmake")

if(PROJECT_IS_TOP_LEVEL)
    add_subdirectory("../test" "test")
    add_subdirectory("test" "edition-test")
    add_subdirectory("../src/bin" "bin")
    add_subdirectory("../packaging" "packaging")
endif()
