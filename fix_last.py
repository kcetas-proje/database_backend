import re

def fix(path, replacements):
    try:
        with open(path, 'r') as f: c = f.read()
        orig = c
        for o, n in replacements: c = c.replace(o, n)
        if c != orig:
            with open(path, 'w') as f: f.write(c)
    except: pass

fix('Models/FaturaKalemiCreateDto.cs', [
    ('public KalemTipi KalemTipi { get; set; } = null!;', 'public KalemTipi KalemTipi { get; set; }')
])

fix('Services/FaturaSeeder.cs', [
    ('KalemTipi = "ENERJI"', 'KalemTipi = KalemTipi.ENERJI'),
    ('KalemTipi = "DAGITIM_BEDELI"', 'KalemTipi = KalemTipi.DAGITIM_BEDELI'),
    ('KalemTipi = "VERGI_FON"', 'KalemTipi = KalemTipi.VERGI_FON'),
    ('KalemTipi = "GECIKME"', 'KalemTipi = KalemTipi.GECIKME')
])

fix('Services/IsEmriSeeder.cs', [
    ('isEmri.Tip == "SAYAC_DEGISIM"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME'),
    ('isEmri.Tip == "SOKME"', 'isEmri.Tip == IsEmriTipi.SOKME'),
    ('isEmri.Tip == "KESME"', 'isEmri.Tip == IsEmriTipi.KESME'),
    ('isEmri.Tip == "ACMA"', 'isEmri.Tip == IsEmriTipi.ACMA'),
    ('Tip = "BAGLAMA"', 'Tip = IsEmriTipi.BAGLAMA'),
    ('Tip = "DEGISTIRME"', 'Tip = IsEmriTipi.DEGISTIRME'),
    ('Tip = "SOKME"', 'Tip = IsEmriTipi.SOKME'),
    ('Tip = "KESME"', 'Tip = IsEmriTipi.KESME'),
    ('Tip = "ACMA"', 'Tip = IsEmriTipi.ACMA'),
])

fix('Controllers/TuketimNoktasıController.cs', [
    ('nokta.BaglantiDurumu', 'nokta.BaglantiDurumu.ToString()')
])

fix('Controllers/SayaclarController.cs', [
    ('sayac.Durum', 'sayac.Durum.ToString()')
])

fix('Controllers/SozlesmelerController.cs', [
    ('sozlesme.Durum', 'sozlesme.Durum.ToString()')
])

fix('Controllers/EndeksOkumaController.cs', [
    ('dto.OkumaTipi == null ? OkumaTipi.RUTIN_DONEM : dto.OkumaTipi,', 'dto.OkumaTipi ?? OkumaTipi.RUTIN_DONEM,'),
    ('dto.OkumaKaynagi == null ? OkumaKaynagi.MANUEL : dto.OkumaKaynagi,', 'dto.OkumaKaynagi ?? OkumaKaynagi.MANUEL,'),
    ('eskiOkuma.OkumaTipi', 'eskiOkuma.OkumaTipi.ToString()'),
    ('nokta.BaglantiDurumu', 'nokta.BaglantiDurumu.ToString()')
])

fix('Controllers/IsEmirleriController.cs', [
    ('dto.OkumaTipi = eskiOkuma.OkumaTipi;', 'dto.OkumaTipi = eskiOkuma.OkumaTipi.ToString();'),
    ('dto.OkumaKaynagi = eskiOkuma.OkumaKaynagi;', 'dto.OkumaKaynagi = eskiOkuma.OkumaKaynagi.ToString();'),
    ('isEmri.Tip == "ENDEKS_OKUMA"', 'isEmri.Tip == IsEmriTipi.ENDEKS_OKUMA'),
    ('isEmri.Tip == "KESIF_INCELEME"', 'isEmri.Tip == IsEmriTipi.KESIF_INCELEME'),
    ('isEmri.Tip == "SAYAC_ARIZA"', 'isEmri.Tip == IsEmriTipi.SAYAC_ARIZA'),
    ('isEmri.Tip == "MUHURLEME"', 'isEmri.Tip == IsEmriTipi.MUHURLEME')
])

fix('Controllers/FaturaController.cs', [
    ('fatura.FaturaTipi', 'fatura.FaturaTipi.ToString()'),
    ('fatura.Durum', 'fatura.Durum.ToString()'),
    ('nokta.BaglantiDurumu', 'nokta.BaglantiDurumu.ToString()'),
    ('kalem.KalemTipi', 'kalem.KalemTipi.ToString()')
])

fix('Controllers/KullanicilarController.cs', [
    ('Status = KullaniciDurumu.AKTIF', 'Status = "AKTIF"')
])

