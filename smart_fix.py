import re
import subprocess

def run_build():
    result = subprocess.run(['dotnet', 'build'], capture_output=True, text=True)
    return result.stdout

def fix_errors():
    output = run_build()
    errors = []
    pattern = r"([a-zA-Z0-9_/\.\-]+)\((\d+),(\d+)\): error CS(\d+): (.*) \[.*\]"
    for line in output.split('\n'):
        match = re.search(pattern, line)
        if match:
            errors.append({
                'file': match.group(1).strip(),
                'line': int(match.group(2)),
                'code': match.group(4),
                'message': match.group(5)
            })
            
    if not errors: return False
    
    fixes_made = 0
    lines_cache = {}
    
    for err in errors:
        filepath = err['file']
        line_num = err['line']
        if filepath not in lines_cache:
            try:
                with open(filepath, 'r') as f:
                    lines_cache[filepath] = f.readlines()
            except: continue
            
        line_idx = line_num - 1
        text = lines_cache[filepath][line_idx]
        
        # CS0019: '==' işleci 'string' ve 'IsEmriDurumu'
        if err['code'] == '0019':
            m = re.search(r"'(string)' ve '([A-Za-z0-9_]+)'", err['message'])
            m2 = re.search(r"'([A-Za-z0-9_]+)' ve '(string)'", err['message'])
            enum_type = None
            if m: enum_type = m.group(2)
            if m2: enum_type = m2.group(1)
            if enum_type:
                # Add .ToString() to the enum side OR parse the string side.
                # Adding .ToString() is much easier for ==
                # e.g. f.Durum == "ACIK" -> f.Durum.ToString() == "ACIK"
                text = re.sub(rf'([a-zA-Z0-9_\.]+{enum_type}[a-zA-Z0-9_\.]*)\s*==', rf'\1.ToString() ==', text)
                text = re.sub(rf'([a-zA-Z0-9_\.]+(?:Durum|Tip|Tipi|Kaynagi|Faz|Sistem|TuketiciGrubu|Faz))\s*==', rf'\1.ToString() ==', text)
                text = re.sub(rf'([a-zA-Z0-9_\.]+(?:Durum|Tip|Tipi|Kaynagi|Faz|Sistem|TuketiciGrubu|Faz))\s*!=', rf'\1.ToString() !=', text)
        
        # CS0029: Cannot implicitly convert type 'string' to 'Enum'
        if err['code'] == '0029':
            m = re.search(r"'string' .* '(KcetasAboneApi\.Models\.)?([A-Za-z0-9_]+)'", err['message'])
            if m:
                enum_type = m.group(2)
                # Assignment: a = b; -> a = Enum.Parse<Enum>(b);
                ass = re.search(r'([a-zA-Z0-9_\.]+)\s*=\s*([a-zA-Z0-9_\.\(\)\?]+)(;|,)', text)
                if ass:
                    left = ass.group(1)
                    right = ass.group(2)
                    ending = ass.group(3)
                    # if right is a string literal "..."
                    str_lit = re.search(r'"([A-Z_]+)"', right)
                    if str_lit:
                        text = text.replace(right, f"{enum_type}.{str_lit.group(1)}")
                    else:
                        text = text.replace(right, f"System.Enum.Parse<{enum_type}>({right})")
                elif "return " in text:
                    ret = re.search(r'return\s+(.+);', text)
                    if ret:
                        val = ret.group(1)
                        str_lit = re.search(r'"([A-Z_]+)"', val)
                        if str_lit:
                            text = text.replace(val, f"{enum_type}.{str_lit.group(1)}")
                        else:
                            text = text.replace(val, f"System.Enum.Parse<{enum_type}>({val})")

        if text != lines_cache[filepath][line_idx]:
            lines_cache[filepath][line_idx] = text
            fixes_made += 1
            
    for filepath, lines in lines_cache.items():
        with open(filepath, 'w') as f:
            f.writelines(lines)
            
    return fixes_made > 0

while fix_errors():
    pass
