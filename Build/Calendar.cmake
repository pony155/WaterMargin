include_guard(GLOBAL)

find_program(WATERMARGIN_DOTNET_EXECUTABLE dotnet REQUIRED)
set(WATERMARGIN_ROOT "${CMAKE_CURRENT_LIST_DIR}/..")

if(CMAKE_BUILD_TYPE)
    set(WATERMARGIN_DOTNET_CONFIGURATION ${CMAKE_BUILD_TYPE})
else()
    set(WATERMARGIN_DOTNET_CONFIGURATION Debug)
endif()

add_custom_target(WaterMarginCalendarRuntime
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${WATERMARGIN_DOTNET_EXECUTABLE} build
        ${WATERMARGIN_ROOT}/Source/WaterMargin.Calendar/WaterMargin.Calendar.csproj
        --configuration ${WATERMARGIN_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        -p:RestoreIgnoreFailedSources=true
    WORKING_DIRECTORY ${WATERMARGIN_ROOT}
    COMMENT "Building the WaterMargin ancient Chinese calendar runtime"
    VERBATIM
)

if(BUILD_TESTING)
    add_custom_target(WaterMarginCalendarUnitTests
        COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
            ${WATERMARGIN_DOTNET_EXECUTABLE} build
            ${WATERMARGIN_ROOT}/Tests/WaterMargin.Calendar.Tests/WaterMargin.Calendar.Tests.csproj
            --configuration ${WATERMARGIN_DOTNET_CONFIGURATION}
            -p:Platform=AnyCPU
            -p:RestoreIgnoreFailedSources=true
        WORKING_DIRECTORY ${WATERMARGIN_ROOT}
        COMMENT "Compiling the WaterMargin calendar unit-test target (execution is CI-owned)"
        VERBATIM
    )
    add_dependencies(WaterMarginCalendarUnitTests WaterMarginCalendarRuntime)

    add_test(NAME WaterMarginCalendarTests
        COMMAND ${WATERMARGIN_DOTNET_EXECUTABLE} run
            --project ${WATERMARGIN_ROOT}/Tests/WaterMargin.Calendar.Tests/WaterMargin.Calendar.Tests.csproj
            --configuration ${WATERMARGIN_DOTNET_CONFIGURATION}
            -p:Platform=AnyCPU
            --no-build
            --no-restore
    )
    set_tests_properties(WaterMarginCalendarTests PROPERTIES
        LABELS WaterMarginCalendarUnit
    )
endif()
