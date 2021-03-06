﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Vino.Core.TimedTask
{
    public class TimedTask
    {
        [MaxLength(32)]
        public string Id { set; get; }

        [MaxLength(64)]
        public string Name { set; get; }

        [MaxLength(256)]
        public string Identifier { set; get; }

        public bool IsEnabled { get; set; } = true;

        public bool AutoReset { set; get; } = true;

        public int Interval { get; set; } = 1000 * 60; // 1分钟

        public DateTime BeginTime { set; get; }

        public DateTime ExpireTime { set; get; }

        public int RunTimes { set; get; }
    }
}
