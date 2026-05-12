using VBSPOSS.ViewModels;

namespace VBSPOSS.Models
{
    public class SaveTransferVillageRequest
    {
        public long MasterId { get; set; }

        public List<TransferDataPosDetailViewModel> Details { get; set; }
    }
}
