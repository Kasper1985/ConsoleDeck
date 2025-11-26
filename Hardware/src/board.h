#pragma once

#include <stdint.h>
#include <stdbool.h>

#define BOARD_BUTTON_COUNT 10
#define BOARD_FIRST_BUTTON_GPIO 0
#define KEYCODE_START 0xF0

void Board_Init(void);
void Board_Task(void);