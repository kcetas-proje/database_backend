import re

def fix(path, replacements):
    try:
        with open(path, 'r') as f: c = f.read()
        orig = c
        for o, n in replacements: c = c.replace(o, n)
        if c != orig:
            with open(path, 'w') as f: f.write(c)
    except: pass

fix('Controllers/KullanicilarController.cs', [
    ('nokta.BaglantiDurumu', 'nokta.BaglantiDurumu.ToString()'), # For whatever reason it tries to assign BaglantiDurumu to KullaniciDurumu? Wait, why would it?
])

fix('Controllers/FaturaController.cs', [
    ('fatura.Tip', 'fatura.FaturaTipi'),
    ('System.Enum.Parse<FaturaTipi>(fatura.FaturaTipi)', 'fatura.FaturaTipi'),
    ('System.Enum.Parse<FaturaDurumu>(fatura.Durum)', 'fatura.Durum'),
    ('System.Enum.Parse<BaglantiDurumu>(nokta.BaglantiDurumu)', 'nokta.BaglantiDurumu'),
    ('nokta.BaglantiDurumu = "BAGLANABILIR"', 'nokta.BaglantiDurumu = BaglantiDurumu.BAGLANABILIR'),
    ('System.Enum.Parse<IsEmriDurumu>(isEmri.Durum)', 'isEmri.Durum'),
    ('dto.FaturaTipi = fatura.FaturaTipi;', 'dto.FaturaTipi = fatura.FaturaTipi.ToString();'),
    ('dto.Durum = fatura.Durum;', 'dto.Durum = fatura.Durum.ToString();'),
    ('fatura.FaturaTipi = Enum.Parse<FaturaTipi>(dto.FaturaTipi)', 'fatura.FaturaTipi = dto.FaturaTipi'),
    ('kalem.KalemTipi = Enum.Parse<KalemTipi>(dtoKalem.KalemTipi)', 'kalem.KalemTipi = dtoKalem.KalemTipi')
])

fix('Controllers/SayaclarController.cs', [
    ('Faz = sayac.Faz,', 'Faz = sayac.Faz?.ToString(),'),
    ('Durum = sayac.Durum', 'Durum = sayac.Durum.ToString()'),
    ('.RuleFor(s => s.Faz, f => f.PickRandom(new Faz?[] { Faz.TEK_FAZ, Faz.UC_FAZ }))', '.RuleFor(s => s.Faz, f => f.PickRandom(new[] { Faz.TEK_FAZ, Faz.UC_FAZ }))')
])

fix('Controllers/SozlesmelerController.cs', [
    ('Durum = sozlesme.Durum,', 'Durum = sozlesme.Durum.ToString(),')
])

fix('Controllers/IsEmirleriController.cs', [
    ('dto.Tip = isEmri.Tip;', 'dto.Tip = isEmri.Tip.ToString();'),
    ('dto.Durum = isEmri.Durum;', 'dto.Durum = isEmri.Durum.ToString();'),
    ('isEmri.Tip == "KESME"', 'isEmri.Tip == IsEmriTipi.KESME'),
    ('isEmri.Tip == "ACMA"', 'isEmri.Tip == IsEmriTipi.ACMA'),
    ('System.Enum.Parse<IsEmriTipi>(isEmri.Tip)', 'isEmri.Tip'),
    ('System.Enum.Parse<IsEmriDurumu>(isEmri.Durum)', 'isEmri.Durum'),
    ('isEmri.Tip == IsEmriTipi.SAYAC_DEGISIM', 'isEmri.Tip == IsEmriTipi.DEGISTIRME')
])

fix('Services/IsEmriSeeder.cs', [
    ('IsEmriTipi.SAYAC_DEGISIM', 'IsEmriTipi.DEGISTIRME'),
    ('IsEmriTipi.YENI_BAGLANTI')
])

fix('Services/FaturaSeeder.cs', [
    ('KalemTipi.ENERJI_BEDELI', 'KalemTipi.ENERJI'),
    ('KalemTipi.GECIKME_ZAMMI', 'KalemTipi.GECIKME')
])

# Re-run fix_errors_v3 because it's dynamic
