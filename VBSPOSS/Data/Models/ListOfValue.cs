using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Data.Models
{
    #region ---Model ListOfValue - Danh mục chung ---
    public class ListOfValue
    {
        [Column("Id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("ParentId")]
        public int ParentId { get; set; }

        [Column("ParentCode")]
        public string ParentCode { get; set; }

        [Column("OrderNo")]
        public int OrderNo { get; set; }

        [Column("OrderNoText")]
        public string OrderNoText { get; set; }
        
        [Column("Code")]
        public string Code { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("ShortName")]
        public string ShortName { get; set; }

        [Column("Status")]
        public int Status { get; set; }

        [Column("CodeOfLovUsed")]
        public string? CodeOfLovUsed { get; set; }

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("LevelCode")]
        public string LevelCode { get; set; }

        [Column("SumLevelFlag")]
        public int SumLevelFlag { get; set; }

        [Column("PrintType")]
        public int PrintType { get; set; }

        [Column("EditableFlag")]
        public int EditableFlag { get; set; }

        [Column("CategoryLevel")]
        public int CategoryLevel { get; set; }

        [Column("CreatedBy")]
        public string? CreatedBy { get; set; }

        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [Column("ModifiedBy")]
        public string? ModifiedBy { get; set; }

        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }

    public class CellValue
    {
        public string Code { get; set; }
    }
    #endregion

    
}
