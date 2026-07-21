import glob

files = glob.glob('**/*.cs', recursive=True)
for filepath in files:
    if 'Migrations' in filepath or 'obj' in filepath or 'bin' in filepath or 'AppDbContext.cs' in filepath or 'Models/' in filepath:
        continue
    
    with open(filepath, 'r') as f:
        c = f.read()
    orig_c = c
    
    c = c.replace('KullaniciDurumu.AKTIF', '"AKTIF"')
    c = c.replace('KullaniciDurumu.PASIF', '"PASIF"')
    
    # Wait, but KullanicilarController.cs NEEDS KullaniciDurumu.AKTIF!
    if 'KullanicilarController.cs' in filepath:
        c = c.replace('"AKTIF"', 'KullaniciDurumu.AKTIF')
        c = c.replace('"PASIF"', 'KullaniciDurumu.PASIF')
        
    if c != orig_c:
        with open(filepath, 'w') as f:
            f.write(c)
