using System.ComponentModel.DataAnnotations.Schema;

namespace VBSPOSS.Data.IntellectIDC.Models
{
    public class CellValue
    {
        public string Code { get; set; }
    }


    public class QueryResult
    {
        [Column("VALUE")]
        public string Value { get; set; }     // Tên property phải khớp với AS
    }

}
