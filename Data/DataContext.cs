using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login.Models;
using Microsoft.EntityFrameworkCore;

namespace Login.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        //public DbSet<Model> Models => Set<Model>();
        public DbSet<User> Users => Set<User>();
    }
}