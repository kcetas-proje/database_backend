import re
import glob

def fix(path, replacements):
    with open(path, 'r') as f:
        c = f.read()
    orig = c
    for old, new in replacements:
        c = c.replace(old, new)
    if c != orig:
        with open(path, 'w') as f:
            f.write(c)

# IsEmriSeeder
fix('Services/IsEmriSeeder.cs', [
    ('string[] tipler =', 'IsEmriTipi[] tipler ='),
    ('string[] iptalNedenleri', 'IsEmriDurumu[] iptalNedenleri'), # Just in case IsEmriDurumu was used there
    ('string[] field', 'IsEmriTipi[] field') # In case there are other arrays
])

# FaturaSeeder
fix('Services/FaturaSeeder.cs', [
    ('string[] tipler =', 'FaturaTipi[] tipler ='),
    ('string[] kalemTipleri =', 'KalemTipi[] kalemTipleri ='),
    ('string[] kalemTipleri2 =', 'KalemTipi[] kalemTipleri2 ='),
])

# SayaclarController
fix('Controllers/SayaclarController.cs', [
    ('.RuleFor(s => s.Faz, f => f.PickRandom(new[] { Faz.TEK_FAZ, Faz.UC_FAZ }))', '.RuleFor(s => s.Faz, f => f.PickRandom(new Faz?[] { Faz.TEK_FAZ, Faz.UC_FAZ }))'),
    ('.RuleFor(s => s.Faz, f => f.PickRandom<Faz>())', '.RuleFor(s => s.Faz, f => f.PickRandom<Faz?>())'),
    ('Faz.TEK_FAZ, Faz.UC_FAZ', '(Faz?)Faz.TEK_FAZ, (Faz?)Faz.UC_FAZ')
])

# Fix remaining enum bugs
for path in glob.glob('Controllers/*.cs') + glob.glob('Services/*.cs'):
    fix(path, [
        ('public string OkumaTipi', 'public OkumaTipi OkumaTipi'),
        ('public string OkumaKaynagi', 'public OkumaKaynagi OkumaKaynagi'),
    ])

