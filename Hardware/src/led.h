#pragma once

#include <stdint.h>
#include <stdbool.h>
#include "enums.h"

#define NEOPIXEL_PIN 16
#define PIO_SM 0

void LED_Init(void);
void LED_Clear(void);
void LED_SetRGB(uint8_t r, uint8_t g, uint8_t b);
void LED_ReportMessage(tLEDMessage message);