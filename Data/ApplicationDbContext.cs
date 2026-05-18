using Microsoft.EntityFrameworkCore;
using MroPlan.Models;

namespace MroPlan.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Personel> Personeller { get; set; } = null!;
        public DbSet<Yetkinlik> Yetkinlikler { get; set; } = null!;
        public DbSet<YetkinlikGecmisi> YetkinlikGecmisleri { get; set; } = null!;
        public DbSet<BakimPlani> BakimPlanlari { get; set; } = null!;
        public DbSet<BakimGrubu> BakimGruplari { get; set; } = null!;
        public DbSet<ParcaSablonu> ParcaSablonlari { get; set; } = null!;
        public DbSet<BakimKontrolKaydi> BakimKontrolKayitlari { get; set; } = null!;
        public DbSet<EgitimModulu> EgitimModulleri { get; set; } = null!;
        public DbSet<PersonelEgitim> PersonelEgitimleri { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BakimKontrolKaydi ilişkileri
            modelBuilder.Entity<BakimKontrolKaydi>()
                .HasOne(k => k.BakimPlani)
                .WithMany(p => p.KontrolKayitlari)
                .HasForeignKey(k => k.BakimPlaniId);

            modelBuilder.Entity<BakimKontrolKaydi>()
                .HasOne(k => k.AtananPersonel)
                .WithMany()
                .HasForeignKey(k => k.AtananPersonelId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<BakimKontrolKaydi>()
                .HasOne(k => k.SupervisorPersonel)
                .WithMany()
                .HasForeignKey(k => k.SupervisorPersonelId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<BakimKontrolKaydi>()
                .HasOne(k => k.GelistirmePersonel)
                .WithMany()
                .HasForeignKey(k => k.GelistirmePersonelId)
                .OnDelete(DeleteBehavior.SetNull);

            // Personel → BakimGrubu FK (nullable)
            modelBuilder.Entity<Personel>()
                .HasOne(p => p.BakimGrubu)
                .WithMany()
                .HasForeignKey(p => p.BakimGrubuId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Yetkinlik → Personel FK
            modelBuilder.Entity<Yetkinlik>()
                .HasOne(y => y.Personel)
                .WithMany()
                .HasForeignKey(y => y.PersonelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Yetkinlik → ParcaSablonu FK
            modelBuilder.Entity<Yetkinlik>()
                .HasOne(y => y.ParcaSablonu)
                .WithMany()
                .HasForeignKey(y => y.ParcaSablonuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Yetkinlik: (SicilNo, ParcaPN) çifti benzersiz
            modelBuilder.Entity<Yetkinlik>()
                .HasIndex(y => new { y.SicilNo, y.ParcaPN })
                .IsUnique();

            // YetkinlikGecmisi → Yetkinlik FK
            modelBuilder.Entity<YetkinlikGecmisi>()
                .HasOne(g => g.Yetkinlik)
                .WithMany()
                .HasForeignKey(g => g.YetkinlikId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
