#include "tusb.h"
#include "usb_descriptors.h"
#include "led.h"

#include "hid_app.h"

static repeating_timer_t taskInterruptTimer;
bool LED_Blink(repeating_timer_t *rt)
{
    static bool led_state = false;
    static int count = 0;

    if (led_state) {
        LED_Clear();
    } else {
        LED_SetRGB(0, 0, 255); // Blue blink
    }
    led_state = !led_state;
    count ++;

    if (count >= 6) {
        return false; // Stop the timer after 6 blinks
    }
    return true;
}

void hid_task(void)
{
    // This function can be used to implement periodic HID tasks if needed
    // Currently, no periodic tasks are required for this HID application
    return;
}

// TinyUSB device state callbacks
void tud_mount_cb(void)
{
    add_repeating_timer_ms(100, LED_Blink, NULL, &taskInterruptTimer);
    return;
}

void tud_umount_cb(void)
{
    return;
    // led_set_blink_interval(250);
}

void tud_suspend_cb(bool remote_wakeup_en)
{
    (void)remote_wakeup_en;
    return;
    // led_set_blink_interval(2500);
}

void tud_resume_cb(void)
{
    return;
    // led_set_blink_interval(tud_mounted() ? 1000 : 250);
}


// HID callbacks
void tud_hid_report_complete_cb(uint8_t instance, uint8_t const *report, uint16_t len)
{
    (void)instance;
    (void)len;
}

uint16_t tud_hid_get_report_cb(uint8_t instance, uint8_t report_id, hid_report_type_t report_type, uint8_t *buffer, uint16_t reqlen)
{
    (void)instance;
    (void)report_id;
    (void)report_type;
    (void)buffer;
    (void)reqlen;
    return 0;
}

void tud_hid_set_report_cb(uint8_t instance, uint8_t report_id, hid_report_type_t report_type, uint8_t const *buffer, uint16_t bufsize)
{
    (void)instance;
   return;
}