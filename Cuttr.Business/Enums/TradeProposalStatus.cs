using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cuttr.Business.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TradeProposalStatus
    {
        Pending, Accepted, Rejected, Completed
    }
}
