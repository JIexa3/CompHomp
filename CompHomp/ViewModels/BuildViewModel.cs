using System;
using System.Collections.Generic;

namespace CompHomp.ViewModels
{
    public class BuildViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImagePath { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public string AuthorName { get; set; }
        public List<BuildCommentViewModel> Comments { get; set; }
    }
}
