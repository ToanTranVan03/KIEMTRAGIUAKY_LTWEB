using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOEIC.AppEntities
{
    [Table("AppStudentAnswers")]
    public class StudentAnswer : Entity<int>
    {
        public string SelectedOption { get; set; } // A, B, C, D or Null
        public bool IsCorrect { get; set; }

        public int ExamResultId { get; set; }
        [ForeignKey("ExamResultId")]
        public virtual ExamResult ExamResult { get; set; }

        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
    }
}
