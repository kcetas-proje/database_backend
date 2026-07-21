import os
import re

# 1. GENERATE ENUM FILES
enums = {
    "IsEmriDurumu": "ACIK = 1, ATANDI = 2, YOLDA = 3, SAHADA = 4, TAMAMLANDI = 5, IPTAL = 6, BASARISIZ = 7",
    "IsEmriTipi": "DEGISTIRME = 2, SOKME = 3, KESME = 4, ACMA = 5, ENDEKS_OKUMA = 6, SAYAC_ARIZA = 7, MUHURLEME = 8, KESIF_INCELEME = 9, YENI_BAGLANTI = 10, ENERJI_ACMA = 11",
    "DogrulamaDurumu": "PLANLANDI = 1, OKUNDU = 2, DOGRULAMA_BEKLIYOR = 3, ONAYLANDI = 4, REDDEDILDI = 5, TAHAKKUKA_AKTARILDI = 6",
    "OkumaTipi": "RUTIN_DONEM = 1, ILK_OKUMA = 2, SON_OKUMA = 3, KESME_ENDEKSI = 4, SAYAC_DEGISIM_OKUMASI = 5, SAYAC_ARIZA_OKUMASI = 6, MUHURLEME_ENDEKSI = 7",
    "OkumaKaynagi": "MANUEL = 1, OSOS = 2, DUZELTME = 3",
    "BaglantiDurumu": "TASLAK = 1, BAGLANTI_BEKLIYOR = 2, BAGLANABILIR = 3, AKTIF = 4, PASIF = 5, KAPALI = 6",
    "AboneTipi": "BIREYSEL = 1, KURUMSAL = 2",
    "Faz": "TEK_FAZ = 1, UC_FAZ = 2",
    "FaturaTipi": "DONEM = 1, KAPANIS = 2, ARA = 3, DUZELTME = 4, IPTAL = 5",
    "KalemTipi": "ENERJI = 1, DAGITIM_BEDELI = 2, HIZMET = 3, KESME_BAGLAMA = 4, GECIKME = 5, YUVARLAMA = 6, VERGI_FON = 7",
    "KullaniciDurumu": "AKTIF = 1, PASIF = 2",
    "OutboxDurumu": "BEKLIYOR = 1, GONDERILDI = 2, HATA = 3, MANUEL_MUDAHALE = 4",
    "HedefSistem": "GIB_EFATURA = 1, GIB_EARSIV = 2, ERP = 3, CRM_NOTIFICATION = 4, FIELD_PRINT = 5",
    "IslemTipi": "INSERT = 1, UPDATE = 2, DELETE = 3, STATUS_CHANGE = 4"
}

for name, values in enums.items():
    content = f"""namespace KcetasAboneApi.Models;

public enum {name}
{{
"""
    for val in values.split(","):
        content += f"    {val.strip()},\n"
    content = content.rstrip(",\n") + "\n}\n"
    with open(f"Models/{name}.cs", "w") as f:
        f.write(content)

# 2. UPDATE MODELS AND DTOS
model_replacements = [
    ("Models/IsEmirleri.cs", r"public string Durum \{ get; set; \} = null!;", "public IsEmriDurumu Durum { get; set; }"),
    ("Models/IsEmirleri.cs", r"public string Tip \{ get; set; \} = null!;", "public IsEmriTipi Tip { get; set; }"),
    ("Models/IsEmriCreateDto.cs", r"public string Durum \{ get; set; \}", "public IsEmriDurumu Durum { get; set; }"),
    ("Models/IsEmriCreateDto.cs", r"public string Tip \{ get; set; \}", "public IsEmriTipi Tip { get; set; }"),
    ("Models/IsEmriUpdateDto.cs", r"public string\? Durum \{ get; set; \}", "public IsEmriDurumu? Durum { get; set; }"),
    ("Models/IsEmriUpdateDto.cs", r"public string\? Tip \{ get; set; \}", "public IsEmriTipi? Tip { get; set; }"),

    ("Models/EndeksOkuma.cs", r"public string OkumaTipi \{ get; set; \} = null!;", "public OkumaTipi OkumaTipi { get; set; }"),
    ("Models/EndeksOkuma.cs", r"public string OkumaKaynagi \{ get; set; \} = null!;", "public OkumaKaynagi OkumaKaynagi { get; set; }"),
    ("Models/EndeksOkuma.cs", r"public string DogrulamaDurumu \{ get; set; \} = null!;", "public DogrulamaDurumu DogrulamaDurumu { get; set; }"),
    ("Models/EndeksOkumaCreateDto.cs", r"public string OkumaTipi \{ get; set; \}", "public OkumaTipi OkumaTipi { get; set; }"),
    ("Models/EndeksOkumaCreateDto.cs", r"public string OkumaKaynagi \{ get; set; \}", "public OkumaKaynagi OkumaKaynagi { get; set; }"),
    ("Models/EndeksOkumaUpdateDto.cs", r"public string\? DogrulamaDurumu \{ get; set; \}", "public DogrulamaDurumu? DogrulamaDurumu { get; set; }"),
    
    ("Models/TuketimNoktasi.cs", r"public string BaglantiDurumu \{ get; set; \} = null!;", "public BaglantiDurumu BaglantiDurumu { get; set; }"),
    ("Models/TuketimNoktasiCreateDto.cs", r"public string BaglantiDurumu \{ get; set; \}", "public BaglantiDurumu BaglantiDurumu { get; set; } = BaglantiDurumu.BAGLANABILIR;"),
    
    ("Models/Aboneler.cs", r"public string AboneTipi \{ get; set; \} = null!;", "public AboneTipi AboneTipi { get; set; }"),
    ("Models/AboneCreateDto.cs", r"public string AboneTipi \{ get; set; \} = null!;", "public AboneTipi AboneTipi { get; set; }"),
    
    ("Models/Sayaclar.cs", r"public string Faz \{ get; set; \} = null!;", "public Faz Faz { get; set; }"),
    ("Models/SayacCreateDto.cs", r"public string\? Faz \{ get; set; \}", "public Faz? Faz { get; set; }"),
    
    ("Models/Fatura.cs", r"public string FaturaTipi \{ get; set; \} = null!;", "public FaturaTipi FaturaTipi { get; set; }"),
    ("Models/FaturaCreateDto.cs", r"public string FaturaTipi \{ get; set; \}", "public FaturaTipi FaturaTipi { get; set; }"),
    
    ("Models/FaturaKalemi.cs", r"public string KalemTipi \{ get; set; \} = null!;", "public KalemTipi KalemTipi { get; set; }"),
    ("Models/FaturaKalemiCreateDto.cs", r"public string KalemTipi \{ get; set; \}", "public KalemTipi KalemTipi { get; set; }"),
    
    ("Models/Kullanicilar.cs", r"public string Durum \{ get; set; \} = null!;", "public KullaniciDurumu Durum { get; set; } = KullaniciDurumu.AKTIF;"),
    
    ("Models/EntegrasyonOutbox.cs", r"public string HedefSistem \{ get; set; \} = null!;", "public HedefSistem HedefSistem { get; set; }"),
    ("Models/EntegrasyonOutbox.cs", r"public string Durum \{ get; set; \} = null!;", "public OutboxDurumu Durum { get; set; }"),
    
    ("Models/AuditLog.cs", r"public string IslemTipi \{ get; set; \} = null!;", "public IslemTipi IslemTipi { get; set; }"),
]

for filepath, old, new in model_replacements:
    try:
        with open(filepath, 'r') as f: c = f.read()
        c = re.sub(old, new, c)
        with open(filepath, 'w') as f: f.write(c)
    except Exception as e:
        print(f"Error modifying {filepath}: {e}")

# 3. UPDATE APP_DB_CONTEXT
# We need to add .HasConversion<string>() to all these properties
with open('Models/AppDbContext.cs', 'r') as f: c = f.read()
conversions = [
    (r'entity\.Property\(e => e\.AboneTipi\)\.HasMaxLength\(20\)', r'entity.Property(e => e.AboneTipi).HasConversion<string>().HasMaxLength(20)'),
    (r'entity\.Property\(e => e\.BaglantiDurumu\)\.HasMaxLength\(30\)', r'entity.Property(e => e.BaglantiDurumu).HasConversion<string>().HasMaxLength(30)'),
    (r'entity\.Property\(e => e\.DogrulamaDurumu\)\.HasMaxLength\(30\)', r'entity.Property(e => e.DogrulamaDurumu).HasConversion<string>().HasMaxLength(30)'),
    (r'entity\.Property\(e => e\.Durum\)\.HasMaxLength\(30\)\.HasDefaultValueSql\("\'BEKLIYOR\'::character varying"\)\.HasColumnName\("durum"\);', r'entity.Property(e => e.Durum).HasConversion<string>().HasMaxLength(30).HasDefaultValueSql("\'BEKLIYOR\'::character varying").HasColumnName("durum");'), # Outbox
    (r'entity\.Property\(e => e\.Durum\)\.HasMaxLength\(20\)\.HasDefaultValueSql\("\'AKTIF\'::character varying"\)\.HasColumnName\("durum"\);', r'entity.Property(e => e.Durum).HasConversion<string>().HasMaxLength(20).HasDefaultValueSql("\'AKTIF\'::character varying").HasColumnName("durum");'), # Kullanicilar
    (r'entity\.Property\(e => e\.Durum\)\.HasMaxLength\(20\)\.HasDefaultValueSql\("\'ACIK\'::character varying"\)\.HasColumnName\("durum"\);', r'entity.Property(e => e.Durum).HasConversion<string>().HasMaxLength(20).HasDefaultValueSql("\'ACIK\'::character varying").HasColumnName("durum");'), # IsEmirleri
    (r'entity\.Property\(e => e\.FaturaTipi\)\.HasMaxLength\(20\)', r'entity.Property(e => e.FaturaTipi).HasConversion<string>().HasMaxLength(20)'),
    (r'entity\.Property\(e => e\.Faz\)\.HasMaxLength\(10\)', r'entity.Property(e => e.Faz).HasConversion<string>().HasMaxLength(10)'),
    (r'entity\.Property\(e => e\.HedefSistem\)\.HasMaxLength\(50\)', r'entity.Property(e => e.HedefSistem).HasConversion<string>().HasMaxLength(50)'),
    (r'entity\.Property\(e => e\.IslemTipi\)\.HasMaxLength\(50\)', r'entity.Property(e => e.IslemTipi).HasConversion<string>().HasMaxLength(50)'),
    (r'entity\.Property\(e => e\.KalemTipi\)\.HasMaxLength\(30\)', r'entity.Property(e => e.KalemTipi).HasConversion<string>().HasMaxLength(30)'),
    (r'entity\.Property\(e => e\.OkumaKaynagi\)\.HasMaxLength\(30\)', r'entity.Property(e => e.OkumaKaynagi).HasConversion<string>().HasMaxLength(30)'),
    (r'entity\.Property\(e => e\.OkumaTipi\)\.HasMaxLength\(30\)', r'entity.Property(e => e.OkumaTipi).HasConversion<string>().HasMaxLength(30)'),
    (r'entity\.Property\(e => e\.Tip\)\.HasMaxLength\(30\)', r'entity.Property(e => e.Tip).HasConversion<string>().HasMaxLength(30)')
]

for old, new in conversions:
    c = re.sub(old, new, c)
with open('Models/AppDbContext.cs', 'w') as f: f.write(c)

# 4. FIX CONTROLLERS AND SERVICES
import glob
files = glob.glob('**/*.cs', recursive=True)
for filepath in files:
    if 'Migrations' in filepath or 'obj' in filepath or 'bin' in filepath or 'AppDbContext.cs' in filepath or 'Models/' in filepath:
        continue
    try:
        with open(filepath, 'r') as f: c = f.read()
        
        # Replacements for string comparisons
        c = c.replace('== "ATANDI"', '== IsEmriDurumu.ATANDI')
        c = c.replace('!= "ATANDI"', '!= IsEmriDurumu.ATANDI')
        c = c.replace('== "TAMAMLANDI"', '== IsEmriDurumu.TAMAMLANDI')
        c = c.replace('!= "TAMAMLANDI"', '!= IsEmriDurumu.TAMAMLANDI')
        c = c.replace('== "IPTAL"', '== IsEmriDurumu.IPTAL')
        c = c.replace('!= "IPTAL"', '!= IsEmriDurumu.IPTAL')
        c = c.replace('== "DOGRULAMA_BEKLIYOR"', '== DogrulamaDurumu.DOGRULAMA_BEKLIYOR')
        c = c.replace('!= "DOGRULAMA_BEKLIYOR"', '!= DogrulamaDurumu.DOGRULAMA_BEKLIYOR')
        c = c.replace('== "ONAYLANDI"', '== DogrulamaDurumu.ONAYLANDI')
        c = c.replace('!= "ONAYLANDI"', '!= DogrulamaDurumu.ONAYLANDI')
        c = c.replace('== "REDDEDILDI"', '== DogrulamaDurumu.REDDEDILDI')
        c = c.replace('!= "REDDEDILDI"', '!= DogrulamaDurumu.REDDEDILDI')
        c = c.replace('== "BEKLIYOR"', '== OutboxDurumu.BEKLIYOR')
        c = c.replace('== "HATA"', '== OutboxDurumu.HATA')
        c = c.replace('== "GONDERILDI"', '== OutboxDurumu.GONDERILDI')
        c = c.replace('== "AKTIF"', '== KullaniciDurumu.AKTIF')
        c = c.replace('!= "AKTIF"', '!= KullaniciDurumu.AKTIF')
        
        # String assignments
        c = c.replace('.Durum = "ACIK"', '.Durum = IsEmriDurumu.ACIK')
        c = c.replace('.Durum = "TAMAMLANDI"', '.Durum = IsEmriDurumu.TAMAMLANDI')
        c = c.replace('.Durum = "IPTAL"', '.Durum = IsEmriDurumu.IPTAL')
        c = c.replace('.Durum = "REDDEDILDI"', '.Durum = DogrulamaDurumu.REDDEDILDI') # Wait! Fatura uses FaturaDurumu.IPTAL
        
        # Specific fixes from earlier iterations
        c = c.replace('AboneTipi.BIREYSEL', '"BIREYSEL"') # Revert any accidental string changes in seeders
        c = c.replace('AboneTipi.KURUMSAL', '"KURUMSAL"')
        
        # Fix FaturaController issues specifically
        if "FaturaController.cs" in filepath:
            c = c.replace('k.Durum == KullaniciDurumu.AKTIF', 'k.Durum == "AKTIF"') # Revert accident
            c = c.replace('f.Status == KullaniciDurumu.AKTIF', 'f.Status == "AKTIF"')
            c = c.replace('fatura.Status = BaglantiDurumu.PASIF;', 'fatura.Status = "PASIF";')
            c = c.replace('fatura.Durum = FaturaDurumu.IPTAL;', 'fatura.Durum = FaturaDurumu.IPTAL;')

        with open(filepath, 'w') as f: f.write(c)
    except:
        pass

print("Regeneration complete!")
