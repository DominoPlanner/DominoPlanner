using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    [ProtoContract]
    public class ProtocolPositionParameters
    {
        [ProtoMember(1)]
        public int xConstant;
        [ProtoMember(2)]
        public int yConstant;
        [ProtoMember(3)]
        public int xMultiplier;
        [ProtoMember(4)]
        public int yMultiplier;
        [ProtoMember(5)]
        public int WidthMultiplier;
        [ProtoMember(6)]
        public int HeightMultiplier;
        public int Finalize(int i, int j, int width, int height)
        {
            return xConstant + yConstant + i * xMultiplier + j * yMultiplier + width * WidthMultiplier + height * HeightMultiplier;
        }

    }
}
