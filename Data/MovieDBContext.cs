﻿using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Entities;

namespace MovieAPIDemo.Data
{
    public class MovieDBContext: DbContext
    {
        public MovieDBContext(DbContextOptions<MovieDBContext> options):base(options)
        {
            
        }

        public DbSet<Movie>Movie { get; set; }
        public DbSet<Person> Person { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
