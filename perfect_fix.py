import re
import glob

def replace_regex(path, pattern, replacement):
    with open(path, 'r') as f: c = f.read()
    c = re.sub(pattern, replacement, c)
    with open(path, 'w') as f: f.write(c)

replace_regex('Helpers/FakerHelper.cs', r'public static string Faz\(\)', r'public static Faz GetFaz()')
replace_regex('Helpers/FakerHelper.cs', r'Faker\.PickRandom\(\(Faz\?\)Faz\.TEK_FAZ,\s*\(Faz\?\)Faz\.UC_FAZ\)', r'Faker.PickRandom(Faz.TEK_FAZ, Faz.UC_FAZ)')

replace_regex('Controllers/SayaclarController.cs', r'FakerHelper\.Faz\(\)', r'FakerHelper.GetFaz()')
replace_regex('Controllers/SayaclarController.cs', r'IsEmriDurumu\.ARIZALI', r'SayacDurumu.ARIZALI')

replace_regex('Controllers/SozlesmelerController.cs', r'IsEmriDurumu\.PASIF', r'SozlesmeDurumu.PASIF')

replace_regex('Controllers/KullanicilarController.cs', r'BaglantiDurumu\.AKTIF', r'KullaniciDurumu.AKTIF')

replace_regex('Services/TuketimNoktasıSeeder.cs', r'BaglantiDurumu\s*=\s*f\.PickRandom\(new\[\]\s*\{\s*"BAGLI",\s*"KAPALI",\s*"BAGLANABILIR",\s*"BAGLANTI_BEKLIYOR"\s*\}\)', r'BaglantiDurumu = f.PickRandom<BaglantiDurumu>()')

replace_regex('Services/SozlesmeSeeder.cs', r'AboneTipi\s*=\s*f\.PickRandom\(aboneTipleri\)', r'AboneTipi = f.PickRandom<AboneTipi>()')

replace_regex('Services/FaturaSeeder.cs', r'KalemTipi\s*=\s*"ENERJI_BEDELI"', r'KalemTipi = KalemTipi.ENERJI')
replace_regex('Services/FaturaSeeder.cs', r'KalemTipi\s*=\s*"GECIKME_ZAMMI"', r'KalemTipi = KalemTipi.GECIKME')

replace_regex('Services/IsEmriSeeder.cs', r'isEmri\.Tip\s*==\s*"SAYAC_DEGISIM"', r'isEmri.Tip == IsEmriTipi.DEGISTIRME')
replace_regex('Services/IsEmriSeeder.cs', r'isEmri\.Tip\s*==\s*"SOKME"', r'isEmri.Tip == IsEmriTipi.SOKME')
replace_regex('Services/IsEmriSeeder.cs', r'isEmri\.Tip\s*==\s*"KESME"', r'isEmri.Tip == IsEmriTipi.KESME')
replace_regex('Services/IsEmriSeeder.cs', r'isEmri\.Tip\s*==\s*"ACMA"', r'isEmri.Tip == IsEmriTipi.ACMA')

replace_regex('Controllers/TuketimNoktasıController.cs', r'nokta\.BaglantiDurumu\s*=\s*dto\.BaglantiDurumu;', r'nokta.BaglantiDurumu = dto.BaglantiDurumu.Value;')

replace_regex('Models/AppDbContext.cs', r'IslemTipi\.INSERT\.ToString\(\)', r'IslemTipi.INSERT')
replace_regex('Models/AppDbContext.cs', r'IslemTipi\.UPDATE\.ToString\(\)', r'IslemTipi.UPDATE')
replace_regex('Models/AppDbContext.cs', r'IslemTipi\.DELETE\.ToString\(\)', r'IslemTipi.DELETE')
replace_regex('Models/AppDbContext.cs', r'IslemTipi\.STATUS_CHANGE\.ToString\(\)', r'IslemTipi.STATUS_CHANGE')

replace_regex('Controllers/IsEmirleriController.cs', r'isEmri\.Tip\s*==\s*"SAYAC_DEGISIM"', r'isEmri.Tip == IsEmriTipi.DEGISTIRME')
replace_regex('Controllers/IsEmirleriController.cs', r'isEmri\.Tip\s*==\s*"SOKME"', r'isEmri.Tip == IsEmriTipi.SOKME')
replace_regex('Controllers/IsEmirleriController.cs', r'isEmri\.Tip\s*==\s*"KESME"', r'isEmri.Tip == IsEmriTipi.KESME')
replace_regex('Controllers/IsEmirleriController.cs', r'isEmri\.Tip\s*==\s*"ACMA"', r'isEmri.Tip == IsEmriTipi.ACMA')
replace_regex('Controllers/IsEmirleriController.cs', r'isEmri\.Tip\s*==\s*"ENDEKS_OKUMA"', r'isEmri.Tip == IsEmriTipi.ENDEKS_OKUMA')

replace_regex('Controllers/FaturaController.cs', r'dto\.FaturaTipi', r'System.Enum.Parse<FaturaTipi>(dto.FaturaTipi)')
replace_regex('Controllers/FaturaController.cs', r'fatura\.Durum\.ToString\(\)\s*!=\s*FaturaDurumu\.ODENDI', r'fatura.Durum != FaturaDurumu.ODENDI')
replace_regex('Controllers/FaturaController.cs', r'fatura\.Durum\.ToString\(\)\s*=\s*FaturaDurumu\.ODENDI;', r'fatura.Durum = FaturaDurumu.ODENDI;')
replace_regex('Controllers/FaturaController.cs', r'fatura\.Durum\.ToString\(\)\s*==\s*FaturaDurumu\.ODENMEDI', r'fatura.Durum == FaturaDurumu.ODENMEDI')

replace_regex('Controllers/EntegrasyonOutboxController.cs', r'OutboxDurumu\s*=\s*isEmri\.Durum,', r'OutboxDurumu = OutboxDurumu.BEKLIYOR,')
