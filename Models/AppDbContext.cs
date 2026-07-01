using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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

    public virtual DbSet<Abone> Abones { get; set; }

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=Proje;Username=postgres;Password=123456Cc!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Abone>(entity =>
        {
            entity.HasKey(e => e.AboneId).HasName("abone_pkey");

            entity.ToTable("abone");

            entity.HasIndex(e => e.AboneNo, "uq_abone_abone_no").IsUnique();

            entity.Property(e => e.AboneId).HasColumnName("abone_id");
            entity.Property(e => e.AboneNo)
                .HasMaxLength(30)
                .HasColumnName("abone_no");
            entity.Property(e => e.AboneTipi)
                .HasMaxLength(20)
                .HasColumnName("abone_tipi");
            entity.Property(e => e.Ad)
                .HasMaxLength(100)
                .HasColumnName("ad");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EPosta)
                .HasMaxLength(150)
                .HasColumnName("e_posta");
            entity.Property(e => e.IletisimTercihi)
                .HasMaxLength(20)
                .HasColumnName("iletisim_tercihi");
            entity.Property(e => e.KullaniciId).HasColumnName("kullanici_id");
            entity.Property(e => e.Soyad)
                .HasMaxLength(100)
                .HasColumnName("soyad");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AKTIF'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Tckn)
                .HasMaxLength(11)
                .HasColumnName("tckn");
            entity.Property(e => e.Telefon)
                .HasMaxLength(20)
                .HasColumnName("telefon");
            entity.Property(e => e.Unvan)
                .HasMaxLength(255)
                .HasColumnName("unvan");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Vkn)
                .HasMaxLength(10)
                .HasColumnName("vkn");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AboneCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("abone_created_by_fkey");

            entity.HasOne(d => d.Kullanici).WithMany(p => p.AboneKullanicis)
                .HasForeignKey(d => d.KullaniciId)
                .HasConstraintName("abone_kullanici_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AboneUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("abone_updated_by_fkey");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("audit_log_pkey");

            entity.ToTable("audit_log");

            entity.HasIndex(e => new { e.VarlikTipi, e.VarlikId }, "idx_audit_log_varlik");

            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.EskiDeger)
                .HasColumnType("jsonb")
                .HasColumnName("eski_deger");
            entity.Property(e => e.IslemGerekcesi)
                .HasMaxLength(255)
                .HasColumnName("islem_gerekcesi");
            entity.Property(e => e.IslemTipi)
                .HasMaxLength(20)
                .HasColumnName("islem_tipi");
            entity.Property(e => e.IslemZamani)
                .HasDefaultValueSql("now()")
                .HasColumnName("islem_zamani");
            entity.Property(e => e.KullaniciId).HasColumnName("kullanici_id");
            entity.Property(e => e.VarlikId).HasColumnName("varlik_id");
            entity.Property(e => e.VarlikTipi)
                .HasMaxLength(50)
                .HasColumnName("varlik_tipi");
            entity.Property(e => e.YeniDeger)
                .HasColumnType("jsonb")
                .HasColumnName("yeni_deger");

            entity.HasOne(d => d.Kullanici).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.KullaniciId)
                .HasConstraintName("audit_log_kullanici_id_fkey");
        });

        modelBuilder.Entity<EndeksOkuma>(entity =>
        {
            entity.HasKey(e => e.OkumaId).HasName("endeks_okuma_pkey");

            entity.ToTable("endeks_okuma");

            entity.HasIndex(e => e.Donem, "idx_endeks_okuma_donem");

            entity.HasIndex(e => e.SayacId, "idx_endeks_okuma_sayac_id");

            entity.HasIndex(e => new { e.SayacId, e.Donem, e.OkumaTipi }, "uq_endeks_okuma_mukerrer")
                .IsUnique()
                .HasFilter("((status)::text = 'AKTIF'::text)");

            entity.Property(e => e.OkumaId).HasColumnName("okuma_id");
            entity.Property(e => e.AnomaliMi).HasColumnName("anomali_mi");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DogrulamaDurumu)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PLANLANDI'::character varying")
                .HasColumnName("dogrulama_durumu");
            entity.Property(e => e.Donem)
                .HasMaxLength(7)
                .HasColumnName("donem");
            entity.Property(e => e.KullaniciId).HasColumnName("kullanici_id");
            entity.Property(e => e.OkumaKaynagi)
                .HasMaxLength(20)
                .HasColumnName("okuma_kaynagi");
            entity.Property(e => e.OkumaTipi)
                .HasMaxLength(30)
                .HasColumnName("okuma_tipi");
            entity.Property(e => e.OkumaZamani).HasColumnName("okuma_zamani");
            entity.Property(e => e.OkunamamaNedeni)
                .HasMaxLength(50)
                .HasColumnName("okunamama_nedeni");
            entity.Property(e => e.OncekiEndeks)
                .HasPrecision(14, 3)
                .HasColumnName("onceki_endeks");
            entity.Property(e => e.SayacId).HasColumnName("sayac_id");
            entity.Property(e => e.SozlesmeId).HasColumnName("sozlesme_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AKTIF'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.YeniEndeks)
                .HasPrecision(14, 3)
                .HasColumnName("yeni_endeks");

            entity.HasOne(d => d.Kullanici).WithMany(p => p.EndeksOkumas)
                .HasForeignKey(d => d.KullaniciId)
                .HasConstraintName("endeks_okuma_kullanici_id_fkey");

            entity.HasOne(d => d.Sayac).WithMany(p => p.EndeksOkumas)
                .HasForeignKey(d => d.SayacId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("endeks_okuma_sayac_id_fkey");

            entity.HasOne(d => d.Sozlesme).WithMany(p => p.EndeksOkumas)
                .HasForeignKey(d => d.SozlesmeId)
                .HasConstraintName("endeks_okuma_sozlesme_id_fkey");
        });

        modelBuilder.Entity<EntegrasyonOutbox>(entity =>
        {
            entity.HasKey(e => e.OutboxId).HasName("entegrasyon_outbox_pkey");

            entity.ToTable("entegrasyon_outbox");

            entity.HasIndex(e => e.Durum, "idx_entegrasyon_outbox_durum");

            entity.HasIndex(e => e.IdempotencyKey, "uq_entegrasyon_outbox_idempotency").IsUnique();

            entity.Property(e => e.OutboxId).HasColumnName("outbox_id");
            entity.Property(e => e.CorrelationId)
                .HasMaxLength(100)
                .HasColumnName("correlation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValueSql("'BEKLIYOR'::character varying")
                .HasColumnName("durum");
            entity.Property(e => e.FaturaId).HasColumnName("fatura_id");
            entity.Property(e => e.GonderimZamani).HasColumnName("gonderim_zamani");
            entity.Property(e => e.HataKodu)
                .HasMaxLength(30)
                .HasColumnName("hata_kodu");
            entity.Property(e => e.HataMesaji)
                .HasMaxLength(500)
                .HasColumnName("hata_mesaji");
            entity.Property(e => e.HedefSistem)
                .HasMaxLength(30)
                .HasColumnName("hedef_sistem");
            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(100)
                .HasColumnName("idempotency_key");
            entity.Property(e => e.Payload)
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.SonDenemeTarihi).HasColumnName("son_deneme_tarihi");

            entity.HasOne(d => d.Fatura).WithMany(p => p.EntegrasyonOutboxes)
                .HasForeignKey(d => d.FaturaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entegrasyon_outbox_fatura_id_fkey");
        });

        modelBuilder.Entity<Fatura>(entity =>
        {
            entity.HasKey(e => e.FaturaId).HasName("fatura_pkey");

            entity.ToTable("fatura");

            entity.HasIndex(e => e.Donem, "idx_fatura_donem");

            entity.HasIndex(e => e.Durum, "idx_fatura_durum");

            entity.HasIndex(e => e.SozlesmeId, "idx_fatura_sozlesme_id");

            entity.HasIndex(e => e.FaturaNo, "uq_fatura_fatura_no").IsUnique();

            entity.HasIndex(e => new { e.SozlesmeId, e.Donem, e.FaturaTipi }, "uq_fatura_mukerrer")
                .IsUnique()
                .HasFilter("(((status)::text = 'AKTIF'::text) AND ((durum)::text <> 'IPTAL'::text))");

            entity.Property(e => e.FaturaId).HasColumnName("fatura_id");
            entity.Property(e => e.AboneId).HasColumnName("abone_id");
            entity.Property(e => e.Carpan)
                .HasPrecision(10, 3)
                .HasColumnName("carpan");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DagitimBedeli)
                .HasPrecision(14, 2)
                .HasColumnName("dagitim_bedeli");
            entity.Property(e => e.Donem)
                .HasMaxLength(7)
                .HasColumnName("donem");
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValueSql("'TASLAK'::character varying")
                .HasColumnName("durum");
            entity.Property(e => e.EnerjiBedeli)
                .HasPrecision(14, 2)
                .HasColumnName("enerji_bedeli");
            entity.Property(e => e.FaturaNo)
                .HasMaxLength(40)
                .HasColumnName("fatura_no");
            entity.Property(e => e.FaturaTarihi).HasColumnName("fatura_tarihi");
            entity.Property(e => e.FaturaTipi)
                .HasMaxLength(20)
                .HasDefaultValueSql("'DONEM'::character varying")
                .HasColumnName("fatura_tipi");
            entity.Property(e => e.HizmetBedeli)
                .HasPrecision(14, 2)
                .HasColumnName("hizmet_bedeli");
            entity.Property(e => e.IlkEndeks)
                .HasPrecision(14, 3)
                .HasColumnName("ilk_endeks");
            entity.Property(e => e.KesmeBaglamaBedeli)
                .HasPrecision(14, 2)
                .HasColumnName("kesme_baglama_bedeli");
            entity.Property(e => e.OkumaId).HasColumnName("okuma_id");
            entity.Property(e => e.ReaktifEnduktif)
                .HasPrecision(14, 3)
                .HasColumnName("reaktif_enduktif");
            entity.Property(e => e.ReaktifKapasitif)
                .HasPrecision(14, 3)
                .HasColumnName("reaktif_kapasitif");
            entity.Property(e => e.SonEndeks)
                .HasPrecision(14, 3)
                .HasColumnName("son_endeks");
            entity.Property(e => e.SonOdemeTarihi).HasColumnName("son_odeme_tarihi");
            entity.Property(e => e.SozlesmeId).HasColumnName("sozlesme_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AKTIF'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TekilKod)
                .HasMaxLength(40)
                .HasColumnName("tekil_kod");
            entity.Property(e => e.ToplamTutar)
                .HasPrecision(14, 2)
                .HasColumnName("toplam_tutar");
            entity.Property(e => e.TuketimKwh)
                .HasPrecision(14, 3)
                .HasColumnName("tuketim_kwh");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.VergiFonToplam)
                .HasPrecision(14, 2)
                .HasColumnName("vergi_fon_toplam");

            entity.HasOne(d => d.Abone).WithMany(p => p.Faturas)
                .HasForeignKey(d => d.AboneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fatura_abone_id_fkey");

            entity.HasOne(d => d.Okuma).WithMany(p => p.Faturas)
                .HasForeignKey(d => d.OkumaId)
                .HasConstraintName("fatura_okuma_id_fkey");

            entity.HasOne(d => d.Sozlesme).WithMany(p => p.Faturas)
                .HasForeignKey(d => d.SozlesmeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fatura_sozlesme_id_fkey");
        });

        modelBuilder.Entity<FaturaKalemi>(entity =>
        {
            entity.HasKey(e => e.FaturaKalemId).HasName("fatura_kalemi_pkey");

            entity.ToTable("fatura_kalemi");

            entity.Property(e => e.FaturaKalemId).HasColumnName("fatura_kalem_id");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(255)
                .HasColumnName("aciklama");
            entity.Property(e => e.BirimFiyat)
                .HasPrecision(14, 4)
                .HasColumnName("birim_fiyat");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FaturaId).HasColumnName("fatura_id");
            entity.Property(e => e.KalemTipi)
                .HasMaxLength(30)
                .HasColumnName("kalem_tipi");
            entity.Property(e => e.Miktar)
                .HasPrecision(14, 3)
                .HasColumnName("miktar");
            entity.Property(e => e.Tutar)
                .HasPrecision(14, 2)
                .HasColumnName("tutar");

            entity.HasOne(d => d.Fatura).WithMany(p => p.FaturaKalemis)
                .HasForeignKey(d => d.FaturaId)
                .HasConstraintName("fatura_kalemi_fatura_id_fkey");
        });

        modelBuilder.Entity<Il>(entity =>
        {
            entity.HasKey(e => e.IlId).HasName("il_pkey");

            entity.ToTable("il");

            entity.HasIndex(e => e.IlAdi, "uq_il_il_adi").IsUnique();

            entity.Property(e => e.IlId).HasColumnName("il_id");
            entity.Property(e => e.IlAdi)
                .HasMaxLength(50)
                .HasColumnName("il_adi");
            entity.Property(e => e.PlakaKodu).HasColumnName("plaka_kodu");
        });

        modelBuilder.Entity<Ilce>(entity =>
        {
            entity.HasKey(e => e.IlceId).HasName("ilce_pkey");

            entity.ToTable("ilce");

            entity.HasIndex(e => e.IlId, "idx_ilce_il_id");

            entity.HasIndex(e => new { e.IlId, e.IlceAdi }, "uq_ilce_il_ad").IsUnique();

            entity.Property(e => e.IlceId).HasColumnName("ilce_id");
            entity.Property(e => e.IlId).HasColumnName("il_id");
            entity.Property(e => e.IlceAdi)
                .HasMaxLength(50)
                .HasColumnName("ilce_adi");

            entity.HasOne(d => d.Il).WithMany(p => p.Ilces)
                .HasForeignKey(d => d.IlId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ilce_il_id_fkey");
        });

        modelBuilder.Entity<IsEmirleri>(entity =>
        {
            entity.HasKey(e => e.IsEmriId).HasName("is_emirleri_pkey");

            entity.ToTable("is_emirleri");

            entity.HasIndex(e => e.Durum, "idx_is_emirleri_durum");

            entity.HasIndex(e => e.TuketimNoktasiId, "idx_is_emirleri_tuketim_noktasi_id");

            entity.HasIndex(e => e.IsEmriNo, "uq_is_emirleri_no").IsUnique();

            entity.Property(e => e.IsEmriId).HasColumnName("is_emri_id");
            entity.Property(e => e.AtananKullaniciId).HasColumnName("atanan_kullanici_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValueSql("'ACIK'::character varying")
                .HasColumnName("durum");
            entity.Property(e => e.EskiSayacNo)
                .HasMaxLength(50)
                .HasColumnName("eski_sayac_no");
            entity.Property(e => e.Gerekce)
                .HasMaxLength(255)
                .HasColumnName("gerekce");
            entity.Property(e => e.IsEmriNo)
                .HasMaxLength(40)
                .HasColumnName("is_emri_no");
            entity.Property(e => e.MuhurNo)
                .HasMaxLength(40)
                .HasColumnName("muhur_no");
            entity.Property(e => e.Oncelik)
                .HasMaxLength(10)
                .HasDefaultValueSql("'NORMAL'::character varying")
                .HasColumnName("oncelik");
            entity.Property(e => e.PlanlananTarih).HasColumnName("planlanan_tarih");
            entity.Property(e => e.SahaSonucu)
                .HasMaxLength(255)
                .HasColumnName("saha_sonucu");
            entity.Property(e => e.SayacId).HasColumnName("sayac_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AKTIF'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Tip)
                .HasMaxLength(30)
                .HasColumnName("tip");
            entity.Property(e => e.TuketimNoktasiId).HasColumnName("tuketim_noktasi_id");
            entity.Property(e => e.TutanakNo)
                .HasMaxLength(40)
                .HasColumnName("tutanak_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.YeniSayacNo)
                .HasMaxLength(50)
                .HasColumnName("yeni_sayac_no");

            entity.HasOne(d => d.AtananKullanici).WithMany(p => p.IsEmirleris)
                .HasForeignKey(d => d.AtananKullaniciId)
                .HasConstraintName("is_emirleri_atanan_kullanici_id_fkey");

            entity.HasOne(d => d.Sayac).WithMany(p => p.IsEmirleris)
                .HasForeignKey(d => d.SayacId)
                .HasConstraintName("is_emirleri_sayac_id_fkey");

            entity.HasOne(d => d.TuketimNoktasi).WithMany(p => p.IsEmirleris)
                .HasForeignKey(d => d.TuketimNoktasiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("is_emirleri_tuketim_noktasi_id_fkey");
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.KullaniciId).HasName("kullanicilar_pkey");

            entity.ToTable("kullanicilar");

            entity.HasIndex(e => e.RolId, "idx_kullanicilar_rol_id");

            entity.HasIndex(e => e.EPosta, "uq_kullanicilar_e_posta").IsUnique();

            entity.HasIndex(e => e.KullaniciAdi, "uq_kullanicilar_kullanici_adi").IsUnique();

            entity.Property(e => e.KullaniciId).HasColumnName("kullanici_id");
            entity.Property(e => e.AdSoyad)
                .HasMaxLength(150)
                .HasColumnName("ad_soyad");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AKTIF'::character varying")
                .HasColumnName("durum");
            entity.Property(e => e.EPosta)
                .HasMaxLength(150)
                .HasColumnName("e_posta");
            entity.Property(e => e.KullaniciAdi)
                .HasMaxLength(50)
                .HasColumnName("kullanici_adi");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.SifreHash)
                .HasMaxLength(255)
                .HasColumnName("sifre_hash");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Rol).WithMany(p => p.Kullanicilars)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("kullanicilar_rol_id_fkey");
        });

        modelBuilder.Entity<Roller>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("roller_pkey");

            entity.ToTable("roller");

            entity.HasIndex(e => e.RolAdi, "uq_roller_rol_adi").IsUnique();

            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.Aciklama)
                .HasMaxLength(255)
                .HasColumnName("aciklama");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.RolAdi)
                .HasMaxLength(50)
                .HasColumnName("rol_adi");
        });

        modelBuilder.Entity<Sayaclar>(entity =>
        {
            entity.HasKey(e => e.SayacId).HasName("sayaclar_pkey");

            entity.ToTable("sayaclar");

            entity.HasIndex(e => e.TuketimNoktasiId, "idx_sayaclar_tuketim_noktasi_id");

            entity.HasIndex(e => e.SeriNo, "uq_sayaclar_seri_no_takili")
                .IsUnique()
                .HasFilter("((durum)::text = 'TAKILI'::text)");

            entity.Property(e => e.SayacId).HasColumnName("sayac_id");
            entity.Property(e => e.Carpan)
                .HasPrecision(10, 3)
                .HasDefaultValue(1m)
                .HasColumnName("carpan");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValueSql("'DEPODA'::character varying")
                .HasColumnName("durum");
            entity.Property(e => e.Faz)
                .HasMaxLength(10)
                .HasColumnName("faz");
            entity.Property(e => e.Marka)
                .HasMaxLength(50)
                .HasColumnName("marka");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("model");
            entity.Property(e => e.MuhurNo)
                .HasMaxLength(40)
                .HasColumnName("muhur_no");
            entity.Property(e => e.SeriNo)
                .HasMaxLength(50)
                .HasColumnName("seri_no");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AKTIF'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TuketimNoktasiId).HasColumnName("tuketim_noktasi_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SayaclarCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("sayaclar_created_by_fkey");

            entity.HasOne(d => d.TuketimNoktasi).WithMany(p => p.Sayaclars)
                .HasForeignKey(d => d.TuketimNoktasiId)
                .HasConstraintName("sayaclar_tuketim_noktasi_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SayaclarUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("sayaclar_updated_by_fkey");
        });

        modelBuilder.Entity<Sozlesmeler>(entity =>
        {
            entity.HasKey(e => e.SozlesmeId).HasName("sozlesmeler_pkey");

            entity.ToTable("sozlesmeler");

            entity.HasIndex(e => e.AboneId, "idx_sozlesmeler_abone_id");

            entity.HasIndex(e => e.TuketimNoktasiId, "idx_sozlesmeler_tuketim_noktasi_id");

            entity.HasIndex(e => e.SozlesmeNo, "uq_sozlesmeler_sozlesme_no").IsUnique();

            entity.HasIndex(e => e.TuketimNoktasiId, "uq_sozlesmeler_tek_aktif")
                .IsUnique()
                .HasFilter("((statu)::text = 'AKTIF'::text)");

            entity.Property(e => e.SozlesmeId).HasColumnName("sozlesme_id");
            entity.Property(e => e.AboneId).HasColumnName("abone_id");
            entity.Property(e => e.BaslangicTarihi).HasColumnName("baslangic_tarihi");
            entity.Property(e => e.BitisTarihi).HasColumnName("bitis_tarihi");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.GuvenceBedeli)
                .HasPrecision(14, 2)
                .HasColumnName("guvence_bedeli");
            entity.Property(e => e.SozlesmeNo)
                .HasMaxLength(40)
                .HasColumnName("sozlesme_no");
            entity.Property(e => e.SozlesmeTipi)
                .HasMaxLength(30)
                .HasColumnName("sozlesme_tipi");
            entity.Property(e => e.Statu)
                .HasMaxLength(20)
                .HasDefaultValueSql("'TASLAK'::character varying")
                .HasColumnName("statu");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AKTIF'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TarifeGrubu)
                .HasMaxLength(30)
                .HasColumnName("tarife_grubu");
            entity.Property(e => e.TuketimNoktasiId).HasColumnName("tuketim_noktasi_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Abone).WithMany(p => p.Sozlesmelers)
                .HasForeignKey(d => d.AboneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sozlesmeler_abone_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SozlesmelerCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("sozlesmeler_created_by_fkey");

            entity.HasOne(d => d.TuketimNoktasi).WithOne(p => p.Sozlesmeler)
                .HasForeignKey<Sozlesmeler>(d => d.TuketimNoktasiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sozlesmeler_tuketim_noktasi_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SozlesmelerUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("sozlesmeler_updated_by_fkey");
        });

        modelBuilder.Entity<TuketimNoktasi>(entity =>
        {
            entity.HasKey(e => e.TuketimNoktasiId).HasName("tuketim_noktasi_pkey");

            entity.ToTable("tuketim_noktasi");

            entity.HasIndex(e => e.IlceId, "idx_tuketim_noktasi_ilce_id");

            entity.HasIndex(e => e.TekilKod, "uq_tuketim_noktasi_tekil_kod").IsUnique();

            entity.Property(e => e.TuketimNoktasiId).HasColumnName("tuketim_noktasi_id");
            entity.Property(e => e.AcikAdres)
                .HasMaxLength(500)
                .HasColumnName("acik_adres");
            entity.Property(e => e.BagimsizBolumNo)
                .HasMaxLength(20)
                .HasColumnName("bagimsiz_bolum_no");
            entity.Property(e => e.BaglantiDurumu)
                .HasMaxLength(20)
                .HasDefaultValueSql("'TASLAK'::character varying")
                .HasColumnName("baglanti_durumu");
            entity.Property(e => e.BaglantiGucuKw)
                .HasPrecision(10, 2)
                .HasColumnName("baglanti_gucu_kw");
            entity.Property(e => e.BinaNo)
                .HasMaxLength(20)
                .HasColumnName("bina_no");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IlceId).HasColumnName("ilce_id");
            entity.Property(e => e.KoordinatLat)
                .HasPrecision(10, 6)
                .HasColumnName("koordinat_lat");
            entity.Property(e => e.KoordinatLon)
                .HasPrecision(10, 6)
                .HasColumnName("koordinat_lon");
            entity.Property(e => e.Mahalle)
                .HasMaxLength(100)
                .HasColumnName("mahalle");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AKTIF'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TekilKod)
                .HasMaxLength(40)
                .HasColumnName("tekil_kod");
            entity.Property(e => e.TesisatNo)
                .HasMaxLength(40)
                .HasColumnName("tesisat_no");
            entity.Property(e => e.TuketiciGrubu)
                .HasMaxLength(30)
                .HasColumnName("tuketici_grubu");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TuketimNoktasiCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("tuketim_noktasi_created_by_fkey");

            entity.HasOne(d => d.Ilce).WithMany(p => p.TuketimNoktasis)
                .HasForeignKey(d => d.IlceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tuketim_noktasi_ilce_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TuketimNoktasiUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("tuketim_noktasi_updated_by_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
