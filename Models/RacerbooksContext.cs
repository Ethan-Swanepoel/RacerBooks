using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RacerBooks.Models;

public partial class RacerbooksContext : DbContext
{
    public RacerbooksContext()
    {
    }

    public RacerbooksContext(DbContextOptions<RacerbooksContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => new { e.Email, e.ItemCode }).HasName("PK__Cart__0A3DC5CB5B3D30C8");

            entity.ToTable("Cart");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ItemCode)
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.HasOne(d => d.EmailNavigation).WithMany(p => p.Carts)
                .HasForeignKey(d => d.Email)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__Email__08B54D69");

            entity.HasOne(d => d.ItemCodeNavigation).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ItemCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__ItemCode__09A971A2");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemCode).HasName("PK__Item__3ECC0FEB501462B0");

            entity.ToTable("Item");

            entity.Property(e => e.ItemCode)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.ItemDetails)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ItemPrice).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("PK__Users__A9D1053547513D2C");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FirebaseUuid)
                .HasMaxLength(512)
                .IsUnicode(false)
                .HasColumnName("FirebaseUUID");
            entity.Property(e => e.UserRole)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
