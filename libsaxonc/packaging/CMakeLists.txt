include(GNUInstallDirs)
include(CMakePackageConfigHelpers)

configure_package_config_file(../cmake/SaxonCConfig.cmake.in
       ${CMAKE_CURRENT_BINARY_DIR}/SaxonC${SAXON_EDITION}Config.cmake
       INSTALL_DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonC${SAXON_EDITION}
)
write_basic_package_version_file(
        ${CMAKE_CURRENT_BINARY_DIR}/SaxonC${SAXON_EDITION}ConfigVersion.cmake
        VERSION ${SaxonC_VERSION}
        COMPATIBILITY SameMajorVersion
)

install(TARGETS ${saxonc}
        EXPORT "SaxonC${SAXON_EDITION}"
        LIBRARY DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}
        FILE_SET common_headers
        FILE_SET edition_headers
        FILE_SET export_header
)

install(EXPORT "SaxonC${SAXON_EDITION}"
        DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonC${SAXON_EDITION}
        NAMESPACE "SaxonC${SAXON_EDITION}::"
)

install(FILES ${CMAKE_CURRENT_BINARY_DIR}/SaxonC${SAXON_EDITION}Config.cmake
        ${CMAKE_CURRENT_BINARY_DIR}/SaxonC${SAXON_EDITION}ConfigVersion.cmake
        DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonC${SAXON_EDITION}
)
