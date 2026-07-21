import re
import glob

for path in glob.glob('Controllers/*.cs') + glob.glob('Services/*.cs'):
    with open(path, 'r') as f: c = f.read()
    orig = c
    c = c.replace('string dinamikOkumaTipi =', 'OkumaTipi dinamikOkumaTipi =')
    
    # Check for other similar local variables
    # e.g. `string durum = ` where it is assigned an enum
    # but `string` to Enum might be everywhere. Let's rely on specific ones for now.
    
    if c != orig:
        with open(path, 'w') as f: f.write(c)

print("Fixed explicit string variables")
