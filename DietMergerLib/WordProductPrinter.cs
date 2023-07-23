using DietMergerLib.Models;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietMergerLib
{
    public class WordProductPrinter
    {
        public void PrintProducts(List<ProductCategoryGroup> categories)
        {
            WordDocument doc = new WordDocument();

            // Add a new section
            IWSection section = doc.AddSection();
            DrawAsTable(categories, section);

            MemoryStream stream = new MemoryStream();
            //Saves the Word document to  MemoryStream
            doc.Save(stream, FormatType.Docx);

            using (FileStream fileStream = new FileStream($"Products-{DateTime.Now.Millisecond}.docx", FileMode.Create, FileAccess.Write))
            {
                stream.WriteTo(fileStream);
            }
        }

        private static void DrawAsTable(List<ProductCategoryGroup> categories, IWSection section)
        {
            IWTable productsTable = section.AddTable();
            productsTable.ApplyStyleForBandedColumns = true;
            productsTable.ApplyStyle(BuiltinTableStyle.LightList);
            productsTable.ResetCells(1, 5);
            productsTable.Rows[0].Cells[0].AddParagraph().AppendText("Nazwa");
            productsTable.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            productsTable.Rows[0].Cells[1].AddParagraph().AppendText("Ilość");
            productsTable.Rows[0].Cells[1].Paragraphs[0].ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            productsTable.Rows[0].Cells[2].AddParagraph().AppendText("Jednostka");
            productsTable.Rows[0].Cells[2].Paragraphs[0].ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            productsTable.Rows[0].Cells[3].AddParagraph().AppendText("Waga");
            productsTable.Rows[0].Cells[3].Paragraphs[0].ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            productsTable.Rows[0].Cells[4].AddParagraph().AppendText("[g]");
            productsTable.Rows[0].Cells[4].Paragraphs[0].ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            foreach (var category in categories)
            {
                // Add a new paragraph for the category name
                WTableRow categoryRow = productsTable.AddRow();
                IWParagraph paragraph = categoryRow.Cells[0].AddParagraph();
                paragraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                paragraph.AppendText(category.Name ?? "").CharacterFormat.Bold = true;
                categoryRow.RowFormat.BackColor = Syncfusion.Drawing.Color.LightGray;
                categoryRow.Cells[0].CellFormat.HorizontalMerge = CellMerge.Start;
                categoryRow.Cells[1].CellFormat.HorizontalMerge = CellMerge.Continue;
                categoryRow.Cells[2].CellFormat.HorizontalMerge = CellMerge.Continue;
                categoryRow.Cells[3].CellFormat.HorizontalMerge = CellMerge.Continue;
                categoryRow.Cells[4].CellFormat.HorizontalMerge = CellMerge.Continue;
                // Add rows for each product
                foreach (var product in category.Products)
                {
                    WTableRow row = productsTable.AddRow();
                    row.RowFormat.BackColor = Syncfusion.Drawing.Color.White;
                    row.Cells[0].CellFormat.HorizontalMerge = CellMerge.None;
                    row.Cells[0].AddParagraph().AppendText(product.Name ?? "").CharacterFormat.Bold = false;
                    row.Cells[1].AddParagraph().AppendText(product.Quantity?.ToString() ?? "");
                    row.Cells[2].AddParagraph().AppendText(product.QuantityUnit ?? "");
                    row.Cells[3].AddParagraph().AppendText(product.Weight?.ToString() ?? "");
                    row.Cells[4].AddParagraph().AppendText(product.WeightUnit ?? "");
                }
            }
        }

        private static void DrawAsList(List<ProductCategoryGroup> categories, IWSection section)
        {
            foreach (var category in categories)
            {
                // Add a new paragraph for the category name
                IWParagraph categoryParagraph = section.AddParagraph();
                categoryParagraph.AppendText(category.Name).CharacterFormat.Bold = true;



                // Add items for each product
                foreach (var product in category.Products)
                {
                    IWParagraph productParagraph = section.AddParagraph();
                    productParagraph.AppendCheckBox();
                    productParagraph.AppendText("\t" + product.Name);
                    productParagraph.AppendText("\t" + product.Quantity);
                    productParagraph.AppendText("\t" + product.QuantityUnit);
                    productParagraph.AppendText("\t" + product.Weight);
                    productParagraph.AppendText("\t" + product.WeightUnit);
                }
            }
        }
    }
}
