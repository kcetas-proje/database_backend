import subprocess
import re

out = subprocess.run(['dotnet', 'build'], capture_output=True, text=True).stdout

def replace_in_file(fp, lnum, old, new):
    try:
        with open(fp, 'r') as f: lines = f.readlines()
        idx = lnum - 1
        lines[idx] = lines[idx].replace(old, new)
        with open(fp, 'w') as f: f.writelines(lines)
    except Exception as e:
        pass

for line in out.split('\n'):
    m = re.search(r"([a-zA-Z0-9_/\.\-\s]+)\((\d+),(\d+)\): error CS(\d+): (.*) \[.*\]", line)
    if m:
        fp, lnum, cnum, err_code, msg = m.groups()
        lnum = int(lnum)
        
        if "AppDbContext.cs" in fp:
            replace_in_file(fp, lnum, 'IslemTipi.INSERT', 'KcetasAboneApi.Models.IslemTipi.INSERT')
            replace_in_file(fp, lnum, 'IslemTipi.UPDATE', 'KcetasAboneApi.Models.IslemTipi.UPDATE')
            replace_in_file(fp, lnum, 'IslemTipi.DELETE', 'KcetasAboneApi.Models.IslemTipi.DELETE')
            replace_in_file(fp, lnum, 'IslemTipi.STATUS_CHANGE', 'KcetasAboneApi.Models.IslemTipi.STATUS_CHANGE')
        elif "FakerHelper.cs" in fp:
            replace_in_file(fp, lnum, 'public static string Faz()', 'public static Faz GetFaz()')
            replace_in_file(fp, lnum, 'Faz.TEK_FAZ, Faz.UC_FAZ', '(Faz?)Faz.TEK_FAZ, (Faz?)Faz.UC_FAZ')
        elif "SayaclarController.cs" in fp:
            replace_in_file(fp, lnum, 'FakerHelper.Faz()', 'FakerHelper.GetFaz()')
            replace_in_file(fp, lnum, 'sayac.Durum = IsEmriDurumu.ARIZALI', 'sayac.Durum = SayacDurumu.ARIZALI')
        elif "SozlesmelerController.cs" in fp:
            replace_in_file(fp, lnum, 'sozlesme.Durum = IsEmriDurumu.PASIF', 'sozlesme.Durum = SozlesmeDurumu.PASIF')
        elif "IsEmirleriController.cs" in fp:
            replace_in_file(fp, lnum, 'isEmri.Tip == "SAYAC_DEGISIM"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME')
            replace_in_file(fp, lnum, 'isEmri.Tip == "SAYAC_ARIZA"', 'isEmri.Tip == IsEmriTipi.SAYAC_ARIZA')
            replace_in_file(fp, lnum, 'isEmri.Tip == "MUHURLEME"', 'isEmri.Tip == IsEmriTipi.MUHURLEME')
            replace_in_file(fp, lnum, 'isEmri.Tip == "KESIF_INCELEME"', 'isEmri.Tip == IsEmriTipi.KESIF_INCELEME')
        elif "FaturaController.cs" in fp:
            replace_in_file(fp, lnum, 'fatura.Durum.ToString() == FaturaDurumu.ODENDI', 'fatura.Durum == FaturaDurumu.ODENDI')
            replace_in_file(fp, lnum, 'fatura.Durum.ToString() != FaturaDurumu.ODENMEDI', 'fatura.Durum != FaturaDurumu.ODENMEDI')
            replace_in_file(fp, lnum, 'fatura.Durum.ToString() != FaturaDurumu.GONDERILDI', 'fatura.Durum != FaturaDurumu.GONDERILDI')
            replace_in_file(fp, lnum, 'fatura.Durum.ToString() == FaturaDurumu.GONDERILDI', 'fatura.Durum == FaturaDurumu.GONDERILDI')
            replace_in_file(fp, lnum, 'fatura.Durum.ToString() == FaturaDurumu.IPTAL', 'fatura.Durum == FaturaDurumu.IPTAL')
            replace_in_file(fp, lnum, 'System.Enum.Parse<FaturaTipi>(dto.FaturaTipi)', 'dto.FaturaTipi')
            replace_in_file(fp, lnum, 'fatura.Durum.ToString() = FaturaDurumu.ODENDI;', 'fatura.Durum = FaturaDurumu.ODENDI;')
            replace_in_file(fp, lnum, 'fatura.Durum.ToString() = FaturaDurumu.IPTAL;', 'fatura.Durum = FaturaDurumu.IPTAL;')
            replace_in_file(fp, lnum, 'fatura.Durum.ToString() = dto.Durum;', 'fatura.Durum = dto.Durum;')
            replace_in_file(fp, lnum, 'System.Enum.Parse<KalemTipi>(dtoKalem.KalemTipi)', 'dtoKalem.KalemTipi')
        elif "KullanicilarController.cs" in fp:
            replace_in_file(fp, lnum, 'Status = BaglantiDurumu.AKTIF', 'Status = KullaniciDurumu.AKTIF')
        elif "TuketimNoktasıSeeder.cs" in fp:
            replace_in_file(fp, lnum, 'BaglantiDurumu = f.PickRandom(new[] { "BAGLI", "KAPALI", "BAGLANABILIR", "BAGLANTI_BEKLIYOR" })', 'BaglantiDurumu = f.PickRandom<BaglantiDurumu>()')
        elif "TuketimNoktasıController.cs" in fp:
            replace_in_file(fp, lnum, 'nokta.BaglantiDurumu = dto.BaglantiDurumu;', 'nokta.BaglantiDurumu = dto.BaglantiDurumu.Value;')
            replace_in_file(fp, lnum, 'string.IsNullOrEmpty(dto.BaglantiDurumu)', 'dto.BaglantiDurumu == null')
        elif "SozlesmeSeeder.cs" in fp:
            replace_in_file(fp, lnum, 'AboneTipi = f.PickRandom(aboneTipleri)', 'AboneTipi = f.PickRandom<AboneTipi>()')
        elif "EntegrasyonOutboxController.cs" in fp:
            replace_in_file(fp, lnum, 'Durum = isEmri.Durum,', 'Durum = OutboxDurumu.BEKLIYOR,')
        elif "IsEmriSeeder.cs" in fp:
            replace_in_file(fp, lnum, 'isEmri.Tip == "SAYAC_DEGISIM"', 'isEmri.Tip == IsEmriTipi.DEGISTIRME')
            replace_in_file(fp, lnum, 'isEmri.Tip == "SOKME"', 'isEmri.Tip == IsEmriTipi.SOKME')
            replace_in_file(fp, lnum, 'isEmri.Tip == "KESME"', 'isEmri.Tip == IsEmriTipi.KESME')
            replace_in_file(fp, lnum, 'isEmri.Tip == "ACMA"', 'isEmri.Tip == IsEmriTipi.ACMA')
        elif "FaturaSeeder.cs" in fp:
            replace_in_file(fp, lnum, 'KalemTipi = "ENERJI_BEDELI"', 'KalemTipi = KalemTipi.ENERJI')
            replace_in_file(fp, lnum, 'KalemTipi = "GECIKME_ZAMMI"', 'KalemTipi = KalemTipi.GECIKME')
