> Limitation in Copilot Studio: Consider below list of files in SharePoint Data source with 3 MB file size limitation.

  1. PDF 
  2. Doc \ Docx
  3. ppt \ pptx
  4. Text files

> There are couple of options to Split large PDF files (>3 MB) and sync splitted pages back to SharePoint.

|Options  |  Consequences\Challenges|
|---|---|
|<b>*Option 1:*</b><br>We can sync files using one drive in VM and trigger split document process using task scheduler.<br> just trigger spliting process and move files to sharepoint one drive location. <br>Advatange with  this option is we don't worry about upload or download files from Sharepoint.<br> One drive service will take care of that |Need separate dedicated VM. <br> Need Service account to sync files to SharePoint using one drive. <br> Need to choose PDF library to split large PDF files <br> PDF File(s)chunks will be uploaded in SharePoint using Service account context.|
|<b>*Option 2:*</b><br>Create Logic App (or) Power Automate flow with trigger (<i>When File is created or updated in SharePoint<i>)<br>Check file size exceeds 3MB size then Call azure function to split PDF files based on size up to 3 MB and upload files to SharePoint using SharePoint Clint Library. | Azure function and Logic app(or) PowerAutomate cost will be involved.<br> Need to choose PDF library to split PDF files. <br>Still Consider upload PDF chunks to Shapoint directly from Azure function for better performance <br>Managed Identity for Azure function need proper permission to upload PDF chunks in Sharepoint. |
|<b>*Option 3*:</b><br>Trigger Azure function timely basis to find large document (>3MB) and call split PDF method and upload split files directly to Sharepoint from azure function |Need to check how long it will take to search\ filter files in Sharepoint <br> Chances of time out issues if huge no of files in SharePoint.<br>Azure function and Logic app(or) PowerAutomate cost will be involved.<br>Choose PDF library to split PDF file. <br>Managed Identity for Azure function need proper permission to upload PDF chunks in Sharepoint. <br> More no of Azure function calls even there is no new files in SharePoint. it is some additional cost in this approach |
|<b>*Option 4:*</b><br>Create Logic App(or) Power Automate flow  with trigger (File is created or updated in SharePoint)<br> split PDF using thrid PDF connectors ( [Adobe](https://learn.microsoft.com/en-us/connectors/adobepdftools/) , [Encodian](https://learn.microsoft.com/en-us/connectors/encodiandocumentmanager/) , Muhimbi,[PDF4me](https://learn.microsoft.com/en-us/connectors/pdf4me/))| There is DLP policy violation on sharing data between SharePoint business connector with Non business Connector <br> Logic app(or) PowerAutomate and cost will be involved.<br>Third party Connectors pricing will be added. <br>Even Override DLP Policy, these connectors will only split PDF based on no of pages so No much control on splittig based on size of the file |
|<b>*Option 5*:</b><br>Trigger Azure function using [SharePoint web hook](https://learn.microsoft.com/en-us/sharepoint/dev/apis/webhooks/sharepoint-webhooks-using-azure-functions) <br> when document is changed \added in document library | There is no much details about change happened  in the event payload.<BR>We need to call [Get Changes API](https://msdn.microsoft.com/library/office/dn531433.aspx#bk_ListGetChanges) to query the collection of changes |
|<b>*Option 6*:</b><br> PowerAutomate Desktop will provide built-in PDF extraction action to split PDF files| Additional Power automate dasktop flow license for required.<br> Dedicated VM is required to maintain PAD flow. <br>No much control on splittig based on size of the file|

---


> We can implement PDF split functionality using below list of Nuget packages


| Library Name | Description  | NuGet Package  | Pricing Details  |Features|
|---|---|---|----|---|
|**PDFSharp**	|An open-source library for creating and modifying PDF documents in C# and .NET.|[PDFsharp](https://www.nuget.org/packages/PDFsharp/6.1.0#supportedframeworks-body-tab)|Free and open-source.| 1. Page Manipulations (Get page count, concatenate PDF files, insert pages, and split PDFs) <br> 2. Not possible to extract text from images in PDF directly but we can get this using Open source packages(Tesseract OCR ,Iron OCR) <br> 3. Not supported to conversion to PDF |
| **Aspose.PDF**  | A comprehensive commercial library for creating, editing, and manipulating PDF files. | [Aspose.PDF](https://www.nuget.org/packages/Aspose.PDF) <br> [Aspose.Words](https://www.nuget.org/packages/Aspose.Words) <br>  [Aspose.Slides.NET](https://www.nuget.org/packages/Aspose.Slides.NET/)<br>[Aspose.OCR](https://www.nuget.org/packages/Aspose.OCR/)<br> [Aspose.Cells](https://www.nuget.org/packages/Aspose.Cells/) <br> [Aspose.Total](https://www.nuget.org/packages/Aspose.Total) <br>  | Provides both free and [paid support](https://purchase.aspose.com/pricing/) options. Paid support offers prioritized issue resolution. Refund policy available. |  1. Page Manipulations (Get page count, concatenate PDF files, insert pages, and split PDFs) <br> 2. Extract Text from image or PDF <br> 3. Conversion of word, Excel , PPT to PDF<br> And lot more.|
| **Syncfusion.Pdf.Net.Core** | A comprehensive library for working with PDF files in .NET Core.  | [Syncfusion.Pdf.Net.Core](https://www.nuget.org/packages/Syncfusion.Pdf.Net.Core) |Available for free and [paid support](https://www.syncfusion.com/sales/teamlicense). |  1. Page Manipulations (Get page count, concatenate PDF files, insert pages, and split PDFs) <br> 2. Extract Text from image or PDF <br> 3. Conversion of word, Excel , PPT to PDF<br> And lot more.|
| **EvoPdf.PdfSplit**   | A commercial library for splitting PDF files in .NET Core and .NET Standard applications. | [EvoPdf.PdfSplit.NetCore](https://www.nuget.org/packages/EvoPdf.PdfSplit.NetCore)  | Available for[paid support](https://www.evopdf.com/buy.aspx).  |  1. Page Manipulations (Get page count, concatenate PDF files, insert pages, and split PDFs). <br> 2. Not supported for extraction of text from images in PDF <br> 3. Not supported to conversion to PDF|  
| **OfficeConverter**   | An open-source library with MIT License to convert Micrsoft Office documents to PDF using.NET Standard libraries. | [OfficeConverter](https://www.nuget.org/packages/OfficeConverter) |  Free and open-source with MIT license |  1. Convert Microsoft Office document to PDF . <br> 2. Supported versions : DOC, .DOCM, .DOCX, .DOT, .DOTM, .ODT, .XLS, .XLSB, .XLSM, .XLSX, .XLT, .XLTM, .XLTX, .XLW, .ODS, .POT, .PPT, .POTM, .POTX, .PPS, .PPSM, .PPSX, .PPTM, .PPTX and .ODP files to PDF  | 
| **DocumentFormat.OpenXml**   |An open-source library from .NET community for manipulation of office documents | [DocumentFormat.OpenXml](https://www.nuget.org/packages/DocumentFormat.OpenXml/)  | Available with [MIT License](https://www.evopdf.com/buy.aspx).  |  1. Manipulating Open XML documents, such as .docx, .xlsx, and .pptx files. It does not support operations on PDF files. <br> 2. It does not support operations on PDF files.|  
---
<br>

<br>

> ### Triggers and Bindings in Azure Functions

|**Triggers**:|
|---|
| 1. Triggers cause an Azure Function to run.<br> 2. A trigger defines how a function is invoked, and every function must have exactly one trigger.<br> 3. Triggers have associated data, often provided as the payload of the function.|

|*list of triggers include* : |
|---|
|1.*Queue Trigger*: Executes when a new message arrives in a queue. <br> 2. *Timer Trigger*: Executes on a schedule (e.g., every 5 minutes).<br> 3. **HTTP Trigger**: Executes when an HTTP request is made to a specific endpoint. <br> 4. *Event Grid Trigger*: Executes when an event is published to an Azure Event Grid topic.

|**Bindings:**|
|---|
|Bindings connect other resources to an Azure Function declaratively. <br>Bindings can be connected as input bindings, output bindings, or both. <br> Data from bindings is provided to the function as parameters.<br>|

|*List of bindings include*:|
|---|
|1.*Blob Storage Binding*: Reads or writes data to/from Azure Blob Storage. <br>2.*Queue Binding*: Reads or writes messages from/to an Azure Queue. <br>3.*Cosmos DB Binding*: Reads or writes data to/from Azure Cosmos DB. <br>4.*SendGrid Binding*: Sends emails using SendGrid. <br>5. **HTTP Output Binding**: Sends an HTTP response from your function. |