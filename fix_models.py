import re
import glob

# Fix the models
files = glob.glob('Models/*.cs')
for filepath in files:
    with open(filepath, 'r') as f:
        c = f.read()
        
    orig_c = c
    c = re.sub(r'public string Durum \{ get; set; \}.*', r'public IsEmriDurumu Durum { get; set; } = IsEmriDurumu.ACIK;', c) if 'IsEmirleri.cs' in filepath else c
    c = re.sub(r'public string Tip \{ get; set; \}.*', r'public IsEmriTipi Tip { get; set; }', c) if 'IsEmirleri.cs' in filepath else c
    
    if 'EndeksOkuma.cs' in filepath:
        c = re.sub(r'public string OkumaTipi \{ get; set; \}.*', r'public OkumaTipi OkumaTipi { get; set; }', c)
        c = re.sub(r'public string OkumaKaynagi \{ get; set; \}.*', r'public OkumaKaynagi OkumaKaynagi { get; set; }', c)
        c = re.sub(r'public string DogrulamaDurumu \{ get; set; \}.*', r'public DogrulamaDurumu DogrulamaDurumu { get; set; }', c)
        
    if 'TuketimNoktasi.cs' in filepath:
        c = re.sub(r'public string BaglantiDurumu \{ get; set; \}.*', r'public BaglantiDurumu BaglantiDurumu { get; set; }', c)
        
    if 'Aboneler.cs' in filepath:
        c = re.sub(r'public string AboneTipi \{ get; set; \}.*', r'public AboneTipi AboneTipi { get; set; }', c)
        
    if 'Sayaclar.cs' in filepath:
        c = re.sub(r'public string Faz \{ get; set; \}.*', r'public Faz Faz { get; set; }', c)
        
    if 'Fatura.cs' in filepath:
        c = re.sub(r'public string FaturaTipi \{ get; set; \}.*', r'public FaturaTipi FaturaTipi { get; set; }', c)
        
    if 'FaturaKalemi.cs' in filepath:
        c = re.sub(r'public string KalemTipi \{ get; set; \}.*', r'public KalemTipi KalemTipi { get; set; }', c)
        
    if 'Kullanicilar.cs' in filepath:
        c = re.sub(r'public string Durum \{ get; set; \}.*', r'public KullaniciDurumu Durum { get; set; } = KullaniciDurumu.AKTIF;', c)
        
    if 'EntegrasyonOutbox.cs' in filepath:
        c = re.sub(r'public string HedefSistem \{ get; set; \}.*', r'public HedefSistem HedefSistem { get; set; }', c)
        c = re.sub(r'public string Durum \{ get; set; \}.*', r'public OutboxDurumu Durum { get; set; }', c)
        
    if 'AuditLog.cs' in filepath:
        c = re.sub(r'public string IslemTipi \{ get; set; \}.*', r'public IslemTipi IslemTipi { get; set; }', c)

    # DTOs
    if 'CreateDto' in filepath or 'UpdateDto' in filepath:
        c = re.sub(r'public string Durum \{ get; set; \}.*', r'public IsEmriDurumu Durum { get; set; }', c) if 'IsEmri' in filepath else c
        c = re.sub(r'public string\? Durum \{ get; set; \}.*', r'public IsEmriDurumu? Durum { get; set; }', c) if 'IsEmri' in filepath else c
        c = re.sub(r'public string Tip \{ get; set; \}.*', r'public IsEmriTipi Tip { get; set; }', c) if 'IsEmri' in filepath else c
        c = re.sub(r'public string\? Tip \{ get; set; \}.*', r'public IsEmriTipi? Tip { get; set; }', c) if 'IsEmri' in filepath else c
        
        if 'EndeksOkuma' in filepath:
            c = re.sub(r'public string OkumaTipi \{ get; set; \}.*', r'public OkumaTipi OkumaTipi { get; set; }', c)
            c = re.sub(r'public string OkumaKaynagi \{ get; set; \}.*', r'public OkumaKaynagi OkumaKaynagi { get; set; }', c)
            c = re.sub(r'public string\? DogrulamaDurumu \{ get; set; \}.*', r'public DogrulamaDurumu? DogrulamaDurumu { get; set; }', c)
            c = re.sub(r'public string DogrulamaDurumu \{ get; set; \}.*', r'public DogrulamaDurumu DogrulamaDurumu { get; set; }', c)

        if 'TuketimNoktasi' in filepath:
            c = re.sub(r'public string BaglantiDurumu \{ get; set; \}.*', r'public BaglantiDurumu BaglantiDurumu { get; set; } = BaglantiDurumu.BAGLANABILIR;', c)
            
        if 'Abone' in filepath:
            c = re.sub(r'public string AboneTipi \{ get; set; \}.*', r'public AboneTipi AboneTipi { get; set; }', c)
            
        if 'Sayac' in filepath:
            c = re.sub(r'public string\? Faz \{ get; set; \}.*', r'public Faz? Faz { get; set; }', c)
            c = re.sub(r'public string Faz \{ get; set; \}.*', r'public Faz Faz { get; set; }', c)
            
        if 'Fatura' in filepath:
            c = re.sub(r'public string FaturaTipi \{ get; set; \}.*', r'public FaturaTipi FaturaTipi { get; set; }', c)
            c = re.sub(r'public string KalemTipi \{ get; set; \}.*', r'public KalemTipi KalemTipi { get; set; }', c)
            
    if c != orig_c:
        with open(filepath, 'w') as f:
            f.write(c)

print("Models fixed!")
