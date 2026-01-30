## 1. Amaç

Bu case’in amacı; GS1 standartlarına uygun *L3 serilizasyon ve agregasyon sistemi* konusunda öğrencilerin teknik yetkinliğini değerlendirmektir.

---

## 2. Genel Mimari Beklentisi

Sistem aşağıdaki bileşenlerden oluşmalıdır:

- *Backend & API*
  - .NET (ASP.NET Core Web API)
  - MS SQL Server
- *Client Uygulama*
  - Windows Forms (.NET)
- *Entegrasyon Katmanı*
  - Yazıcı
  - Doğrulama kamerası
  - PLC (simülasyon kabul edilir)

---

## 3. Backend (L3 Sistem – .NET Web API)

### 3.1 Müşteri Yönetimi
- Müşteri oluşturma
- Müşteri listeleme

*Örnek Alanlar*
- Firma Adı  
- GLN  
- Açıklama  

---

### 3.2 Ürün Yönetimi
- Müşteriye bağlı ürün tanımlama

*Alanlar*
- GTIN (01)  
- Ürün Adı  

---

### 3.3 İş Emri (Work Order) Yönetimi
- Ürün bazlı iş emri oluşturma

*İş Emri Bilgileri*
- Üretim adedi  
- Lot / Batch No (10)  
- Son Kullanma Tarihi (17)  
- Seri numarası başlangıç değeri  
- İş emri durumu  

---

### 3.4 GS1 Identifier Üretimi

İş emrine bağlı olarak aşağıdaki GS1 Application Identifier’lar oluşturulmalıdır:

- *(01)* GTIN  
- *(21)* Seri Numarası  
- *(17)* Son Kullanma Tarihi  
- *(10)* Batch / Lot No  

> Fiziksel karekod basımı zorunlu değildir.  
> GS1 string çıktısı yeterlidir.

---

### 3.5 Seri Numarası ve SSCC Yönetimi
- Seri numaraları çakışmasız üretilmelidir  
- SSCC (00) oluşturulmalıdır  
- Ürün → Koli → Palet agregasyonu desteklenmelidir  

---

### 3.6 İş Emri Detay API

Aşağıdaki endpoint zorunludur:

Bu endpoint aşağıdaki bilgileri tek bir response olarak döndürmelidir:
- İş emri bilgileri  
- Ürün bilgileri  
- Üretilmiş seri numaraları  
- SSCC ve agregasyon yapısı  

---

## 7. Teknik Beklentiler
- Katmanlı mimari  
- Clean Code prensipleri  
- Dependency Injection  
- Exception ve logging yaklaşımı  
- Veritabanında:
  - Foreign Key’ler  
  - Unique constraint’ler  

---

## 8. Teslimat
- Kaynak kod (GitHub)  
- MSSQL script veya EF Core migration  
- README:
  - Kurulum adımları  
  - Varsayımlar  
  - Mimari açıklama  

---

## 9. Değerlendirme Kriterleri

| Başlık | Açıklama |
|------|---------|
| Mimari | L3 + otomasyon yaklaşımı |
| GS1 Uyumu | AI formatlarının doğruluğu |
| Entegrasyon | API |
| Kod Kalitesi | Okunabilirlik ve yapı |
| Gerçekçilik |

---
dam-weyo-vfc