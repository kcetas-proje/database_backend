import re

def fix(path, replacements):
    try:
        with open(path, 'r') as f: c = f.read()
        orig = c
        for o, n in replacements: c = c.replace(o, n)
        if c != orig:
            with open(path, 'w') as f: f.write(c)
    except: pass

fix('Controllers/FaturaController.cs', [
    ('fatura.Durum.ToString() == FaturaDurumu.ODENDI', 'fatura.Durum == FaturaDurumu.ODENDI'),
    ('fatura.Durum.ToString() != FaturaDurumu.ODENMEDI', 'fatura.Durum != FaturaDurumu.ODENMEDI'),
    ('fatura.Durum.ToString() != FaturaDurumu.GONDERILDI', 'fatura.Durum != FaturaDurumu.GONDERILDI'),
    ('fatura.Durum.ToString() = FaturaDurumu.ODENDI;', 'fatura.Durum = FaturaDurumu.ODENDI;'),
    ('fatura.Durum.ToString() = System.Enum.Parse<IsEmriDurumu>(dto.Durum.ToString());', 'fatura.Durum = dto.Durum;'),
    ('fatura.Durum.ToString() == FaturaDurumu.GONDERILDI', 'fatura.Durum == FaturaDurumu.GONDERILDI'),
    ('fatura.Durum.ToString() == FaturaDurumu.IPTAL', 'fatura.Durum == FaturaDurumu.IPTAL'),
    ('fatura.Durum.ToString() = FaturaDurumu.IPTAL;', 'fatura.Durum = FaturaDurumu.IPTAL;')
])

fix('Controllers/IsEmirleriController.cs', [
    ('OkumaTipi = "ACILIS",', 'OkumaTipi = OkumaTipi.ILK_OKUMA,'),
    ('OkumaKaynagi = "MOBIL",', 'OkumaKaynagi = OkumaKaynagi.MOBIL,'),
    ('System.Enum.Parse<OkumaTipi>("ACILIS")', 'OkumaTipi.ILK_OKUMA'),
    ('System.Enum.Parse<OkumaKaynagi>("MOBIL")', 'OkumaKaynagi.MOBIL'),
])

fix('Services/IsEmriSeeder.cs', [
    ('IsEmriTipi.ACIL', 'IsEmriTipi.ARIZA')
])
