import glob
import re

files = glob.glob('Models/*.cs')
for filepath in files:
    with open(filepath, 'r') as f: c = f.read()
    orig = c
    c = re.sub(r'public string(\?)? ([A-Za-z0-9_]+Durum[A-Za-z0-9_]*|Tip|OkumaTipi|OkumaKaynagi|Faz|BaglantiDurumu|AboneTipi|FaturaTipi|KalemTipi|HedefSistem|IslemTipi)\s*\{\s*get;\s*set;\s*\}\s*=\s*.*?;', 
               lambda m: f"public {m.group(2)}{m.group(1) or ''} {m.group(2)} {{ get; set; }}", c)
    if c != orig:
        with open(filepath, 'w') as f: f.write(c)
