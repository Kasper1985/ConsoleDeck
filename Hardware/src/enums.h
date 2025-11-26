#pragma once

typedef enum
{
    GPIO_LOW = 0,
    GPIO_HIGH = 1
} tGpioPinState;

typedef enum
{
    NONE = 0,
    MESSAGE_KEYBOARD = 1,
    MESSAGE_ERROR  = 2,
    MESSAGE_ROTARY = 3
} tLEDMessage;

typedef enum
{
    BUTTON_PRESSED = 0,
    BUTTON_RELEASED = 1
} tButtonState;