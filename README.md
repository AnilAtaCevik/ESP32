# Home Assistant OS VirtualBox Kurulum Dokümantasyonu

## Genel Bakış

## Önkoşullar


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
