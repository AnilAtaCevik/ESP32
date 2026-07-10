using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

namespace ESP32_BLE_Android;

public partial class MainPage : ContentPage
{
    private IBluetoothLE _bluetoothLE;
    private IAdapter _adapter;
    private IDevice? _esp32Device;
    private ICharacteristic? _commandCharacteristic;

    private bool _isScanning = false;
    private ObservableCollection<IDevice> _bulunanCihazlar = new ObservableCollection<IDevice>();
    private List<IDevice> _geciciCihazlar = new List<IDevice>();

    public MainPage()
    {
        InitializeComponent();

        CihazListesi.ItemsSource = _bulunanCihazlar;

        _bluetoothLE = CrossBluetoothLE.Current;
        _adapter = CrossBluetoothLE.Current.Adapter;

        _adapter.DeviceDisconnected += OnDeviceDisconnected;
        _adapter.DeviceConnectionLost += OnDeviceConnectionLost;
        _bluetoothLE.StateChanged += OnBluetoothStateChanged;
        _adapter.DeviceDiscovered += OnDeviceDiscovered;
    }

    private void OnDeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs a)
    {
        if (a.Device != null && !string.IsNullOrEmpty(a.Device.Name))
        {
            if (!_geciciCihazlar.Any(d => d.Id == a.Device.Id))
            {
                _geciciCihazlar.Add(a.Device);
            }
        }
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CihazlariTaraAsync();
    }

    private async void YenileButonu_Clicked(object sender, EventArgs e)
    {
        await CihazlariTaraAsync();
    }

    private async Task CihazlariTaraAsync()
    {
        if (_isScanning) return;

        bool izinVerildi = await IzinleriKontrolEt();
        if (!izinVerildi)
        {
            await DisplayAlert("İzin Hatası", "Uygulamanın çalışması için Bluetooth ve Konum izinlerine ihtiyacı var.", "Tamam");
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
            DurumLabel.Text = "Durum: Cihazlar Aranıyor...";
            DurumLabel.TextColor = Colors.Orange;
            YenileButonu.IsEnabled = false;

            _bulunanCihazlar.Clear();
            _geciciCihazlar.Clear();

            await _adapter.StartScanningForDevicesAsync();

            await Task.Delay(5000);

            if (_isScanning)
            {
                await _adapter.StopScanningForDevicesAsync();
                _isScanning = false;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var cihaz in _geciciCihazlar)
                    {
                        _bulunanCihazlar.Add(cihaz);
                    }

                    if (_esp32Device == null)
                    {
                        DurumLabel.Text = "Durum: Tarama Tamamlandı. Listeden cihaz seçin.";
                        DurumLabel.TextColor = Colors.Gray;
                    }
                    YenileButonu.IsEnabled = true;
                });
            }
        }
        catch (Exception)
        {
            DurumLabel.Text = "Tarama hatası oluştu.";
            YenileButonu.IsEnabled = true;
            _isScanning = false;
        }
    }

    private async void CihazListesi_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is IDevice secilenCihaz)
        {
            if (_esp32Device != null)
            {
                try
                {
                    await _adapter.DisconnectDeviceAsync(_esp32Device);
                }
                catch { }
            }

            _esp32Device = secilenCihaz;

            await _adapter.StopScanningForDevicesAsync();
            _isScanning = false;

            MainThread.BeginInvokeOnMainThread(() => YenileButonu.IsEnabled = true);

            await Task.Delay(500);
            await BaglanToEsp32();
        }

        ((ListView)sender).SelectedItem = null;
    }

    private async Task BaglanToEsp32()
    {
        if (!_bluetoothLE.IsOn || _esp32Device == null) return;

        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DurumLabel.Text = "Durum: Bağlanılıyor...";
            });

            var baglantiAyarlari = new Plugin.BLE.Abstractions.ConnectParameters(forceBleTransport: true);
            await _adapter.ConnectToDeviceAsync(_esp32Device, baglantiAyarlari);

            _commandCharacteristic = null;

            string[] guvenliServisler = {
                "4fafc201-1fb5-459e-8fcc-c5c9c331914b",
                "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
            };

            string[] guvenliKarakteristikler = {
                "beb5483e-36e1-4688-b7f5-ea07361b26a8",
                "8b7c6d5e-4f3a-2b1c-0d9e-8f7a6b5c4d3e"
            };

            var servisler = await _esp32Device.GetServicesAsync();

            foreach (var servis in servisler)
            {
                if (guvenliServisler.Contains(servis.Id.ToString().ToLower()))
                {
                    var karakteristikler = await servis.GetCharacteristicsAsync();

                    foreach (var karakteristik in karakteristikler)
                    {
                        if (guvenliKarakteristikler.Contains(karakteristik.Id.ToString().ToLower()) && karakteristik.CanWrite)
                        {
                            _commandCharacteristic = karakteristik;
                            break;
                        }
                    }
                }

                if (_commandCharacteristic != null) break;
            }

            if (_commandCharacteristic != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DurumLabel.Text = $"Durum: {_esp32Device.Name} Bağlandı!";
                    DurumLabel.TextColor = Colors.Green;
                    ToggleLedButtons(true);
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DurumLabel.Text = "Hata: Cihazda yetkili servis bulunamadı.";
                    DurumLabel.TextColor = Colors.Red;
                });
            }
        }
        catch (DeviceConnectionException)
        {
            if (!_bluetoothLE.IsOn) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DurumLabel.Text = "Bağlantı başarısız, tekrar deneniyor...";
                DurumLabel.TextColor = Colors.Red;
            });

            await Task.Delay(2000);
            await BaglanToEsp32();
        }
    }

    private void OnBluetoothStateChanged(object? sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (e.NewState == Plugin.BLE.Abstractions.Contracts.BluetoothState.Off)
            {
                DurumLabel.Text = "Durum: Telefonun Bluetooth'u kapatıldı!";
                DurumLabel.TextColor = Colors.Red;
                ToggleLedButtons(false);
                YenileButonu.IsEnabled = true;
                _isScanning = false;
                _esp32Device = null;
            }
            else if (e.NewState == Plugin.BLE.Abstractions.Contracts.BluetoothState.On)
            {
                DurumLabel.Text = "Durum: Bluetooth açıldı. Cihazı arayabilirsiniz.";
                DurumLabel.TextColor = Colors.Gray;
            }
        });
    }

    private async void OnDeviceConnectionLost(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            DurumLabel.Text = "Durum: Bağlantı Koptu! Yeniden bağlanılıyor...";
            DurumLabel.TextColor = Colors.Red;
            ToggleLedButtons(false);
            await BaglanToEsp32();
        });
    }

    private void OnDeviceDisconnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DurumLabel.Text = "Durum: Bağlantı Kesildi.";
            DurumLabel.TextColor = Colors.Gray;
            ToggleLedButtons(false);
            YenileButonu.IsEnabled = true;
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
        catch (Exception)
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