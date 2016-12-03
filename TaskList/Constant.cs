using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskList
{
    class Constant
    {
        public static readonly string UNDEAL = "T2阶段处理";
        public static readonly string PROGRESSING = "进行中";
        public static readonly string FINISHED = "已归档";
        public static readonly string EXCEPTION = "工单挂起中";

        public static readonly string TYPE_CHUANSHU = "传输";
        public static readonly string TYPE_DEFAULT = "其它";

        public static readonly int WORK_PERIOD_MINUTE = 5;
        public static readonly int REFRESH_TABLE_PERIOD_MINUTE = 1;
        public static readonly int ARCH_PERIOD_DAY = 3;
        public static readonly int WARNING_PERIOD_MINUTE = 30;

        public static readonly int WAIT_TIME = 3000;

        public static readonly string DEFAULT_PASSWORD = "aaa,123";
        public static readonly string SERVER_ROOT = @"http://10.224.145.100:8080";

        public static readonly int ITEM_TaskId = 700000003;
        public static readonly int ITEM_Topic = 700000011;
        public static readonly int ITEM_Type = 800050154;
        public static readonly int ITEM_Status = 700000010;
        public static readonly int ITEM_T2time = 800050020;
        public static readonly int ITEM_HappenTime = 700000017;
        public static readonly int ITEM_Descrition = 800050009;
        public static readonly int ITEN_Location= 800059005;


    }
}
