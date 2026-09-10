add_executable(Validate Validate.c)
target_link_libraries(Validate PRIVATE ${saxonc})
target_link_libraries(Validate PRIVATE "${SAXON_LIBRARY}")
target_include_directories(Validate PRIVATE
    PATHS "${SAXON_LIBRARY_INCLUDE_DIR}"
)

install(TARGETS Validate)
