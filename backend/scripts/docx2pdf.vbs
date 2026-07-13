// Word VBScript: Convert DOCX to PDF
// Usage: cscript //nologo docx2pdf.vbs "input.docx" "output.pdf"
var args = WScript.Arguments;
if (args.length < 2) {
    WScript.Echo("Usage: cscript docx2pdf.vbs <input.docx> <output.pdf>");
    WScript.Quit(1);
}
var docxPath = args(0);
var pdfPath = args(1);

try {
    var word = new ActiveXObject("Word.Application");
    word.Visible = false;
    word.DisplayAlerts = 0; // wdAlertsNone

    var doc = word.Documents.Open(docxPath, false, true); // ReadOnly
    doc.SaveAs2(pdfPath, 17); // 17 = wdFormatPDF
    doc.Close(false);
    word.Quit();

    WScript.Echo("OK");
} catch (e) {
    WScript.Echo("ERROR: " + e.message);
    WScript.Quit(1);
}
