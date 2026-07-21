import re

def rep(path, o, n):
    try:
        with open(path, 'r') as f: c = f.read()
        if o in c:
            c = c.replace(o, n)
            with open(path, 'w') as f: f.write(c)
    except: pass

# TuketimNoktasıSeeder.cs(65)
rep('Services/TuketimNoktasıSeeder.cs', 
    'BaglantiDurumu = f.PickRandom(new[] { "BAGLI", "KAPALI", "BAGLANABILIR", "BAGLANTI_BEKLIYOR" })', 
    'BaglantiDurumu = f.PickRandom<BaglantiDurumu>()')

# SozlesmeSeeder.cs(82)
rep('Services/SozlesmeSeeder.cs', 
    'AboneTipi = f.PickRandom(aboneTipleri)', 
    'AboneTipi = f.PickRandom<AboneTipi>()')

# AppDbContext.cs
rep('Models/AppDbContext.cs', 'IslemTipi.INSERT.ToString()', 'KcetasAboneApi.Models.IslemTipi.INSERT')
rep('Models/AppDbContext.cs', 'IslemTipi.UPDATE.ToString()', 'KcetasAboneApi.Models.IslemTipi.UPDATE')
rep('Models/AppDbContext.cs', 'IslemTipi.DELETE.ToString()', 'KcetasAboneApi.Models.IslemTipi.DELETE')
rep('Models/AppDbContext.cs', 'IslemTipi.STATUS_CHANGE.ToString()', 'KcetasAboneApi.Models.IslemTipi.STATUS_CHANGE')

# TuketimNoktasıController.cs(155)
rep('Controllers/TuketimNoktasıController.cs', 
    'nokta.BaglantiDurumu = dto.BaglantiDurumu;', 
    'nokta.BaglantiDurumu = dto.BaglantiDurumu.Value;')

# FaturaSeeder.cs
rep('Services/FaturaSeeder.cs', 'KalemTipi = "ENERJI_BEDELI"', 'KalemTipi = KalemTipi.ENERJI')
rep('Services/FaturaSeeder.cs', 'KalemTipi = "GECIKME_ZAMMI"', 'KalemTipi = KalemTipi.GECIKME')

# EntegrasyonOutboxController.cs
rep('Controllers/EntegrasyonOutboxController.cs', 'Durum = isEmri.Durum,', 'Durum = OutboxDurumu.BEKLIYOR,')

# IsEmriSeeder.cs
rep('Services/IsEmriSeeder.cs', 'isEmri.Tip == "SAYAC_DEGISIM"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME')
rep('Services/IsEmriSeeder.cs', 'isEmri.Tip == "SOKME"', 'isEmri.Tip == IsEmriTipi.SOKME')
rep('Services/IsEmriSeeder.cs', 'isEmri.Tip == "KESME"', 'isEmri.Tip == IsEmriTipi.KESME')

# KullanicilarController.cs
rep('Controllers/KullanicilarController.cs', 'Status = BaglantiDurumu.AKTIF', 'Status = KullaniciDurumu.AKTIF')

# SozlesmelerController.cs
rep('Controllers/SozlesmelerController.cs', 'sozlesme.Durum = IsEmriDurumu.PASIF', 'sozlesme.Durum = SozlesmeDurumu.PASIF')

# FaturaController.cs
rep('Controllers/FaturaController.cs', 'dto.FaturaTipi', 'System.Enum.Parse<FaturaTipi>(dto.FaturaTipi)')
rep('Controllers/FaturaController.cs', 'fatura.Durum.ToString() != FaturaDurumu.ODENDI', 'fatura.Durum != FaturaDurumu.ODENDI')
rep('Controllers/FaturaController.cs', 'fatura.Durum.ToString() = FaturaDurumu.ODENDI;', 'fatura.Durum = FaturaDurumu.ODENDI;')
rep('Controllers/FaturaController.cs', 'fatura.Durum.ToString() = dto.Durum;', 'fatura.Durum = dto.Durum;')

# SayaclarController.cs
rep('Controllers/SayaclarController.cs', 'sayac.Durum = IsEmriDurumu.ARIZALI', 'sayac.Durum = SayacDurumu.ARIZALI')

# IsEmirleriController.cs
rep('Controllers/IsEmirleriController.cs', 'isEmri.Tip == "SAYAC_DEGISIM"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME')
rep('Controllers/IsEmirleriController.cs', 'isEmri.Tip == "SOKME"', 'isEmri.Tip == IsEmriTipi.SOKME')
rep('Controllers/IsEmirleriController.cs', 'isEmri.Tip == "KESME"', 'isEmri.Tip == IsEmriTipi.KESME')
rep('Controllers/IsEmirleriController.cs', 'isEmri.Tip == "ACMA"', 'isEmri.Tip == IsEmriTipi.ACMA')
