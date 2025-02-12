using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.Services
{
    public interface IExpoPushNotificationService
    {
        Task SendPushNotificationAsync(string expoPushToken, string title, string body, object data = null);
    }
}
