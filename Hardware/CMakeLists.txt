cmake_minimum_required(VERSION 3.20)

# Import the Pico SDK
include(pico_sdk_import.cmake)

# Project name and languages
project(HID_Device C CXX ASM)
set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)

# initialize the Raspberry Pi Pico SDK
pico_sdk_init()


# Add executable
add_executable(HID_Device
    src/main.c
    src/board.c
    src/hid_app.c
    src/led.c
    src/usb_descriptors.c
    src/rotary_encoder.c
    src/ws2812.pio
)

# Add PIO header generation
pico_generate_pio_header(HID_Device
    ${CMAKE_CURRENT_LIST_DIR}/src/ws2812.pio
    OUTPUT_DIR ${CMAKE_CURRENT_LIST_DIR}/src
)

# Enable USB (disable UART if not needed)
pico_enable_stdio_usb(HID_Device 0)
pico_enable_stdio_uart(HID_Device 1)


# Link required libraries
target_link_libraries(HID_Device
    pico_stdlib
    pico_unique_id
    tinyusb_device
    hardware_pio
)

target_include_directories(HID_Device PRIVATE
    src
    ${PICO_SDK_PATH}/lib/tinyusb/hw
)


# Create UF2, bin, and hex files
pico_add_extra_outputs(HID_Device)