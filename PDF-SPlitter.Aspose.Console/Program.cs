using Aspose.Pdf;
using Aspose.Pdf.Text;
using Aspose.Words;
using Aspose.Words.Saving;
var sourcefile= args[0];
 sourcefile= @"C:\Users\rkolla\Downloads\PDFExtracted\Source\New Microsoft Word Document.docx";
ConvertWordToPDF (sourcefile);

// Load the PDF document
Aspose.Pdf.Document pdfDocument = new (sourcefile);

int pageCount = 1;
foreach (Page pdfPage in pdfDocument.Pages)
{
    Aspose.Pdf.Document newDocument = new ();
    newDocument.Pages.Add(pdfPage);
    newDocument.Save($"page_{pageCount}_out.pdf");
    pageCount++;
}



void ExtractTextFromPDF(string sourcefile)
{
    // Load the PDF document
    Aspose.Pdf.Document pdfDocument = new(sourcefile);

    // Create a TextAbsorber
    TextAbsorber textAbsorber = new();

    // Accept the absorber for all pages
    pdfDocument.Pages.Accept(textAbsorber);

    // Get the extracted text
    string extractedText = textAbsorber.Text;

    // Now you can work with the extracted text
    Console.WriteLine(extractedText);
}

void ConvertWordToPDF(string sourcefile)
{
    // Load the license in your application to avoid watermark in the output PDF
    // License WordToPdfLicense = new License();
    // WordToPdfLicense.SetLicense("Aspose.Word.lic");

    // Load the word file to be converted to PDF
    Aspose.Words.Document sampleDocx = new (sourcefile);

    // Instantiate the PdfSaveOptions class object before converting the Docx to PDF
     Aspose.Words.Saving.PdfSaveOptions options = new ();

     Aspose.Words.Saving.PdfSaveOptions pdfOptions = new(){
        Compliance = PdfCompliance.PdfA1b,
        JpegQuality = 90
    };

    // Set the page numbers of the document to be rendered to output PDF
    options.PageSet = new PageSet(1, 3);

    // Set page mode to full screen while opening it in a viewer
    options.PageMode = PdfPageMode.FullScreen;

    // Set the output PDF document compliance mode
    options.Compliance = PdfCompliance.Pdf17;

    // Save the resultant PDF file using the above mentioned options
    sampleDocx.Save("Output.pdf", pdfOptions);
}
