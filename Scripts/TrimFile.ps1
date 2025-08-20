param(
    [Parameter(Mandatory=$true,Position=0)][string]$file
)

$content = [System.IO.File]::ReadAllText($file)
$content = $content.Trim()
[System.IO.File]::WriteAllText($file, $content)