using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TodoLibrary.Models;

namespace TodoLibrary.DataAccess
{
    public class TodoContext : DbContext 
    {

        public TodoContext(DbContextOptions<TodoContext> options)
              : base(options)
        {
        }

        public DbSet<TodoModel> Todos { get; set; }

    }


    
}
