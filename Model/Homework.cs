using FlytDex.Domain.Model.FlytDex.Links;
using FlytDex.Shared.Enums;
using System;
using System.Collections.Generic;

namespace FlytDex.Domain.Model.FlytDex
{
    public class Homework : SchoolEntity
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public HomeworkStatus Status { get; set; }

        public DateTime DueDateTime { get; set; }

        public DateTime SetDateTime { get; set; }

        public Guid? HomeworkTemplateId { get; set; }
        public virtual HomeworkTemplate HomeworkTemplate { get; set; }

        public Guid LessonId { get; set; }
        public virtual Lesson Lesson { get; set; }

		public virtual ICollection<LinkHomeworkResource> LinkHomeworkResources { get; set; }

		public virtual ICollection<LinkStudentHomework> LinkStudentHomeworks { get; set; }
	}
}
