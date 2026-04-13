using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class AnswerChoiceEntity
    {
        public int Id { get; set; }
        public int AnswerId { get; set; }
        public int OptionId { get; set; }

        public virtual AnswerEntity Answer { get; set; }
        public virtual AnswerOptionEntity Option { get; set; }
    }
}
