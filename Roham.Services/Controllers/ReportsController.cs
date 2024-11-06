using Microsoft.AspNetCore.Mvc;
using Roham.Services.Helpers;
using Roham.Services.Models;
using SqlKata.Execution;
using Stimulsoft.Base;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Export;

namespace Roham.Services.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController(
    IWebHostEnvironment _webHostEnvironment,
    QueryFactory _queryFactory) : Controller
{
    [HttpPost("export")]
    public async Task<IActionResult> Print([FromBody] PrintInputModel printInputModel)
    {
        try
        {
            var report_item = await _queryFactory
            .Query("reports")
            .Where("name", printInputModel.report_name)
            .FirstOrDefaultAsync();

            var rootPath = _webHostEnvironment.ContentRootPath;
            var report = new Stimulsoft.Report.StiReport();

            report.LoadFromString(report_item.content);

            var fileContent = System.IO.File.ReadAllBytes(($"{rootPath}/wwwroot/fonts/B Nazanin.ttf"));
            var resource = new StiResource("B Nazanin", StiResourceType.FontTtf, fileContent, false);
            report.Dictionary.Resources.Add(resource);

            //StiFontCollection.AddResourceFont("B Nazanin", fileContent, "ttf", "B Nazanin");

            if (!string.IsNullOrEmpty(printInputModel.data))
            {
                var dataset = StiJsonToDataSetConverterV2.GetDataSet(printInputModel.data);
                report.Dictionary.Databases.Clear();
                report.RegData(dataset);
            }

            report.Render();

            MemoryStream stream = new();

            var random_number = RandomHelper.GetRandomNumber(10000000, 999999999999);
            var out_put_name = $"{printInputModel.report_name}-{random_number}";
            var out_put_type = "";
            if (printInputModel.print_type == "pdf")
            {
                StiPdfExportService pdf_service = new();

                pdf_service.ExportPdf(report, stream, new StiPdfExportSettings { });

                out_put_name += ".pdf";
                out_put_type = "application/pdf";
            }
            else if (printInputModel.print_type == "xls")
            {
                StiExcelExportService excel_service = new();

                excel_service.ExportExcel(report, stream, new StiExcelExportSettings { });
                out_put_name += ".xls";
                out_put_type = "application/vnd.ms-excel";
            }
            else if (printInputModel.print_type == "doc")
            {
                StiWord2007ExportService excel_service = new();

                excel_service.ExportWord(report, stream, new StiWord2007ExportSettings { });
                out_put_name += ".doc";
                out_put_type = "application/vnd.ms-word";
            }

            var currentFilePath = $"wwwroot\\exports\\{out_put_name}";

            var physicalFilePath = Path.Combine(rootPath, currentFilePath);

            using (var ws = new MemoryStream())
            {
                stream.WriteTo(ws);

                byte[] data = stream.ToArray();

                var result = await _queryFactory.Query("files").InsertGetIdAsync<int>(new
                {
                    name = out_put_name,
                    type = out_put_type,
                    size = stream.Length,
                    content = data,
                    cat = "exports"
                });
            }

            return Ok(new
            {
                code = 200,
                data = new
                {
                    file_name = out_put_name
                }
            });
        }
        catch (Exception exp)
        {
            return Ok(new
            {
                code = 500,
                message = "خطا در خروجی گزارش!",
                error = exp.Message
            });
        }
    }
}
