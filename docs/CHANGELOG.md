# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-30

### Added

#### Proje Altyapısı
- Clean Architecture solution yapısı (Domain, Application, Infrastructure, API, Client, Shared)
- Docker ve docker-compose desteği (SQL Server + API)
- Swagger UI entegrasyonu

#### Domain Katmanı
- BaseEntity (Id, CreatedAt, UpdatedAt)
- Customer entity (FirmaAdi, GLN, Aciklama)
- Product entity (GTIN, UrunAdi, CustomerId)
- WorkOrder entity (Miktar, LotNo, SonKullanmaTarihi, Durum)
- SerialNumber entity (SeriNo, GS1Barkod, Durum)
- PackingUnit entity (SSCC, Tip, ParentId)
- Enum'lar: WorkOrderStatus, SerialNumberStatus, PackingUnitType

#### Infrastructure
- AppDbContext (Entity Framework Core)
- Entity konfigürasyonları (Fluent API)
- Auto-migration desteği

#### Application Servisleri
- CustomerService (CRUD + GLN üretimi)
- ProductService (CRUD)
- WorkOrderService (CRUD + seri numarası üretimi)
- GS1Service (GS1-128 barkod string oluşturma)
- SSCCGenerator (Luhn algoritması ile SSCC üretimi)
- AggregationService (Koli/Palet hiyerarşisi)

#### API Endpoints
- Müşteri: GET, POST, PUT, DELETE, generate-gln
- Ürün: GET, POST, PUT, DELETE
- İş Emri: GET, POST, DELETE, detail, aggregate
- Health: GET /api/health (detaylı sistem durumu)

#### Windows Forms Client
- Modern side menu tasarımı
- Müşteri yönetimi (liste, ekle, düzenle, sil)
- Ürün yönetimi (liste, ekle, düzenle, sil)
- İş emri yönetimi (liste, ekle, sil, detay)
- Agregasyon işlemi (Ürün -> Koli -> Palet)
- Ayarlar sayfası (API URL test, sistem durumu gösterimi)

#### Shared DTO Projesi
- Client ve API arasında paylaşımlı DTO'lar
- HealthReportDto, CustomerDto, ProductDto, WorkOrderDto vb.

## [0.1.0] - 2026-01-28

### Added
- Proje başlangıcı ve dokümantasyon (README, TODO)
- Clean Architecture solution yapısı planlandı
