#include "pico/stdlib.h"
#include "tusb.h"
#include "usb_descriptors.h"
#include "enums.h"
#include "led.h"
#include "board.h"

static bool lastButtonStates[BOARD_BUTTON_COUNT] = {GPIO_HIGH}; // Initially all buttons unpressed (pull-up)

static tGpioPinState GetPinState(uint8_t pin)
{
    return (tGpioPinState)gpio_get(pin);
}

static void ProcessButton(uint8_t buttonIndex, tButtonState state)
{
    if (tud_suspended()) tud_remote_wakeup(); // Wake up host over USB if it is in suspended mode and REMOTE_WAKEUP feature is enabled by host
    if (!tud_hid_ready()) return; // Skip if HID is not ready yet

    uint16_t key = 0;
    if (state == BUTTON_PRESSED)
    {
        key = buttonIndex == 0 ? HID_USAGE_CONSUMER_PLAY_PAUSE : 0xFFF0 + buttonIndex; // Example mapping
    }
    tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
    /*switch(buttonIndex)
    {
        // Play / Pause button
        case 0:
            if (state == BUTTON_PRESSED)
            {
                key = HID_USAGE_CONSUMER_PLAY_PAUSE;
                tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
            }
            else // BUTTON_RELEASED
            {
                uint16_t key = 0;
                tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
            }
            break;

        // Map other buttons to special code starting with 0xE0 (F13+)
        default:
            if (state == BUTTON_PRESSED)
            {
                uint16_t key = 0x0FF0 + buttonIndex; // Example mapping
                tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
                //uint8_t keycode[6] = {HID_KEY_F13 + buttonIndex - 1, HID_KEY_CONTROL_RIGHT, HID_KEY_SHIFT_RIGHT};
                //tud_hid_keyboard_report(REPORT_ID_KEYBOARD, 0, keycode);
            }
            else // BUTTON_RELEASED
            {
                uint16_t key = 0;
                tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
                //tud_hid_keyboard_report(REPORT_ID_KEYBOARD, 0, NULL);
            }
            break;
    }*/
}


void Board_Init(void)
{
    // Initialize all button GPIOs, set as input with pull-up resistors
    for (uint8_t i = 0; i < BOARD_BUTTON_COUNT; ++i)
    {
        gpio_init(BOARD_FIRST_BUTTON_GPIO + i);
        gpio_set_dir(BOARD_FIRST_BUTTON_GPIO + i, GPIO_IN);
        gpio_pull_up(BOARD_FIRST_BUTTON_GPIO + i);
    }
}

void Board_Task(void)
{
    bool is_any_button_pressed = false;
    for (uint8_t i = 0; i < BOARD_BUTTON_COUNT; ++i)
    {
        tGpioPinState currentState = GetPinState(BOARD_FIRST_BUTTON_GPIO + i);
        bool btn_pressed = (currentState == GPIO_LOW && lastButtonStates[i] == GPIO_HIGH);
        bool btn_released = (currentState == GPIO_HIGH && lastButtonStates[i] == GPIO_LOW);
        if (btn_pressed)
        {
            is_any_button_pressed = true;
            ProcessButton(i, BUTTON_PRESSED);
        }
        else if (btn_released)
        {
            ProcessButton(i, BUTTON_RELEASED);
        }

        // Update last button state
        lastButtonStates[i] = currentState;
    }

    if (is_any_button_pressed) LED_ReportFeedback(KEYBOARD); // Indicate button press
}