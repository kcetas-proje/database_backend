-- Veritabanı Hızlandırma İndeksleri
-- Bu script, sunucudaki PostgreSQL veritabanında sadece 1 kez çalıştırılmalıdır.

START TRANSACTION;

-- ==========================================
-- 1. BİLEŞİK (COMPOSITE) İNDEKS EKLEMELERİ
-- (Çoklu kolonla yapılan sorguları devasa oranda hızlandırır)
-- ==========================================

-- Faturaları sözleşme ve döneme göre ararken
CREATE INDEX IF NOT EXISTS idx_fatura_sozlesme_donem ON fatura (sozlesme_id, donem);

-- Faturaları durumlarına göre ve döneme göre listelerken
CREATE INDEX IF NOT EXISTS idx_fatura_durum_donem ON fatura (durum, donem);

-- Endeks okumalarını sayaç ve döneme göre hızlıca bulmak için
CREATE INDEX IF NOT EXISTS idx_endeks_sayac_donem ON endeks_okuma (sayac_id, donem);

-- İş emirlerini tüketim noktası ve mevcut duruma göre ararken
CREATE INDEX IF NOT EXISTS idx_isemirleri_tuketim_durum ON is_emirleri (tuketim_noktasi_id, durum);

-- Sözleşmeleri tüketim noktası ve aktif/pasif durumuna göre filtrelerken
CREATE INDEX IF NOT EXISTS idx_sozlesmeler_tuketim_durum ON sozlesmeler (tuketim_noktasi_id, durum);


-- ==========================================
-- 2. JSONB SÜTUNLARI İÇİN GIN İNDEKS EKLEMELERİ
-- (JSON verilerin içinde yapılan aramaları 'Full Table Scan' olmaktan kurtarır)
-- ==========================================

-- Audit Log tablolarında JSON içerisinde hızlı arama yapabilmek için
CREATE INDEX IF NOT EXISTS idx_audit_log_eski_deger_gin ON audit_log USING GIN (eski_deger);
CREATE INDEX IF NOT EXISTS idx_audit_log_yeni_deger_gin ON audit_log USING GIN (yeni_deger);

-- Entegrasyon Outbox payload içerisinde hızlı arama yapabilmek için
CREATE INDEX IF NOT EXISTS idx_entegrasyon_outbox_payload_gin ON entegrasyon_outbox USING GIN (payload);

COMMIT;

-- Bilgi: Eğer PostgreSQL versiyonunuz JSONB sorgularını çok yoğun alıyorsa,
-- GIN indeksleri hayat kurtarıcı olacaktır.
