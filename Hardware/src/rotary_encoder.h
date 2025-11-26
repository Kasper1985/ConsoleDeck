#pragma once

#include "pico/stdlib.h"

#ifndef KY_040_H
#define KY_040_H

typedef enum
{
    KY_040_PIN_CLK = 12,
    KY_040_PIN_DT = 11,
    KY_040_PIN_SW = 10
} tKy040Pin;

typedef enum
{
    ENCODER_DIRECTION_CLOCKWISE = 1,
    ENCODER_DIRECTION_STOPPED = 0,
    ENCODER_DIRECTION_COUNTERCLOCKWISE = -1
} tEncoderDirection;

void Encoder_Init(void);
void Encoder_Task(void);

#endif