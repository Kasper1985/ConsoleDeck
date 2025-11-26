# Start with a base image containing the necessary tools
FROM ubuntu:latest

# Install the necessary packages
RUN apt update && apt install -y \
    cmake \
    gcc-arm-none-eabi \
    libnewlib-arm-none-eabi \
    libstdc++-arm-none-eabi-newlib \
    git \
    python3 \
    g++ \
    build-essential

# Clone the Raspberry Pi Pico SDK repository
RUN git clone https://github.com/raspberrypi/pico-sdk.git --branch master
RUN cd pico-sdk && git submodule update --init --recursive

# Set the PICO_SDK_PATH environment variable
ENV PICO_SDK_PATH=/pico-sdk

# Create a new directory for the build
RUN mkdir build

# Set the workdir to the build directory
WORKDIR /build