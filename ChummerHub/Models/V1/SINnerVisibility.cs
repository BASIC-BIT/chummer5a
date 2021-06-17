using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChummerHub.Models.V1
{
    public class SINnerVisibility
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        public bool IsPublic { get; set; }

        public bool IsGroupVisible { get; set; }

        public List<SINnerUserRight> UserRights { get; set; }

        public SINnerVisibility()
        {
            UserRights = new List<SINnerUserRight>();
            this.IsGroupVisible = true;
        }
    }
}
