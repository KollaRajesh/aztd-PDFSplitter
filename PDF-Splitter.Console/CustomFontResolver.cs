using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;


// Implement a custom font resolver
class CustomFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        // Load the font data (e.g., Arial.ttf) and return it as a byte array
        // You can load the font from a file, stream, or any other source
        // Replace the path below with the actual path to the Arial font file
        string fontPath = @"C:\Windows\Fonts\arial.ttf"; // Example path
        return File.ReadAllBytes(fontPath);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Return font information based on the family name and style
        // You can customize this logic to handle different font styles
        return new FontResolverInfo("Arial#");
    }
}
