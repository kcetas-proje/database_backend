import re
import subprocess

def build():
    res = subprocess.run(['dotnet', 'build'], capture_output=True, text=True)
    return res.stdout

def fix_all():
    out = build()
    lines_cache = {}
    fixes = 0
    
    for line in out.split('\n'):
        m = re.search(r"([a-zA-Z0-9_/\.\-]+)\((\d+),(\d+)\): error CS(\d+): (.*) \[.*\]", line)
        if not m: continue
        
        filepath, lnum, cnum, code, msg = m.groups()
        lnum = int(lnum)
        
        if filepath not in lines_cache:
            try:
                with open(filepath, 'r') as f: lines_cache[filepath] = f.readlines()
            except: continue
            
        idx = lnum - 1
        text = lines_cache[filepath][idx]
        orig = text
        
        # CS0029: Cannot implicitly convert type A to B
        if code == '0029':
            # Is it Enum -> string? (e.g. 'KcetasAboneApi.Models.IsEmriDurumu' to 'string')
            if "'KcetasAboneApi.Models." in msg and "türüne dönüştürülemez" in msg and "'string' türüne" in msg:
                # E.g. dto.Durum = isEmri.Durum; -> dto.Durum = isEmri.Durum.ToString();
                # Simple fix: if there's a semicolon, inject .ToString() before it (if it's an assignment)
                ass = re.search(r'([A-Za-z0-9_\.]+)\s*=\s*([A-Za-z0-9_\.\(\)]+)(;|,)', text)
                if ass:
                    text = text.replace(ass.group(2) + ass.group(3), ass.group(2) + ".ToString()" + ass.group(3))
            
            # Is it string -> Enum? (e.g. 'string' to 'KcetasAboneApi.Models.OkumaKaynagi')
            elif "'string' türü" in msg and "türüne dönüştürülemez" in msg and "'KcetasAboneApi.Models." in msg:
                m_enum = re.search(r"'(KcetasAboneApi\.Models\.)?([A-Za-z0-9_]+)'", msg)
                if m_enum:
                    enum_type = m_enum.group(2)
                    ass = re.search(r'([A-Za-z0-9_\.]+)\s*=\s*([A-Za-z0-9_\.\?]+)(;|,)', text)
                    if ass:
                        # Need to Enum.Parse
                        text = text.replace(ass.group(2) + ass.group(3), f"System.Enum.Parse<{enum_type}>({ass.group(2)})" + ass.group(3))
                        
        # CS0019: Operator == cannot be applied to operands of type 'string' and 'Enum'
        if code == '0019':
            # Just add .ToString() to the left side or the enum side.
            # We can literally just search for == and add .ToString() to whatever is NOT a string literal
            if "==" in text:
                m_eq = re.search(r'([A-Za-z0-9_\.]+)\s*==\s*"([A-Z_]+)"', text)
                if m_eq:
                    text = text.replace(m_eq.group(1), m_eq.group(1) + ".ToString()")
                else:
                    m_eq2 = re.search(r'"([A-Z_]+)"\s*==\s*([A-Za-z0-9_\.]+)', text)
                    if m_eq2:
                        text = text.replace(m_eq2.group(2), m_eq2.group(2) + ".ToString()")
            elif "!=" in text:
                m_eq = re.search(r'([A-Za-z0-9_\.]+)\s*!=\s*"([A-Z_]+)"', text)
                if m_eq:
                    text = text.replace(m_eq.group(1), m_eq.group(1) + ".ToString()")
                else:
                    m_eq2 = re.search(r'"([A-Z_]+)"\s*!=\s*([A-Za-z0-9_\.]+)', text)
                    if m_eq2:
                        text = text.replace(m_eq2.group(2), m_eq2.group(2) + ".ToString()")

        if text != orig:
            lines_cache[filepath][idx] = text
            fixes += 1
            
    for fp, lines in lines_cache.items():
        with open(fp, 'w') as f: f.writelines(lines)
        
    return fixes > 0

while fix_all():
    pass
