using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScanInteropBuilder
{
    public class MagTekScanner
    {
        public MagTekScanner()
        {
            MagTekImageScanInterop.Init();
        }

    }
}
