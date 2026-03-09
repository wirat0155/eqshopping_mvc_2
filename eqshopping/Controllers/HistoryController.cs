using eqshopping.Models.DbView;
using eqshopping.Repositories;
using eqshopping.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Controllers
{
    [Authorize]
    public class HistoryController : BaseController
    {
        private readonly ShoppingTranRepository _shoppingTran;
        private readonly ProductionOrderRepository _poRepo;

        public HistoryController(
            ShoppingTranRepository shoppingTran,
            ProductionOrderRepository poRepo,
            ICompositeViewEngine viewEngine) : base(viewEngine)
        {
            _shoppingTran = shoppingTran;
            _poRepo = poRepo;
        }

        public IActionResult vSearch()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var mode = Request.Form["mode"].FirstOrDefault();
                
                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Sorting
                string sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                string sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                string sortColumn = "";
                if (!string.IsNullOrEmpty(sortColumnIndex))
                {
                    sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
                }

                // Custom filters
                string startDateStr = Request.Form["startDate"].FirstOrDefault();
                string endDateStr = Request.Form["endDate"].FirstOrDefault();
                string posNosStr = Request.Form["posNos"].FirstOrDefault();
                string partNosStr = Request.Form["partNos"].FirstOrDefault();
                string cellNosStr = Request.Form["cellNos"].FirstOrDefault();
                string plantno = Request.Form["plantno"].FirstOrDefault();
                
                DateTime? startDate = null;
                DateTime? endDate = null;
                if(DateTime.TryParse(startDateStr, out DateTime sDate)) startDate = sDate;
                if(DateTime.TryParse(endDateStr, out DateTime eDate)) endDate = eDate;

                List<string> posNos = !string.IsNullOrEmpty(posNosStr) ? posNosStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() : null;
                List<string> partNos = !string.IsNullOrEmpty(partNosStr) ? partNosStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() : null;
                
                object data = null;
                int totalCount = 0;

                if (mode == "ASSY")
                {
                    var result = await _poRepo.GetFinalAssyHistory(plantno, posNos, partNos, startDate, endDate, skip, pageSize, sortColumn, sortDirection);
                    data = result.Data;
                    totalCount = result.TotalCount;
                }
                else
                {
                    // EQS Mode (Default)
                    List<string> cellNos = !string.IsNullOrEmpty(cellNosStr) ? cellNosStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() : null;
                    var result = await _shoppingTran.GetHistory(plantno, posNos, partNos, cellNos, startDate, endDate, skip, pageSize);
                    data = result.Data;
                    totalCount = result.TotalCount;
                }
                
                return Json(new { draw = draw, recordsFiltered = totalCount, recordsTotal = totalCount, data = data });
            }
            catch (Exception ex)
            {
               return StatusCode(500, new { success = false, text = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel(string mode, string startDate, string endDate, string posNos, string partNos, string cellNos, string plantno)
        {
             try
            {
                DateTime? sDateVal = null;
                DateTime? eDateVal = null;
                if(DateTime.TryParse(startDate, out DateTime sDate)) sDateVal = sDate;
                else sDateVal = DateTime.Today.AddMonths(-1);

                if(DateTime.TryParse(endDate, out DateTime eDate)) eDateVal = eDate;
                else eDateVal = DateTime.Today;

                List<string> posNosList = !string.IsNullOrEmpty(posNos) ? posNos.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() : null;
                List<string> partNosList = !string.IsNullOrEmpty(partNos) ? partNos.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() : null;
                
                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    if (mode == "ASSY")
                    {
                        var (data, totalCount) = await _poRepo.GetFinalAssyHistory(plantno, posNosList, partNosList, sDateVal, eDateVal, 0, int.MaxValue, null, null);
                        
                        var worksheet = workbook.Worksheets.Add("FinalAssyHistory");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "ID";
                        worksheet.Cell(currentRow, 2).Value = "Assy Label";
                        worksheet.Cell(currentRow, 3).Value = "Seq No";
                        worksheet.Cell(currentRow, 4).Value = "Check User";
                        worksheet.Cell(currentRow, 5).Value = "Check Date";
                        worksheet.Cell(currentRow, 6).Value = "Part No";
                        worksheet.Cell(currentRow, 7).Value = "Actual Part No";
                        worksheet.Cell(currentRow, 8).Value = "Check Flag";

                        foreach (var item in data)
                        {
                            currentRow++;
                            // Dynamic object usage
                            // Dapper dynamic row is IDictionary<string, object>
                            var row = (IDictionary<string, object>)item;

                            worksheet.Cell(currentRow, 1).Value = row.ContainsKey("id") ? row["id"]?.ToString() : "";
                            worksheet.Cell(currentRow, 2).Value = row.ContainsKey("assylabel") ? row["assylabel"]?.ToString() : "";
                            worksheet.Cell(currentRow, 3).Value = row.ContainsKey("sequenceno") ? row["sequenceno"]?.ToString() : "";
                            worksheet.Cell(currentRow, 4).Value = row.ContainsKey("checkuser") ? row["checkuser"]?.ToString() : "";
                            
                            if (row.ContainsKey("checkdate") && row["checkdate"] != null && DateTime.TryParse(row["checkdate"].ToString(), out DateTime cd)) {
                                worksheet.Cell(currentRow, 5).Value = cd;
                                worksheet.Cell(currentRow, 5).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                            }
                            
                            worksheet.Cell(currentRow, 6).Value = row.ContainsKey("partno") ? row["partno"]?.ToString() : "";
                            worksheet.Cell(currentRow, 7).Value = row.ContainsKey("actual_partno") ? row["actual_partno"]?.ToString() : "";
                            worksheet.Cell(currentRow, 8).Value = row.ContainsKey("checkflag") ? row["checkflag"]?.ToString() : "";
                        }
                        worksheet.Columns().AdjustToContents();
                    }
                    else
                    {
                        // EQS Mode
                        List<string> cellNosList = !string.IsNullOrEmpty(cellNos) ? cellNos.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() : null;
                        var (data, totalCount) = await _shoppingTran.GetHistory(plantno, posNosList, partNosList, cellNosList, sDateVal, eDateVal, 0, int.MaxValue);

                        var worksheet = workbook.Worksheets.Add("ScanHistory");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Tran ID";
                        worksheet.Cell(currentRow, 2).Value = "Plant No";
                        worksheet.Cell(currentRow, 3).Value = "POS No";
                        worksheet.Cell(currentRow, 4).Value = "Part No";
                        worksheet.Cell(currentRow, 5).Value = "Cell No";
                        worksheet.Cell(currentRow, 6).Value = "Finish Date";
                        worksheet.Cell(currentRow, 7).Value = "Start Check User";
                        worksheet.Cell(currentRow, 8).Value = "Seq No";
                        worksheet.Cell(currentRow, 9).Value = "Equipment No";
                        worksheet.Cell(currentRow, 10).Value = "Check Date";
                        worksheet.Cell(currentRow, 11).Value = "Check User";

                        foreach (var item in data)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = item.tranid;
                            worksheet.Cell(currentRow, 2).Value = item.plantno;
                            worksheet.Cell(currentRow, 3).Value = item.posno;
                            worksheet.Cell(currentRow, 4).Value = item.partno;
                            worksheet.Cell(currentRow, 5).Value = item.cellno;
                            worksheet.Cell(currentRow, 6).Value = item.checkingfinishdate;
                            worksheet.Cell(currentRow, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                            worksheet.Cell(currentRow, 7).Value = item.StartCheckUser;
                            worksheet.Cell(currentRow, 8).Value = item.seqno;
                            worksheet.Cell(currentRow, 9).Value = item.equipmentno;
                            worksheet.Cell(currentRow, 10).Value = item.checkingdate;
                            worksheet.Cell(currentRow, 10).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                            worksheet.Cell(currentRow, 11).Value = item.checkinguser;
                        }
                        worksheet.Columns().AdjustToContents();
                    }

                    using (var stream = new System.IO.MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        string fileName = $"History_Export_{mode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
               return StatusCode(500, ex.Message);
            }
        }
    }
}
