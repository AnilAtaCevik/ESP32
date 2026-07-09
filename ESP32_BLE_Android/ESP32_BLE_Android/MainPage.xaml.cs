using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Text;

namespace ESP32_BLE_Android;

public partial class MainPage : ContentPage
{
    private IBluetoothLE _bluetoothLE;
    private IAdapter _adapter;
    private IDevice _esp32Device;
    private ICharacteristic _commandCharacteristic;

    private const string ServiceUuid = "4fafc201-1fb5-459e-8fcc-c5c9c331914b";
    private const string CharacteristicUuid = "beb5483e-36e1-4688-b7f5-ea07361b26a8";
    private const string TargetDeviceName = "ESP32S3_Staj";

    private bool _isScanning = false;

    public MainPage()
    {
        InitializeComponent();

        _bluetoothLE = CrossBluetoothLE.Current;
        _adapter = CrossBluetoothLE.Current.Adapter;

        _adapter.DeviceDisconnected += OnDeviceDisconnected;
        _adapter.DeviceConnectionLost += OnDeviceConnectionLost;
    }

    private async Task<bool> IzinleriKontrolEt()
    {
        PermissionStatus locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (locationStatus != PermissionStatus.Granted)
        {
            locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        PermissionStatus bluetoothStatus = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
        if (bluetoothStatus != PermissionStatus.Granted)
        {
            bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
        }

        return locationStatus == PermissionStatus.Granted && bluetoothStatus == PermissionStatus.Granted;
    }

    private async void TaraButonu_Clicked(object sender, EventArgs e)
    {
        if (_isScanning) return;

        bool izinVerildi = await IzinleriKontrolEt();
        if (!izinVerildi)
        {
            await DisplayAlert("İzin Hatası", "Uygulamanın ESP32'yi bulabilmesi için Bluetooth ve Konum izinlerine ihtiyacı var.", "Tamam");
            return;
        }

        if (!_bluetoothLE.IsOn)
        {
            await DisplayAlert("Hata", "Lütfen telefonunuzun Bluetooth özelliğini açın.", "Tamam");
            return;
        }

        try
        {
            _isScanning = true;
            DurumLabel.Text = "Durum: ESP32 Aranıyor...";
            DurumLabel.TextColor = Colors.Orange;
            TaraButonu.IsEnabled = false;
            _esp32Device = null;

            _adapter.DeviceDiscovered += async (s, a) =>
            {
                if (a.Device.Name == TargetDeviceName && _esp32Device == null)
                {
                    _esp32Device = a.Device;
                    await _adapter.StopScanningForDevicesAsync();
                    await Task.Delay(500);
                    await BaglanToEsp32();
                }
            };

            await _adapter.StartScanningForDevicesAsync();
        }
        catch (Exception ex)
        {
            DurumLabel.Text = "Tarama hatası oluştu.";
            TaraButonu.IsEnabled = true;
            _isScanning = false;
        }
    }

    private async Task BaglanToEsp32()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DurumLabel.Text = "Durum: Bağlanılıyor...";
            });

            var baglantiAyarlari = new Plugin.BLE.Abstractions.ConnectParameters(forceBleTransport: true);
            await _adapter.ConnectToDeviceAsync(_esp32Device, baglantiAyarlari);

            var service = await _esp32Device.GetServiceAsync(Guid.Parse(ServiceUuid));
            if (service != null)
            {
                _commandCharacteristic = await service.GetCharacteristicAsync(Guid.Parse(CharacteristicUuid));

                if (_commandCharacteristic != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DurumLabel.Text = "Durum: ESP32'ye Bağlandı!";
                        DurumLabel.TextColor = Colors.Green;
                        ToggleLedButtons(true);
                    });
                }
            }
        }
        catch (DeviceConnectionException)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DurumLabel.Text = "Bağlantı başarısız, tekrar deneniyor...";
                DurumLabel.TextColor = Colors.Red;
            });

            await Task.Delay(2000);
            await BaglanToEsp32();
        }
    }

    private async void OnDeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            DurumLabel.Text = "Durum: Bağlantı Koptu! Yeniden bağlanılıyor...";
            DurumLabel.TextColor = Colors.Red;
            ToggleLedButtons(false);
            await BaglanToEsp32();
        });
    }

    private void OnDeviceDisconnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DurumLabel.Text = "Durum: Bağlantı Kesildi.";
            DurumLabel.TextColor = Colors.Gray;
            ToggleLedButtons(false);
            TaraButonu.IsEnabled = true;
            _isScanning = false;
        });
    }

    private async Task CommandGonder(string komut)
    {
        if (_commandCharacteristic == null) return;

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(komut);
            await _commandCharacteristic.WriteAsync(bytes);
        }
        catch (Exception ex)
        {
            DurumLabel.Text = "Veri gönderme hatası.";
        }
    }

    private async void Kirmizi_Clicked(object sender, EventArgs e) => await CommandGonder("red");
    private async void Yesil_Clicked(object sender, EventArgs e) => await CommandGonder("green");
    private async void Mavi_Clicked(object sender, EventArgs e) => await CommandGonder("blue");
    private async void Beyaz_Clicked(object sender, EventArgs e) => await CommandGonder("white");
    private async void Sondur_Clicked(object sender, EventArgs e) => await CommandGonder("off");

    private void ToggleLedButtons(bool isEnabled)
    {
        KirmiziBtn.IsEnabled = isEnabled;
        YesilBtn.IsEnabled = isEnabled;
        MaviBtn.IsEnabled = isEnabled;
        BeyazBtn.IsEnabled = isEnabled;
        SondurBtn.IsEnabled = isEnabled;
    }
}