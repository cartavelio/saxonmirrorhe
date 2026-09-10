cmake_minimum_required(VERSION 3.27)

project(SaxonCCoreHE VERSION ${SAXON_VERSION})

set(SaxonCCore_EDITION "he" CACHE STRING "Saxon edition")
set(SaxonCCore_VERSION_MAJOR "${SaxonCCoreHE_VERSION_MAJOR}")
set(SaxonCCore_VERSION "${SaxonCCoreHE_VERSION}")

include("../cmake/libsaxonc-core.cmake")
