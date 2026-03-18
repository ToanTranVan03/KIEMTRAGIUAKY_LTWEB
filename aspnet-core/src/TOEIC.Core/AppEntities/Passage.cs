using Abp.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOEIC.AppEntities
{
    [Table("AppPassages")]
    public class Passage : Entity<int>
    {
        public string Content { get; set; }

        public int ExamId { get; set; }
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        public virtual ICollection<Question> Questions { get; set; }
    }
}
