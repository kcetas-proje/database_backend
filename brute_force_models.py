import glob
import re

files = glob.glob('Models/*.cs')

for filepath in files:
    with open(filepath, 'r') as f:
        c = f.read()
    orig = c
    
    # Models and DTOs
    c = re.sub(r'public string(\?)?\s+Faz\s*\{\s*get;\s*set;\s*\}', r'public Faz\1 Faz { get; set; }', c)
    c = re.sub(r'public string(\?)?\s+OkumaTipi\s*\{\s*get;\s*set;\s*\}', r'public OkumaTipi\1 OkumaTipi { get; set; }', c)
    c = re.sub(r'public string(\?)?\s+OkumaKaynagi\s*\{\s*get;\s*set;\s*\}', r'public OkumaKaynagi\1 OkumaKaynagi { get; set; }', c)
    c = re.sub(r'public string(\?)?\s+BaglantiDurumu\s*\{\s*get;\s*set;\s*\}', r'public BaglantiDurumu\1 BaglantiDurumu { get; set; }', c)
    c = re.sub(r'public string(\?)?\s+FaturaTipi\s*\{\s*get;\s*set;\s*\}', r'public FaturaTipi\1 FaturaTipi { get; set; }', c)
    c = re.sub(r'public string(\?)?\s+KalemTipi\s*\{\s*get;\s*set;\s*\}', r'public KalemTipi\1 KalemTipi { get; set; }', c)
    c = re.sub(r'public string(\?)?\s+AboneTipi\s*\{\s*get;\s*set;\s*\}', r'public AboneTipi\1 AboneTipi { get; set; }', c)
    c = re.sub(r'public string(\?)?\s+DogrulamaDurumu\s*\{\s*get;\s*set;\s*\}', r'public DogrulamaDurumu\1 DogrulamaDurumu { get; set; }', c)
    c = re.sub(r'public string(\?)?\s+Durum\s*\{\s*get;\s*set;\s*\}', r'public IsEmriDurumu\1 Durum { get; set; }', c) if 'IsEmri' in filepath else c
    c = re.sub(r'public string(\?)?\s+Tip\s*\{\s*get;\s*set;\s*\}', r'public IsEmriTipi\1 Tip { get; set; }', c) if 'IsEmri' in filepath else c

    if c != orig:
        with open(filepath, 'w') as f:
            f.write(c)
