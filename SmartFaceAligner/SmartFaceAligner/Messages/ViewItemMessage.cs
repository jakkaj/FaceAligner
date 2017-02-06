using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlingCore.Portable.Messages.XamlingMessenger;

namespace SmartFaceAligner.Messages
{
    public class ViewItemMessage : XMessage
    {
        public double Offset { get; }
        public double Width { get; }

        public ViewItemMessage(double offset, double width)
        {
            Offset = offset;
            Width = width;
        }
    }
}
