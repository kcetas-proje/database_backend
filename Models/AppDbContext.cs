using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KcetasAboneApi.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public long? MevcutKullaniciId { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<EndeksOkuma> EndeksOkumas { get; set; }
    public virtual DbSet<EntegrasyonOutbox> EntegrasyonOutboxes { get; set; }
    public virtual DbSet<Fatura> Faturas { get; set; }
    public virtual DbSet<FaturaKalemi> FaturaKalemis { get; set; }
    public virtual DbSet<Il> Ils { get; set; }
    public virtual DbSet<Ilce> Ilces { get; set; }
    public virtual DbSet<IsEmirleri> IsEmirleris { get; set; }
    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }
    public virtual DbSet<Roller> Rollers { get; set; }
    public virtual DbSet<Sayaclar> Sayaclars { get; set; }
    public virtual DbSet<Sozlesmeler> Sozlesmelers { get; set; }
    public virtual DbSet<TuketimNoktasi> TuketimNoktasis { get; set; }
    public virtual DbSet<Aboneler> Abonelers { get; set; }
    public virtual DbSet<Tarifeler> Tarifelers { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var degisenler = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        var auditListesi = new List<(EntityEntry Entry, AuditLog Log)>();

        foreach (var entry in degisenler)
        {
            var auditLog = new AuditLog
            {
                VarlikTipi = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                IslemTipi = entry.State switch
                {
                    EntityState.Added => "EKLEME",
                    EntityState.Modified => "GUNCELLEME",
                    EntityState.Deleted => "SILME",
                    _ => "DIGER"
                },
                IslemZamani = DateTime.UtcNow,
                KullaniciId = this.MevcutKullaniciId ?? 1,
                IslemGerekcesi = "Sistem Otomasyon İşlemi"
            };

            if (entry.State == EntityState.Added)
            {
                auditLog.YeniDeger = System.Text.Json.JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }
            else if (entry.State == EntityState.Deleted)
            {
                auditLog.EskiDeger = System.Text.Json.JsonSerializer.Serialize(entry.OriginalValues.ToObject());
            }
            else if (entry.State == EntityState.Modified)
            {
                auditLog.EskiDeger = System.Text.Json.JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                auditLog.YeniDeger = System.Text.Json.JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }

            auditListesi.Add((entry, auditLog));
        }

        var sonuc = await base.SaveChangesAsync(cancellationToken);

        foreach (var item in auditListesi)
        {
            var pkName = item.Entry.Metadata.FindPrimaryKey()?.Properties.Select(p => p.Name).FirstOrDefault();
            
            if (pkName != null)
            {
                item.Log.VarlikId = Convert.ToInt64(item.Entry.Property(pkName).CurrentValue);
            }
            
            AuditLogs.Add(item.Log);
        }

        if (auditListesi.Any())
        {
            await base.SaveChangesAsync(cancellationToken);
        }

        return sonuc;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("pk_audit_log");
            entity.ToTable("audit_log");
            entity.HasIndex(e => new { e.VarlikTipi, e.VarlikId }, "idx_audit_log_varlik");
            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.EskiDeger).HasColumnType("jsonb").HasColumnName("eski_deger");
            entity.Property(e => e.IslemGerekcesi).HasMaxLength(255).HasColumnName("islem_gerekcesi");
            entity.Property(e => e.IslemTipi).HasMaxLength(20).HasColumnName("islem_tipi");
            entity.Property(e => e.IslemZamani).HasDefaultValueSql("now()").HasColumnName("islem_zamani");
            entity.Property(e => e.KullaniciId).HasColumnName("kullanici_id");
            entity.Property(e => e.VarlikId).HasColumnName("varlik_id");
            entity.Property(e => e.VarlikTipi).HasMaxLength(50).HasColumnName("varlik_tipi");
            entity.Property(e => e.YeniDeger).HasColumnType("jsonb").HasColumnName("yeni_deger");
            
            entity.HasOne(d => d.Kullanici).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.KullaniciId)
                .HasConstraintName("fk_audit_log_kullanici");
        });

        modelBuilder.Entity<EntegrasyonOutbox>(entity =>
        {
            entity.HasKey(e => e.OutboxId).HasName("pk_entegrasyon_outbox");
            entity.ToTable("entegrasyon_outbox");
            entity.HasIndex(e => e.Durum, "idx_entegrasyon_outbox_durum");
            entity.HasIndex(e => e.IdempotencyKey, "uq_entegrasyon_outbox_idempotency").IsUnique();
            entity.Property(e => e.OutboxId).HasColumnName("outbox_id");
            entity.Property(e => e.CorrelationId).HasMaxLength(100).HasColumnName("correlation_id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.Durum).HasMaxLength(20).HasDefaultValueSql("'BEKLIYOR'::character varying").HasColumnName("durum");
            entity.Property(e => e.FaturaId).HasColumnName("fatura_id");
            entity.Property(e => e.GonderimZamani).HasColumnName("gonderim_zamani");
            entity.Property(e => e.HataKodu).HasMaxLength(30).HasColumnName("hata_kodu");
            entity.Property(e => e.HataMesaji).HasMaxLength(500).HasColumnName("hata_mesaji");
            entity.Property(e => e.HedefSistem).HasMaxLength(30).HasColumnName("hedef_sistem");
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100).HasColumnName("idempotency_key");
            entity.Property(e => e.Payload).HasColumnType("jsonb").HasColumnName("payload");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.SonDenemeTarihi).HasColumnName("son_deneme_tarihi");

            entity.HasOne(d => d.Fatura).WithMany(p => p.EntegrasyonOutboxes)
                .HasForeignKey(d => d.FaturaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_entegrasyon_outbox_fatura");
        });

        modelBuilder.Entity<Fatura>(entity =>
        {
            entity.HasKey(e => e.FaturaId).HasName("pk_fatura");
            entity.ToTable("fatura");
            entity.HasIndex(e => e.Donem, "idx_fatura_donem");
            entity.HasIndex(e => e.Durum, "idx_fatura_durum");
            entity.HasIndex(e => e.SozlesmeId, "idx_fatura_sozlesme_id");
            entity.HasIndex(e => e.FaturaNo, "uq_fatura_fatura_no").IsUnique();
            entity.Property(e => e.FaturaId).HasColumnName("fatura_id");
            entity.Property(e => e.Carpan).HasPrecision(10, 3).HasColumnName("carpan");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.DagitimBedeli).HasPrecision(14, 2).HasColumnName("dagitim_bedeli");
            entity.Property(e => e.Donem).HasMaxLength(7).HasColumnName("donem");
            entity.Property(e => e.Durum).HasMaxLength(20).HasDefaultValueSql("'TASLAK'::character varying").HasColumnName("durum");
            entity.Property(e => e.EnerjiBedeli).HasPrecision(14, 2).HasColumnName("enerji_bedeli");
            entity.Property(e => e.FaturaNo).HasMaxLength(40).HasColumnName("fatura_no");
            entity.Property(e => e.FaturaTarihi).HasColumnName("fatura_tarihi");
            entity.Property(e => e.FaturaTipi).HasMaxLength(20).HasDefaultValueSql("'DONEM'::character varying").HasColumnName("fatura_tipi");
            entity.Property(e => e.HizmetBedeli).HasPrecision(14, 2).HasColumnName("hizmet_bedeli");
            entity.Property(e => e.IlkEndeks).HasPrecision(14, 3).HasColumnName("ilk_endeks");
            entity.Property(e => e.KesmeBaglamaBedeli).HasPrecision(14, 2).HasColumnName("kesme_baglama_bedeli");
            entity.Property(e => e.OkumaId).HasColumnName("okuma_id");
            entity.Property(e => e.ReaktifEnduktif).HasPrecision(14, 3).HasColumnName("reaktif_enduktif");
            entity.Property(e => e.ReaktifKapasitif).HasPrecision(14, 3).HasColumnName("reaktif_kapasitif");
            entity.Property(e => e.SonEndeks).HasPrecision(14, 3).HasColumnName("son_endeks");
            entity.Property(e => e.SonOdemeTarihi).HasColumnName("son_odeme_tarihi");
            entity.Property(e => e.SozlesmeId).HasColumnName("sozlesme_id");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValueSql("'AKTIF'::character varying").HasColumnName("status");
            entity.Property(e => e.TekilKod).HasMaxLength(40).HasColumnName("tekil_kod");
            entity.Property(e => e.ToplamTutar).HasPrecision(14, 2).HasColumnName("toplam_tutar");
            entity.Property(e => e.TuketimKwh).HasPrecision(14, 3).HasColumnName("tuketim_kwh");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.VergiFonToplam).HasPrecision(14, 2).HasColumnName("vergi_fon_toplam");

            entity.HasOne(d => d.Okuma).WithMany(p => p.Faturas)
                .HasForeignKey(d => d.OkumaId)
                .HasConstraintName("fk_fatura_okuma");

            entity.HasOne(d => d.Sozlesme).WithMany(p => p.Faturas)
                .HasForeignKey(d => d.SozlesmeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_fatura_sozlesme");
        });

        modelBuilder.Entity<FaturaKalemi>(entity =>
        {
            entity.HasKey(e => e.FaturaKalemId).HasName("pk_fatura_kalemi");
            entity.ToTable("fatura_kalemi");
            entity.Property(e => e.FaturaKalemId).HasColumnName("fatura_kalem_id");
            entity.Property(e => e.Aciklama).HasMaxLength(255).HasColumnName("aciklama");
            entity.Property(e => e.BirimFiyat).HasPrecision(14, 4).HasColumnName("birim_fiyat");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.FaturaId).HasColumnName("fatura_id");
            entity.Property(e => e.KalemTipi).HasMaxLength(30).HasColumnName("kalem_tipi");
            entity.Property(e => e.Miktar).HasPrecision(14, 3).HasColumnName("miktar");
            entity.Property(e => e.Tutar).HasPrecision(14, 2).HasColumnName("tutar");

            entity.HasOne(d => d.Fatura).WithMany(p => p.FaturaKalemis)
                .HasForeignKey(d => d.FaturaId)
                .HasConstraintName("fk_fatura_kalemi_fatura");
        });

        modelBuilder.Entity<Il>(entity =>
        {
            entity.HasKey(e => e.IlId).HasName("pk_il");
            entity.ToTable("il");
            entity.HasIndex(e => e.IlAdi, "uq_il_il_adi").IsUnique();
            entity.Property(e => e.IlId).HasColumnName("il_id");
            entity.Property(e => e.IlAdi).HasMaxLength(50).HasColumnName("il_adi");
            entity.Property(e => e.PlakaKodu).HasColumnName("plaka_kodu");
        });

        modelBuilder.Entity<Ilce>(entity =>
        {
            entity.HasKey(e => e.IlceId).HasName("pk_ilce");
            entity.ToTable("ilce");
            entity.HasIndex(e => e.IlId, "idx_ilce_il_id");
            entity.HasIndex(e => new { e.IlId, e.IlceAdi }, "uq_ilce_il_adi").IsUnique();
            entity.Property(e => e.IlceId).HasColumnName("ilce_id");
            entity.Property(e => e.IlId).HasColumnName("il_id");
            entity.Property(e => e.IlceAdi).HasMaxLength(50).HasColumnName("ilce_adi");

            entity.HasOne(d => d.Il).WithMany(p => p.Ilces)
                .HasForeignKey(d => d.IlId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ilce_il");
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.KullaniciId).HasName("pk_kullanicilar");
            entity.ToTable("kullanicilar");
            entity.HasIndex(e => e.RolId, "idx_kullanicilar_rol_id");
            entity.HasIndex(e => e.EPosta, "uq_kullanicilar_e_posta").IsUnique();
            entity.HasIndex(e => e.KullaniciAdi, "uq_kullanicilar_kullanici_adi").IsUnique();
            entity.Property(e => e.KullaniciId).HasColumnName("kullanici_id");
            entity.Property(e => e.AdSoyad).HasMaxLength(150).HasColumnName("ad_soyad");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.Durum).HasMaxLength(20).HasDefaultValueSql("'AKTIF'::character varying").HasColumnName("durum");
            entity.Property(e => e.EPosta).HasMaxLength(150).HasColumnName("e_posta");
            entity.Property(e => e.KullaniciAdi).HasMaxLength(50).HasColumnName("kullanici_adi");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.SifreHash).HasMaxLength(255).HasColumnName("sifre_hash");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Rol).WithMany(p => p.Kullanicilars)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_kullanicilar_roller");
        });

        modelBuilder.Entity<Roller>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("pk_roller");
            entity.ToTable("roller");
            entity.HasIndex(e => e.RolAdi, "uq_roller_rol_adi").IsUnique();
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.Aciklama).HasMaxLength(255).HasColumnName("aciklama");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.RolAdi).HasMaxLength(50).HasColumnName("rol_adi");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}