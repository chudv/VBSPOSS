using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.ViewModels
{
    public class ListOfValueViewModel
    {
        public int OrderNoAll { get; set; }       //Thêm

        public int Id { get; set; }

        public int ParentId { get; set; }

        public int ParentId_01 { get; set; }      //Thêm

        public int ParentId_02 { get; set; }      //Thêm

        public int ParentId_03 { get; set; }      //Thêm

        public int ParentId_04 { get; set; }      //Thêm

        public int ParentId_05 { get; set; }      //Thêm

        public string ParentCode { get; set; }

        public string ParentText { get; set; }      //Thêm

        public int OrderNo { get; set; }

        public string OrderNoText { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public int Status { get; set; }
        
        public string StatusText { get; set; }        //Thêm

        public string CodeOfLovUsed { get; set; }

        public string CodeOfLovUsedText { get; set; }        //Thêm

        public string Notes { get; set; }

        public string LevelCode { get; set; }

        public int SumLevelFlag { get; set; }

        public int PrintType { get; set; }

        public string PrintTypeText { get; set; }        //Thêm

        public int EditableFlag { get; set; }

        public int CategoryLevel { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public List<String> CodeOfLovUsedList { get; set; }       //Thêm

        public string StrTmp01 { get; set; }      //Thêm

        public string StrTmp02 { get; set; }      //Thêm
    }
}
