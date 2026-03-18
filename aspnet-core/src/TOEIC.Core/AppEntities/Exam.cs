using Abp.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TOEIC.Authorization.Users;

namespace TOEIC.AppEntities
{
    public enum ExamStatus
    {
        Draft = 0,
        Published = 1
    }

    [Table("AppExams")]
    public class Exam : Entity<int>
    {
        public string Title { get; set; }
        public int Duration { get; set; } // minutes
        public ExamStatus Status { get; set; }

        public long? CreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual User CreatorUser { get; set; }

        public virtual ICollection<Passage> Passages { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<ExamResult> ExamResults { get; set; }
    }
}
