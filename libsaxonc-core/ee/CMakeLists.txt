cmake_minimum_required(VERSION 3.27)

project(SaxonCCoreEE VERSION ${SAXON_VERSION})

set(SaxonCCore_EDITION "ee" CACHE STRING "Saxon edition")
set(SaxonCCore_VERSION_MAJOR "${SaxonCCoreEE_VERSION_MAJOR}")
set(SaxonCCore_VERSION "${SaxonCCoreEE_VERSION}")

include("../cmake/libsaxonc-core.cmake")
