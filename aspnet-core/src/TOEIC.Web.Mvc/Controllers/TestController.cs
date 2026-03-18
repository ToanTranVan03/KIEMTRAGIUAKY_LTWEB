using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TOEIC.Authorization;
using TOEIC.Controllers;
using TOEIC.Exams;
using TOEIC.Exams.Dto;

namespace TOEIC.Web.Controllers;

[AbpMvcAuthorize(PermissionNames.Pages_Test)]
public class TestController : TOEICControllerBase
{
    private readonly ExamAppService _examAppService;

    public TestController(ExamAppService examAppService)
    {
        _examAppService = examAppService;
    }

    public async Task<ActionResult> Index()
    {
        var exams = await _examAppService.GetPublishedExams();
        return View(exams);
    }

    public async Task<ActionResult> Take(int id)
    {
        var exam = await _examAppService.GetExamForTaking(id);
        ViewBag.StartTime = DateTime.Now;
        return View(exam);
    }

    [HttpPost]
    public async Task<ActionResult> Submit([FromBody] SubmitExamDto input)
    {
        var startTimeStr = Request.Headers["X-Start-Time"].ToString();
        DateTime? startTime = null;
        if (DateTimeOffset.TryParse(startTimeStr, out var startTimeOffset))
        {
            startTime = startTimeOffset.LocalDateTime;
        }
        else if (DateTime.TryParse(startTimeStr, out var parsedStartTime))
        {
            startTime = parsedStartTime;
        }

        var result = await _examAppService.SubmitExam(input, startTime);
        return Json(new { success = true, resultId = result.Id });
    }

    public async Task<ActionResult> Result(int id)
    {
        var result = await _examAppService.GetExamResult(id);
        return View(result);
    }

    [AbpMvcAuthorize(PermissionNames.Pages_Test_History)]
    public async Task<ActionResult> History()
    {
        var results = await _examAppService.GetHistoryResults();
        ViewBag.CanManageExamHistory = PermissionChecker.IsGranted(PermissionNames.Pages_Exams);
        return View(results);
    }
}
