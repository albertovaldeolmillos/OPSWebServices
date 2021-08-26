using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Configuration;

/// <summary>
/// Summary description for ITextEvents
/// </summary>
public class ITextEvents : PdfPageEventHelper
{

    // This is the contentbyte object of the writer
    PdfContentByte cb;

    // we will put the final number of pages in a template
    PdfTemplate headerTemplate, footerTemplate;

    // this is the BaseFont we are going to use for the header / footer
    BaseFont bf = null;

    // This keeps track of the creation time
    DateTime PrintTime = DateTime.Now;


    #region Fields
    private string _header;
    private string _headerData;
    private string _footerData;
    #endregion

    #region Properties
    public string Header
    {
        get { return _header; }
        set { _header = value; }
    }
    public string HeaderData
    {
        get { return _headerData; }
        set { _headerData = value; }
    }
    public string FooterData
    {
        get { return _footerData; }
        set { _footerData = value; }
    }
    #endregion

    public ITextEvents(string strHeaderData, string strFooterData)
    {
        _headerData = strHeaderData;
        _footerData = strFooterData;
    }

    public override void OnOpenDocument(PdfWriter writer, Document document)
    {
        try
        {
            PrintTime = DateTime.Now;
            bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb = writer.DirectContent;
            headerTemplate = cb.CreateTemplate(100, 100);
            footerTemplate = cb.CreateTemplate(50, 50);
        }
        catch (DocumentException de)
        {

        }
        catch (System.IO.IOException ioe)
        {

        }
    }

    public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
    {
        base.OnEndPage(writer, document);

        iTextSharp.text.Font headerTextBlack = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
        iTextSharp.text.Font footerTextBlack = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
        iTextSharp.text.Font footerTextWhite = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.WHITE);

        //Create PdfTable object
        PdfPTable pdfTabHeader = new PdfPTable(1);
        PdfPTable pdfTabFooter = new PdfPTable(2);
        float[] widths = new float[] { 4f, 1f };
        pdfTabFooter.SetWidths(widths);

        // Create logo
        string imageURL = ConfigurationManager.AppSettings["ReportHeaderLogo"].ToString();
        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
        jpg.ScaleToFit(document.PageSize.Width, 90f);

        // Create header text
        Phrase p1Header = new Phrase(_headerData, headerTextBlack);

        String text = "Página " + writer.PageNumber;

        // Create footer text
        Phrase p1Footer = new Phrase(ConfigurationManager.AppSettings["ReportFooterText1"].ToString(), footerTextWhite);
        Phrase p2Footer = new Phrase(ConfigurationManager.AppSettings["ReportFooterText2"].ToString(), footerTextWhite);
        Phrase p3Footer = new Phrase(text, footerTextWhite);
        Phrase p4Footer = new Phrase(_footerData, footerTextWhite);
        Phrase pBlank = new Phrase(" ", footerTextWhite);

        // Header
        PdfPCell pdfCell1 = new PdfPCell(jpg);
        PdfPCell pdfCell8 = new PdfPCell(p1Header);
        PdfPCell pdfCell9 = new PdfPCell(pBlank);

        // Footer
        PdfPCell pdfCell2 = new PdfPCell(p1Footer);
        PdfPCell pdfCell3 = new PdfPCell(p3Footer);
        PdfPCell pdfCell4 = new PdfPCell(p2Footer);
        PdfPCell pdfCell5 = new PdfPCell();
        PdfPCell pdfCell6 = new PdfPCell(p4Footer);
        PdfPCell pdfCell7 = new PdfPCell();

        //set the alignment of the cells
        pdfCell1.HorizontalAlignment = Element.ALIGN_CENTER;
        pdfCell2.HorizontalAlignment = Element.ALIGN_CENTER;
        pdfCell3.HorizontalAlignment = Element.ALIGN_LEFT;
        pdfCell4.HorizontalAlignment = Element.ALIGN_CENTER;
        pdfCell5.HorizontalAlignment = Element.ALIGN_CENTER;
        pdfCell6.HorizontalAlignment = Element.ALIGN_CENTER;
        pdfCell7.HorizontalAlignment = Element.ALIGN_CENTER;
        pdfCell8.HorizontalAlignment = Element.ALIGN_CENTER;
        pdfCell9.HorizontalAlignment = Element.ALIGN_CENTER;

        pdfCell1.VerticalAlignment = Element.ALIGN_TOP;
        pdfCell2.VerticalAlignment = Element.ALIGN_MIDDLE;
        pdfCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
        pdfCell4.VerticalAlignment = Element.ALIGN_MIDDLE;
        pdfCell5.VerticalAlignment = Element.ALIGN_MIDDLE;
        pdfCell6.VerticalAlignment = Element.ALIGN_MIDDLE;
        pdfCell7.VerticalAlignment = Element.ALIGN_MIDDLE;
        pdfCell8.VerticalAlignment = Element.ALIGN_MIDDLE;
        pdfCell9.VerticalAlignment = Element.ALIGN_MIDDLE;

        // Set cell borders
        pdfCell1.Border = 0;
        pdfCell3.BorderColor = iTextSharp.text.BaseColor.WHITE;
        pdfCell3.Border = Rectangle.LEFT_BORDER;
        pdfCell3.BorderWidthLeft = 2f;
        pdfCell3.PaddingLeft = 10f;
        pdfCell5.BorderColor = iTextSharp.text.BaseColor.WHITE;
        pdfCell5.Border = Rectangle.LEFT_BORDER;
        pdfCell5.BorderWidthLeft = 2f;
        pdfCell7.BorderColor = iTextSharp.text.BaseColor.WHITE;
        pdfCell7.Border = Rectangle.LEFT_BORDER;
        pdfCell7.BorderWidthLeft = 2f;
        pdfCell2.Border = 0;
        pdfCell4.Border = 0;
        pdfCell6.Border = 0;
        pdfCell8.Border = 0;
        pdfCell9.Border = 0;

        // Set background color
        BaseColor colorFooter = new BaseColor(89, 59, 140);
        pdfCell2.BackgroundColor = colorFooter;
        pdfCell3.BackgroundColor = colorFooter;
        pdfCell4.BackgroundColor = colorFooter;
        pdfCell5.BackgroundColor = colorFooter;
        pdfCell6.BackgroundColor = colorFooter;
        pdfCell7.BackgroundColor = colorFooter;

        // Add cells into tables
        pdfTabHeader.AddCell(pdfCell1);
        pdfTabHeader.AddCell(pdfCell8);
        pdfTabHeader.AddCell(pdfCell9);

        pdfTabFooter.AddCell(pdfCell2);
        pdfTabFooter.AddCell(pdfCell3);
        pdfTabFooter.AddCell(pdfCell4);
        pdfTabFooter.AddCell(pdfCell5);
        pdfTabFooter.AddCell(pdfCell6);
        pdfTabFooter.AddCell(pdfCell7);

        pdfTabHeader.TotalWidth = document.PageSize.Width - 20f;
        pdfTabFooter.TotalWidth = document.PageSize.Width - 20f;

        pdfTabHeader.WriteSelectedRows(0, -1, 0, document.PageSize.Height - 10, writer.DirectContent);
        pdfTabFooter.WriteSelectedRows(0, -1, 10, document.PageSize.GetBottom(50), writer.DirectContent);
    }

    public override void OnCloseDocument(PdfWriter writer, Document document)
    {
        base.OnCloseDocument(writer, document);

        headerTemplate.BeginText();
        headerTemplate.SetFontAndSize(bf, 12);
        headerTemplate.SetTextMatrix(0, 0);
        headerTemplate.ShowText((writer.PageNumber - 1).ToString());
        headerTemplate.EndText();

        footerTemplate.BeginText();
        footerTemplate.SetFontAndSize(bf, 12);
        footerTemplate.SetTextMatrix(0, 0);
        footerTemplate.ShowText((writer.PageNumber - 1).ToString());
        footerTemplate.EndText();


    }
}