# Kişisel Notlar - Trackflow

> Bu dosya .gitignore'da, GitHub'a gitmeyecek.
> Geliştirme sırasında lazım olacak teknik notlar.

---

## Docker Yapısı

**Çalıştırma:**
```bash
docker-compose up -d
```

**Swagger:** http://localhost:5000/swagger

**Bileşenler:**
| Servis | Port | Açıklama |
|--------|------|----------|
| sqlserver | 1433 | MSSQL 2022 |
| api | 5000 | ASP.NET Core Web API |

**Connection String:**
```
Server=sqlserver;Database=TrackflowDb;User=sa;Password=Trackflow123!;TrustServerCertificate=true
```

**Logları görmek:**
```bash
docker-compose logs -f api
```

**Temizlik:**
```bash
docker-compose down -v  # volume'ları da siler
```

---

## GS1 Application Identifier'lar

Ödevde istenen AI'lar:

| AI | Açıklama | Format | Örnek |
|----|----------|--------|-------|
| (01) | GTIN | 14 hane | 08690123456789 |
| (21) | Seri Numarası | Max 20 karakter | ABC123456 |
| (17) | Son Kullanma Tarihi | YYMMDD | 261231 |
| (10) | Lot/Batch No | Max 20 karakter | LOT001 |

**Birleşik GS1 String Örneği:**
```
(01)08690123456789(21)SN00001(17)261231(10)LOT001
```

---

## SSCC (Serial Shipping Container Code)

- AI: (00)
- Toplam 18 hane
- Format: Extension + GS1 Company Prefix + Serial Reference + Check Digit

**Yapı:**
```
0 + 8690123 + 000000001 + [Check Digit]
^   ^         ^           ^
|   |         |           Luhn ile hesaplanır
|   |         Sıralı numara (9 hane)
|   Firma GLN'den türetilir
Extension digit (genelde 0)
```

**Luhn Algoritması (Check Digit):**
1. Sağdan başla, tek pozisyonları 3 ile çarp
2. Çift pozisyonları 1 ile çarp
3. Topla
4. 10'a tamamla

---

## Entity İlişkileri

```
Customer (1) -----> (*) Product
                         |
                         v
                    (*) WorkOrder
                         |
                         v
                    (*) SerialNumber -----> PackingUnit (Koli)
                                                |
                                                v
                                           PackingUnit (Palet)
```

---

## Durum (Status) Enum'ları

**WorkOrderStatus:**
- Created = 0
- InProgress = 1
- Completed = 2
- Cancelled = 3

**SerialNumberStatus:**
- Generated = 0
- Printed = 1
- Verified = 2
- Rejected = 3
- Aggregated = 4

**PackingUnitType:**
- Box = 0 (Koli)
- Pallet = 1 (Palet)

---

## API Response Yapısı (WorkOrder Detail)

GET /api/workorders/{id}/detail dönüşü:

```json
{
  "workOrder": {
    "id": "guid",
    "lotNo": "LOT001",
    "sonKullanmaTarihi": "2026-12-31",
    "miktar": 100,
    "durum": "InProgress"
  },
  "product": {
    "gtin": "08690123456789",
    "urunAdi": "Test Ürün"
  },
  "customer": {
    "firmaAdi": "Test Firma",
    "gln": "8690123000000"
  },
  "serialNumbers": [
    {
      "seriNo": "SN00001",
      "gs1Barkod": "(01)08690123456789(21)SN00001(17)261231(10)LOT001",
      "durum": "Printed"
    }
  ],
  "aggregation": {
    "pallets": [
      {
        "sscc": "008690123000000011",
        "boxes": [
          {
            "sscc": "008690123000000028",
            "serialNumbers": ["SN00001", "SN00002"]
          }
        ]
      }
    ]
  }
}
```

---

## Proje Referansları

```
Domain        <- (hiçbir şeye bağımlı değil)
Application   <- Domain
Infrastructure <- Application (ve Domain implicit)
API           <- Infrastructure (ve diğerleri implicit)
Client        <- (bağımsız, API'yi HTTP ile çağırır)
```

---

## Dikkat Edilecekler

1. **Repository Pattern KULLANMA** - EF Core'un DbContext'i yeterli
2. **Audit Fields** - CreatedAt, UpdatedAt her entity'de olsun
3. **Async/Await** - Tüm DB işlemleri async olmalı
4. **DTO kullan** - Entity'leri direkt API'den dönme
5. **Auto-migrate** - Program.cs'de Database.Migrate() çağır
