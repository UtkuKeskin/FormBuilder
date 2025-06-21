using FormBuilder.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Data.Seed
{
    public static class TopicSeeder
    {
        public static void SeedTopics(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Topic>().HasData(
                new Topic { Id = 1, Name = "Education" },
                new Topic { Id = 2, Name = "Quiz" },
                new Topic { Id = 3, Name = "Poll" },
                new Topic { Id = 4, Name = "Survey" },
                new Topic { Id = 5, Name = "Other" }
            );
        }
    }
}
