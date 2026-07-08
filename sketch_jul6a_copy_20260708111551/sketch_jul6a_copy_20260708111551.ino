#include <Adafruit_NeoPixel.h>
#include <Preferences.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>

#define RGB_PIN 48
#define NUM_PIXELS 1

#define SERVICE_UUID        "4fafc201-1fb5-459e-8fcc-c5c9c331914b"
#define CHARACTERISTIC_UUID "beb5483e-36e1-4688-b7f5-ea07361b26a8"

Adafruit_NeoPixel pixel(NUM_PIXELS, RGB_PIN, NEO_GRB + NEO_KHZ800);
Preferences preferences;

BLEServer* pServer = nullptr;
BLECharacteristic* pCharacteristic = nullptr;
bool deviceConnected = false;
String lastColor = "off";

void ApplyCommand(String command, bool isBluetooth);
void ApplyColor(String cmd);

class MyCallbacks: public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic *pCharacteristic) {
        std::string value = pCharacteristic->getValue();
        if (value.length() > 0) {
            String command = "";
            for (int i = 0; i < value.length(); i++) {
                command += value[i];
            }
            ApplyCommand(command, true);
        }
    }
};

class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
        deviceConnected = true;
    }
    void onDisconnect(BLEServer* pServer) {
        deviceConnected = false;
        pServer->startAdvertising(); 
    }
};

void setup() {
    Serial.begin(115200);

    // BLE SERVER SETUP
    BLEDevice::init("ESP32S3_Staj");
    pServer = BLEDevice::createServer();
    pServer->setCallbacks(new MyServerCallbacks());

    BLEService *pService = pServer->createService(SERVICE_UUID);

    pCharacteristic = pService->createCharacteristic(
                        CHARACTERISTIC_UUID,
                        BLECharacteristic::PROPERTY_READ |
                        BLECharacteristic::PROPERTY_WRITE |
                        BLECharacteristic::PROPERTY_NOTIFY
                      );

    pCharacteristic->setCallbacks(new MyCallbacks());
    pCharacteristic->addDescriptor(new BLE2902());

    pService->start();
    BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
    pAdvertising->addServiceUUID(SERVICE_UUID);
    pAdvertising->setScanResponse(true);
    pAdvertising->setMinPreferred(0x06);  
    pAdvertising->setMinPreferred(0x12);
    BLEDevice::startAdvertising();

    // LED SETUP
    pixel.begin();
    pixel.setBrightness(100);

    preferences.begin("led_memory", false);
    lastColor = preferences.getString("color", "off");
    ApplyColor(lastColor);
}

void loop() {
    if (Serial.available() > 0) {
        String command = Serial.readStringUntil('\n');
        ApplyCommand(command, false);
    }
}

void ApplyCommand(String command, bool isBluetooth){
    command.trim();                                 
    command.toLowerCase();                         

    if (command == "red" || command == "green" || command == "blue" || command == "white" || command == "off") {
        preferences.putString("color", command);
        ApplyColor(command);
    }
    else if (command == "kim") {
        if (!isBluetooth){
            Serial.println("ben_esp32");
        }
        else {
            pCharacteristic->setValue("ben_esp32");
            pCharacteristic->notify();
        }
    }
}

void ApplyColor(String cmd) {
    if (cmd == "red") {
        pixel.setPixelColor(0, pixel.Color(255, 0, 0));
    } 
    else if (cmd == "green") {
        pixel.setPixelColor(0, pixel.Color(0, 255, 0));
    } 
    else if (cmd == "blue") {
        pixel.setPixelColor(0, pixel.Color(0, 0, 255));
    } 
    else if (cmd == "white") {
        pixel.setPixelColor(0, pixel.Color(255, 255, 255));
    } 
    else if (cmd == "off") {
        pixel.setPixelColor(0, pixel.Color(0, 0, 0));
    }
    pixel.show();
}