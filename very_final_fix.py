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
    ('using Bogus;', 'using Bogus;\nusing KcetasAboneApi.Models;')
])

fix('Controllers/EntegrasyonOutboxController.cs', [
    ('OutboxDurumu = isEmri.Durum,', 'OutboxDurumu = OutboxDurumu.BEKLIYOR,') # Wait, maybe the property is not `Durum` but `OutboxDurumu` or `Durum = isEmri.Durum.ToString()`. 
    # Actually EntegrasyonOutboxController.cs:77 => `Durum = isEmri.Durum,`. 
    # It says 'KcetasAboneApi.Models.IsEmriDurumu' cannot be converted to 'KcetasAboneApi.Models.OutboxDurumu'.
    # I'll just change `isEmri.Durum` to `OutboxDurumu.BEKLIYOR`.
    # Wait, my previous script did `Durum = isEmri.Durum,` -> `Durum = OutboxDurumu.BEKLIYOR,`. 
    # Let's do regex.
])

import glob

for path in glob.glob('**/*.cs', recursive=True):
    with open(path, 'r') as f: c = f.read()
    orig = c

    # Common conversions
    c = c.replace('sozlesme.Durum = IsEmriDurumu.PASIF;', 'sozlesme.Durum = SozlesmeDurumu.PASIF;')
    c = c.replace('sozlesme.Durum == IsEmriDurumu.PASIF', 'sozlesme.Durum == SozlesmeDurumu.PASIF')
    c = c.replace('sayac.Durum = IsEmriDurumu.ARIZALI;', 'sayac.Durum = SayacDurumu.ARIZALI;')
    c = c.replace('sayac.Durum == IsEmriDurumu.ARIZALI', 'sayac.Durum == SayacDurumu.ARIZALI')
    c = c.replace('Status = BaglantiDurumu.AKTIF', 'Status = KullaniciDurumu.AKTIF')
    c = c.replace('nokta.BaglantiDurumu = dto.BaglantiDurumu.Value;', 'nokta.BaglantiDurumu = dto.BaglantiDurumu ?? BaglantiDurumu.BAGLI;')
    
    # FaturaController fixes (366, 368, 475)
    c = c.replace('fatura.Durum.ToString() != FaturaDurumu.ODENDI', 'fatura.Durum != FaturaDurumu.ODENDI')
    c = c.replace('fatura.Durum.ToString() = FaturaDurumu.ODENDI;', 'fatura.Durum = FaturaDurumu.ODENDI;')
    c = c.replace('dto.FaturaTipi', 'System.Enum.Parse<FaturaTipi>(dto.FaturaTipi)') # if it expects FaturaTipi enum but dto has string. Wait! I changed dto.FaturaTipi to Enum globally! Let's check. If dto.FaturaTipi is FaturaTipi, we don't need Enum.Parse. The error in FaturaController(251,55) is `CS1503: 1 bağımsız değişkeni: 'KcetasAboneApi.Models.FaturaTipi' öğesinden 'string?' öğesine dönüştürülemiyor`. Meaning we are passing FaturaTipi to something that expects string. Oh! `fatura.FaturaTipi.ToString()`.
    
    c = c.replace('string.IsNullOrEmpty(fatura.FaturaTipi.ToString())', 'fatura.FaturaTipi == null')
    c = c.replace('string.IsNullOrEmpty(dto.FaturaTipi.ToString())', 'dto.FaturaTipi == null')
    c = c.replace('string.IsNullOrEmpty(dtoKalem.KalemTipi.ToString())', 'dtoKalem.KalemTipi == null')
    
    c = c.replace('fatura.FaturaTipi.ToString() == FaturaTipi.KAPANIS', 'fatura.FaturaTipi == FaturaTipi.KAPANIS')

    # Fix error CS0029: 'string' to Enum conversions
    c = c.replace('Tip = "KESME"', 'Tip = IsEmriTipi.KESME')
    c = c.replace('Tip = "ACMA"', 'Tip = IsEmriTipi.ACMA')
    c = c.replace('Tip = "BAGLAMA"', 'Tip = IsEmriTipi.BAGLAMA')
    c = c.replace('Tip = "DEGISTIRME"', 'Tip = IsEmriTipi.DEGISTIRME')
    c = c.replace('Tip = "SOKME"', 'Tip = IsEmriTipi.SOKME')
    
    c = c.replace('KalemTipi = "ENERJI"', 'KalemTipi = KalemTipi.ENERJI')
    c = c.replace('KalemTipi = "DAGITIM_BEDELI"', 'KalemTipi = KalemTipi.DAGITIM_BEDELI')
    c = c.replace('KalemTipi = "VERGI_FON"', 'KalemTipi = KalemTipi.VERGI_FON')
    c = c.replace('KalemTipi = "GECIKME"', 'KalemTipi = KalemTipi.GECIKME')
    c = c.replace('KalemTipi = "YUVARLAMA"', 'KalemTipi = KalemTipi.YUVARLAMA')
    
    c = c.replace('isEmri.Tip == "SAYAC_DEGISIM"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME')
    c = c.replace('isEmri.Tip == "SOKME"', 'isEmri.Tip == IsEmriTipi.SOKME')
    c = c.replace('isEmri.Tip == "KESME"', 'isEmri.Tip == IsEmriTipi.KESME')
    c = c.replace('isEmri.Tip == "ACMA"', 'isEmri.Tip == IsEmriTipi.ACMA')
    c = c.replace('isEmri.Tip == "ENDEKS_OKUMA"', 'isEmri.Tip == IsEmriTipi.ENDEKS_OKUMA')

    c = c.replace('IslemTipi = "INSERT"', 'IslemTipi = IslemTipi.INSERT')
    c = c.replace('IslemTipi = "UPDATE"', 'IslemTipi = IslemTipi.UPDATE')
    c = c.replace('IslemTipi = "DELETE"', 'IslemTipi = IslemTipi.DELETE')
    c = c.replace('IslemTipi = "STATUS_CHANGE"', 'IslemTipi = IslemTipi.STATUS_CHANGE')

    c = c.replace('BaglantiDurumu = "BAGLI"', 'BaglantiDurumu = BaglantiDurumu.BAGLI')
    c = c.replace('AboneTipi = "MESKEN"', 'AboneTipi = AboneTipi.MESKEN')
    c = c.replace('AboneTipi = "TICARETHANE"', 'AboneTipi = AboneTipi.TICARETHANE')

    if c != orig:
        with open(path, 'w') as f: f.write(c)

