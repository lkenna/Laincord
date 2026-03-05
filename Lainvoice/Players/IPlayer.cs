using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lainvoice.Players
{
    public interface IPlayer
    {
        public abstract void AddSamples(byte[] pcmData, int pcmLength, uint ssrc);
    }
}
