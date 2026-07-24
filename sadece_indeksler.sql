START TRANSACTION;
ALTER INDEX "IX_sozlesmeler_tuketim_noktasi_id" RENAME TO idx_sozlesmeler_tuketim_id;

ALTER INDEX "IX_sozlesmeler_abone_id" RENAME TO idx_sozlesmeler_abone_id;

ALTER INDEX "IX_sayaclar_tuketim_noktasi_id" RENAME TO idx_sayaclar_tuketim_id;

ALTER INDEX "IX_is_emirleri_tuketim_noktasi_id" RENAME TO idx_isemirleri_tuketim_id;

ALTER INDEX "IX_is_emirleri_atanan_kullanici_id" RENAME TO idx_isemirleri_atanan_id;

ALTER INDEX "IX_endeks_okuma_sozlesme_id" RENAME TO idx_endeks_sozlesme_id;

ALTER INDEX "IX_endeks_okuma_sayac_id" RENAME TO idx_endeks_sayac_id;

CREATE INDEX idx_sozlesmeler_durum ON sozlesmeler (durum);

CREATE UNIQUE INDEX uq_sozlesmeler_sozlesme_no ON sozlesmeler (sozlesme_no);

CREATE UNIQUE INDEX uq_sayaclar_seri_no ON sayaclar (seri_no);

CREATE INDEX idx_isemirleri_durum ON is_emirleri (durum);

CREATE UNIQUE INDEX uq_isemirleri_no ON is_emirleri (is_emri_no);

CREATE INDEX idx_endeks_donem ON endeks_okuma (donem);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260724051249_TümIndeks', '10.0.9');

COMMIT;

