﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Context_Layer.Models
{
    [Table("Emp_Vacation_Credit")]
    public partial class EmpVacationCredit
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("empId")]
        public int? EmpId { get; set; }
        [Column("vacationTypeId")]
        public int? VacationTypeId { get; set; }
        [Column("currentCredit")]
        public int? CurrentCredit { get; set; }

        [ForeignKey("EmpId")]
        [InverseProperty("EmpVacationCredits")]
        public virtual User Emp { get; set; }
        [ForeignKey("VacationTypeId")]
        [InverseProperty("EmpVacationCredits")]
        public virtual VacationType VacationType { get; set; }
    }
}