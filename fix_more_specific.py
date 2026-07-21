import re
import glob

def fix(path, replacements):
    try:
        with open(path, 'r') as f: c = f.read()
        orig = c
        for o, n in replacements: c = c.replace(o, n)
        if c != orig:
            with open(path, 'w') as f: f.write(c)
    except: pass

fix('Models/AppDbContext.cs', [
    ('IslemTipi = "INSERT"', 'IslemTipi = IslemTipi.INSERT'),
    ('IslemTipi = "UPDATE"', 'IslemTipi = IslemTipi.UPDATE'),
    ('IslemTipi = "DELETE"', 'IslemTipi = IslemTipi.DELETE'),
    ('IslemTipi = "STATUS_CHANGE"', 'IslemTipi = IslemTipi.STATUS_CHANGE')
])

fix('Controllers/EndeksOkumaController.cs', [
    ('string.IsNullOrEmpty(dto.OkumaTipi)', 'dto.OkumaTipi == null'),
    ('string.IsNullOrEmpty(dto.OkumaKaynagi)', 'dto.OkumaKaynagi == null'),
    ('System.Enum.Parse<OkumaTipi>(eskiOkuma.OkumaTipi)', 'eskiOkuma.OkumaTipi'),
    ('System.Enum.Parse<BaglantiDurumu>(nokta.BaglantiDurumu)', 'nokta.BaglantiDurumu')
])

fix('Controllers/SozlesmelerController.cs', [
    ('System.Enum.Parse<SozlesmeDurumu>(sozlesme.Durum)', 'sozlesme.Durum')
])

fix('Controllers/KullanicilarController.cs', [
    ('nokta.BaglantiDurumu.ToString()', 'nokta.BaglantiDurumu') # If I broke it earlier
])

fix('Controllers/SayaclarController.cs', [
    ('Faz = sayac.Faz?.ToString(),', 'Faz = sayac.Faz,'),
    ('System.Enum.Parse<Faz>(dto.Faz)', 'dto.Faz')
])

fix('Controllers/IsEmirleriController.cs', [
    ('dto.OkumaTipi = eskiOkuma.OkumaTipi;', 'dto.OkumaTipi = eskiOkuma.OkumaTipi.ToString();'),
    ('dto.OkumaKaynagi = eskiOkuma.OkumaKaynagi;', 'dto.OkumaKaynagi = eskiOkuma.OkumaKaynagi.ToString();')
])

fix('Controllers/FaturaController.cs', [
    ('System.Enum.Parse<FaturaTipi>(fatura.FaturaTipi.ToString())', 'fatura.FaturaTipi')
])

