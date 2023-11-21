using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpsClient.Utils
{
    public interface IMySettings
    {
        string serverHostname { get; set; }
        string serverPort { get; set; }
    }
}
