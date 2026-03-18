using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TOEIC.Authorization.Users;

namespace TOEIC.AppEntities
{
    [Table("AppExamResults")]
    public class ExamResult : Entity<int>
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ExpectedScore { get; set; } // /495
        public int TotalCorrect { get; set; }
        public int TotalWrong { get; set; }
        public int TotalSkipped { get; set; }

        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int ExamId { get; set; }
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; }
    }
}
