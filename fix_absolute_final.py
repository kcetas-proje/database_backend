import re

def fix(path, replacements):
    try:
        with open(path, 'r') as f: c = f.read()
        orig = c
        for o, n in replacements: c = c.replace(o, n)
        if c != orig:
            with open(path, 'w') as f: f.write(c)
    except: pass

fix('Helpers/FakerHelper.cs', [
    ('public static Faz Faz()', 'public static Faz GetFaz()'), # method name conflicts with enum name
])

fix('Controllers/SayaclarController.cs', [
    ('FakerHelper.Faz()', 'FakerHelper.GetFaz()'),
    ('sayac.Durum = SayacDurumu.ARIZALI', 'sayac.Durum = SayacDurumu.ARIZALI;'),
    ('sayac.Durum == IsEmriDurumu.ARIZALI', 'sayac.Durum == SayacDurumu.ARIZALI')
])

fix('Controllers/EntegrasyonOutboxController.cs', [
    ('Durum = isEmri.Durum,', 'Durum = OutboxDurumu.BEKLIYOR,')
])

fix('Services/TuketimNoktasıSeeder.cs', [
    ('BaglantiDurumu = f.PickRandom(new[] { "BAGLI", "KAPALI", "BAGLANABILIR", "BAGLANTI_BEKLIYOR" })', 'BaglantiDurumu = f.PickRandom<BaglantiDurumu>()'),
    ('BaglantiDurumu = f.PickRandom(durumlar)', 'BaglantiDurumu = f.PickRandom<BaglantiDurumu>()')
])

fix('Services/SozlesmeSeeder.cs', [
    ('string[] aboneTipleri', 'AboneTipi[] aboneTipleri'),
    ('"MESKEN", "TICARETHANE", "SANAYI", "TARIMSAL_SULAMA", "AYDINLATMA"', 'AboneTipi.MESKEN, AboneTipi.TICARETHANE, AboneTipi.SANAYI, AboneTipi.TARIMSAL_SULAMA, AboneTipi.AYDINLATMA')
])

fix('Controllers/TuketimNoktasıController.cs', [
    ('string.IsNullOrEmpty(dto.BaglantiDurumu)', 'dto.BaglantiDurumu == null'),
    ('nokta.BaglantiDurumu = dto.BaglantiDurumu;', 'nokta.BaglantiDurumu = dto.BaglantiDurumu.Value;')
])

fix('Controllers/SozlesmelerController.cs', [
    ('sozlesme.Durum = IsEmriDurumu.PASIF', 'sozlesme.Durum = SozlesmeDurumu.PASIF')
])

fix('Controllers/FaturaController.cs', [
    ('fatura.Durum.ToString() == FaturaDurumu.ODENDI', 'fatura.Durum == FaturaDurumu.ODENDI'),
    ('fatura.Durum.ToString() != FaturaDurumu.ODENMEDI', 'fatura.Durum != FaturaDurumu.ODENMEDI'),
    ('fatura.Durum.ToString() != FaturaDurumu.GONDERILDI', 'fatura.Durum != FaturaDurumu.GONDERILDI'),
    ('fatura.Durum.ToString() = FaturaDurumu.ODENDI;', 'fatura.Durum = FaturaDurumu.ODENDI;'),
    ('fatura.Durum.ToString() = FaturaDurumu.IPTAL;', 'fatura.Durum = FaturaDurumu.IPTAL;'),
    ('fatura.Durum.ToString() == FaturaDurumu.GONDERILDI', 'fatura.Durum == FaturaDurumu.GONDERILDI'),
    ('fatura.Durum.ToString() == FaturaDurumu.IPTAL', 'fatura.Durum == FaturaDurumu.IPTAL'),
    ('fatura.Durum = IsEmriDurumu.IPTAL', 'fatura.Durum = FaturaDurumu.IPTAL'),
    ('fatura.Durum.ToString() = dto.Durum;', 'fatura.Durum = dto.Durum;'),
    ('fatura.FaturaTipi == null', 'fatura.FaturaTipi == null') # dummy
])

fix('Controllers/KullanicilarController.cs', [
    ('Status = BaglantiDurumu.AKTIF', 'Status = KullaniciDurumu.AKTIF')
])

fix('Controllers/IsEmirleriController.cs', [
    ('OkumaKaynagi.MOBIL', 'OkumaKaynagi.MANUEL'),
    ('isEmri.Tip == "KESIF_INCELEME"', 'isEmri.Tip == IsEmriTipi.KESIF_INCELEME'),
    ('isEmri.Tip == "SAYAC_ARIZA"', 'isEmri.Tip == IsEmriTipi.SAYAC_ARIZA'),
    ('isEmri.Tip == "MUHURLEME"', 'isEmri.Tip == IsEmriTipi.MUHURLEME')
])
