#include <Adafruit_NeoPixel.h>
#include <Preferences.h>

#define RGB_PIN 48
#define NUM_PIXELS 1

Adafruit_NeoPixel pixel(NUM_PIXELS, RGB_PIN, NEO_GRB + NEO_KHZ800);
Preferences preferences;

String sonRenk = "sondur";

void setup() {
  pixel.begin();
  pixel.setBrightness(100);
  Serial.begin(115200);

  preferences.begin("led_hafiza", false);
  
  sonRenk = preferences.getString("renk", "sondur");
  
  renkUygula(sonRenk);
}

void loop() {
  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n'); 
    command.trim();                                 
    command.toLowerCase();                         

    if (command == "kirmizi" || command == "red" || 
        command == "yesil" || command == "green" || 
        command == "mavi" || command == "blue" || 
        command == "beyaz" || command == "white" || 
        command == "sondur" || command == "off") {
      
      preferences.putString("renk", command);
      
      renkUygula(command);
    }
    else if (command == "kim") {
      Serial.println("ben_esp32");
    }
  }
}

void renkUygula(String cmd) {
  if (cmd == "kirmizi" || cmd == "red") {
    pixel.setPixelColor(0, pixel.Color(255, 0, 0));
  } 
  else if (cmd == "yesil" || cmd == "green") {
    pixel.setPixelColor(0, pixel.Color(0, 255, 0));
  } 
  else if (cmd == "mavi" || cmd == "blue") {
    pixel.setPixelColor(0, pixel.Color(0, 0, 255));
  } 
  else if (cmd == "beyaz" || cmd == "white") {
    pixel.setPixelColor(0, pixel.Color(255, 255, 255));
  } 
  else if (cmd == "sondur" || cmd == "off") {
    pixel.setPixelColor(0, pixel.Color(0, 0, 0));
  }
  pixel.show();
}

