#pragma once

#include "enums.h"
#include <stdint.h>

#define NEOPIXEL_PIN 16
#define PIO_SM 0
#define FEEDBACK_BRIGHTNESS 50 // Brightness level (0-255)
#define FEEDBACK_DURATION_MS 200 // Duration to show feedback in milliseconds

typedef enum
{
    NONE = 0,
    KEYBOARD,
    ROTARY,
    ERROR,
} tLEDFeedbackType;

void LED_Init(void);
void LED_Clear(void);
void LED_SetRGB(uint8_t r, uint8_t g, uint8_t b);
void LED_ReportFeedback(tLEDFeedbackType message);