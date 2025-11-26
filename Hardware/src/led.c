#include "hardware/pio.h"
#include "ws2812.pio.h"
#include "enums.h"
#include "led.h"

static PIO pio = pio0;
static uint sm = PIO_SM;

void LED_Init(void)
{
    uint offset = pio_add_program(pio, &ws2812_program);
    ws2812_program_init(pio, sm, offset, NEOPIXEL_PIN, 800000, false);
    LED_SetRGB(0, 0, 0); // Turn off at start
}

void LED_SetRGB(uint8_t r, uint8_t g, uint8_t b)
{
    // WS2812 expects GRB order
    uint32_t grb = ((g << 16) | (r << 8) | b);
    pio_sm_put_blocking(pio, sm, grb << 8u);
}

void LED_Clear(void)
{
    LED_SetRGB(0, 0, 0);
}

void LED_ReportMessage(tLEDMessage message)
{
    switch (message)
    {
        case MESSAGE_KEYBOARD:
            LED_SetRGB(0, 255, 0); // Green for keyboard
            break;
        case MESSAGE_ERROR:
            LED_SetRGB(255, 0, 0); // Red for error
            break;
        case MESSAGE_ROTARY:
            LED_SetRGB(0, 0, 255); // Blue for rotary
            break;
        
        default:
            LED_SetRGB(0, 0, 0); // Turn off for NONE or unknown
            break;
    }
}