$bytes = [IO.File]::ReadAllBytes('c:\Sentinel\SafetySentinel\Assets\Sentinel.png')
$b64 = [Convert]::ToBase64String($bytes)
[IO.File]::WriteAllText('c:\Sentinel\b64out.txt', $b64)
Write-Host "Encoded $($bytes.Length) bytes to base64 ($($b64.Length) chars)"
