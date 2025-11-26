#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "led.h"
#include "board.h"
#include "rotary_encoder.h"
#include "hid_app.h"
#include "usb_descriptors.h"
#include "main.h"

static repeating_timer_t taskInterruptTimer;

int main(void)
{
	stdio_init_all();

	LED_Init();
	Board_Init();
	Encoder_Init();

	// init device stack on configured roothub port
	tusb_rhport_init_t dev_init = {.role = TUSB_ROLE_DEVICE, .speed = TUSB_SPEED_AUTO};
	tusb_init(BOARD_TUD_RHPORT, &dev_init);

	add_repeating_timer_ms(READ_INTERVAL_MS, ReadOnInterrupt, NULL, &taskInterruptTimer);

	while (true) {
		__wfi(); // Wait for interrupt
	}
}

bool ReadOnInterrupt(repeating_timer_t *rt)
{
	tud_task();
	//hid_task();
	Board_Task();
	Encoder_Task();

	return true;
}
