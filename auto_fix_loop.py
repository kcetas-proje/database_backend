import subprocess
import re

def fix_errors():
    out = subprocess.run(['dotnet', 'build'], capture_output=True, text=True).stdout
    
    fixes = 0
    errors = []
    for line in out.split('\n'):
        m = re.search(r"([a-zA-Z0-9_/\.\-\s]+)\((\d+),(\d+)\): error CS(\d+): (.*) \[.*\]", line)
        if m:
            fp, lnum, cnum, err_code, msg = m.groups()
            errors.append((fp, int(lnum), err_code, msg))
            
    if not errors:
        print("0 errors!")
        return False
        
    for fp, lnum, err_code, msg in errors:
        try:
            with open(fp, 'r') as f: lines = f.readlines()
            idx = lnum - 1
            line_text = lines[idx]
            orig_text = line_text
            
            # CS0029: Cannot implicitly convert type 'A' to 'B'
            if err_code == "0029":
                if "örtülü olarak 'string' türüne dönüştürülemez" in msg:
                    # Enum -> string (needs .ToString())
                    line_text = re.sub(r'(\w+\.[A-Za-z0-9_]+)\s*([,;}])', r'\1.ToString()\2', line_text)
                    line_text = re.sub(r'(isEmri|fatura|sayac|nokta|sozlesme|eskiOkuma)\.([A-Za-z0-9_]+)\s*([,;}])', r'\1.\2.ToString()\3', line_text)
                elif "'string' türü örtülü olarak" in msg:
                    # string -> Enum (needs Enum.Parse)
                    enum_match = re.search(r"'(KcetasAboneApi\.Models\.[A-Za-z0-9_]+)'", msg)
                    if enum_match:
                        enum_type = enum_match.group(1).split('.')[-1]
                        line_text = re.sub(r'(\w+\.[A-Za-z0-9_]+)\s*([,;}])', f'(System.Enum.Parse<{enum_type}>(\\1))\2', line_text)
                        
            # CS0019: Operator '==' cannot be applied to operands of type 'A' and 'B'
            elif err_code == "0019":
                if "string" in msg:
                    enum_match = re.search(r"'([A-Za-z0-9_]+)'", msg.replace('string', '').replace(' ', '').replace("''", ''))
                    if enum_match:
                        enum_type = enum_match.group(1)
                        line_text = re.sub(r'==\s*"([A-Za-z0-9_]+)"', f'== {enum_type}.\\1', line_text)
                        line_text = re.sub(r'!=\s*"([A-Za-z0-9_]+)"', f'!= {enum_type}.\\1', line_text)

            # CS1503: Argument 1: cannot convert from 'A' to 'B'
            elif err_code == "1503":
                if "ReadOnlySpan<char>" in msg:
                    line_text = re.sub(r'([A-Za-z0-9_]+\.[A-Za-z0-9_]+)\s*([,\)])', r'\1.ToString()\2', line_text)

            if line_text != orig_text:
                lines[idx] = line_text
                with open(fp, 'w') as f: f.writelines(lines)
                fixes += 1
        except Exception as e:
            print(f"Error fixing {fp}:{lnum} - {e}")
            
    print(f"Fixed {fixes} errors")
    return fixes > 0

while fix_errors():
    pass
