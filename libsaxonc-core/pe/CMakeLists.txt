cmake_minimum_required(VERSION 3.27)

project(SaxonCCorePE VERSION ${SAXON_VERSION})

set(SaxonCCore_EDITION "pe" CACHE STRING "Saxon edition")
set(SaxonCCore_VERSION_MAJOR "${SaxonCCorePE_VERSION_MAJOR}")
set(SaxonCCore_VERSION "${SaxonCCorePE_VERSION}")

include("../cmake/libsaxonc-core.cmake")
