using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoLibrary.Models
{
   public  class TodoModel
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Task { get; set; }
        public Guid AssignedTo { get; set; }
        public bool IsComplete { get; set; }
    }
}
