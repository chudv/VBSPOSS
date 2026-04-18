using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IAttachedFileService
    {
        Task<List<AttachedFileInfoView>> GetttachedFileSync(string pPosCode,
       string pFileType);

        Task<string> UploadFileAsync(IFormFile file, string description, string createdBy);
    }
}
