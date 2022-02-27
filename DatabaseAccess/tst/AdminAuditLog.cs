using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("admin_audit_log")]
    public partial class AdminAuditLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("table")]
        [StringLength(50)]
        public string Table { get; set; }
        [Required]
        [Column("table_key")]
        [StringLength(100)]
        public string TableKey { get; set; }
        [Required]
        [Column("action")]
        [StringLength(50)]
        public string Action { get; set; }
        [Required]
        [Column("old_value")]
        public string OldValue { get; set; }
        [Required]
        [Column("new_value")]
        public string NewValue { get; set; }
        [Required]
        [Column("user")]
        [StringLength(50)]
        public string User { get; set; }
        [Column("timestamp", TypeName = "timestamp with time zone")]
        public DateTime Timestamp { get; set; }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
    }
}
