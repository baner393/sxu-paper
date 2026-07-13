# Word PowerShell: Convert DOCX to PDF
param(
    [Parameter(Mandatory=$true)][string]$InputDocx,
    [Parameter(Mandatory=$true)][string]$OutputPdf
)

try {
    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    $word.DisplayAlerts = [Microsoft.Office.Interop.Word.WdAlertLevel]::wdAlertsNone

    $doc = $word.Documents.Open($InputDocx, $false, $true)  # ReadOnly
    $doc.SaveAs2($OutputPdf, 17)  # 17 = wdFormatPDF
    $doc.Close($false)
    $word.Quit()

    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null
    Write-Output "OK"
} catch {
    Write-Error "ERROR: $_"
    exit 1
}
