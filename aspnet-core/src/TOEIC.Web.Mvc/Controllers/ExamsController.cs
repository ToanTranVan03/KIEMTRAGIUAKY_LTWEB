using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TOEIC.Authorization;
using TOEIC.Controllers;
using TOEIC.Exams;
using TOEIC.Exams.Dto;

namespace TOEIC.Web.Controllers;

[AbpMvcAuthorize(PermissionNames.Pages_Exams)]
public class ExamsController : TOEICControllerBase
{
    private readonly ExamAppService _examAppService;

    public ExamsController(ExamAppService examAppService)
    {
        _examAppService = examAppService;
    }

    public async Task<ActionResult> Index()
    {
        var exams = await _examAppService.GetAllExams();
        return View(exams);
    }

    public async Task<ActionResult> Detail(int id)
    {
        var exam = await _examAppService.GetExamDetail(id);
        return View(exam);
    }

    [HttpGet]
    public ActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult> Upload(IFormFile file, string title, int? duration, string shortDescription)
    {
        ViewBag.FormTitle = title;
        ViewBag.FormDuration = duration;
        ViewBag.FormShortDescription = shortDescription;

        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "Vui lòng chọn file Word (.docx).";
            return View();
        }

        if (!file.FileName.EndsWith(".docx", System.StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.Error = "Chỉ chấp nhận file .docx.";
            return View();
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _examAppService.ImportFromWord(stream);

            if (!string.IsNullOrWhiteSpace(title) || (duration.HasValue && duration.Value > 0))
            {
                await _examAppService.UpdateExamMetadata(result.Id, title, duration);
                result = await _examAppService.GetExamDetail(result.Id);
            }

            TempData["Success"] = $"Import thanh cong de thi \"{result.Title}\" voi {result.Questions.Count} cau hoi!";
            return RedirectToAction("Detail", new { id = result.Id });
        }
        catch (Abp.UI.UserFriendlyException ex)
        {
            ViewBag.Error = ex.Message;
            return View();
        }
    }

    [HttpPost]
    public async Task<JsonResult> EditQuestion([FromBody] UpdateQuestionDto input)
    {
        await _examAppService.UpdateQuestion(input);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<JsonResult> Publish(int id)
    {
        await _examAppService.PublishExam(id);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<JsonResult> Unpublish(int id)
    {
        await _examAppService.UnpublishExam(id);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<JsonResult> Delete(int id)
    {
        await _examAppService.DeleteExam(id);
        return Json(new { success = true });
    }
}
