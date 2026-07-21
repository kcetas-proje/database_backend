import re
import subprocess
import os

def run_build():
    result = subprocess.run(['dotnet', 'build'], capture_output=True, text=True)
    return result.stdout

def extract_errors(build_output):
    errors = []
    pattern = r"([a-zA-Z0-9_/\.\-]+)\((\d+),(\d+)\): error CS(\d+): (.*) \[.*\]"
    for line in build_output.split('\n'):
        match = re.search(pattern, line)
        if match:
            filepath = match.group(1).strip()
            line_num = int(match.group(2))
            col_num = int(match.group(3))
            error_code = match.group(4)
            message = match.group(5)
            errors.append({
                'file': filepath,
                'line': line_num,
                'code': error_code,
                'message': message
            })
    return errors

def fix_errors():
    output = run_build()
    errors = extract_errors(output)
    if not errors:
        print("No errors found!")
        return False
    
    fixes_made = 0
    file_lines = {}
    
    for err in errors:
        filepath = err['file']
        line_num = err['line']
        if filepath not in file_lines:
            try:
                with open(filepath, 'r') as f:
                    file_lines[filepath] = f.readlines()
            except:
                continue
                
        line_idx = line_num - 1
        line_text = file_lines[filepath][line_idx]
        
        target_enum = None
        
        # Parse Turkish error messages
        # error CS0019: '==' işleci 'string' ve 'IsEmriDurumu' türündeki
        match_op = re.search(r"'(string)' ve '([A-Za-z0-9_]+)'", err['message'])
        if match_op: target_enum = match_op.group(2)
        
        match_op2 = re.search(r"'([A-Za-z0-9_]+)' ve '(string)'", err['message'])
        if match_op2: target_enum = match_op2.group(1)
        
        # error CS0029: 'string' türü örtülü olarak 'KcetasAboneApi.Models.OkumaKaynagi' türüne dönüştürülemez
        match_conv1 = re.search(r"'string' .* '(KcetasAboneApi\.Models\.)?([A-Za-z0-9_]+)'", err['message'])
        if match_conv1: target_enum = match_conv1.group(2)
        
        match_conv2 = re.search(r"'(KcetasAboneApi\.Models\.)?([A-Za-z0-9_]+)' .* 'string'", err['message'])
        if match_conv2: target_enum = match_conv2.group(2)
        
        if not target_enum:
            if "Durum" in line_text or "durum" in line_text:
                if "IsEmri" in filepath: target_enum = "IsEmriDurumu"
                elif "Outbox" in filepath: target_enum = "OutboxDurumu"
                elif "Kullanici" in filepath: target_enum = "KullaniciDurumu"
                elif "Fatura" in filepath: target_enum = "FaturaDurumu"
                elif "Dogrulama" in line_text: target_enum = "DogrulamaDurumu"
            elif "Tip" in line_text or "tip" in line_text:
                if "IsEmri" in filepath: target_enum = "IsEmriTipi"
                elif "Fatura" in filepath: target_enum = "FaturaTipi"
            elif "OkumaTipi" in line_text: target_enum = "OkumaTipi"
            elif "OkumaKaynagi" in line_text: target_enum = "OkumaKaynagi"
            elif "BaglantiDurumu" in line_text: target_enum = "BaglantiDurumu"
            elif "AboneTipi" in line_text: target_enum = "AboneTipi"
            elif "KalemTipi" in line_text: target_enum = "KalemTipi"
            elif "Faz" in line_text: target_enum = "Faz"
            elif "HedefSistem" in line_text: target_enum = "HedefSistem"
            elif "IslemTipi" in line_text: target_enum = "IslemTipi"

        if target_enum:
            new_line = line_text
            # Fix equality checks: == "VALUE" -> == target_enum.VALUE
            new_line = re.sub(r'([!=]=)\s*"([A-Z_]+)"', rf'\1 {target_enum}.\2', new_line)
            new_line = re.sub(r'"([A-Z_]+)"\s*([!=]=)', rf'{target_enum}.\1 \2', new_line)
            
            # Fix assignment: X = "VALUE" -> X = target_enum.VALUE
            new_line = re.sub(r'=\s*"([A-Z_]+)"', rf'= {target_enum}.\1', new_line)

            # Fix assignment from string var to Enum var
            # e.g. f.Durum = dto.Durum;
            if new_line == line_text and ("=" in line_text or "return " in line_text or "(" in line_text):
                assignment_match = re.search(r'([A-Za-z0-9_\.]+)\s*=\s*([A-Za-z0-9_\.]+);', line_text)
                if assignment_match:
                    left = assignment_match.group(1)
                    right = assignment_match.group(2)
                    if right not in ["null", "0", "1", "true", "false"]:
                        new_line = line_text.replace(right, f"Enum.Parse<{target_enum}>({right})")
                
                # Fix return statement implicit string -> enum
                if "return " in line_text and "Enum.Parse" not in line_text:
                    ret_match = re.search(r'return\s+([A-Za-z0-9_\.]+);', line_text)
                    if ret_match:
                        right = ret_match.group(1)
                        if right != "null":
                            new_line = line_text.replace(f"return {right};", f"return Enum.Parse<{target_enum}>({right});")

                # Implicit method call arg string -> enum
                # Try a broader regex for property assignment in object initializers
                init_match = re.search(r'([A-Za-z0-9_]+)\s*=\s*([A-Za-z0-9_\.]+),', line_text)
                if init_match:
                    right = init_match.group(2)
                    if right not in ["null", "true", "false"] and not right.isdigit():
                        new_line = line_text.replace(f" {right},", f" Enum.Parse<{target_enum}>({right}),")

            if new_line != line_text:
                file_lines[filepath][line_idx] = new_line
                fixes_made += 1

    for filepath, lines in file_lines.items():
        with open(filepath, 'w') as f:
            f.writelines(lines)
            
    print(f"Made {fixes_made} fixes.")
    return fixes_made > 0

while True:
    if not fix_errors():
        break
