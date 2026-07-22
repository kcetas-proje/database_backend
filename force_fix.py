import re
import glob

# For each enum, we know what the valid string values are.
enum_map = {
    "IsEmriDurumu": ["ACIK", "ATANDI", "YOLDA", "SAHADA", "TAMAMLANDI", "IPTAL", "BASARISIZ"],
    "IsEmriTipi": ["DEGISTIRME", "SOKME", "KESME", "ACMA", "ENDEKS_OKUMA", "SAYAC_ARIZA", "MUHURLEME", "KESIF_INCELEME", "YENI_BAGLANTI", "ENERJI_ACMA"],
    "DogrulamaDurumu": ["PLANLANDI", "OKUNDU", "DOGRULAMA_BEKLIYOR", "ONAYLANDI", "REDDEDILDI", "TAHAKKUKA_AKTARILDI"],
    "OkumaTipi": ["RUTIN_DONEM", "ILK_OKUMA", "SON_OKUMA", "KESME_ENDEKSI", "SAYAC_DEGISIM_OKUMASI", "SAYAC_ARIZA_OKUMASI", "MUHURLEME_ENDEKSI"],
    "OkumaKaynagi": ["MANUEL", "OSOS", "DUZELTME"],
    "BaglantiDurumu": ["TASLAK", "BAGLANTI_BEKLIYOR", "BAGLANABILIR", "AKTIF", "PASIF", "KAPALI"],
    "AboneTipi": ["BIREYSEL", "KURUMSAL"],
    "Faz": ["TEK_FAZ", "UC_FAZ"],
    "FaturaTipi": ["DONEM", "KAPANIS", "ARA", "DUZELTME", "IPTAL"],
    "KalemTipi": ["ENERJI_BEDELI", "ENERJI", "DAGITIM_BEDELI", "HIZMET", "KESME_BAGLAMA", "GECIKME", "YUVARLAMA", "VERGI_FON"],
    "KullaniciDurumu": ["AKTIF", "PASIF"],
    "OutboxDurumu": ["BEKLIYOR", "GONDERILDI", "HATA", "MANUEL_MUDAHALE"],
    "HedefSistem": ["GIB_EFATURA", "GIB_EARSIV", "ERP", "CRM_NOTIFICATION", "FIELD_PRINT"],
    "IslemTipi": ["INSERT", "UPDATE", "DELETE", "STATUS_CHANGE"]
}

files = glob.glob('**/*.cs', recursive=True)
for filepath in files:
    if 'Migrations' in filepath or 'obj' in filepath or 'bin' in filepath or 'AppDbContext.cs' in filepath or 'Models/' in filepath:
        continue
    
    with open(filepath, 'r') as f:
        c = f.read()
    orig_c = c
    
    # Direct replacement for literal string assignments and comparisons
    for enum_type, values in enum_map.items():
        for val in values:
            # == "VALUE" -> == EnumType.VALUE
            c = re.sub(rf'==\s*"{val}"', f'== {enum_type}.{val}', c)
            c = re.sub(rf'!=\s*"{val}"', f'!= {enum_type}.{val}', c)
            
            # = "VALUE" -> = EnumType.VALUE
            # Be careful not to replace things inside strings or unrelated
            c = re.sub(rf'(Durum|Tip|Tipi|Kaynagi|Faz|Sistem)\s*=\s*"{val}"', rf'\1 = {enum_type}.{val}', c)
            
            # Bogus pickRandom("VALUE")
            c = c.replace(f'"{val}"', f'{enum_type}.{val}')

    # DTO implicit conversion fixes (string -> Enum)
    # e.g. f.Durum = dto.Durum;
    c = re.sub(r'([A-Za-z0-9_\.]+Durum)\s*=\s*([A-Za-z0-9_\.]+Durum);', r'\1 = System.Enum.Parse<IsEmriDurumu>(\2);', c)
    c = re.sub(r'([A-Za-z0-9_\.]+Tip)\s*=\s*([A-Za-z0-9_\.]+Tip);', r'\1 = System.Enum.Parse<IsEmriTipi>(\2);', c)
    c = re.sub(r'([A-Za-z0-9_\.]+OkumaTipi)\s*=\s*([A-Za-z0-9_\.]+OkumaTipi);', r'\1 = System.Enum.Parse<OkumaTipi>(\2);', c)
    c = re.sub(r'([A-Za-z0-9_\.]+OkumaKaynagi)\s*=\s*([A-Za-z0-9_\.]+OkumaKaynagi);', r'\1 = System.Enum.Parse<OkumaKaynagi>(\2);', c)
    c = re.sub(r'([A-Za-z0-9_\.]+DogrulamaDurumu)\s*=\s*([A-Za-z0-9_\.]+DogrulamaDurumu);', r'\1 = System.Enum.Parse<DogrulamaDurumu>(\2);', c)
    
    # Fix the generic parsing bug where left == right causes issues
    c = re.sub(r'System\.Enum\.Parse<IsEmriDurumu>\(([a-zA-Z0-9_]+)\.Durum\)', r'System.Enum.Parse<IsEmriDurumu>(\1.Durum)', c)
    
    if c != orig_c:
        # Revert IsEmriDurumu.AKTIF -> KullaniciDurumu.AKTIF if needed
        # We blindly replaced AKTIF with BaglantiDurumu.AKTIF or KullaniciDurumu.AKTIF depending on iteration.
        # Actually, let's fix it manually.
        c = c.replace('BaglantiDurumu.AKTIF', 'KullaniciDurumu.AKTIF') # If we messed up KullaniciDurumu
        c = c.replace('IsEmriDurumu.AKTIF', '"AKTIF"') # IsEmriDurumu doesn't have AKTIF
        
        with open(filepath, 'w') as f:
            f.write(c)

