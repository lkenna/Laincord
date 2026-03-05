using Laincord.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laincord.Enums
{
    public enum BasicTitlebarSetting
    {
        [Display("Automatically determine based on my theme")]
        Automatic,

        [Display("Always use native OS titlebar")]
        AlwaysNative,

        [Display("Always use custom basic-theme-fallback titlebar")]
        AlwaysNonNative,
    }
}
