#include "pico/stdlib.h"
#include "tusb.h"
#include "usb_descriptors.h"
#include "enums.h"
#include "led.h"
#include "rotary_encoder.h"

static tGpioPinState lastStateCLK;
static tGpioPinState lastStateDT;
static tGpioPinState lastStateSW;
static tGpioPinState lastSyncedState; // Last state where both pins were the same
static tGpioPinState nextSyncedState; // Next state where both pins will be the same
static int currentEncoderPosition;
static int encoderPulses; // The encoder pulses once for every 2 positions
static tEncoderDirection lastEncoderDirection;
static tEncoderDirection currentEncoderDirection;
static bool encoderPulsed = false;

static tGpioPinState GetEncoderPinState(tKy040Pin pin)
{
    return (tGpioPinState)gpio_get(pin);
}

static void ProcessEncoderSwitch(tButtonState state)
{
    if (tud_suspended()) tud_remote_wakeup(); // Wake up host over USB if it is in suspended mode and REMOTE_WAKEUP feature is enabled by host
    if (!tud_hid_ready()) return; // Skip if HID is not ready yet

    if (state == BUTTON_PRESSED)
    {
        LED_ReportFeedback(KEYBOARD);
        uint16_t key = HID_USAGE_CONSUMER_MUTE;
        tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
    }
    else // BUTTON_RELEASED
    {
        uint16_t key = 0;
        tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
    }
}

static void ProcessEncoderPulsed(tEncoderDirection direction)
{
    if (tud_suspended()) tud_remote_wakeup(); // Wake up host over USB if it is in suspended mode and REMOTE_WAKEUP feature is enabled by host
    if (!tud_hid_ready()) return; // Skip if HID is not ready yet

    LED_ReportFeedback(ROTARY);
    uint16_t key;
    if (direction == ENCODER_DIRECTION_CLOCKWISE)
    {
        key = HID_USAGE_CONSUMER_VOLUME_INCREMENT;
        encoderPulsed = true;
    }
    else if (direction == ENCODER_DIRECTION_COUNTERCLOCKWISE)
    {
        key = HID_USAGE_CONSUMER_VOLUME_DECREMENT;
        encoderPulsed = true;
    }    
    tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
}

static void UpdateEncoderPositionAndDirection()
{
    if (currentEncoderDirection == ENCODER_DIRECTION_CLOCKWISE)
    {
        currentEncoderPosition++;
        lastEncoderDirection = ENCODER_DIRECTION_CLOCKWISE;
    }
    else if (currentEncoderDirection == ENCODER_DIRECTION_COUNTERCLOCKWISE)
    {
        currentEncoderPosition--;
        lastEncoderDirection = ENCODER_DIRECTION_COUNTERCLOCKWISE;
    }
}

static void UpdateEncoderPulses()
{
    if (lastEncoderDirection == currentEncoderDirection)
    {
        encoderPulses++;
    }
    else
    {
        encoderPulses = 1;
    }

    if (encoderPulses == 2)
    {
        encoderPulses = 0;
        ProcessEncoderPulsed(currentEncoderDirection);
    }
}

static void UpdateSyncedState()
{
    lastSyncedState = nextSyncedState;
    nextSyncedState = lastSyncedState == GPIO_LOW ? GPIO_HIGH : GPIO_LOW;
}

static void UpdateEncoderCountAndSyncState()
{
    UpdateEncoderPositionAndDirection();
    UpdateEncoderPulses();
    UpdateSyncedState();
}


void Encoder_Init(void)
{
    // Setup GPIOs
    gpio_init(KY_040_PIN_CLK);
    gpio_init(KY_040_PIN_DT);
    gpio_init(KY_040_PIN_SW);
    gpio_set_dir(KY_040_PIN_CLK, GPIO_IN);
    gpio_set_dir(KY_040_PIN_DT, GPIO_IN);
    gpio_set_dir(KY_040_PIN_SW, GPIO_IN);

    // Initial states
    lastStateCLK = GetEncoderPinState(KY_040_PIN_CLK);
    lastStateDT = GetEncoderPinState(KY_040_PIN_DT);
    lastStateSW = GetEncoderPinState(KY_040_PIN_SW);
    lastSyncedState = GPIO_LOW;
    nextSyncedState = GPIO_HIGH;
    currentEncoderPosition = 0;
    encoderPulses = 0;
    currentEncoderDirection = ENCODER_DIRECTION_STOPPED;
    lastEncoderDirection = ENCODER_DIRECTION_STOPPED;
}

void Encoder_Task(void)
{
    if (encoderPulsed)
    {
        // Release the key after pulsing
        uint16_t key = 0;
        tud_hid_report(REPORT_ID_CONSUMER_CONTROL, &key, sizeof(key));
        encoderPulsed = false;
        return;
    }

    tGpioPinState currentStateCLK = GetEncoderPinState(KY_040_PIN_CLK);
    tGpioPinState currentStateDT = GetEncoderPinState(KY_040_PIN_DT);
    tGpioPinState currentStateSW = GetEncoderPinState(KY_040_PIN_SW);

    bool encoderSwitchPressed = (currentStateSW == GPIO_LOW && lastStateSW == GPIO_HIGH);
    bool encoderSwitchReleased = (currentStateSW == GPIO_HIGH && lastStateSW == GPIO_LOW);


    bool encoderMovingClockwise = (currentStateCLK == nextSyncedState && currentStateDT == lastSyncedState);
    bool encoderMovingCounterClockwise = (currentStateDT == nextSyncedState && currentStateCLK == lastSyncedState);
    bool encoderNotMoving = (currentStateCLK == lastSyncedState && currentStateDT == lastSyncedState);
    bool bothPinsHitNextSyncState = (currentStateCLK == nextSyncedState && currentStateDT == nextSyncedState);

    if (encoderMovingClockwise)
    {
        currentEncoderDirection = ENCODER_DIRECTION_CLOCKWISE;
    }
    else if (encoderMovingCounterClockwise)
    {
        currentEncoderDirection = ENCODER_DIRECTION_COUNTERCLOCKWISE;
    }
    else if (encoderNotMoving)
    {
        currentEncoderDirection = ENCODER_DIRECTION_STOPPED;
    }
    else if (bothPinsHitNextSyncState)
    {
        UpdateEncoderCountAndSyncState();
        ProcessEncoderPulsed(ENCODER_DIRECTION_STOPPED);
    }

    lastStateCLK = currentStateCLK;
    lastStateDT = currentStateDT;
    lastStateSW = currentStateSW;

    if (encoderSwitchPressed)
    {
        ProcessEncoderSwitch(BUTTON_PRESSED);
    }
    else if (encoderSwitchReleased)
    {
        ProcessEncoderSwitch(BUTTON_RELEASED);
    }
}