using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class ConnectionRequestEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("FromUser")]
        public int FromUserId { get; set; }

        [Required]
        [ForeignKey("ToUser")]
        public int ToUserId { get; set; }

        [ForeignKey("Organization")]
        public int? OrganizationId { get; set; }

        [Required]
        [StringLength(20)]
        public string RequestType { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ProcessedDate { get; set; }

        public virtual UserEntity FromUser { get; set; }
        public virtual UserEntity ToUser { get; set; }
        public virtual OrganizationEntity Organization { get; set; }
    }
}
