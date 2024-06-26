using System.Diagnostics;
using System.IO;
using System.Dynamic;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using Microsoft.Extensions.Configuration;

// Build a config object and retrieve user settings.
IConfiguration config = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
string UserName = Environment.UserName;
//  bool enableAddReferencePage = Convert.ToBoolean(config["AddReferencePage"]);
var splitOptions = config.GetSection("SplitOptions").Get<List<SplitOption>>();
var SharedDocuments=config["SharedDocuments"]?? $@"C:\Users\{UserName}\Capgemini Technology Services India Limited\All Company - Documents";
// var ProcessedFolderName = config["ProcessedFolder"]?? "Processed";
// var DestinationFolderName = config["DestinationFolder"]?? "Destination";



//"../../../../Downloads/PDFExtracted/Source/"
// Get the source folder Path from the command line arguments
args = args?.Length > 0 ? args : new[] { $"C:/Users/{UserName}/Capgemini Technology Services India Limited/All Company - Documents/PDFExtract" };
var sourcePath = args[0];

// This is required to avoid the "No data is available for encoding 1252" exception when saving the PdfDocument
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
// Set the custom font resolver
PdfSharp.Fonts.GlobalFontSettings.FontResolver = new CustomFontResolver();

ValidationOnSourcePath(sourcePath);

// Get the source folder name
DirectoryInfo directoryInfo = new(sourcePath);
var sourceFolderName = directoryInfo?.Name;
var SharePointDocumentfolder=Path.Combine(SharedDocuments, sourceFolderName);

// Create the folders if they do not exist
// Dictionary<string, string> splitProcessPaths = new()
// {
//     {"Source", sourcePath},
//     { ProcessedFolderName,string.Empty },
//     { DestinationFolderName, string.Empty }
// };

// Create the folders if they do not exist
//splitProcessPaths.Keys.ToList().ForEach(folder => splitProcessPaths[folder] = CreateFolderIfNotExist(directoryInfo?.Parent?.FullName, folder));

// Process the PDF files in the source folder
Directory.GetFiles(

    //splitProcessPaths[sourceFolderName], "*.pdf"
    
    sourcePath, "*.pdf" ).ToList().ForEach((Action<string>)(file =>
{
    Console.WriteLine($"Processing {file} ....", ConsoleColor.Yellow);
    if (!IsFileSizeExceeds(file))
    {
        Console.WriteLine($"Skipping the file {file} for splitting", ConsoleColor.Yellow);
       
       // File.Move(file, Path.Combine(splitProcessPaths[ProcessedFolderName], Path.GetFileName(file)), true);
        //Console.WriteLine($"Moved {file} to {splitProcessPaths[ProcessedFolderName]},", ConsoleColor.Green); return;
    }else {
        Console.WriteLine($" Proceeding with the file {file} for splitting", ConsoleColor.Green);

        // Open the file
        using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import))
        {
            if (inputDocument.PageCount > 1)
            {
                // // Add the original document reference to the new document
                splitOptions.Where(option => option.IsValid()).ToList().ForEach(option =>
                {
                    if (option.SplitBy == SplitBy.Page)
                        SplitDocumentByPage(inputDocument, option, SharePointDocumentfolder);
                    else if (option.SplitBy == SplitBy.Size)
                        SplitDocumentBySize(inputDocument, option, SharePointDocumentfolder);
                });
            }
            else
            {
                Console.WriteLine($"The document {file} has only one page. Skipping the document for splitting", ConsoleColor.Yellow);
            }
            Console.WriteLine($"Completed processing of {file}", ConsoleColor.Green);
            //File.Move(file, Path.Combine(splitProcessPaths[ProcessedFolderName], Path.GetFileName(file)), true);
            //Console.WriteLine($"Moved {file} to {splitProcessPaths[ProcessedFolderName]},", ConsoleColor.Green);
        }
    }
}));

/// <summary>
/// Split the document by page
/// </summary>
/// <param name="inputDocument"></param>
/// <param name="option"></param>
static void SplitDocumentByPage( PdfDocument inputDocument ,SplitOption option, string sharePointFolder)
{

       if (inputDocument == null && inputDocument?.PageCount == 0)
        return;

       var outputDocument = new PdfDocument { Version = inputDocument.Version };
        var OriginalFile = Path.Combine(sharePointFolder, Path.GetFileName(inputDocument?.FullPath));
        outputDocument.Info.Creator = inputDocument.Info.Creator;


        var  sourcePath =  inputDocument?.FullPath;
        var pdfFileName = Path.GetFileNameWithoutExtension(inputDocument?.FullPath);
         var chunksFolder = Path.Combine(sourcePath, $"{pdfFileName}-Pages");

     _ = CreateFolderIfNotExists(chunksFolder);

    int index = 1;
    for (int idx = 0; idx < inputDocument.PageCount; idx++)
    {
        // Add the page 
         _ = outputDocument.AddPage(inputDocument.Pages[idx]);
     // Save the document if the page count is equal to the size
     if (option.Size ==outputDocument.PageCount){
     
         if (outputDocument.PageCount>0 && option.AddReferencePage)
              AddOriginalDocumentReference(outputDocument, OriginalFile);

         var docName = Path.Combine(chunksFolder, pdfFileName + $" - Page {index++}.pdf");

        if (File.Exists(docName))
            File.Delete(docName);

         outputDocument.Info.Title = $"Page {index} of {inputDocument.Info.Title}";
 
         outputDocument.Save(docName);
         outputDocument = new PdfDocument { Version = outputDocument.Version };
    }

     // Save the last page
     if (outputDocument.PageCount>0  && idx == inputDocument.PageCount - 1)
     {
            if (option.AddReferencePage)
                   AddOriginalDocumentReference(outputDocument, OriginalFile);
             var docName = Path.Combine(chunksFolder, pdfFileName + $" - Page {index++}.pdf");
               outputDocument.Info.Title = $"Page {index} of {inputDocument.Info.Title}";

            outputDocument.Save(docName);
     }
     
   }
 }

    

/// <summary>
/// Split the document by size
/// </summary>
/// <param name="inputDocument"></param>
/// <param name="option"></param>
/// <param name="pdfFileName"></param>
/// <param name="chunksFolder"></param>
/// <param name="originalDocumentName"></param>
 static void SplitDocumentBySize( PdfDocument inputDocument,SplitOption option, string sharePointFolder)
 {
    if (inputDocument == null && inputDocument?.PageCount == 0)
        return;
    
    var  sourcePath =  Path.GetDirectoryName(inputDocument?.FullPath);
    var OriginalFile = Path.Combine(sharePointFolder, Path.GetFileName(inputDocument?.FullPath));
    var pdfFileName = Path.GetFileNameWithoutExtension(inputDocument?.FullPath);
    var chunksFolder = Path.Combine(sourcePath, pdfFileName);

    var MaxSize = option.Size == 0 ? 3 : option.Size;
    var limitSize = MaxSize - 0.2;
    var ThresholdSize = MaxSize - 0.5;
       
     _ = CreateFolderIfNotExists(chunksFolder);

     FileInfo fileInfo = new FileInfo(inputDocument?.FullPath);
     DateTime filelastModified = fileInfo.LastWriteTime;

     var directory = new DirectoryInfo(chunksFolder);
   DateTime chunkFolderLastModified = directory.LastWriteTime;
    if (filelastModified > chunkFolderLastModified && directory.GetFiles().Length > 0)
    {
        Console.WriteLine($"The document {pdfFileName} has been modified. Deleting the existing chunks folder", ConsoleColor.Yellow);
        Directory.Delete(chunksFolder, true);
        _ = CreateFolderIfNotExists(chunksFolder);
    }
   
   if (directory.GetFiles().Length == 0)
    {
    
     var outputDoc = new PdfDocument { Version = inputDocument.Version };
        // outputDocument.Info.Title = $"Page {idx + 1} of {inputDocument.Info.Title}";
      outputDoc.Info.Creator = inputDocument.Info.Creator;
      int index = 1;
        for (int idx = 0; idx < inputDocument.PageCount; idx++)
        {
            // Add the page and save it
            PdfPage newPage = inputDocument.Pages[idx];

            var docSize = GetDocSizeInMB(outputDoc);
            var pageSize = GetPageSizeInMB(newPage);
            var totalSize = docSize + pageSize;

            if (pageSize > limitSize)
            {
                if (outputDoc.PageCount == 0)
                {
                    outputDoc.AddPage(newPage);
                    if (outputDoc.PageCount > 0 && option.AddReferencePage)
                        AddOriginalDocumentReference(outputDoc, OriginalFile);

                    outputDoc.Save(Path.Combine(chunksFolder, $"{pdfFileName} - {index++}.pdf"));
                }
                else
                {
                    if (outputDoc.PageCount > 0 && option.AddReferencePage)
                        AddOriginalDocumentReference(outputDoc, OriginalFile);

                    outputDoc.Save(Path.Combine(chunksFolder, $"{pdfFileName} - {index++}.pdf"));

                    outputDoc = new PdfDocument { Version = outputDoc.Version };
                    outputDoc.AddPage(newPage);

                    if (outputDoc.PageCount > 0 && option.AddReferencePage)
                        AddOriginalDocumentReference(outputDoc, OriginalFile);

                    outputDoc.Save(Path.Combine(chunksFolder, $"{pdfFileName} - {index++}.pdf"));
                }
                outputDoc = new PdfDocument { Version = outputDoc.Version };
            }
            else if (totalSize < ThresholdSize)
            {
                _ = outputDoc.AddPage(newPage);
            }
            else if (totalSize > ThresholdSize && totalSize < limitSize)
            {
                _ = outputDoc.AddPage(newPage);
                if (outputDoc.PageCount > 0 && option.AddReferencePage)
                    AddOriginalDocumentReference(outputDoc, OriginalFile);

                outputDoc.Save(Path.Combine(chunksFolder, $"{pdfFileName} - {index++}.pdf"));
                outputDoc = new PdfDocument { Version = outputDoc.Version };
            }
            else if (totalSize > limitSize)
            {
                if (outputDoc.PageCount > 0 && option.AddReferencePage)
                    AddOriginalDocumentReference(outputDoc, OriginalFile);

                outputDoc.Save(Path.Combine(chunksFolder, $"{pdfFileName} - {index++}.pdf"));

                outputDoc = new PdfDocument { Version = outputDoc.Version };
                _ = outputDoc.AddPage(newPage);

                if (outputDoc.PageCount > 0 && option.AddReferencePage)
                    AddOriginalDocumentReference(outputDoc, OriginalFile);

                outputDoc.Save(Path.Combine(chunksFolder, $"{pdfFileName} - {index++}.pdf"));

                outputDoc = new PdfDocument { Version = outputDoc.Version };
            }


            if (idx == inputDocument.PageCount - 1)
            {
                if (outputDoc.PageCount > 0 && option.AddReferencePage)
                    AddOriginalDocumentReference(outputDoc, OriginalFile);

                outputDoc.Save(Path.Combine(chunksFolder, $"{pdfFileName} - {index++}.pdf"));
            }
        }
    }
 }

// <summary>   
///    Check if the document size exceeds 2.5 MB
/// </summary>
static double GetPageSizeInMB( PdfPage currentPage)
{  

    double currentPageSize = 0;
    if (currentPage!=null && currentPage.Contents.Elements.Count > 0)
        {
            using (var stream = new MemoryStream())
            {
                PdfDocument CurrentPageDoc=new();
                _ = CurrentPageDoc.AddPage(currentPage);
                CurrentPageDoc.Save(stream, false);
                currentPageSize = ConvertBytesToMegabytes(stream.Length);
            }
        }
        return currentPageSize;
}

/// <summary>
/// Get the document size in MB
/// </summary>
/// <param name="document"></param>
/// <returns></returns>
static double GetDocSizeInMB(PdfDocument document)
{ 
    double size = 0;
    if (document == null || document?.PageCount == 0)
    {
        return size;
    }
    using (var stream = new MemoryStream())
    {
        document?.Save(stream, false);
       size=ConvertBytesToMegabytes(stream.Length) ;
    }
    return size;
}

/// <summary>   
///    Check if the document size exceeds 2.5 MB
/// </summary>
static bool IsDocumentSizeExceeds(PdfDocument document)
{
    using (var stream = new MemoryStream())
    {
       
        document.Save(stream, false);
        if (ConvertBytesToMegabytes(stream.Length) > 2.5 && ConvertBytesToMegabytes(stream.Length) < 2.8)
        {
            return true;
        }
        else if (ConvertBytesToMegabytes(stream.Length) > 2.8)
        {
            return false;
        }
    }
    return false;
}

/// <summary>
/// Create a folder if it does not exist
/// </summary>
/// <param name="rootPath"></param>
/// <param name="FolderName"></param>
/// <returns></returns>
static string CreateFolderIfNotExist(string rootPath, string FolderName)
{
    return CreateFolderIfNotExists(Path.Combine(rootPath, FolderName));
}
/// <summary>
/// Create a folder if it does not exist
/// </summary>
/// <param name="FullPath"></param>
/// <returns></returns>
static string CreateFolderIfNotExists(string FullPath)
{
    var rootPath = Path.GetPathRoot(FullPath);
    if (string.IsNullOrWhiteSpace(rootPath))
    {
        Console.WriteLine($"{rootPath} does not exist. , please check", ConsoleColor.Red);
        throw new DirectoryNotFoundException($"{rootPath} does not exist. , please check");
    }
    if (!Directory.Exists(FullPath))
    {
        Directory.CreateDirectory(FullPath);
        Console.WriteLine($"{FullPath} folder created successfully.", ConsoleColor.Green);
    }
    return FullPath;
}
/// <summary>
/// Check if the file size exceeds 3 MB
/// </summary>
/// <param name="filePath"></param>
/// <returns></returns>
static bool IsFileSizeExceeds(string filePath)
{
    FileInfo fileInfo = new(filePath);
    long fileSizeInBytes = fileInfo.Length;
    double fileSizeInMB = ConvertBytesToMegabytes(fileSizeInBytes);
    if (fileSizeInMB >= 3)
    {
        Console.WriteLine($"size of the {filePath}  is greater than 3 MB.");
        return true;
    }
    else
    {
        Console.WriteLine("File size is not greater than 3 MB.");
        return false;
    }
}

/// <summary>
/// Convert bytes to Megabytes
/// </summary>
/// <param name="bytes"></param>
/// <returns></returns>
static double ConvertBytesToMegabytes(long bytes)
{
    return bytes / 1024.0 / 1024.0;
}

/// <summary>
/// Validate the source path
/// </summary>
/// <param name="sourcePath"></param>
static void ValidationOnSourcePath(string sourcePath)
{
    if (!Path.Exists(sourcePath))
    {
        Console.WriteLine($"{sourcePath} does not exist. , please check", ConsoleColor.Red);
        throw new DirectoryNotFoundException($"{sourcePath} does not exist. , please check");
    }

    if (Directory.GetFiles(sourcePath, "*.pdf").Length == 0)
    {
        Console.WriteLine($"{sourcePath} does not contain any PDF files. , please check", ConsoleColor.Red);
        throw new FileNotFoundException($"{sourcePath} does not contain any PDF files. , please check");
    }
}
/// <summary>
/// Add the original document reference to the new document
/// </summary>
/// <param name="pdfDocument"></param>
/// <param name="originalDocumentName"></param>
static void AddOriginalDocumentReference(PdfDocument pdfDocument,string originalDocumentName)
{
    
   // XGraphics gfx = XGraphics.FromPdfPage(newPage);
    PdfPage newPage = pdfDocument.AddPage();
    XGraphics gfx = XGraphics.FromPdfPage(newPage);
    // Calculate the center position
        double centerX = newPage.Width / 2;
        double centerY = newPage.Height / 2;
    XFont font = new("Arial", 30, XFontStyleEx.Italic);
    string newText = $"Reference:";
    // Create a centered rectangle 
        XRect rectText = new XRect(centerX - 50, centerY - 10, 100, 20);
    gfx.DrawString(newText, font, XBrushes.Black, rectText, XStringFormats.TopLeft);
       
    // Set up footer content
     string footerText = $"Originial PDF Document: {originalDocumentName}";
     string url = "https://www.example.com";

    // Add the hyperlink
    XRect linkRect = new XRect(10, newPage.Height - 30, 200, 20);
    gfx.DrawRectangle(XBrushes.Transparent, linkRect);
    gfx.DrawString(footerText, new XFont("Arial", 10), XBrushes.Black, linkRect, XStringFormats.TopLeft);

    // Create the link annotation
    PdfRectangle rect = new PdfRectangle(linkRect);
    PdfLinkAnnotation link = PdfLinkAnnotation.CreateDocumentLink(rect,2);
    link.Elements.SetString("/A/URI", url);
    newPage.Annotations.Add(link);
}