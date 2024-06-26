using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
//Load the PDF file
var sourcefile= args[0];
using (PdfReader pdfReader = new PdfReader(sourcefile))
using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
{
    for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
    {
        PdfPage pdfPage = pdfDocument.GetPage(page);
        string pageText = PdfTextExtractor.GetTextFromPage(pdfPage);
        // Process the extracted text from each page
        Console.WriteLine(pageText);
    }
}
