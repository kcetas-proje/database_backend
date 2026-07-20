using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KcetasAboneApi.Migrations
{
    /// <inheritdoc />
    public partial class BildirimlerTablosuEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aboneler",
                columns: table => new
                {
                    abone_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    abone_no = table.Column<string>(type: "text", nullable: false),
                    abone_tipi = table.Column<string>(type: "text", nullable: false),
                    ad = table.Column<string>(type: "text", nullable: true),
                    soyad = table.Column<string>(type: "text", nullable: true),
                    unvan = table.Column<string>(type: "text", nullable: true),
                    tckn = table.Column<string>(type: "text", nullable: true),
                    vkn = table.Column<string>(type: "text", nullable: true),
                    telefon = table.Column<string>(type: "text", nullable: true),
                    e_posta = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aboneler", x => x.abone_id);
                });

            migrationBuilder.CreateTable(
                name: "Bildirimler",
                columns: table => new
                {
                    BildirimId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KullaniciId = table.Column<int>(type: "integer", nullable: false),
                    Baslik = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Icerik = table.Column<string>(type: "text", nullable: false),
                    OkunduMu = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bildirimler", x => x.BildirimId);
                });

            migrationBuilder.CreateTable(
                name: "il",
                columns: table => new
                {
                    il_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    il_adi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plaka_kodu = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_il", x => x.il_id);
                });

            migrationBuilder.CreateTable(
                name: "roller",
                columns: table => new
                {
                    rol_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rol_adi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    aciklama = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roller", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "tarifeler",
                columns: table => new
                {
                    tarife_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tarife_kodu = table.Column<string>(type: "text", nullable: false),
                    tarife_adi = table.Column<string>(type: "text", nullable: false),
                    gunduz_birim_fiyat = table.Column<decimal>(type: "numeric", nullable: false),
                    puant_birim_fiyat = table.Column<decimal>(type: "numeric", nullable: true),
                    gece_birim_fiyat = table.Column<decimal>(type: "numeric", nullable: true),
                    induktif_birim_fiyat = table.Column<decimal>(type: "numeric", nullable: true),
                    kapasitif_birim_fiyat = table.Column<decimal>(type: "numeric", nullable: true),
                    demand_birim_fiyat = table.Column<decimal>(type: "numeric", nullable: true),
                    kdv_orani = table.Column<decimal>(type: "numeric", nullable: false),
                    dagitim_bedeli = table.Column<decimal>(type: "numeric", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifeler", x => x.tarife_id);
                });

            migrationBuilder.CreateTable(
                name: "ilce",
                columns: table => new
                {
                    ilce_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    il_id = table.Column<short>(type: "smallint", nullable: false),
                    ilce_adi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ilce", x => x.ilce_id);
                    table.ForeignKey(
                        name: "fk_ilce_il",
                        column: x => x.il_id,
                        principalTable: "il",
                        principalColumn: "il_id");
                });

            migrationBuilder.CreateTable(
                name: "kullanicilar",
                columns: table => new
                {
                    kullanici_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rol_id = table.Column<short>(type: "smallint", nullable: false),
                    ad_soyad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    e_posta = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    kullanici_adi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sifre_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'AKTIF'::character varying"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kullanicilar", x => x.kullanici_id);
                    table.ForeignKey(
                        name: "fk_kullanicilar_roller",
                        column: x => x.rol_id,
                        principalTable: "roller",
                        principalColumn: "rol_id");
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    audit_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    varlik_tipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    varlik_id = table.Column<long>(type: "bigint", nullable: false),
                    islem_tipi = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    eski_deger = table.Column<string>(type: "jsonb", nullable: true),
                    yeni_deger = table.Column<string>(type: "jsonb", nullable: true),
                    kullanici_id = table.Column<long>(type: "bigint", nullable: true),
                    islem_gerekcesi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    islem_zamani = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.audit_id);
                    table.ForeignKey(
                        name: "fk_audit_log_kullanici",
                        column: x => x.kullanici_id,
                        principalTable: "kullanicilar",
                        principalColumn: "kullanici_id");
                });

            migrationBuilder.CreateTable(
                name: "tuketim_noktasi",
                columns: table => new
                {
                    tuketim_noktasi_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tekil_kod = table.Column<string>(type: "text", nullable: false),
                    ilce_id = table.Column<int>(type: "integer", nullable: false),
                    mahalle = table.Column<string>(type: "text", nullable: false),
                    bina_no = table.Column<string>(type: "text", nullable: true),
                    bagimsiz_bolum_no = table.Column<string>(type: "text", nullable: true),
                    acik_adres = table.Column<string>(type: "text", nullable: false),
                    koordinat_lat = table.Column<decimal>(type: "numeric", nullable: true),
                    koordinat_lon = table.Column<decimal>(type: "numeric", nullable: true),
                    baglanti_gucu_kw = table.Column<decimal>(type: "numeric", nullable: false),
                    tuketici_grubu = table.Column<string>(type: "text", nullable: false),
                    baglanti_durumu = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tuketim_noktasi", x => x.tuketim_noktasi_id);
                    table.ForeignKey(
                        name: "FK_tuketim_noktasi_ilce_ilce_id",
                        column: x => x.ilce_id,
                        principalTable: "ilce",
                        principalColumn: "ilce_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tuketim_noktasi_kullanicilar_created_by",
                        column: x => x.created_by,
                        principalTable: "kullanicilar",
                        principalColumn: "kullanici_id");
                    table.ForeignKey(
                        name: "FK_tuketim_noktasi_kullanicilar_updated_by",
                        column: x => x.updated_by,
                        principalTable: "kullanicilar",
                        principalColumn: "kullanici_id");
                });

            migrationBuilder.CreateTable(
                name: "sayaclar",
                columns: table => new
                {
                    sayac_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    seri_no = table.Column<string>(type: "text", nullable: false),
                    tuketim_noktasi_id = table.Column<long>(type: "bigint", nullable: true),
                    marka = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    uretim_yili = table.Column<int>(type: "integer", nullable: false),
                    faz = table.Column<string>(type: "text", nullable: true),
                    carpan = table.Column<decimal>(type: "numeric", nullable: false),
                    muhur_no = table.Column<string>(type: "text", nullable: true),
                    durum = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sayaclar", x => x.sayac_id);
                    table.ForeignKey(
                        name: "FK_sayaclar_kullanicilar_created_by",
                        column: x => x.created_by,
                        principalTable: "kullanicilar",
                        principalColumn: "kullanici_id");
                    table.ForeignKey(
                        name: "FK_sayaclar_kullanicilar_updated_by",
                        column: x => x.updated_by,
                        principalTable: "kullanicilar",
                        principalColumn: "kullanici_id");
                    table.ForeignKey(
                        name: "FK_sayaclar_tuketim_noktasi_tuketim_noktasi_id",
                        column: x => x.tuketim_noktasi_id,
                        principalTable: "tuketim_noktasi",
                        principalColumn: "tuketim_noktasi_id");
                });

            migrationBuilder.CreateTable(
                name: "sozlesmeler",
                columns: table => new
                {
                    sozlesme_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sozlesme_no = table.Column<string>(type: "text", nullable: false),
                    tuketim_noktasi_id = table.Column<long>(type: "bigint", nullable: false),
                    abone_id = table.Column<long>(type: "bigint", nullable: false),
                    tarife_id = table.Column<long>(type: "bigint", nullable: false),
                    sozlesme_tipi = table.Column<string>(type: "text", nullable: false),
                    durum = table.Column<string>(type: "text", nullable: false),
                    baslangic_tarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    bitis_tarihi = table.Column<DateOnly>(type: "date", nullable: true),
                    guvence_bedeli = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sozlesmeler", x => x.sozlesme_id);
                    table.ForeignKey(
                        name: "FK_sozlesmeler_aboneler_abone_id",
                        column: x => x.abone_id,
                        principalTable: "aboneler",
                        principalColumn: "abone_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sozlesmeler_tarifeler_tarife_id",
                        column: x => x.tarife_id,
                        principalTable: "tarifeler",
                        principalColumn: "tarife_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sozlesmeler_tuketim_noktasi_tuketim_noktasi_id",
                        column: x => x.tuketim_noktasi_id,
                        principalTable: "tuketim_noktasi",
                        principalColumn: "tuketim_noktasi_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "is_emirleri",
                columns: table => new
                {
                    is_emri_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_emri_no = table.Column<string>(type: "text", nullable: false),
                    tuketim_noktasi_id = table.Column<long>(type: "bigint", nullable: false),
                    sayac_id = table.Column<long>(type: "bigint", nullable: true),
                    tip = table.Column<string>(type: "text", nullable: false),
                    oncelik = table.Column<string>(type: "text", nullable: false),
                    planlanan_tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    atanan_kullanici_id = table.Column<long>(type: "bigint", nullable: true),
                    durum = table.Column<string>(type: "text", nullable: false),
                    saha_sonucu = table.Column<string>(type: "text", nullable: true),
                    gerekce = table.Column<string>(type: "text", nullable: true),
                    muhur_no = table.Column<string>(type: "text", nullable: true),
                    tutanak_no = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sokulen_seri_no = table.Column<string>(type: "text", nullable: true),
                    yeni_seri_no = table.Column<string>(type: "text", nullable: true),
                    yeni_muhur_no = table.Column<string>(type: "text", nullable: true),
                    eski_muhur_no = table.Column<string>(type: "text", nullable: true),
                    damga_yili = table.Column<int>(type: "integer", nullable: true),
                    akim_trafosu_seri_no = table.Column<string>(type: "text", nullable: true),
                    akim_trafosu_marka = table.Column<string>(type: "text", nullable: true),
                    gerilim_trafosu_seri_no = table.Column<string>(type: "text", nullable: true),
                    gerilim_trafosu_marka = table.Column<string>(type: "text", nullable: true),
                    ariza_tipi = table.Column<string>(type: "text", nullable: true),
                    kesme_noktasi = table.Column<string>(type: "text", nullable: true),
                    kesme_nedeni = table.Column<string>(type: "text", nullable: true),
                    abone_durumu = table.Column<string>(type: "text", nullable: true),
                    sayac_durumu = table.Column<string>(type: "text", nullable: true),
                    pano_direk_no = table.Column<string>(type: "text", nullable: true),
                    kesif_sonucu = table.Column<string>(type: "text", nullable: true),
                    yapi_tesis_tipi = table.Column<string>(type: "text", nullable: true),
                    hat_mesafesi = table.Column<string>(type: "text", nullable: true),
                    talep_gucu = table.Column<string>(type: "text", nullable: true),
                    inceleme_notu = table.Column<string>(type: "text", nullable: true),
                    acma_noktasi = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_is_emirleri", x => x.is_emri_id);
                    table.ForeignKey(
                        name: "FK_is_emirleri_kullanicilar_atanan_kullanici_id",
                        column: x => x.atanan_kullanici_id,
                        principalTable: "kullanicilar",
                        principalColumn: "kullanici_id");
                    table.ForeignKey(
                        name: "FK_is_emirleri_sayaclar_sayac_id",
                        column: x => x.sayac_id,
                        principalTable: "sayaclar",
                        principalColumn: "sayac_id");
                    table.ForeignKey(
                        name: "FK_is_emirleri_tuketim_noktasi_tuketim_noktasi_id",
                        column: x => x.tuketim_noktasi_id,
                        principalTable: "tuketim_noktasi",
                        principalColumn: "tuketim_noktasi_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "endeks_okuma",
                columns: table => new
                {
                    okuma_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sayac_id = table.Column<long>(type: "bigint", nullable: false),
                    is_emri_id = table.Column<long>(type: "bigint", nullable: true),
                    sozlesme_id = table.Column<long>(type: "bigint", nullable: true),
                    okuma_tipi = table.Column<string>(type: "text", nullable: false),
                    okuma_kaynagi = table.Column<string>(type: "text", nullable: false),
                    onceki_endeks = table.Column<decimal>(type: "numeric", nullable: true),
                    yeni_endeks = table.Column<decimal>(type: "numeric", nullable: false),
                    donem = table.Column<string>(type: "text", nullable: true),
                    okuma_zamani = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    kullanici_id = table.Column<long>(type: "bigint", nullable: true),
                    okunamama_nedeni = table.Column<string>(type: "text", nullable: true),
                    dogrulama_durumu = table.Column<string>(type: "text", nullable: false),
                    anomali_mi = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_endeks_okuma", x => x.okuma_id);
                    table.ForeignKey(
                        name: "FK_endeks_okuma_is_emirleri_is_emri_id",
                        column: x => x.is_emri_id,
                        principalTable: "is_emirleri",
                        principalColumn: "is_emri_id");
                    table.ForeignKey(
                        name: "FK_endeks_okuma_kullanicilar_kullanici_id",
                        column: x => x.kullanici_id,
                        principalTable: "kullanicilar",
                        principalColumn: "kullanici_id");
                    table.ForeignKey(
                        name: "FK_endeks_okuma_sayaclar_sayac_id",
                        column: x => x.sayac_id,
                        principalTable: "sayaclar",
                        principalColumn: "sayac_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_endeks_okuma_sozlesmeler_sozlesme_id",
                        column: x => x.sozlesme_id,
                        principalTable: "sozlesmeler",
                        principalColumn: "sozlesme_id");
                });

            migrationBuilder.CreateTable(
                name: "fatura",
                columns: table => new
                {
                    fatura_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fatura_no = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    sozlesme_id = table.Column<long>(type: "bigint", nullable: false),
                    tekil_kod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    fatura_tipi = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'DONEM'::character varying"),
                    donem = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    fatura_tarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    son_odeme_tarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    okuma_id = table.Column<long>(type: "bigint", nullable: true),
                    ilk_endeks = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: true),
                    son_endeks = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: true),
                    tuketim_kwh = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: true),
                    reaktif_enduktif = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: true),
                    reaktif_kapasitif = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: true),
                    carpan = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    enerji_bedeli = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    dagitim_bedeli = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    hizmet_bedeli = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    kesme_baglama_bedeli = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    vergi_fon_toplam = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    toplam_tutar = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'TASLAK'::character varying"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'AKTIF'::character varying"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fatura", x => x.fatura_id);
                    table.ForeignKey(
                        name: "fk_fatura_okuma",
                        column: x => x.okuma_id,
                        principalTable: "endeks_okuma",
                        principalColumn: "okuma_id");
                    table.ForeignKey(
                        name: "fk_fatura_sozlesme",
                        column: x => x.sozlesme_id,
                        principalTable: "sozlesmeler",
                        principalColumn: "sozlesme_id");
                });

            migrationBuilder.CreateTable(
                name: "entegrasyon_outbox",
                columns: table => new
                {
                    outbox_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fatura_id = table.Column<long>(type: "bigint", nullable: false),
                    hedef_sistem = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'BEKLIYOR'::character varying"),
                    hata_kodu = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    hata_mesaji = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    son_deneme_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gonderim_zamani = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entegrasyon_outbox", x => x.outbox_id);
                    table.ForeignKey(
                        name: "fk_entegrasyon_outbox_fatura",
                        column: x => x.fatura_id,
                        principalTable: "fatura",
                        principalColumn: "fatura_id");
                });

            migrationBuilder.CreateTable(
                name: "fatura_kalemi",
                columns: table => new
                {
                    fatura_kalem_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fatura_id = table.Column<long>(type: "bigint", nullable: false),
                    kalem_tipi = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    aciklama = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    miktar = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    birim_fiyat = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    tutar = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fatura_kalemi", x => x.fatura_kalem_id);
                    table.ForeignKey(
                        name: "fk_fatura_kalemi_fatura",
                        column: x => x.fatura_id,
                        principalTable: "fatura",
                        principalColumn: "fatura_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_kullanici_id",
                table: "audit_log",
                column: "kullanici_id");

            migrationBuilder.CreateIndex(
                name: "idx_audit_log_varlik",
                table: "audit_log",
                columns: new[] { "varlik_tipi", "varlik_id" });

            migrationBuilder.CreateIndex(
                name: "IX_endeks_okuma_is_emri_id",
                table: "endeks_okuma",
                column: "is_emri_id");

            migrationBuilder.CreateIndex(
                name: "IX_endeks_okuma_kullanici_id",
                table: "endeks_okuma",
                column: "kullanici_id");

            migrationBuilder.CreateIndex(
                name: "IX_endeks_okuma_sayac_id",
                table: "endeks_okuma",
                column: "sayac_id");

            migrationBuilder.CreateIndex(
                name: "IX_endeks_okuma_sozlesme_id",
                table: "endeks_okuma",
                column: "sozlesme_id");

            migrationBuilder.CreateIndex(
                name: "IX_entegrasyon_outbox_fatura_id",
                table: "entegrasyon_outbox",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "idx_entegrasyon_outbox_durum",
                table: "entegrasyon_outbox",
                column: "durum");

            migrationBuilder.CreateIndex(
                name: "uq_entegrasyon_outbox_idempotency",
                table: "entegrasyon_outbox",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fatura_okuma_id",
                table: "fatura",
                column: "okuma_id");

            migrationBuilder.CreateIndex(
                name: "idx_fatura_donem",
                table: "fatura",
                column: "donem");

            migrationBuilder.CreateIndex(
                name: "idx_fatura_durum",
                table: "fatura",
                column: "durum");

            migrationBuilder.CreateIndex(
                name: "idx_fatura_sozlesme_id",
                table: "fatura",
                column: "sozlesme_id");

            migrationBuilder.CreateIndex(
                name: "uq_fatura_fatura_no",
                table: "fatura",
                column: "fatura_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fatura_kalemi_fatura_id",
                table: "fatura_kalemi",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "uq_il_il_adi",
                table: "il",
                column: "il_adi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ilce_il_id",
                table: "ilce",
                column: "il_id");

            migrationBuilder.CreateIndex(
                name: "uq_ilce_il_adi",
                table: "ilce",
                columns: new[] { "il_id", "ilce_adi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_is_emirleri_atanan_kullanici_id",
                table: "is_emirleri",
                column: "atanan_kullanici_id");

            migrationBuilder.CreateIndex(
                name: "IX_is_emirleri_sayac_id",
                table: "is_emirleri",
                column: "sayac_id");

            migrationBuilder.CreateIndex(
                name: "IX_is_emirleri_tuketim_noktasi_id",
                table: "is_emirleri",
                column: "tuketim_noktasi_id");

            migrationBuilder.CreateIndex(
                name: "idx_kullanicilar_rol_id",
                table: "kullanicilar",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "uq_kullanicilar_e_posta",
                table: "kullanicilar",
                column: "e_posta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_kullanicilar_kullanici_adi",
                table: "kullanicilar",
                column: "kullanici_adi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_roller_rol_adi",
                table: "roller",
                column: "rol_adi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sayaclar_created_by",
                table: "sayaclar",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_sayaclar_tuketim_noktasi_id",
                table: "sayaclar",
                column: "tuketim_noktasi_id");

            migrationBuilder.CreateIndex(
                name: "IX_sayaclar_updated_by",
                table: "sayaclar",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_sozlesmeler_abone_id",
                table: "sozlesmeler",
                column: "abone_id");

            migrationBuilder.CreateIndex(
                name: "IX_sozlesmeler_tarife_id",
                table: "sozlesmeler",
                column: "tarife_id");

            migrationBuilder.CreateIndex(
                name: "IX_sozlesmeler_tuketim_noktasi_id",
                table: "sozlesmeler",
                column: "tuketim_noktasi_id");

            migrationBuilder.CreateIndex(
                name: "IX_tuketim_noktasi_created_by",
                table: "tuketim_noktasi",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_tuketim_noktasi_ilce_id",
                table: "tuketim_noktasi",
                column: "ilce_id");

            migrationBuilder.CreateIndex(
                name: "IX_tuketim_noktasi_updated_by",
                table: "tuketim_noktasi",
                column: "updated_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "Bildirimler");

            migrationBuilder.DropTable(
                name: "entegrasyon_outbox");

            migrationBuilder.DropTable(
                name: "fatura_kalemi");

            migrationBuilder.DropTable(
                name: "fatura");

            migrationBuilder.DropTable(
                name: "endeks_okuma");

            migrationBuilder.DropTable(
                name: "is_emirleri");

            migrationBuilder.DropTable(
                name: "sozlesmeler");

            migrationBuilder.DropTable(
                name: "sayaclar");

            migrationBuilder.DropTable(
                name: "aboneler");

            migrationBuilder.DropTable(
                name: "tarifeler");

            migrationBuilder.DropTable(
                name: "tuketim_noktasi");

            migrationBuilder.DropTable(
                name: "ilce");

            migrationBuilder.DropTable(
                name: "kullanicilar");

            migrationBuilder.DropTable(
                name: "il");

            migrationBuilder.DropTable(
                name: "roller");
        }
    }
}
