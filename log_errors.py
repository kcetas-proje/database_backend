import re
import subprocess

out = subprocess.run(['dotnet', 'build'], capture_output=True, text=True).stdout
errors = []

for line in out.split('\n'):
    m = re.search(r"([a-zA-Z0-9_/\.\-]+)\((\d+),(\d+)\): error CS(\d+): (.*) \[.*\]", line)
    if m:
        fp, lnum, cnum, code, msg = m.groups()
        lnum = int(lnum)
        with open(fp, 'r') as f:
            lines = f.readlines()
        text = lines[lnum - 1].strip()
        errors.append(f"{fp}:{lnum} => {msg}\n   LINE: {text}")

with open('errors.log', 'w') as f:
    f.write('\n\n'.join(errors))
