#include <Adafruit_NeoPixel.h>
#include <Preferences.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>
#include <WiFi.h>
#include <esp_now.h>
#include <esp_wifi.h> 

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

uint8_t seninMacAdresi[] = {0xB4, 0x3A, 0x45, 0xA5, 0x59, 0x28}; 

void ApplyCommand(String command, bool isBluetooth, bool localTrigger);
void ApplyColor(String cmd);

void sanaGonder(const String &message) {
    esp_now_send(seninMacAdresi, (const uint8_t *)message.c_str(), message.length());
    delay(10);
    esp_now_send(seninMacAdresi, (const uint8_t *)message.c_str(), message.length());
}

void receiveCallback(const esp_now_recv_info_t *recvInfo, const uint8_t *data, int dataLen) {
    char buffer[ESP_NOW_MAX_DATA_LEN + 1];
    int msgLen = min(ESP_NOW_MAX_DATA_LEN, dataLen);
    strncpy(buffer, (const char *)data, msgLen);
    buffer[msgLen] = 0;

    String incomingCommand = String(buffer);
    incomingCommand.trim();
    incomingCommand.toLowerCase();

    if (incomingCommand == "red") ApplyColor("green"); 
    else if (incomingCommand == "green") ApplyColor("red"); 
    else if (incomingCommand == "blue") ApplyColor("white"); 
    else if (incomingCommand == "white") ApplyColor("blue"); 
    else if (incomingCommand == "off") ApplyColor("off"); 
}

class MyCallbacks: public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic *pCharacteristic) {
        String command = pCharacteristic->getValue();
        command.trim();
        command.toLowerCase();
        
        if (command.length() > 0) {
            ApplyCommand(command, true, true);
        }
    }
};

class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) { deviceConnected = true; }
    void onDisconnect(BLEServer* pServer) { deviceConnected = false; pServer->startAdvertising(); }
};

void setup() {
    Serial.begin(115200);

    pixel.begin();
    pixel.setBrightness(100); 
    preferences.begin("led_memory", false);
    lastColor = preferences.getString("color", "off");
    ApplyColor(lastColor);

    BLEDevice::init("ESP32S3_Staj");
    pServer = BLEDevice::createServer();
    pServer->setCallbacks(new MyServerCallbacks());
    BLEService *pService = pServer->createService(SERVICE_UUID);
    pCharacteristic = pService->createCharacteristic(
                        CHARACTERISTIC_UUID, 
                        BLECharacteristic::PROPERTY_READ | 
                        BLECharacteristic::PROPERTY_WRITE | 
                        BLECharacteristic::PROPERTY_WRITE_NR | 
                        BLECharacteristic::PROPERTY_NOTIFY
                      );
    pCharacteristic->setCallbacks(new MyCallbacks());
    pCharacteristic->addDescriptor(new BLE2902());
    pService->start();
    BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
    pAdvertising->addServiceUUID(SERVICE_UUID);
    pAdvertising->setScanResponse(true);
    BLEDevice::startAdvertising();

    delay(200);

    WiFi.mode(WIFI_STA);
    
    esp_wifi_set_promiscuous(true);
    esp_wifi_set_channel(1, WIFI_SECOND_CHAN_NONE);
    esp_wifi_set_promiscuous(false);
    
    WiFi.disconnect();

    if (esp_now_init() == ESP_OK) {
        Serial.println("ESP-NOW Basariyla Baslatildi");
        esp_now_register_recv_cb(receiveCallback); 
        
        esp_now_peer_info_t peerInfo = {};
        memcpy(peerInfo.peer_addr, seninMacAdresi, 6);
        peerInfo.channel = 1; 
        peerInfo.encrypt = false;
        esp_now_add_peer(&peerInfo);
    }
}

void loop() {
    if (Serial.available() > 0) {
        String command = Serial.readStringUntil('\n');
        ApplyCommand(command, false, true);
    }
}

void ApplyCommand(String command, bool isBluetooth, bool localTrigger){
    command.trim(); command.toLowerCase();                         
    if (command == "red" || command == "green" || command == "blue" || command == "white" || command == "off") {
        preferences.putString("color", command); 
        ApplyColor(command);
        
        if (localTrigger) {
            sanaGonder(command);
        } //kim -> esp32 kismi cikarildi su anlik
    }
}

void ApplyColor(String cmd) {
    if (cmd == "red") pixel.setPixelColor(0, pixel.Color(255, 0, 0));
    else if (cmd == "green") pixel.setPixelColor(0, pixel.Color(0, 255, 0));
    else if (cmd == "blue") pixel.setPixelColor(0, pixel.Color(0, 0, 255));
    else if (cmd == "white") pixel.setPixelColor(0, pixel.Color(255, 255, 255));
    else if (cmd == "off") pixel.setPixelColor(0, pixel.Color(0, 0, 0));
    pixel.show();
}