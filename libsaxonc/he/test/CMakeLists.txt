add_executable(tests test_processor.cpp)
target_link_libraries(tests PRIVATE Catch2::Catch2WithMain)
target_link_libraries(tests PRIVATE ${saxonc})
catch_discover_tests(tests DL_PATHS "$<TARGET_RUNTIME_DLL_DIRS:${saxonc}>")
