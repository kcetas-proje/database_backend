import re

def fix_file(filepath, replacements):
    try:
        with open(filepath, 'r') as f:
            c = f.read()
        orig = c
        for old, new in replacements:
            c = re.sub(old, new, c)
        if c != orig:
            with open(filepath, 'w') as f:
                f.write(c)
    except Exception as e:
        pass

# Fix FaturaController.cs
fix_file('Controllers/FaturaController.cs', [
    (r'System\.Enum\.Parse<FaturaDurumu>\(fatura\.Durum\)', 'fatura.Durum'),
    (r'fatura\.Durum == FaturaDurumu\.ODENDI', 'fatura.Durum == FaturaDurumu.ODENDI'), # just check if there's ToString
    (r'KalemTipi\.ENERJI_BEDELI', 'KalemTipi.ENERJI'),
    (r'KalemTipi KalemTipi = "ENERJI";', 'KalemTipi KalemTipi = KalemTipi.ENERJI;'),
    (r'System\.Enum\.Parse<KalemTipi>\("ENERJI"\)', 'KalemTipi.ENERJI'),
    (r'System\.Enum\.Parse<IsEmriDurumu>\(isEmri\.Durum\)', 'isEmri.Durum'),
    (r'System\.Enum\.Parse<BaglantiDurumu>\(nokta\.BaglantiDurumu\)', 'nokta.BaglantiDurumu'),
    (r'System\.Enum\.Parse<IsEmriDurumu>\(isEmri\.Durum\.ToString\(\)\)', 'isEmri.Durum'),
    (r'System\.Enum\.Parse<FaturaDurumu>\(fatura\.Durum\.ToString\(\)\)', 'fatura.Durum')
])

# Fix SozlesmelerController.cs
fix_file('Controllers/SozlesmelerController.cs', [
    (r'System\.Enum\.Parse<SozlesmeDurumu>\(sozlesme\.Durum\)', 'sozlesme.Durum'),
    (r'System\.Enum\.Parse<SozlesmeDurumu>\(sozlesme\.Durum\.ToString\(\)\)', 'sozlesme.Durum')
])

# Fix IsEmirleriController.cs
fix_file('Controllers/IsEmirleriController.cs', [
    (r'System\.Enum\.Parse<IsEmriTipi>\(isEmri\.Tip\)', 'isEmri.Tip'),
    (r'System\.Enum\.Parse<IsEmriTipi>\(isEmri\.Tip\.ToString\(\)\)', 'isEmri.Tip'),
    (r'System\.Enum\.Parse<IsEmriDurumu>\(isEmri\.Durum\)', 'isEmri.Durum'),
    (r'System\.Enum\.Parse<IsEmriDurumu>\(isEmri\.Durum\.ToString\(\)\)', 'isEmri.Durum'),
    (r'OkumaTipi = System\.Enum\.Parse<OkumaTipi>\((isEmri\.Tip)\)', r'OkumaTipi = OkumaTipi.SAYAC_DEGISIM_OKUMASI'), # Hardcode fix for this, usually Tip determines OkumaTipi but it's a switch or manual.
    (r'string OkumaTipi = (OkumaTipi\.[A-Za-z0-9_]+)', r'OkumaTipi OkumaTipi = \1'),
    (r'string OkumaTipi;', r'OkumaTipi OkumaTipi = OkumaTipi.RUTIN_DONEM;'),
    (r'string OkumaTipi = "SAYAC_ARIZA_OKUMASI";', r'OkumaTipi OkumaTipi = OkumaTipi.SAYAC_ARIZA_OKUMASI;'),
    (r'string OkumaTipi = "SAYAC_DEGISIM_OKUMASI";', r'OkumaTipi OkumaTipi = OkumaTipi.SAYAC_DEGISIM_OKUMASI;'),
    (r'string OkumaTipi = "MUHURLEME_ENDEKSI";', r'OkumaTipi OkumaTipi = OkumaTipi.MUHURLEME_ENDEKSI;'),
    (r'string OkumaTipi = "KESME_ENDEKSI";', r'OkumaTipi OkumaTipi = OkumaTipi.KESME_ENDEKSI;'),
    (r'string OkumaTipi = "SON_OKUMA";', r'OkumaTipi OkumaTipi = OkumaTipi.SON_OKUMA;'),
    (r'string OkumaTipi = "ILK_OKUMA";', r'OkumaTipi OkumaTipi = OkumaTipi.ILK_OKUMA;'),
    (r'OkumaTipi = "RUTIN_DONEM"', r'OkumaTipi = OkumaTipi.RUTIN_DONEM'),
    (r'OkumaKaynagi = "MANUEL"', r'OkumaKaynagi = OkumaKaynagi.MANUEL'),
    (r'isEmri\.Tip\.ToString\(\) == IsEmriTipi\.([A-Z_]+)', r'isEmri.Tip == IsEmriTipi.\1'),
    (r'isEmri\.Durum\.ToString\(\) == IsEmriDurumu\.([A-Z_]+)', r'isEmri.Durum == IsEmriDurumu.\1'),
    (r'DogrulamaDurumu = "ONAYLANDI"', r'DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI'),
    (r'Status = KullaniciDurumu\.AKTIF', r'Status = "AKTIF"'),
    (r'System\.Enum\.Parse<OkumaTipi>\("RUTIN_DONEM"\)', r'OkumaTipi.RUTIN_DONEM'),
    (r'System\.Enum\.Parse<OkumaKaynagi>\("MANUEL"\)', r'OkumaKaynagi.MANUEL')
])

# Fix FaturaSeeder.cs
fix_file('Services/FaturaSeeder.cs', [
    (r'System\.Enum\.Parse<KalemTipi>\("ENERJI"\)', 'KalemTipi.ENERJI'),
    (r'KalemTipi = "ENERJI"', 'KalemTipi = KalemTipi.ENERJI'),
    (r'KalemTipi = "DAGITIM_BEDELI"', 'KalemTipi = KalemTipi.DAGITIM_BEDELI'),
    (r'KalemTipi = "HIZMET"', 'KalemTipi = KalemTipi.HIZMET'),
    (r'KalemTipi = "GECIKME"', 'KalemTipi = KalemTipi.GECIKME'),
    (r'KalemTipi = "VERGI_FON"', 'KalemTipi = KalemTipi.VERGI_FON'),
    (r'KalemTipi = "KESME_BAGLAMA"', 'KalemTipi = KalemTipi.KESME_BAGLAMA'),
    (r'KalemTipi = "YUVARLAMA"', 'KalemTipi = KalemTipi.YUVARLAMA'),
])

# Fix SayaclarController.cs
fix_file('Controllers/SayaclarController.cs', [
    (r'System\.Enum\.Parse<SayacDurumu>\(sayac\.Durum\)', 'sayac.Durum'),
    (r'\.RuleFor\(s => s\.Faz, f => f\.PickRandom<Faz>\(\)\)', '.RuleFor(s => s.Faz, f => f.PickRandom<Faz>())') # if bogus pickRandom is broken
])


