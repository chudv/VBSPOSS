using VBSPOSS.ViewModels;

namespace VBSPOSS.ViewModels
{
    public class TransferDataPosSaveModel
    {
        public TransferDataPosMasterViewModel Master
        {
            get;
            set;
        }

        public List<
            TransferDataPosDetailViewModel>
            Details
        {
            get;
            set;
        }
    }
}
