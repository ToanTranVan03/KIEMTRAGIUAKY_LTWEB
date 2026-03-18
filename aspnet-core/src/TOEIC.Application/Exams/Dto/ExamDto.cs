using Abp.Application.Services.Dto;
using TOEIC.AppEntities;
using System;
using System.Collections.Generic;

namespace TOEIC.Exams.Dto
{
    public class ExamDto : EntityDto<int>
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public ExamStatus Status { get; set; }
        public long? CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public int QuestionCount { get; set; }
    }

    public class ExamDetailDto : EntityDto<int>
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public ExamStatus Status { get; set; }
        public long? CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public List<PassageDto> Passages { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }

    public class PassageDto : EntityDto<int>
    {
        public string Content { get; set; }
        public int ExamId { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }

    public class QuestionDto : EntityDto<int>
    {
        public int PartNumber { get; set; }
        public int QuestionNo { get; set; }
        public string Content { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsShuffle { get; set; }
        public int ExamId { get; set; }
        public int? PassageId { get; set; }
    }

    public class CreateExamDto
    {
        public string Title { get; set; }
        public int Duration { get; set; }
    }

    public class UpdateQuestionDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsShuffle { get; set; }
    }

    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }
        public string SelectedOption { get; set; }
    }

    public class SubmitExamDto
    {
        public int ExamId { get; set; }
        public List<SubmitAnswerDto> Answers { get; set; }
        public int? ElapsedSeconds { get; set; }
    }

    public class ExamResultDto : EntityDto<int>
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ExpectedScore { get; set; }
        public int TotalCorrect { get; set; }
        public int TotalWrong { get; set; }
        public int TotalSkipped { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public List<StudentAnswerDto> StudentAnswers { get; set; }
    }

    public class StudentAnswerDto : EntityDto<int>
    {
        public string SelectedOption { get; set; }
        public bool IsCorrect { get; set; }
        public int QuestionId { get; set; }
        public QuestionDto Question { get; set; }
    }
}
