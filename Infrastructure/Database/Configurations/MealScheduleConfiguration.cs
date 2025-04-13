using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class MealScheduleConfiguration : IEntityTypeConfiguration<MealSchedule>
{
    public void Configure(EntityTypeBuilder<MealSchedule> builder)
    {
        builder.ToTable("MealSchedules");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ShoppingListId).IsRequired();
        
        builder.Property(x => x.DayOfWeek)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(x => x.MealType)
            .IsRequired()
            .HasConversion<int>();
            
        builder.Property(x => x.MealName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Time);

        builder.HasOne(x => x.ShoppingList)
            .WithMany(x => x.MealSchedules)
            .HasForeignKey(x => x.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 