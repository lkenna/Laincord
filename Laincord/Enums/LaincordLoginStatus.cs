using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laincord.Enums
{
    public enum LaincordLoginStatus
    {
        Success,
        UnknownFailure,
        Unauthorized,
        ServerError,
        BadRequest,
    }
}
