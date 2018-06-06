using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DominoPlanner.Core
{
    public class ProtocolDefinition
    {
        public int x;
        public int y;
        public ProtocolPositionParameters xParams;
        public ProtocolPositionParameters yParams;
        public ProtocolDefinition() { }
        public ProtocolDefinition(XElement protocol)
        {
            xParams = new ProtocolPositionParameters();
            yParams = new ProtocolPositionParameters();
            if (protocol.Attributes("xPositionMultiplier").Count() != 0 || protocol.Attributes("yPositionMultiplier").Count() != 0
                || protocol.Attributes("WidthMultiplier").Count() != 0 || protocol.Attributes("HeightMultiplier").Count() != 0)
            {
                xParams.xConstant = GetAttribute(protocol, "xConstant");
                yParams.yConstant = GetAttribute(protocol, "yConstant");
                xParams.xMultiplier = GetAttribute(protocol, "xPositionMultiplier");
                yParams.yMultiplier = GetAttribute(protocol, "yPositionMultiplier");
                xParams.WidthMultiplier = GetAttribute(protocol, "WidthMultiplier");
                yParams.HeightMultiplier = GetAttribute(protocol, "HeightMultiplier");
            }
            else
            {
                xParams.xConstant = GetAttribute(protocol, "xConstant");
                xParams.xMultiplier = GetAttribute(protocol, "xxPositionMultiplier");
                xParams.yMultiplier = GetAttribute(protocol, "xyPositionMultiplier");
                xParams.WidthMultiplier = GetAttribute(protocol, "xWidthMultiplier");
                xParams.HeightMultiplier = GetAttribute(protocol, "xHeightMultiplier");
                yParams.yConstant = GetAttribute(protocol, "yConstant");
                yParams.xMultiplier = GetAttribute(protocol, "yxPositionMultiplier");
                yParams.yMultiplier = GetAttribute(protocol, "yyPositionMultiplier");
                yParams.WidthMultiplier = GetAttribute(protocol, "yWidthMultiplier");
                yParams.HeightMultiplier = GetAttribute(protocol, "yHeightMultiplier");
            }
        }

        internal ProtocolDefinition FinalizeProtocol(int i, int j, int width, int height)
        {
            if (x == 0 && y == 0)
                return new ProtocolDefinition() { x = xParams.Finalize(i, j, width, height), y = yParams.Finalize(i, j, width, height) };
            else return this;
        }

        private static int GetAttribute(XElement element, String id)
        {
            if (element.Attribute(id) == null) return 0;
            else return int.Parse(element.Attribute(id).Value);
        }
    }
}
