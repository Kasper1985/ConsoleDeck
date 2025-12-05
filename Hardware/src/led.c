#include "hardware/pio.h"
#include "pico/stdlib.h"
#include "ws2812.pio.h"
#include "enums.h"
#include "led.h"

static PIO pio = pio0;
static uint sm = PIO_SM;
static bool isOn = false;
static alarm_id_t led_feedback_alarm_id = 0;

static int64_t led_feedback_alarm_cb(alarm_id_t id, void *user_data)
{
    LED_Clear();
    isOn = false;
    return 0; // Do not repeat
}


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

void LED_ReportFeedback(tLEDFeedbackType type)
{
    if (isOn) return; // Ignore if already showing feedback
    
    switch (type)
    {
        case KEYBOARD:
            LED_SetRGB(0, FEEDBACK_BRIGHTNESS, 0); // Green for keyboard
            break;
        case ROTARY:
            LED_SetRGB(0, 0, FEEDBACK_BRIGHTNESS); // Blue for rotary
            break;
        case ERROR:
            LED_SetRGB(FEEDBACK_BRIGHTNESS, 0, 0); // Red for error
            break;
        
        default:
            LED_SetRGB(FEEDBACK_BRIGHTNESS, FEEDBACK_BRIGHTNESS, FEEDBACK_BRIGHTNESS); // Turn on white for unknown
            break;
    }

    isOn = true;

    // Cancel any existing alarm before scheduling a new one
    if (led_feedback_alarm_id) cancel_alarm(led_feedback_alarm_id);
    led_feedback_alarm_id = add_alarm_in_ms(FEEDBACK_DURATION_MS, led_feedback_alarm_cb, NULL, false);
}