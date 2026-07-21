import re

def fix(path, replacements):
    try:
        with open(path, 'r') as f: c = f.read()
        orig = c
        for o, n in replacements: c = c.replace(o, n)
        if c != orig:
            with open(path, 'w') as f: f.write(c)
    except: pass

fix('Models/IsEmriCreateDto.cs', [
    ('public IsEmriTipi Tip { get; set; } = null!;', 'public IsEmriTipi Tip { get; set; }'),
    ('public IsEmriDurumu Durum { get; set; } = "ACIK";', 'public IsEmriDurumu Durum { get; set; } = IsEmriDurumu.ACIK;')
])

fix('Controllers/EndeksOkumaController.cs', [
    ('nokta.BaglantiDurumu = "BAGLANTI_BEKLIYOR";', 'nokta.BaglantiDurumu = BaglantiDurumu.BAGLANTI_BEKLIYOR;')
])

fix('Controllers/EntegrasyonOutboxController.cs', [
    ('Durum = isEmri.Durum,', 'Durum = OutboxDurumu.BEKLIYOR,')
])

fix('Controllers/AuthController.cs', [
    ('kullanici.Durum != "AKTIF"', 'kullanici.Durum != KullaniciDurumu.AKTIF')
])

fix('Controllers/SayaclarController.cs', [
    ('string.IsNullOrEmpty(dto.Faz)', 'dto.Faz == null'),
    ('sayac.Durum == "ARIZALI"', 'sayac.Durum == SayacDurumu.ARIZALI')
])

fix('Controllers/KullanicilarController.cs', [
    ('Status = BaglantiDurumu.AKTIF', 'Status = KullaniciDurumu.AKTIF')
])

fix('Controllers/IsEmirleriController.cs', [
    ('isEmri.Tip == "SAYAC_DEGISIM"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME'),
    ('isEmri.Tip == "ENDEKS_OKUMA"', 'isEmri.Tip == IsEmriTipi.ENDEKS_OKUMA')
])

fix('Controllers/FaturaController.cs', [
    ('string.IsNullOrEmpty(fatura.FaturaTipi)', 'fatura.FaturaTipi == null'),
    ('fatura.FaturaTipi == "KAPANIS"', 'fatura.FaturaTipi == FaturaTipi.KAPANIS'),
    ('fatura.Durum == "ODENDI"', 'fatura.Durum == FaturaDurumu.ODENDI'),
    ('Durum = "ODENMEDI",', 'Durum = FaturaDurumu.ODENMEDI,'),
    ('nokta.BaglantiDurumu == "KAPALI"', 'nokta.BaglantiDurumu == BaglantiDurumu.KAPALI'),
    ('fatura.Durum = "IPTAL";', 'fatura.Durum = FaturaDurumu.IPTAL;'),
    ('kalem.KalemTipi == "ENERJI"', 'kalem.KalemTipi == KalemTipi.ENERJI')
])

fix('Controllers/SozlesmelerController.cs', [
    ('sozlesme.Durum = "PASIF";', 'sozlesme.Durum = SozlesmeDurumu.PASIF;')
])

fix('Services/IsEmriSeeder.cs', [
    ('isEmri.Tip == "SOKME"', 'isEmri.Tip == IsEmriTipi.SOKME'),
    ('isEmri.Tip == "DEGISTIRME"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME')
])

fix('Services/FaturaSeeder.cs', [
    ('KalemTipi.ENERJI_BEDELI', 'KalemTipi.ENERJI'),
    ('KalemTipi.GECIKME_ZAMMI', 'KalemTipi.GECIKME')
])
