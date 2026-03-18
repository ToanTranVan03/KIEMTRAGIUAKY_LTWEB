using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOEIC.AppEntities
{
    [Table("AppQuestions")]
    public class Question : Entity<int>
    {
        public int PartNumber { get; set; } // 5, 6, 7
        public int QuestionNo { get; set; } // 101...
        public string Content { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; } // A, B, C, D
        public bool IsShuffle { get; set; }

        public int ExamId { get; set; }
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        public int? PassageId { get; set; }
        [ForeignKey("PassageId")]
        public virtual Passage Passage { get; set; }
    }
}
