import base64
with open(r'c:\Sentinel\SafetySentinel\Assets\Sentinel.png', 'rb') as f:
    b64 = base64.b64encode(f.read()).decode('ascii')
with open(r'c:\Sentinel\b64out.txt', 'w') as f:
    f.write(b64)
print(f'Encoded {len(b64)} chars')
