# Trackflow - L3 Serilizasyon ve Agregasyon Sistemi

GS1 standartlarına uygun bir L3 serilizasyon, agregasyon ve izlenebilirlik sistemi simülasyonudur.

---

## Kurulum

### Gereksinimler
- Docker Desktop
- .NET 8 SDK (sadece Client için)

### Başlatma

```bash
# API ve Veritabanını başlat
docker-compose up -d --build
```

| Servis | Port | Açıklama |
|--------|------|----------|
| sqlserver | 1433 | SQL Server 2022 |
| api | 5101 | ASP.NET Core Web API |

```bash
# Client'ı başlat
cd Trackflow.Client
dotnet run
```

---

## Kullanım

### API
- Swagger: http://localhost:5101/swagger
- Health Check: http://localhost:5101/api/health

### Client
- Varsayılan API adresi: http://localhost:5101
- Ayarlar sayfasından farklı API adresi test edilebilir

### Docker Komutları

```bash
docker-compose up -d          # Başlat
docker-compose logs -f api    # Logları izle
docker-compose down           # Durdur
docker-compose down -v        # Veritabanı dahil sil
```

---

## Mimari

Clean Architecture (Onion Architecture) yapısında tasarlanmıştır.

```
Trackflow.Domain         # Entity'ler, Enum'lar (bağımsız)
Trackflow.Application    # Servisler, DTO'lar
Trackflow.Infrastructure # EF Core, DbContext
Trackflow.Shared         # Client-API ortak DTO'lar
Trackflow.API            # REST API (ASP.NET Core)
Trackflow.Client         # Windows Forms UI
```

### Katmanlar

| Katman | Sorumluluk |
|--------|------------|
| **Domain** | Entity'ler (Customer, Product, WorkOrder, SerialNumber, PackingUnit) |
| **Application** | GS1 servisleri, SSCC üretimi, Agregasyon mantığı |
| **Infrastructure** | Entity Framework Core, SQL Server bağlantısı |
| **API** | RESTful endpoint'ler, Swagger |
| **Client** | Operatör arayüzü (Windows Forms) |

---

## Özellikler

- Müşteri ve Ürün yönetimi (CRUD)
- İş emri oluşturma ve seri numarası üretimi
- GS1-128 barkod string oluşturma
- SSCC kodu üretimi (Luhn algoritması)
- Agregasyon: Ürün -> Koli -> Palet hiyerarşisi

---

## Varsayımlar

1. **Donanım Simülasyonu**: Fiziksel yazıcı/kamera yok, yazılımda simüle edilmiştir
2. **Seri Numarası**: İş emrindeki başlangıç değerinden sıralı üretilir
3. **SSCC**: GS1 standartlarına uygun, Luhn check digit içerir
4. **Güvenlik**: Auth/login yok, güvenli iç ağ varsayımı

---

## Teslimat İçeriği

- Kaynak kod (tüm katmanlar)
- Docker yapılandırması
- EF Core Migration dosyaları
- Dokümantasyon
