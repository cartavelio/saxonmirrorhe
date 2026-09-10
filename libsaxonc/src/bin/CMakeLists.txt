add_executable(Transform Transform.c)

target_link_libraries(Transform PRIVATE ${saxonc})

add_executable(Query Query.c)
target_link_libraries(Query PRIVATE ${saxonc})

install(TARGETS Transform Query)
