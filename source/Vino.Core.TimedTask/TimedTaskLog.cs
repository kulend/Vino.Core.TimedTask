using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Vino.Core.TimedTask
{
    public class TimedTaskLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { set; get; }

        [MaxLength(32)]
        public string TaskId { set; get; }

        public DateTime BeginTime { set; get; }

        public DateTime EndTime { set; get; }

        public long Duration { set; get; }

        [MaxLength(20)]
        public string Result { set; get; }
    }
}
