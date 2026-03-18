using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using TOEIC.AppEntities;
using TOEIC.Authorization;
using TOEIC.Exams.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TOEIC.Exams
{
    public class ExamAppService : ApplicationService
    {
        private readonly IRepository<Exam, int> _examRepo;
        private readonly IRepository<Passage, int> _passageRepo;
        private readonly IRepository<Question, int> _questionRepo;
        private readonly IRepository<ExamResult, int> _examResultRepo;
        private readonly IRepository<StudentAnswer, int> _studentAnswerRepo;

        public ExamAppService(
            IRepository<Exam, int> examRepo,
            IRepository<Passage, int> passageRepo,
            IRepository<Question, int> questionRepo,
            IRepository<ExamResult, int> examResultRepo,
            IRepository<StudentAnswer, int> studentAnswerRepo)
        {
            _examRepo = examRepo;
            _passageRepo = passageRepo;
            _questionRepo = questionRepo;
            _examResultRepo = examResultRepo;
            _studentAnswerRepo = studentAnswerRepo;
        }

        // ========== TEACHER FUNCTIONS ==========

        [AbpAuthorize(PermissionNames.Pages_Exams)]
        public async Task<List<ExamDto>> GetAllExams()
        {
            var exams = await _examRepo.GetAll()
                .Include(e => e.CreatorUser)
                .Include(e => e.Questions)
                .OrderByDescending(e => e.Id)
                .ToListAsync();

            return exams.Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Duration = e.Duration,
                Status = e.Status,
                CreatedBy = e.CreatedBy,
                CreatorName = e.CreatorUser?.FullName ?? "N/A",
                QuestionCount = e.Questions?.Count ?? 0
            }).ToList();
        }

        [AbpAuthorize(PermissionNames.Pages_Exams)]
        public async Task<ExamDetailDto> GetExamDetail(int examId)
        {
            var exam = await _examRepo.GetAll()
                .Include(e => e.CreatorUser)
                .Include(e => e.Passages).ThenInclude(p => p.Questions)
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) throw new UserFriendlyException("Không tìm thấy đề thi.");

            return new ExamDetailDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Duration = exam.Duration,
                Status = exam.Status,
                CreatedBy = exam.CreatedBy,
                CreatorName = exam.CreatorUser?.FullName ?? "N/A",
                Passages = exam.Passages?.Select(p => new PassageDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ExamId = p.ExamId,
                    Questions = p.Questions?.OrderBy(q => q.QuestionNo).Select(MapQuestion).ToList()
                }).ToList() ?? new(),
                Questions = exam.Questions?.OrderBy(q => q.QuestionNo).Select(MapQuestion).ToList() ?? new()
            };
        }

        /// <summary>
        /// Upload and parse a Word file, then import exam data into DB.
        /// Rollback on error.
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_Exams)]
        public async Task<ExamDetailDto> ImportFromWord(Stream fileStream)
        {
            var parser = new WordExamParser();
            var parseResult = parser.Parse(fileStream);

            if (parseResult.Errors.Any())
            {
                throw new UserFriendlyException(
                    "File Word sai định dạng!\n" + string.Join("\n", parseResult.Errors));
            }

            // Create Exam
            var exam = new Exam
            {
                Title = parseResult.Title,
                Duration = parseResult.Duration,
                Status = ExamStatus.Draft,
                CreatedBy = AbpSession.UserId
            };
            await _examRepo.InsertAndGetIdAsync(exam);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Create Passages
            var passageMap = new Dictionary<int, int>(); // temp index -> real Id
            for (int i = 0; i < parseResult.Passages.Count; i++)
            {
                var p = parseResult.Passages[i];
                p.ExamId = exam.Id;
                var newId = await _passageRepo.InsertAndGetIdAsync(p);
                passageMap[-(i + 1)] = newId;
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            // Create Questions
            foreach (var q in parseResult.Questions)
            {
                q.ExamId = exam.Id;
                if (q.PassageId.HasValue && q.PassageId.Value < 0 && passageMap.ContainsKey(q.PassageId.Value))
                {
                    q.PassageId = passageMap[q.PassageId.Value];
                }
                else
                {
                    q.PassageId = null;
                }
                await _questionRepo.InsertAsync(q);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetExamDetail(exam.Id);
        }

        [AbpAuthorize(PermissionNames.Pages_Exams)]
        public async Task UpdateExamMetadata(int examId, string title, int? duration)
        {
            var exam = await _examRepo.GetAsync(examId);

            if (!string.IsNullOrWhiteSpace(title))
            {
                exam.Title = title.Trim();
            }

            if (duration.HasValue && duration.Value > 0)
            {
                exam.Duration = duration.Value;
            }

            await _examRepo.UpdateAsync(exam);
        }

        [AbpAuthorize(PermissionNames.Pages_Exams)]
        public async Task UpdateQuestion(UpdateQuestionDto input)
        {
            var q = await _questionRepo.GetAsync(input.Id);
            q.Content = input.Content;
            q.OptionA = input.OptionA;
            q.OptionB = input.OptionB;
            q.OptionC = input.OptionC;
            q.OptionD = input.OptionD;
            q.CorrectAnswer = input.CorrectAnswer;
            q.IsShuffle = input.IsShuffle;
            await _questionRepo.UpdateAsync(q);
        }

        [AbpAuthorize(PermissionNames.Pages_Exams)]
        public async Task PublishExam(int examId)
        {
            var exam = await _examRepo.GetAsync(examId);
            exam.Status = ExamStatus.Published;
            await _examRepo.UpdateAsync(exam);
        }

        [AbpAuthorize(PermissionNames.Pages_Exams)]
        public async Task UnpublishExam(int examId)
        {
            var exam = await _examRepo.GetAsync(examId);
            exam.Status = ExamStatus.Draft;
            await _examRepo.UpdateAsync(exam);
        }

        [AbpAuthorize(PermissionNames.Pages_Exams)]
        public async Task DeleteExam(int examId)
        {
            // Delete student answers related
            var resultIds = await _examResultRepo.GetAll()
                .Where(r => r.ExamId == examId).Select(r => r.Id).ToListAsync();
            foreach (var rid in resultIds)
            {
                await _studentAnswerRepo.DeleteAsync(sa => sa.ExamResultId == rid);
            }
            await _examResultRepo.DeleteAsync(r => r.ExamId == examId);
            await _questionRepo.DeleteAsync(q => q.ExamId == examId);
            await _passageRepo.DeleteAsync(p => p.ExamId == examId);
            await _examRepo.DeleteAsync(examId);
        }

        // ========== STUDENT FUNCTIONS ==========

        [AbpAuthorize(PermissionNames.Pages_Test)]
        public async Task<List<ExamDto>> GetPublishedExams()
        {
            var exams = await _examRepo.GetAll()
                .Where(e => e.Status == ExamStatus.Published)
                .Include(e => e.Questions)
                .OrderByDescending(e => e.Id)
                .ToListAsync();

            return exams.Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Duration = e.Duration,
                Status = e.Status,
                QuestionCount = e.Questions?.Count ?? 0
            }).ToList();
        }

        /// <summary>
        /// Get exam questions for the test-taking interface.
        /// Shuffle answers if IsShuffle = true.
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_Test)]
        public async Task<ExamDetailDto> GetExamForTaking(int examId)
        {
            var detail = await GetExamDetail(examId);

            if (detail.Status != ExamStatus.Published)
                throw new UserFriendlyException("Đề thi chưa được xuất bản.");

            // Shuffle options if needed
            var rnd = new Random();
            foreach (var q in detail.Questions)
            {
                if (q.IsShuffle)
                {
                    var options = new List<(string key, string val)>
                    {
                        ("A", q.OptionA), ("B", q.OptionB), ("C", q.OptionC), ("D", q.OptionD)
                    };
                    var shuffled = options.OrderBy(_ => rnd.Next()).ToList();
                    q.OptionA = shuffled[0].val;
                    q.OptionB = shuffled[1].val;
                    q.OptionC = shuffled[2].val;
                    q.OptionD = shuffled[3].val;
                }
                // Remove correct answer from student view
                q.CorrectAnswer = null;
            }

            return detail;
        }

        /// <summary>
        /// Submit exam answers, calculate results, and save to DB.
        /// </summary>
        [AbpAuthorize(PermissionNames.Pages_Test)]
        public async Task<ExamResultDto> SubmitExam(SubmitExamDto input, DateTime? startTime)
        {
            var exam = await _examRepo.GetAll()
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == input.ExamId);

            if (exam == null) throw new UserFriendlyException("Không tìm thấy đề thi.");

            var allQuestions = exam.Questions.ToList();
            int totalCorrect = 0, totalWrong = 0, totalSkipped = 0;
            var answers = new List<StudentAnswer>();

            foreach (var q in allQuestions)
            {
                var studentAns = input.Answers?.FirstOrDefault(a => a.QuestionId == q.Id);
                var selected = studentAns?.SelectedOption;
                bool isCorrect = false;

                if (string.IsNullOrEmpty(selected))
                {
                    totalSkipped++;
                }
                else if (selected.Equals(q.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    totalCorrect++;
                    isCorrect = true;
                }
                else
                {
                    totalWrong++;
                }

                answers.Add(new StudentAnswer
                {
                    QuestionId = q.Id,
                    SelectedOption = selected,
                    IsCorrect = isCorrect
                });
            }

            // Calculate score (simple: each correct = 5 points, max 495)
            int score = Math.Min(totalCorrect * 5, 495);
            var submitTime = DateTime.Now;
            var maxDurationSeconds = Math.Max(exam.Duration, 0) * 60;
            DateTime actualStartTime;

            if (input.ElapsedSeconds.HasValue && input.ElapsedSeconds.Value >= 0)
            {
                var elapsedSeconds = input.ElapsedSeconds.Value;
                if (maxDurationSeconds > 0)
                {
                    elapsedSeconds = Math.Min(elapsedSeconds, maxDurationSeconds);
                }

                actualStartTime = submitTime.AddSeconds(-elapsedSeconds);
            }
            else if (startTime.HasValue)
            {
                actualStartTime = startTime.Value > submitTime ? submitTime : startTime.Value;
            }
            else if (maxDurationSeconds > 0)
            {
                actualStartTime = submitTime.AddSeconds(-maxDurationSeconds);
            }
            else
            {
                actualStartTime = submitTime;
            }

            var examResult = new ExamResult
            {
                ExamId = exam.Id,
                UserId = AbpSession.UserId.Value,
                StartTime = actualStartTime,
                EndTime = submitTime,
                TotalCorrect = totalCorrect,
                TotalWrong = totalWrong,
                TotalSkipped = totalSkipped,
                ExpectedScore = score
            };

            var resultId = await _examResultRepo.InsertAndGetIdAsync(examResult);
            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var sa in answers)
            {
                sa.ExamResultId = resultId;
                await _studentAnswerRepo.InsertAsync(sa);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetExamResult(resultId);
        }

        [AbpAuthorize(PermissionNames.Pages_Test)]
        public async Task<ExamResultDto> GetExamResult(int resultId)
        {
            var canManageExamHistory = CanManageExamHistory();
            var canViewCrossTenantHistory = canManageExamHistory && !AbpSession.TenantId.HasValue;

            ExamResult r;
            if (canViewCrossTenantHistory)
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                {
                    r = await _examResultRepo.GetAll()
                        .Include(x => x.User)
                        .Include(x => x.Exam)
                        .Include(x => x.StudentAnswers).ThenInclude(sa => sa.Question)
                        .FirstOrDefaultAsync(x => x.Id == resultId);
                }
            }
            else
            {
                r = await _examResultRepo.GetAll()
                    .Include(x => x.User)
                    .Include(x => x.Exam)
                    .Include(x => x.StudentAnswers).ThenInclude(sa => sa.Question)
                    .FirstOrDefaultAsync(x => x.Id == resultId);
            }

            if (r == null) throw new UserFriendlyException("Không tìm thấy kết quả.");

            if (!canManageExamHistory && (!AbpSession.UserId.HasValue || r.UserId != AbpSession.UserId.Value))
            {
                throw new UserFriendlyException("Ban khong co quyen xem ket qua nay.");
            }

            return new ExamResultDto
            {
                Id = r.Id,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                ExpectedScore = r.ExpectedScore,
                TotalCorrect = r.TotalCorrect,
                TotalWrong = r.TotalWrong,
                TotalSkipped = r.TotalSkipped,
                UserId = r.UserId,
                UserName = r.User?.FullName ?? "N/A",
                ExamId = r.ExamId,
                ExamTitle = r.Exam?.Title ?? "N/A",
                StudentAnswers = r.StudentAnswers?.OrderBy(sa => sa.Question?.QuestionNo).Select(sa => new StudentAnswerDto
                {
                    Id = sa.Id,
                    SelectedOption = sa.SelectedOption,
                    IsCorrect = sa.IsCorrect,
                    QuestionId = sa.QuestionId,
                    Question = sa.Question != null ? MapQuestion(sa.Question) : null
                }).ToList() ?? new()
            };
        }

        [AbpAuthorize(PermissionNames.Pages_Test_History)]
        public async Task<List<ExamResultDto>> GetHistoryResults()
        {
            var canManageExamHistory = CanManageExamHistory();
            var canViewCrossTenantHistory = canManageExamHistory && !AbpSession.TenantId.HasValue;

            List<ExamResult> results;
            if (canViewCrossTenantHistory)
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                {
                    results = await _examResultRepo.GetAll()
                        .Include(r => r.User)
                        .Include(r => r.Exam)
                        .OrderByDescending(r => r.Id)
                        .ToListAsync();
                }
            }
            else
            {
                IQueryable<ExamResult> query = _examResultRepo.GetAll()
                    .Include(r => r.User)
                    .Include(r => r.Exam);

                if (!canManageExamHistory)
                {
                    if (!AbpSession.UserId.HasValue)
                    {
                        throw new UserFriendlyException("Vui long dang nhap de xem lich su thi.");
                    }

                    query = query.Where(r => r.UserId == AbpSession.UserId.Value);
                }

                results = await query
                    .OrderByDescending(r => r.Id)
                    .ToListAsync();
            }

            return results.Select(r => new ExamResultDto
            {
                Id = r.Id,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                ExpectedScore = r.ExpectedScore,
                TotalCorrect = r.TotalCorrect,
                TotalWrong = r.TotalWrong,
                TotalSkipped = r.TotalSkipped,
                UserId = r.UserId,
                UserName = r.User?.FullName ?? r.User?.UserName ?? "N/A",
                ExamId = r.ExamId,
                ExamTitle = r.Exam?.Title ?? "N/A"
            }).ToList();
        }

        private bool CanManageExamHistory()
        {
            return PermissionChecker.IsGranted(PermissionNames.Pages_Exams);
        }

        private QuestionDto MapQuestion(Question q)
        {
            return new QuestionDto
            {
                Id = q.Id,
                PartNumber = q.PartNumber,
                QuestionNo = q.QuestionNo,
                Content = q.Content,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer,
                IsShuffle = q.IsShuffle,
                ExamId = q.ExamId,
                PassageId = q.PassageId
            };
        }
    }
}
