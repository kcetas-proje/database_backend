# Abone Yönetim Sistemi API (KCETAS MVP)

Bu proje, bir elektrik dağıtım/perakende satış şirketinin abone yönetimi süreçlerini dijitalleştirmek ve otomatize etmek amacıyla geliştirilmiş mevzuat referanslı bir **.NET 8** Web API projesidir.

## 🚀 Proje Hakkında
Bu sistem, 1 milyondan fazla veriyi performans sorunu (Out of Memory) yaşamadan işleyebilen, PostgreSQL üzerinde koşan ve Entity Framework Core kullanan yüksek performanslı bir arka yüz (backend) uygulamasıdır. Tüm listeleme endpoint'leri IQueryable yapısıyla doğrudan SQL seviyesinde filtrelenerek **Server-Side Pagination (Sunucu Taraflı Sayfalama)** standartlarına tam uyumlu hale getirilmiştir.

## 🏗️ Temel Özellikler & Modüller

- **Tüketim Noktası ve Abonelik (Sözleşme) Yönetimi:** Tekil kod bazlı tesisat kayıtları, gerçek/tüzel abone takibi ve perakende satış sözleşmeleri statü yönetimi.
- **Endeks Okuma Sistemi:** Rutin, ilk, son, kesme ve sayaç değişim endeksi girişleri. Hızlı veri girişi için `YeniOkumaSecimAra` gibi birleşik DTO döndüren optimize endpoint'ler.
- **Faturalama ve Tahakkuk:** Girilen endekslere göre enerji bedeli, dağıtım bedeli, KDV vb. kalemleri hesaplayıp fatura oluşturan altyapı.
- **Saha İş Emirleri (Açma / Kesme / Sayaç İşlemleri):** Sayaç bağlama, değişim, sökme ve borçtan dolayı enerji kesme süreçlerini yöneten modül.
- **Entegrasyon Outbox Sistemi:** GİB e-Fatura, ERP ve diğer dış sistemlere gönderilecek fatura vb. belgelerin Idempotency (tekrar önleme), Retry (yeniden deneme) mekanizmalarıyla kuyruğa atılıp arka plan (Worker) servisleriyle eritildiği yapı.
- **Audit Log (Denetim İzleri):** Sistemdeki her INSERT, UPDATE, DELETE işleminin (eski ve yeni değerlerle) otomatik olarak loglandığı Entity Framework Interceptor / `SaveChangesAsync` override mimarisi.
- **Dashboard ve Raporlama:** Ana sayfadaki özet kartları (aktif abone/sayaç sayısı, kesme adayları, açık iş emirleri) ve dönemsel tahakkuk grafiklerini sağlayan, mevcut verileri SQL'de özetleyerek çeken `RaporlarController`.

## 🛠️ Teknolojiler
- **Framework:** .NET 8 (ASP.NET Core Web API)
- **ORM:** Entity Framework Core
- **Veritabanı:** PostgreSQL (JSONB formatında Audit Log desteği dahil)
- **Mimari Yaklaşım:** Controller-Service-DTO katmanlı mimari, Background Services (Worker)

## 🏃 Kurulum ve Çalıştırma

1. **Bağımlılıkları Yükleyin**
   ```bash
   dotnet restore
   ```

2. **Veritabanı Bağlantısını Ayarlayın**
   `appsettings.json` ve/veya `appsettings.Development.json` dosyalarındaki PostgreSQL `DefaultConnection` connection string'ini kendi veritabanınıza göre güncelleyin.

3. **Veritabanını Oluşturun (Migrations)**
   ```bash
   dotnet ef database update
   ```

4. **Projeyi Çalıştırın**
   ```bash
   dotnet run
   ```
   *Uygulama çalıştıktan sonra Swagger arayüzüne `/swagger` adresinden erişerek API'leri test edebilirsiniz.*

## 🔒 Güvenlik & Performans Notları
- Tüm filtreleme ve arama işlemleri (örneğin abone ad/soyad, tüketim noktası tekil kod aramaları) belleğe çekilmeden SQL Server/Postgres üzerinde `Where` ve `Contains` komutlarıyla işlenir.
- Tarih sorgularında (özellikle bitiş tarihleri `23:59:59` olarak) UTC timezone dönüşümleri kullanılarak sınır hataları önlenmiştir.
- Fatura onaylandığında GİB/ERP sistemlerine gönderim işlemleri anlık yapılmaz; `EntegrasyonOutbox` tablosuna yazılır ve asenkron worker servislerle işlenir. (Sistemin tıkanmasını önler).
