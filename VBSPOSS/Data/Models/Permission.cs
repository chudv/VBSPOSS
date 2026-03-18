using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VBSPOSS.Data.Models
{
    public class Permission
    {
        public int Id { get; set; } 
        public string Code { get; set; }    
        public string Description { get; set; }
        public int Status { get; set; } 
    }
}
