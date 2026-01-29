# Trackflow - L3 Serilizasyon ve Agregasyon Sistemi

Bu proje, GS1 standartlarına uygun bir L3 serilizasyon, agregasyon ve izlenebilirlik sistemi simülasyonudur. .NET teknolojileri kullanılarak, endüstriyel standartlara ve **Clean Architecture** prensiplerine uygun olarak geliştirilmiştir.

---

## 🏗️ Mimari Açıklama

Sistem, sürdürülebilirlik, test edilebilirlik ve sorumlulukların ayrılması (SoC) ilkeleri gözetilerek **Clean Architecture** (Onion Architecture) yapısında tasarlanmıştır.

### Katmanlar

1.  **Trackflow.Domain (Core)**:
    *   Sistemin merkezidir. Tüm iş kuralları, varlıklar (Entities - `WorkOrder`, `Product`, `Customer`) ve temel arayüzler burada tanımlıdır.
    *   Hiçbir dış kütüphaneye veya katmana bağımlılığı yoktur.

2.  **Trackflow.Application**:
    *   Uygulama senaryolarını (Use Cases) yönetir.
    *   **GS1 Servisleri**: GTIN, Lot, SKT ve Seri Numaralarını birleştirerek barkod stringlerini üreten servisler buradadır.
    *   **Agregasyon Mantığı**: Ürün -> Koli -> Palet hiyerarşisini ve SSCC (Serial Shipping Container Code) üretimini yönetir.

3.  **Trackflow.Infrastructure**:
    *   Veritabanı erişimi (**Entity Framework Core**), Repository implementasyonları ve dış sistem entegrasyonlarını sağlar.
    *   SQL Server ile iletişim bu katmanda yapılandırılmıştır.

4.  **Trackflow.API**:
    *   Sistemin dış dünyaya açılan RESTful API katmanıdır.
    *   İş emirlerinin yönetilmesi ve etiketleme sistemleri (Yazıcı/Kamera) ile haberleşmeyi simüle eder.

5.  **Trackflow.Client (Windows Forms)**:
    *   Operatörlerin iş emirlerini başlattığı, etiketleme sürecini izlediği kullanıcı arayüzüdür.

---

## 💡 Varsayımlar

Proje geliştirilirken aşağıdaki varsayımlar ve simülasyonlar kabul edilmiştir:

1.  **Donanım Simülasyonu**: Proje kapsamında fiziksel bir Yazıcı, PLC veya Doğrulama Kamerası kullanılmamıştır. Bu cihazların davranışları yazılım içerisinde **Mock Servisler** ile simüle edilmiştir.
2.  **Seri Numarası Üretimi**: Seri numaraları, iş emrinde belirtilen başlangıç değerinden itibaren sıralı (ardışık) olarak ve çakışmasız (Unique) üretilmektedir.
3.  **SSCC Formatı**: Taşıma birimleri (Koli ve Palet) için üretilen SSCC kodları, GS1 standartlarına uygun olarak Luhn algoritması ile kontrol basamağı içerecek şekilde oluşturulur.
4.  **Güvenlik**: Bu aşamada kullanıcı yetkilendirme (Authentication/Authorization) modülleri kapsam dışı bırakılmıştır; sistemin güvenli bir iç ağda çalıştığı varsayılmıştır.

---

## 🚀 Kurulum Adımları

Projeyi yerel ortamınızda çalıştırmak için aşağıdaki adımları izleyin.

### Gereksinimler
*   .NET 8 SDK
*   Microsoft SQL Server (LocalDB veya Express)

### 1. Veritabanının Hazırlanması
Terminal veya komut satırında projenin ana dizinine gidin ve `Trackflow.API` klasörü içindeyken aşağıdaki komutu çalıştırarak veritabanını oluşturun:

```bash
cd Trackflow.API
dotnet ef database update
```
*Not: Veritabanı bağlantı ayarı (Connection String), `appsettings.json` dosyasında yapılandırılmıştır.*

### 2. Backend (API) Başlatma
API projesini ayağa kaldırın:

```bash
dotnet run
```
API çalıştıktan sonra Swagger arayüzüne (genellikle `https://localhost:7082/swagger`) tarayıcıdan erişerek endpoint'leri test edebilirsiniz.

### 3. Client (Windows Forms) Başlatma
*   Visual Studio kullanarak `Trackflow.sln` dosyasını açın.
*   `Trackflow.Client` projesine sağ tıklayıp **"Set as Startup Project"** (Başlangıç Projesi Yap) seçeneğini seçin.
*   `F5` tuşuna basarak uygulamayı başlatın.

---

## 📦 Teslimat İçeriği

*   **Kaynak Kod**: Tüm katmanlar ve proje dosyaları.
*   **Veritabanı**: Entity Framework Core Code-First Migration dosyaları (`Trackflow.Infrastructure/Migrations` altında).
*   **Dokümantasyon**: Mimari kararlar ve kurulum kılavuzu (Bu dosya).