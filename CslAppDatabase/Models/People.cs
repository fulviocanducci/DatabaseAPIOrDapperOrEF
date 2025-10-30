using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CslAppDatabase.Models
{
    public class People
    {
        public People() { }
        public People(string name, bool active)
        {
            Name = name;
            Active = active;
        }
        public People(int id, string name, bool active)
        {
            Id = id;
            Name = name;
            Active = active;
        }
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool Active { get; set; }
    }

    public class PeopleMapping : IEntityTypeConfiguration<People>
    {
        public void Configure(EntityTypeBuilder<People> builder)
        {
            builder.ToTable("People");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("Id");
            builder.Property(c => c.Name).HasColumnName("Name").IsRequired().HasMaxLength(50);
            builder.Property(c => c.Active).HasColumnName("Active").IsRequired();
        }
    }
}