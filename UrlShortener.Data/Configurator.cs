using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Data {
    public static class Configurator
    {
        private const string SegmenetLengthKey = "SegmentLength";
        private const int DefaultSegmentLength = 6;
        public static int SegmentLength
        {
            get
            {
                var segmentLength = ConfigurationManager.AppSettings[SegmenetLengthKey];
                int result;
                if (int.TryParse(segmentLength, out result))
                {
                    if (result <= 0 || result > 20)
                    {
                        return DefaultSegmentLength;
                    }
                    else
                    {
                        return result;
                    }
                }
                return DefaultSegmentLength;
            }
        }
    }
}
