namespace VBSPOSS.Data.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Area { get; set; }
        public string ImageClass { get; set; }
        public string ActiveLi { get; set; }
        public int Status { get; set; }
        public int ParentId { get; set; }
        public bool IsParent { get; set; }
        public bool? HasChild { get; set; }
        public bool? IsSelect { get; set; }
        public int? GroupOrder { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
