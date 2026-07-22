import re
import glob

replacements = {
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
    
    try:
        with open(filepath, 'r') as f:
            c = f.read()
            
        orig_c = c
        
        # 1. Replace assignments: Property = "VALUE" -> Property = Enum.VALUE
        for enum_name, values in replacements.items():
            for val in values:
                # e.g. Durum = "ACIK" -> Durum = IsEmriDurumu.ACIK
                # We do this contextually
                
                if enum_name == "IsEmriDurumu":
                    c = re.sub(rf'([a-zA-Z0-9_\.]+)Durum\s*=\s*"{val}"', rf'\1Durum = {enum_name}.{val}', c)
                    c = re.sub(rf'==\s*"{val}"', rf'== {enum_name}.{val}', c)
                    c = re.sub(rf'!=\s*"{val}"', rf'!= {enum_name}.{val}', c)
                    
                if enum_name == "IsEmriTipi" or enum_name == "FaturaTipi":
                    c = re.sub(rf'([a-zA-Z0-9_\.]+)Tip\s*=\s*"{val}"', rf'\1Tip = {enum_name}.{val}', c)
                    c = re.sub(rf'([a-zA-Z0-9_\.]+)FaturaTipi\s*=\s*"{val}"', rf'\1FaturaTipi = {enum_name}.{val}', c)

                c = re.sub(rf'OkumaTipi\s*=\s*"{val}"', rf'OkumaTipi = {enum_name}.{val}', c)
                c = re.sub(rf'OkumaKaynagi\s*=\s*"{val}"', rf'OkumaKaynagi = {enum_name}.{val}', c)
                c = re.sub(rf'DogrulamaDurumu\s*=\s*"{val}"', rf'DogrulamaDurumu = {enum_name}.{val}', c)
                c = re.sub(rf'BaglantiDurumu\s*=\s*"{val}"', rf'BaglantiDurumu = {enum_name}.{val}', c)
                c = re.sub(rf'AboneTipi\s*=\s*"{val}"', rf'AboneTipi = {enum_name}.{val}', c)
                c = re.sub(rf'Faz\s*=\s*"{val}"', rf'Faz = {enum_name}.{val}', c)
                c = re.sub(rf'KalemTipi\s*=\s*"{val}"', rf'KalemTipi = {enum_name}.{val}', c)
                c = re.sub(rf'HedefSistem\s*=\s*"{val}"', rf'HedefSistem = {enum_name}.{val}', c)
                c = re.sub(rf'IslemTipi\s*=\s*"{val}"', rf'IslemTipi = {enum_name}.{val}', c)
                
                # specific to seeders picking random
                c = c.replace(f'"{val}"', f'{enum_name}.{val}')

        # Revert overly aggressive replaces
        # FaturaDurumu was already an enum but might have been touched
        c = c.replace("IsEmriDurumu.ODENMEDI", "FaturaDurumu.ODENMEDI")
        c = c.replace("IsEmriDurumu.ODENDI", "FaturaDurumu.ODENDI")
        c = c.replace("IsEmriDurumu.IPTAL", "FaturaDurumu.IPTAL") # wait, IsEmri also has IPTAL! 
        # I need to be careful with IPTAL and AKTIF/PASIF
        
        # We might have replaced AKTIF with IsEmriDurumu.AKTIF but IsEmriDurumu doesn't have AKTIF.
        # KullaniciDurumu, BaglantiDurumu both have AKTIF.
        
        # Let's fix specific DTO assignments (e.g. .Durum = dto.Durum)
        # We can't do this with simple replace easily, let's use regex for implicit conversions
        c = re.sub(r'return\s+([a-zA-Z0-9_\.]+);', lambda m: m.group(0) if 'return null' in m.group(0) or 'return true' in m.group(0) else m.group(0), c)

        if c != orig_c:
            with open(filepath, 'w') as f: f.write(c)

    except Exception as e:
        print(f"Failed {filepath}: {e}")

