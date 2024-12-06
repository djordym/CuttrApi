using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Interfaces.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
