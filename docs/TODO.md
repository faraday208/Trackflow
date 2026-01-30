# Trackflow - Yapılacaklar Listesi

> Ödev gereksinimlerine (whatsapp.md) dayalı geliştirme planı.
> Son güncelleme: 2026-01-30

---

## 1. Proje Altyapısı
- [x] Solution dosyası oluştur (`Trackflow.sln`)
- [x] Katman projelerini oluştur:
  - [x] `Trackflow.Domain` (Class Library)
  - [x] `Trackflow.Application` (Class Library)
  - [x] `Trackflow.Infrastructure` (Class Library)
  - [x] `Trackflow.API` (ASP.NET Core Web API)
  - [x] `Trackflow.Client` (Windows Forms)
  - [x] `Trackflow.Shared` (Class Library - Paylaşımlı DTO'lar)
- [x] Proje referanslarını bağla
- [x] Docker yapısını kur (docker-compose + Dockerfile)

---

## 2. Domain Katmanı (Entity'ler)
- [x] `BaseEntity` - Ortak alanlar (Id, CreatedAt, UpdatedAt)
- [x] `Customer` - Müşteri bilgileri
  - Id, FirmaAdi, GLN, Aciklama
- [x] `Product` - Ürün bilgileri
  - Id, CustomerId, GTIN, UrunAdi
- [x] `WorkOrder` - İş emri
  - Id, ProductId, Miktar, LotNo, SonKullanmaTarihi, SeriBaslangic, Durum
- [x] `SerialNumber` - Seri numaraları
  - Id, WorkOrderId, SeriNo, Durum, GS1Barkod
- [x] `PackingUnit` - Koli/Palet (Agregasyon)
  - Id, ParentId, Tip (Koli/Palet), SSCC
- [x] Enum'lar (WorkOrderStatus, SerialNumberStatus, PackingUnitType)

---

## 3. Infrastructure (Veritabanı)
- [x] AppDbContext oluştur
- [x] Entity konfigürasyonları (Fluent API)
  - [x] Foreign Key'ler
  - [x] Unique constraint'ler (GTIN, GLN, SSCC benzersiz olmalı)
- [x] İlk migration'ı oluştur
- [x] Program.cs'de auto-migrate ekle

---

## 4. Application (Servisler)
- [x] `CustomerService` - Müşteri CRUD
- [x] `ProductService` - Ürün CRUD
- [x] `WorkOrderService` - İş emri yönetimi + seri üretimi
- [x] `GS1Service` - Barkod string oluşturma
  - (01) GTIN + (21) Seri + (17) SKT + (10) Lot birleştirme
- [x] `SSCCGenerator` - SSCC kodu üretimi (Luhn algoritması)
- [x] `AggregationService` - Ürün->Koli->Palet hiyerarşisi
- [x] DTO'lar (Request/Response modelleri)

---

## 5. API Endpoints
- [x] **Müşteri**
  - POST /api/customers
  - GET /api/customers
  - PUT /api/customers/{id}
  - DELETE /api/customers/{id}
  - GET /api/customers/generate-gln
- [x] **Ürün**
  - POST /api/products
  - GET /api/products
  - PUT /api/products/{id}
  - DELETE /api/products/{id}
- [x] **İş Emri**
  - POST /api/workorders
  - GET /api/workorders
  - GET /api/workorders/{id}/detail
  - POST /api/workorders/{id}/aggregate
  - DELETE /api/workorders/{id}
- [x] **Sistem**
  - GET /api/health (detaylı health check)
- [x] Swagger UI aktif et

---

## 6. Windows Forms Client
- [x] Ana form tasarımı (side menu + content panel)
- [x] API bağlantısı (HttpClient)
- [x] Müşteri listesi + CRUD (Ekle, Düzenle, Sil)
- [x] Ürün listesi + CRUD (Ekle, Düzenle, Sil)
- [x] İş emri listesi + CRUD (Ekle, Sil)
- [x] İş emri detay görüntüleme
- [x] Agregasyon işlemi
- [x] Ayarlar sayfası (API URL test, sistem durumu)

---

## 7. Son Kontroller
- [x] docker-compose up ile test
- [x] README güncelle
- [x] Kod temizliği

---

## Notlar
- Repository Pattern KULLANMA, direkt DbContext yeterli
- Fiziksel yazıcı/kamera yok, simülasyon yeterli
- Auth/login yok, güvenli ağ varsayımı
- Migration otomatik çalışacak (Docker'da)
