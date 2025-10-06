# CMakeLists.txt - 反切五笔输入法TSF接口构建配置

cmake_minimum_required(VERSION 3.10)

# 设置项目名称
project(fqwb_tsf LANGUAGES CXX)

# 设置C++标准
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 添加TSF接口库
add_library(fqwb_tsf SHARED
    fqwb_tsf.cpp
    fqwb_tsf.h
)

# 设置输出目录
set_target_properties(fqwb_tsf PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin
    LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin
    ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
)

# 包含头文件目录
target_include_directories(fqwb_tsf PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})

# 链接必要的库
target_link_libraries(fqwb_tsf PRIVATE
    user32.lib
    gdi32.lib
    imm32.lib
    ole32.lib
    oleaut32.lib
    uuid.lib
    msctf.lib
)

# 定义DLL导出宏
target_compile_definitions(fqwb_tsf PRIVATE
    UNICODE
    _UNICODE
    WIN32_LEAN_AND_MEAN
    NOMINMAX
)

# 添加示例程序
add_executable(fqwb_tsf_example
    fqwb_tsf_example.cpp
)

# 设置示例程序输出目录
target_include_directories(fqwb_tsf_example PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})
set_target_properties(fqwb_tsf_example PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin
)

# 链接TSF库到示例程序
target_link_libraries(fqwb_tsf_example PRIVATE
    fqwb_tsf
    user32.lib
)

# 添加数据目录
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin/Data)

# 添加安装规则
install(TARGETS fqwb_tsf fqwb_tsf_example
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION bin
    ARCHIVE DESTINATION lib
)

# 复制数据文件到安装目录
install(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/Data/
    DESTINATION bin/Data
    FILES_MATCHING PATTERN "*.dic"
)

# 添加构建示例数据文件的规则
add_custom_command(
    TARGET fqwb_tsf POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/Data/example.dic ${CMAKE_BINARY_DIR}/bin/Data/example.dic
)

# 添加一个帮助目标来显示项目信息
add_custom_target(show_info
    COMMAND ${CMAKE_COMMAND} -E echo "项目: ${PROJECT_NAME} (风琴五笔输入法)"
    COMMAND ${CMAKE_COMMAND} -E echo "C++标准: ${CMAKE_CXX_STANDARD}"
    COMMAND ${CMAKE_COMMAND} -E echo "构建类型: $<CONFIG>"
    COMMAND ${CMAKE_COMMAND} -E echo "输出目录: ${CMAKE_BINARY_DIR}/bin"
)

# 如果使用Visual Studio生成器，设置启动项目
if (CMAKE_GENERATOR MATCHES "Visual Studio")
    set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT fqwb_tsf_example)
endif()