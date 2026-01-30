# Trackflow - Yapılacaklar Listesi

> Ödev gereksinimlerine (whatsapp.md) dayalı geliştirme planı.
> Son güncelleme: 2026-01-29

---

## 1. Proje Altyapısı
- [x] Solution dosyası oluştur (`Trackflow.sln`)
- [x] Katman projelerini oluştur:
  - [x] `Trackflow.Domain` (Class Library)
  - [x] `Trackflow.Application` (Class Library)
  - [x] `Trackflow.Infrastructure` (Class Library)
  - [x] `Trackflow.API` (ASP.NET Core Web API)
  - [x] `Trackflow.Client` (Windows Forms)
- [x] Proje referanslarını bağla
- [x] Docker yapısını kur (docker-compose + Dockerfile)

---

## 2. Domain Katmanı (Entity'ler)
- [ ] `BaseEntity` - Ortak alanlar (Id, CreatedAt, UpdatedAt)
- [ ] `Customer` - Müşteri bilgileri
  - Id, FirmaAdi, GLN, Aciklama
- [ ] `Product` - Ürün bilgileri
  - Id, CustomerId, GTIN, UrunAdi
- [ ] `WorkOrder` - İş emri
  - Id, ProductId, Miktar, LotNo, SonKullanmaTarihi, SeriBaslangic, Durum
- [ ] `SerialNumber` - Seri numaraları
  - Id, WorkOrderId, SeriNo, Durum, GS1Barkod
- [ ] `PackingUnit` - Koli/Palet (Agregasyon)
  - Id, ParentId, Tip (Koli/Palet), SSCC
- [ ] Enum'lar (WorkOrderStatus, SerialNumberStatus, PackingUnitType)

---

## 3. Infrastructure (Veritabanı)
- [ ] AppDbContext oluştur
- [ ] Entity konfigürasyonları (Fluent API)
  - [ ] Foreign Key'ler
  - [ ] Unique constraint'ler (GTIN, GLN, SSCC benzersiz olmalı)
- [ ] İlk migration'ı oluştur
- [ ] Program.cs'de auto-migrate ekle

---

## 4. Application (Servisler)
- [ ] `CustomerService` - Müşteri CRUD
- [ ] `ProductService` - Ürün CRUD
- [ ] `WorkOrderService` - İş emri yönetimi + seri üretimi
- [ ] `GS1Service` - Barkod string oluşturma
  - (01) GTIN + (21) Seri + (17) SKT + (10) Lot birleştirme
- [ ] `SSCCGenerator` - SSCC kodu üretimi (Luhn algoritması)
- [ ] `AggregationService` - Ürün->Koli->Palet hiyerarşisi
- [ ] DTO'lar (Request/Response modelleri)

---

## 5. API Endpoints
- [ ] **Müşteri**
  - POST /api/customers
  - GET /api/customers
- [ ] **Ürün**
  - POST /api/products
  - GET /api/products
- [ ] **İş Emri**
  - POST /api/workorders
  - GET /api/workorders/{id}/detail (TEK RESPONSE)
- [ ] Swagger UI aktif et
- [ ] Basit hata yönetimi middleware

---

## 6. Windows Forms Client
- [ ] Ana form tasarımı
- [ ] API bağlantısı (HttpClient)
- [ ] Müşteri/Ürün listesi
- [ ] İş emri başlatma
- [ ] Etiket basım simülasyonu

---

## 7. Son Kontroller
- [ ] docker-compose up ile test
- [ ] README güncelle
- [ ] Kod temizliği

---

## Notlar
- Repository Pattern KULLANMA, direkt DbContext yeterli
- Fiziksel yazıcı/kamera yok, simülasyon yeterli
- Auth/login yok, güvenli ağ varsayımı
- Migration otomatik çalışacak (Docker'da)
