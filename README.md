# VM ile HAOS Kurulumu ve ESPHome Eklentisi Kullanımı

## Genel Bakış

Bu dokümantasyon, sıfırdan bir akıllı ev otomasyon altyapısının kurulmasını ve fiziksel mikrodenetleyici cihazların sisteme entegre edilmesini adım adım açıklamaktadır. Rehber, sırasıyla aşağıdaki süreçleri kapsamaktadır:

1. **Sanal Makine (VM) Üzerine HAOS Kurulumu:** Oracle VM VirtualBox kullanılarak Home Assistant işletim sisteminin (HAOS) bilgisayar üzerinde yapılandırılması ve çalıştırılması.
2. **ESPHome Eklentisi ile Donanım Yazılımı Yüklenmesi:** Home Assistant içerisine ESPHome eklentisinin kurularak ESP (ESP32-S3 ve ESP-12F) cihazlarına gerekli donanım yazılımlarının (firmware/kod) yüklenmesi.
3. **ESP Cihazlarının Home Assistant Üzerinden Kontrol Edilmesi:** Kodlanan ve yapılandırılan cihazların sisteme dahil edilerek Home Assistant panosu üzerinden (RGB LED renk/parlaklık ayarı, röle aç/kapa işlemleri vb.) anlık olarak yönetilmesi.

## Önkoşullar

<details>
<summary><b>Oracle VirtualBox Kurulumu</b></summary>
*https://www.oracle.com/tr/virtualization/technologies/vm/downloads/virtualbox-downloads.html linkini takip ediniz
* Açılan sayfadaki tablodan işletim sisteminizin karşısındaki indirme butonuna basınız
* İndirdiğiniz dosyayı açarak bilgisayarınıza kurunuz

</details>
 
<details>
<summary><b>Home Assistant Kurulumu (.vdi)</b></summary>
*https://www.home-assistant.io/installation/windows linkini takip ediniz
* Açılan sayfada (.vdi) seçeneğini indiriniz
* İndirdiğiniz zip dosyasının içinden .vdi dosyanızı çıkarınız
</details>

ESP32-S3
HW 622
USB-C
USB-TTL
Jumper

---

## 1. Sanal Makine (VM) Oluşturma

1. Oracle VM VirtualBox'ı açın.
2. Sol üst köşede bulunan **Yeni** butonuna tıklayın.
3. Açılan konfigürasyon penceresinde aşağıdaki bilgileri eksiksiz doldurun:
   * **VM Adı:** "Home Assistant"
   * **İS Türü:** Linux
   * **İS Sürümü:** Other Linux (64-bit)
4. Bilgileri girdikten sonra sağ altta bulunan **Bitir** butonuna tıklayın.

## 2. Sanal Makine Ayarlarını Yapılandırma

1. Sol menüde bulunan **Makineler** listesinden, az önce oluşturduğunuz sanal makineyi seçin.
2. Sol üstte bulunan **Ayarlar** butonuna tıklayın.
3. **Sistem > Anakart** sekmesine gidin:
   * **Ana Bellek** ayarını en az **2048 MB** olarak ayarlayın.
   * **UEFI'yi Etkinleştir** kutucuğunu işaretleyin.
4. **Sistem > İşlemci** sekmesine geçin:
   * **İşlemci Sayısı** ayarını en az **2** olarak ayarlayın.
5. **Depolama** sekmesine gidin:
   * **Aygıtlar** bölümünde, **Denetleyici: SATA** altında halihazırda bulunan varsayılan `.vdi` dosyasına sağ tıklayın ve **Eklemeyi Kaldır** seçeneğini seçin.
   * **Denetleyici: SATA** yazısına sol tıklayın ve hemen yanında bulunan **Sabit disk ekle** butonuna basın.
   * Açılan pencerede sol üstte bulunan **Ekle** butonuna tıklayın.
   * Dosya yolunu izleyerek daha önce zip içerisinden çıkardığınız Home Assistant `.vdi` dosyasını bulup seçin.
6. **Ağ** sekmesine geçin:
   * **Şuna takılı:** ayarını açılır menüden **Köprü Bağdaştırıcısı** olarak değiştirin.
7. Belirtilen bütün ayarları yaptıktan sonra sağ altta bulunan **Tamam** butonuna basarak pencereyi kapatın.

## 3. Sanal Makineyi Başlatma

1. Ana VirtualBox ekranına döndükten sonra, sol üstte bulunan **Başlat** butonuna basarak sanal makinenizi çalıştırın.

## 4. Tarayıcıdan Home Assistant'a Giriş Yapma

1. Sanal makineyi başlattıktan sonra, konsol ekranının açılmasını bekleyin. Ekranda komut satırının en altında `ha >` ibaresi görünene kadar bekleyin.
2. Bu ekranda gösterilen ağ bilgileri arasından IPv4 adresinizi ve Home Assistant URL'sinin sonunda bulunan port numarasını not edin.
3. Bilgisayarınızda bir web tarayıcısı açın.
4. Adres çubuğuna, not ettiğiniz IP adresini yazın, sonuna iki nokta üst üste (`:`) koyun ve port numarasını ekleyerek giriş yapın (Örnek format: `192.168.1.125:8123`).
5. Sayfaya giriş yaptıktan sonra, Home Assistant kurulum ve kayıt ekranı gelene kadar sistemin yüklenmesini bekleyin.
6. Karşınıza çıkan ekrandaki kayıt formunda istenilen bilgileri eksiksiz doldurun ve giriş işlemini tamamlayın.

## 5. Home Assistant'a ESPHome Eklentisi Kurulumu

1. Home Assistant arayüzünde, sol menüde bulunan **Ayarlar** (Settings) sekmesine tıklayın.
2. Açılan menüden **Eklentiler** (Add-ons) bölümüne girin.
3. Ekranın sağ alt köşesinde bulunan **Eklenti Mağazası** (Add-on Store) / Uygulama Yükle butonuna tıklayın.
4. Arama çubuğuna `ESPHome` yazın ve çıkan sonuçlardan **ESPHome** (veya ESPHome Device Builder) seçeneğine tıklayın.
5. Açılan eklenti detay sayfasında, üst kısımda bulunan **Yükle** (Install) butonuna tıklayın ve kurulumun tamamlanmasını bekleyin.
6. Kurulum bittikten sonra, aynı sayfada bulunan kontroller bölümündeki **Kenar Çubuğunda Göster** (Show in sidebar) seçeneğini aktif hale getirin. Bu işlem, ESPHome eklentisine daha hızlı erişebilmeniz için sol menüye bir kısayol ekleyecektir.
7. Sayfada bulunan **Başlat** (Start) butonuna basarak eklentiyi çalıştırın.

## 6. ESPHome Cihaz Oluşturma

1. Sol menüde bulunan **ESPHome** sekmesine tıklayın.
2. Açılan sayfanın sağ alt köşesinde bulunan **Create Device** butonuna tıklayın.
3. Karşınıza çıkan seçeneklerden **Create new project** seçeneğine tıklayın.

<details>
  <summary><b>4. ESP32-S3 İçin Cihaz Ayarları</b></summary>
  
  * Arama çubuğuna `esp32-s3` yazın.
  * Çıkan sonuçlardan en üstteki **Generic ESP32-S3 Board** kartının sağ altında bulunan **Select** butonuna basın.
  * Açılan isim kısmına cihaz adı olarak `esp32-s3` yazın.
  * İşlemi tamamlamak için **Finish setup** butonuna tıklayın.
</details>

<details>
  <summary><b>5. ESP-12F İçin Cihaz Ayarları</b></summary>
  
  * Arama çubuğuna `NodeMCU v2` yazın.
  * Çıkan sonuçlardan en üstteki **NodeMCU v2 (ESP8266)** kartının sağ altında bulunan **Select** butonuna basın.
  * Açılan isim kısmına cihaz adı olarak `esp-12f` yazın.
  * İşlemi tamamlamak için **Finish setup** butonuna tıklayın.
</details>

<details>
  <summary><b>6. Herhangi Bir Cihaz Ekleme</b></summary>
  
  * Ekleyeceğiniz cihazı bilgisayarınıza USB kablosu ile bağlayın.
  * Sanal makinenizin çalıştığı pencereyi açın.
  * Üst menüden **Aygıtlar > USB** yolunu izleyin ve bağladığınız USB cihazını çıkan listeden seçin.
  * Tarayıcınıza geri dönüp **Connect your board** butonuna tıklayın ve açılan menüden cihazınızı seçin.
  * Çıkan sonuçlardan en üstteki kartın sağ altında bulunan **Select** butonuna basın.
  * Açılan isim kısmına cihaz adını yazın.
  * İşlemi tamamlamak için **Finish setup** butonuna tıklayın.
</details>

## 7. Cihazlara Donanım Yazılımı Yükleme

1. Sol menüde bulunan **ESPHome** sekmesine tıklayın.

<details>
<summary><b>ESP32-S3 Yazılımı</b></summary>

* Açılan sayfada oluşturduğunuz `esp32-s3` cihazına tıklayın.
* Sol menüde bulunan Wi-Fi ayarlarındaki **SSID** ve **Password** kısımlarını, bilgisayarınızın bağlı olduğu internet ağının adı ve şifresi ile aynı olacak şekilde değiştirip yanlarındaki **Save** butonlarına tıklayın.
* Sağ tarafta bulunan YAML formatlı kodun en altına aşağıdaki kodu ekleyin. Bu kod, cihazın üzerinde bulunan dahili RGB LED'in rengini ve parlaklığını değiştirmemizi sağlar:

<pre><code>light:
  - platform: esp32_rmt_led_strip # LED kontrol platformunu belirler
    pin: GPIO48                   # Veri iletişiminin yapılacağı pini ayarlar
    num_leds: 1                   # Kontrol edilecek LED sayısını belirtir
    rgb_order: GRB                # Renklerin işlenme sırasını tanımlar
    chipset: WS2812               # Kullanılan LED çipi modelini belirtir
    name: "Dahili RGB LED"        # Arayüzde görünecek isim etiketini belirler</code></pre>

* Az önceki kodun hemen altına aşağıdaki kodu yapıştırın. Bu kod, cihazın içindeki sensörler aracılığıyla Wi-Fi sinyal kalitesini veri olarak göndermesini sağlar:

<pre><code>sensor:
  - platform: wifi_signal         # Wi-Fi sinyal gücünü okuyan platformu belirler
    name: "ESP32 Wi-Fi Sinyali"   # Arayüzde görünecek sensör ismini belirler
    update_interval: 10s          # Verinin 10 saniyede bir güncellenmesini sağlar</code></pre>

* Sağ altta bulunan **Save** butonuna ve ardından **Install** butonuna basın.
* Açılan pencereden **Plug into this computer** seçeneğine tıklayın.
* Compile (derleme) işlemi bittiğinde sağ altta bulunan **Open USB flasher** butonuna tıklayın.
* USB kablonuzu bilgisayarınıza ve cihazınızın **COM** yazan girişine takın.
* Açılan sekmedeki **Connect & install** butonuna tıklayın ve listeden portunuzu seçin.
* Konsol ekranının en altında `Leaving...` yazısını gördükten sonra, sayfa kapanana kadar butona basılı tutun/basın.

</details>

<details>
<summary><b>ESP-12F Yazılımı</b></summary>

* Açılan sayfada oluşturduğunuz `esp-12f` cihazına tıklayın.
* Sol menüde bulunan Wi-Fi ayarlarındaki **SSID** ve **Password** kısımlarını, bilgisayarınızın bağlı olduğu internet ağının adı ve şifresi ile aynı olacak şekilde değiştirip yanlarındaki **Save** butonlarına tıklayın.
* Sağ tarafta bulunan YAML formatlı kodun en altına aşağıdaki kodu ekleyin. Bu kod, cihazın üzerinde bulunan röleyi açıp kapatmamızı sağlar:

<pre><code>switch:
  - platform: gpio                # Genel amaçlı giriş/çıkış pini kontrol platformunu belirler
    name: "Röle"                  # Arayüzde görünecek anahtar (switch) ismini belirler
    pin:
      number: GPIO4               # Rölenin bağlı olduğu fiziksel pini ayarlar
    id: role_1                    # Sistemin arka planda tanıyacağı benzersiz kimliği atar</code></pre>

* Sağ altta bulunan **Save** butonuna ve ardından **Install** butonuna basın.
* Açılan pencereden **Plug into this computer** seçeneğine tıklayın.
* Compile (derleme) işlemi bittiğinde sağ altta bulunan **Open USB flasher** butonuna tıklayın.
* Cihazınızın üzerinde bulunan 2'li pine jumper'ınızı takın (Cihazı flash moduna almak için).
* USB kablonuzu bilgisayarınıza ve cihazınızın 4'lü pinine kırmızı kablo dışa, siyah kablo içe gelecek şekilde takın.
* Açılan sekmedeki **Connect & install** butonuna tıklayın ve listeden portunuzu seçin.
* Konsol ekranının en altında `Leaving...` yazısını gördükten sonra, sayfa kapanana kadar butona basılı tutun/basın.
* USB kablonuzu bilgisayarınızdan çıkarın.
* Jumper'ı 2'li pinden çıkarın (Cihazı flash modundan çıkarmak için).
* USB kablonuzu bilgisayarınıza geri takın.

</details>

> 💡 **Not:** Bu adımları izleyip cihazınıza ilk yazılımları attıktan sonra, sonraki güncellemelerde **Plug into this computer** yerine **On the network** seçeneği ile yazılımlarınızı kablosuz olarak yükleyebilirsiniz.
