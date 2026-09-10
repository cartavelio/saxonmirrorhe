cmake_minimum_required(VERSION 3.27)

project(SaxonCEE VERSION ${SAXON_VERSION})
enable_testing()

set(SaxonC_EDITION "ee" CACHE STRING "Saxon edition")
include("../cmake/common-vars.cmake")

configure_file(${SAXONC_SRC_DIR}/lib/saxonc_version.c.in saxonc_version.c @ONLY)
include("../cmake/libsaxonc.cmake")
include("../cmake/export-header.cmake")

if(PROJECT_IS_TOP_LEVEL)
    add_subdirectory("../test" "test")
    add_subdirectory("test" "edition-test")
    add_subdirectory("../src/bin" "bin")
    add_subdirectory("src/bin" "edition-bin")
    add_subdirectory("../packaging" "packaging")
endif()

#set(SaxonC_VERSION_MAJOR "${SaxonCEE_VERSION_MAJOR}")
#set(SaxonC_VERSION "${SaxonCEE_VERSION}")
