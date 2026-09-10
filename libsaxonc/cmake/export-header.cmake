include(GenerateExportHeader)
generate_export_header(${saxonc} BASE_NAME saxonc)
target_sources(${saxonc}
    PUBLIC
        FILE_SET export_header
        TYPE HEADERS
        BASE_DIRS
            ${CMAKE_CURRENT_BINARY_DIR}
        FILES
            ${CMAKE_CURRENT_BINARY_DIR}/saxonc_export.h
)
