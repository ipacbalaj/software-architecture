using System.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Tracing;

public class FileProcessor(SlowProcessor processor)
{
    public void Start()
    {
        using (Activity? startActivity = new ActivitySource("TracingDemo").StartActivity("FileProcessor.Start"))
        {
            string baseDirectory = AppContext.BaseDirectory;

            string inputFilePath = Path.Combine(baseDirectory, "template.docx");
            string outputFilePath = Path.Combine(baseDirectory, "output.docx");

            string startDate = "10/10/2025";
            string endDate = "10/11/2025";
            string orderNumber = "200";

            File.Copy(inputFilePath, outputFilePath, true);

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputFilePath, true))
            {
                var mainPart = wordDoc.MainDocumentPart;
                if (mainPart != null)
                {
                    var documentText = mainPart.Document.Body.InnerText;
                    foreach (var text in mainPart.Document.Descendants<Text>())
                    {
                        if (text.Text.Contains("START_DATE"))
                            text.Text = text.Text.Replace("START_DATE", startDate);

                        if (text.Text.Contains("END_DATE"))
                            text.Text = text.Text.Replace("END_DATE", endDate);

                        if (text.Text.Contains("NUMAR_COMANDA"))
                            text.Text = text.Text.Replace("NUMAR_COMANDA", orderNumber);
                    }

                    mainPart.Document.Save();
                }

                using (Activity? slowProcActivity = new ActivitySource("TracingDemo").StartActivity("Processor.SlowProcessingFunction"))
                {
                    processor.SlowProcessingFunction();
                }
            }
        }
    }
}