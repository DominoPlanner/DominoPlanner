using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    public class ProtocolPositionParameters
    {
        public int xConstant;
        public int yConstant;
        public int xMultiplier;
        public int yMultiplier;
        public int WidthMultiplier;
        public int HeightMultiplier;
        public int Finalize(int i, int j, int width, int height)
        {
            return xConstant + yConstant + i * xMultiplier + j * yMultiplier + width * WidthMultiplier + height * HeightMultiplier;
        }

    }
}
