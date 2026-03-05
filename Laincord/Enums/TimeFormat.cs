using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Laincord.Attributes;

namespace Laincord.Enums
{
    public enum TimeFormat
    {
        [Display("24-Hour Clock")]
        TwentyFourHour,

        [Display("12-Hour Clock (AM/PM)")]
        TwelveHour
    }
}
