import re

def fix(path, replacements):
    try:
        with open(path, 'r') as f: c = f.read()
        orig = c
        for o, n in replacements: c = c.replace(o, n)
        if c != orig:
            with open(path, 'w') as f: f.write(c)
    except: pass

fix('Controllers/SozlesmelerController.cs', [
    ('sozlesme.Durum = IsEmriDurumu.PASIF', 'sozlesme.Durum = SozlesmeDurumu.PASIF'),
    ('sozlesme.Durum == IsEmriDurumu.PASIF', 'sozlesme.Durum == SozlesmeDurumu.PASIF'),
    ('sozlesme.Durum.ToString()', 'sozlesme.Durum')
])

fix('Controllers/SayaclarController.cs', [
    ('sayac.Durum = IsEmriDurumu.ARIZALI', 'sayac.Durum = SayacDurumu.ARIZALI'),
    ('sayac.Durum == IsEmriDurumu.ARIZALI', 'sayac.Durum == SayacDurumu.ARIZALI'),
    ('sayac.Durum.ToString()', 'sayac.Durum')
])

fix('Controllers/FaturaController.cs', [
    ('fatura.Durum = IsEmriDurumu.IPTAL', 'fatura.Durum = FaturaDurumu.IPTAL'),
    ('fatura.Durum == IsEmriDurumu.IPTAL', 'fatura.Durum == FaturaDurumu.IPTAL'),
    ('System.Enum.Parse<FaturaTipi>(fatura.FaturaTipi)', 'fatura.FaturaTipi'),
    ('System.Enum.Parse<FaturaDurumu>(fatura.Durum)', 'fatura.Durum'),
    ('System.Enum.Parse<BaglantiDurumu>(nokta.BaglantiDurumu)', 'nokta.BaglantiDurumu'),
    ('System.Enum.Parse<KalemTipi>(kalem.KalemTipi)', 'kalem.KalemTipi'),
    ('fatura.Durum != "ODENMEDI"', 'fatura.Durum != FaturaDurumu.ODENMEDI'),
    ('fatura.Durum != "ODENDI"', 'fatura.Durum != FaturaDurumu.ODENDI'),
    ('fatura.Durum == "ODENMEDI"', 'fatura.Durum == FaturaDurumu.ODENMEDI'),
    ('fatura.Durum == "ODENDI"', 'fatura.Durum == FaturaDurumu.ODENDI'),
    ('fatura.Durum = "ODENDI"', 'fatura.Durum = FaturaDurumu.ODENDI'),
    ('fatura.Durum = "ODENMEDI"', 'fatura.Durum = FaturaDurumu.ODENMEDI'),
    ('fatura.FaturaTipi = Enum.Parse<FaturaTipi>(dto.FaturaTipi)', 'fatura.FaturaTipi = dto.FaturaTipi'),
    ('fatura.Durum = Enum.Parse<FaturaDurumu>(dto.Durum)', 'fatura.Durum = dto.Durum'),
    ('fatura.FaturaTipi == null', 'fatura.FaturaTipi == null') # just safety
])

fix('Controllers/KullanicilarController.cs', [
    ('Status = BaglantiDurumu.AKTIF', 'Status = KullaniciDurumu.AKTIF'),
    ('Status = "AKTIF"', 'Status = KullaniciDurumu.AKTIF')
])

fix('Services/FaturaSeeder.cs', [
    ('System.Enum.Parse<KalemTipi>("ENERJI")', 'KalemTipi.ENERJI'),
    ('System.Enum.Parse<KalemTipi>("DAGITIM_BEDELI")', 'KalemTipi.DAGITIM_BEDELI'),
    ('System.Enum.Parse<KalemTipi>("VERGI_FON")', 'KalemTipi.VERGI_FON'),
    ('System.Enum.Parse<KalemTipi>("GECIKME")', 'KalemTipi.GECIKME'),
    ('KalemTipi = "ENERJI_BEDELI"', 'KalemTipi = KalemTipi.ENERJI'),
    ('KalemTipi = "GECIKME_ZAMMI"', 'KalemTipi = KalemTipi.GECIKME'),
])

fix('Services/IsEmriSeeder.cs', [
    ('isEmri.Tip == "SOKME"', 'isEmri.Tip == IsEmriTipi.SOKME'),
    ('isEmri.Tip == "DEGISTIRME"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME'),
    ('System.Enum.Parse<IsEmriTipi>("ACIL")', 'IsEmriTipi.ARIZA'), # there is no ACIL, mapped to ARIZA
    ('IsEmriTipi.ACIL', 'IsEmriTipi.ARIZA')
])

fix('Controllers/IsEmirleriController.cs', [
    ('dto.Tip = isEmri.Tip.ToString();', 'dto.Tip = isEmri.Tip;'),
    ('dto.Durum = isEmri.Durum.ToString();', 'dto.Durum = isEmri.Durum;'),
    ('isEmri.Tip == "KESME"', 'isEmri.Tip == IsEmriTipi.KESME'),
    ('isEmri.Tip == "ACMA"', 'isEmri.Tip == IsEmriTipi.ACMA'),
    ('isEmri.Tip == "ENDEKS_OKUMA"', 'isEmri.Tip == IsEmriTipi.ENDEKS_OKUMA'),
    ('isEmri.Tip == "SAYAC_DEGISIM"', 'isEmri.Tip == IsEmriTipi.SAYAC_DEGISIM'),
    ('isEmri.Tip == "SAYAC_ARIZA"', 'isEmri.Tip == IsEmriTipi.SAYAC_ARIZA'),
    ('isEmri.Tip == "MUHURLEME"', 'isEmri.Tip == IsEmriTipi.MUHURLEME'),
    ('isEmri.Tip == "KESIF_INCELEME"', 'isEmri.Tip == IsEmriTipi.KESIF_INCELEME'),
    ('OkumaTipi = eskiOkuma.OkumaTipi.ToString()', 'OkumaTipi = eskiOkuma.OkumaTipi'),
    ('OkumaKaynagi = eskiOkuma.OkumaKaynagi.ToString()', 'OkumaKaynagi = eskiOkuma.OkumaKaynagi')
])

fix('Models/AppDbContext.cs', [
    ('IslemTipi.INSERT.ToString()', 'IslemTipi.INSERT'),
    ('IslemTipi.UPDATE.ToString()', 'IslemTipi.UPDATE'),
    ('IslemTipi.DELETE.ToString()', 'IslemTipi.DELETE'),
    ('IslemTipi.STATUS_CHANGE.ToString()', 'IslemTipi.STATUS_CHANGE')
])

