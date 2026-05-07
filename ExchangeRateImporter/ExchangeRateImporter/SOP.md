# Exchange Rate Importer — SOP (Standard Operating Procedure)

**Son Güncelleme:** 09.03.2026

---

## 1. Programın Amacı

Bu program, farklı ülkelerin merkez bankası veya finans kuruluşlarından günlük döviz kurlarını (BUY/SELL) otomatik olarak çeker ve **CargoWise One** sistemine XML formatında import eder.

---

## 2. Desteklenen Ülkeler

| Parametre | Ülke | Kaynak | Para Birimleri | CW Şirket Kodu |
|---|---|---|---|---|
| `Turkey` | Türkiye | TCMB (tcmb.gov.tr) | EUR, GBP, USD | TR1 |
| `India` | Hindistan | Axis Bank web sayfası | ~17 para birimi | IND |
| `UAE` | BAE | Global Exchange Rates API | EUR, GBP, JPY, SAR, SGD, USD | AE4 |
| `Australia` | Avustralya | NAB Bank API | ~50 para birimi | AU1 |
| `Vietnam` | Vietnam | Vietcombank API | USD, EUR, GBP | VN1 |

---

## 3. Gereksinimler

### 3.1 Yazılım
- **.NET 8.0 Runtime** yüklü olmalıdır.

### 3.2 Windows Environment Variables (Ortam Değişkenleri)

Aşağıdaki ortam değişkenleri **System Environment Variables** içinde tanımlanmalıdır:

| Değişken Adı | Açıklama |
|---|---|
| `Cargowise_URI_TST` | CargoWise **Test** sunucu API adresi |
| `Cargowise_User_TST` | CargoWise **Test** kullanıcı adı |
| `Cargowise_Password_TST` | CargoWise **Test** şifresi |
| `Cargowise_URI_PRD` | CargoWise **Production** sunucu API adresi |
| `Cargowise_User_PRD` | CargoWise **Production** kullanıcı adı |
| `Cargowise_Password_PRD` | CargoWise **Production** şifresi |
| `Global_Exchange_Rates_API_Key` | Global Exchange Rates API anahtarı (yalnızca UAE için) |

#### Environment Variable Ekleme Adımları:
1. **Win + R** → `sysdm.cpl` → Enter
2. **Advanced** sekmesi → **Environment Variables**
3. **System variables** altında **New** butonuna tıklayın
4. Yukarıdaki tablodan değişken adı ve değerini girin
5. **OK** ile kaydedin
6. **Sunucuyu/bilgisayarı yeniden başlatın** (değişkenlerin aktif olması için)

---

## 4. Kullanım

### 4.1 Komut Satırı Formatı

```
ExchangeRateImporter.exe <Ülke> <Sunucu>
```

| Parametre | Açıklama | Geçerli Değerler |
|---|---|---|
| **1. parametre** (Ülke) | Hangi ülkenin kurları çekilecek | `Turkey`, `India`, `UAE`, `Australia`, `Vietnam` |
| **2. parametre** (Sunucu) | Hangi CargoWise sunucusuna gönderilecek | `TST` (Test), `PRD` (Production) |

### 4.2 Örnekler

```bash
# Türkiye kurlarını TEST sunucusuna gönder
ExchangeRateImporter.exe Turkey TST

# Hindistan kurlarını PRODUCTION sunucusuna gönder
ExchangeRateImporter.exe India PRD

# BAE kurlarını PRODUCTION sunucusuna gönder
ExchangeRateImporter.exe UAE PRD
```

---

## 5. Windows Görev Zamanlayıcısı (Task Scheduler) Kurulumu

Her ülke için ayrı bir zamanlı görev oluşturulmalıdır.

### 5.1 Yeni Görev Oluşturma

1. **Win + R** → `taskschd.msc` → Enter
2. Sağ panelden **Create Task** seçin

### 5.2 General Sekmesi
- **Name:** `ExchangeRate_Turkey_PRD` (örnek)
- **Run whether user is logged on or not:** ✅ İşaretleyin

### 5.3 Triggers Sekmesi
- **New** → **Daily**
- **Start:** Ülkenin merkez bankasının kurları yayınladığı saatten sonra ayarlayın

| Ülke | Önerilen Saat (Sunucu yerel saati) |
|---|---|
| Turkey | 15:30 (TCMB kurları ~15:30'da güncellenir) |
| India | 12:00 IST |
| UAE | 10:00 GST |
| Australia | 09:00 AEST |
| Vietnam | 10:00 ICT |

### 5.4 Actions Sekmesi
- **Action:** Start a program
- **Program/script:** `C:\path\to\ExchangeRateImporter.exe`
- **Add arguments:** `Turkey PRD` (ülke ve sunucu parametresi)
- **Start in:** `C:\path\to\` (programın bulunduğu klasör)

### 5.5 Settings Sekmesi
- **Allow task to be run on demand:** ✅
- **Stop the task if it runs longer than:** `30 minutes`
- **If the task is already running:** `Do not start a new instance`

---

## 6. Log Dosyaları

Program her çalıştığında otomatik olarak log dosyası oluşturur:

- **Konum:** `<program dizini>/TextFiles/`
- **Format:** `ImportLog_<Ülke>_<TarihSaat>.txt`
- **Örnek:** `ImportLog_Turkey_20260309_1530.txt`

### Log İçeriği
```
15:30:05 - EUR BUY: 1 Insert
15:30:06 - EUR SEL: 1 Insert
15:30:07 - GBP BUY: 1 Update
15:30:08 - GBP SEL: 1 Update
15:30:09 - USD BUY: 1 Insert
15:30:10 - USD SEL: 1 Insert
```

| Durum | Anlamı |
|---|---|
| `1 Insert` | Yeni kur kaydı oluşturuldu |
| `1 Update` | Mevcut kur güncellendi |
| `NO UPDATES` | Değişiklik yok, inceleme gerekebilir |
| `Other/Error` | Hata oluştu, Details kısmını inceleyin |

---

## 7. Sorun Giderme

| Sorun | Olası Neden | Çözüm |
|---|---|---|
| Program hiç çalışmıyor | .NET 8 Runtime yüklü değil | `dotnet --version` ile kontrol edin, yoksa yükleyin |
| `Invalid server parameter` hatası | 2. parametre yanlış | `TST` veya `PRD` kullanın |
| `Usage:` mesajı çıkıyor | Eksik parametre | Her iki parametreyi de verin: `Turkey PRD` |
| Kur çekilemiyor (Turkey) | TCMB bağlantı sorunu | İnternet bağlantısını ve `tcmb.gov.tr` erişimini kontrol edin |
| Kur çekilemiyor (UAE) | API anahtarı eksik/geçersiz | `Global_Exchange_Rates_API_Key` env variable'ı kontrol edin |
| CargoWise'a gönderilemiyor | Env variable yanlış/eksik | `Cargowise_URI_TST/PRD` ve credential'ları kontrol edin |
| Log dosyası oluşmuyor | Yetki sorunu | Program dizininde yazma yetkisi olduğundan emin olun |
| Hafta sonu kur gelmiyor (Turkey) | TCMB hafta sonu kur yayınlamaz | Normal davranış — program sonraki iş gününe ayarlar |

---

## 8. Ülkelere Özel Notlar

### Türkiye
- TCMB sadece **iş günleri** kur yayınlar. Program hafta sonları için otomatik olarak bir sonraki iş gününe tarih atar.

### Hindistan
- Axis Bank web sayfasından HTML scraping yapılır. Web sitesi yapısı değişirse program güncellenmeli.
- `CNH` para birimi otomatik olarak `CNY` olarak dönüştürülür (CargoWise ISO uyumluluğu).

### BAE
- `Global_Exchange_Rates_API_Key` ortam değişkeni zorunludur.
- Kurlar ters çevrilir (1/rate) çünkü kaynak "1 AED = X" formatındadır.

### Avustralya
- NAB Bank API'sine erişim 3 aşamalı OAuth token exchange gerektirir. Token'lar otomatik alınır.
- Ham API yanıtı `TextFiles/FX_NAB_raw_<tarih>.json` olarak kaydedilir.

### Vietnam
- Yalnızca **USD, EUR, GBP** kurları çekilir.
- Vietcombank API kullanılır.
