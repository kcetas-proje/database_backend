using Bogus;
using KcetasAboneApi.Models;

namespace KcetasSeeder.Helpers;

public static class FakerHelper
{
    private static readonly Faker Faker = new("tr");
    private static readonly string[] KayseriMahalleleri =
{
    "Ağırnas",
    "Alpaslan",
    "Altınoluk",
    "Anbar",
    "Argıncık",
    "Aydınlıkevler",
    "Bahçelievler",
    "Battalgazi",
    "Belsin",
    "Beştepeler",
    "Camiikebir",
    "Cırgalan",
    "Cumhuriyet",
    "Danışmentgazi",
    "Demokrasi",
    "Erenköy",
    "Erkilet",
    "Esentepe",
    "Fatih",
    "Fevzioğlu",
    "Germir",
    "Gesi",
    "Gevher Nesibe",
    "Gökkent",
    "Gültepe",
    "Hacısaki",
    "Hisarcık",
    "Hunat",
    "İldem",
    "Kazım Karabekir",
    "Keykubat",
    "Kılıçaslan",
    "Köşk",
    "Mevlana",
    "Mimarsinan",
    "Mithatpaşa",
    "Osmanlı",
    "Osman Kavuncu",
    "Sahabiye",
    "Selçuklu",
    "Seyrani",
    "Şeker",
    "Şirintepe",
    "Tacettin Veli",
    "Talatpaşa",
    "Tuna",
    "Yavuz Selim",
    "Yenidoğan",
    "Yeniköy",
    "Yenişehir",
    "Yeşil",
    "Yıldırım",
    "Yıldırım Beyazıt",
    "Yıldız",
    "Yunus Emre",
    "Zümrüt",
    "Barbaros",
    "Kayabaşı",
    "Karpuzatan",
    "Mehmet Akif Ersoy",
    "Saraybosna",
    "Sanayi",
    "Organize Sanayi",
    "Harbiş",
    "Alsancak",
    "Turgut Reis",
    "Reşadiye",
    "Kiçikapı",
    "Serçeönü",
    "Küçük Mustafa",
    "Akdam",
    "Akyazı",
    "Bahçesaray",
    "Bulgurcu",
    "Fırınönü",
    "Gönenkent",
    "Karakoyunlu",
    "Karamustafapaşa",
    "Örenşehir",
    "Saraycık",
    "Yenicami"
};

    // ===========================
    // KİŞİ BİLGİLERİ
    // ===========================

    public static string Ad()
        => Faker.Name.FirstName();

    public static string Soyad()
        => Faker.Name.LastName();

    public static string AdSoyad()
        => Faker.Name.FullName();

    public static string Telefon()
        => Faker.Phone.PhoneNumber("05#########");

    public static string Email()
        => Faker.Internet.Email();

   
    // ===========================
    // KİMLİK
    // ===========================

    public static string Tckn()
    {
        return Faker.Random.ReplaceNumbers("###########");
    }

    public static string Vkn()
    {
        return Faker.Random.ReplaceNumbers("##########");
    }

    // ===========================
    // ADRES
    // ===========================

    public static string Mahalle()
    => Faker.PickRandom(KayseriMahalleleri);

    public static string Sokak()
        => Faker.Address.StreetName();

    public static string BinaNo()
        => Faker.Random.Int(1, 150).ToString();

    public static string DaireNo()
        => Faker.Random.Int(1, 30).ToString();

    public static string AcikAdres()
        => $"{Mahalle()} Mah. {Sokak()} No:{BinaNo()} D:{DaireNo()}";

    // ===========================
    // SAYAÇ
    // ===========================

    public static Faz GetFaz()
        => Faker.PickRandom(Faz.TEK_FAZ, Faz.UC_FAZ);

    public static string SayacMarka()
        => Faker.PickRandom("Luna", "Makel", "Siemens", "Landis", "Klemsan");

    public static string SayacModel()
        => Faker.Random.AlphaNumeric(6).ToUpper();

    public static string MuhurNo()
        => $"MHR{Faker.Random.Number(100000, 999999)}";

    // ===========================
    // TÜKETİM
    // ===========================

    public static decimal BaglantiGucu()
        => Faker.Random.Decimal(3, 15);

    public static decimal Endeks()
        => Faker.Random.Decimal(1000, 50000);

    public static decimal Tuketim()
        => Faker.Random.Decimal(50, 600);

    // ===========================
    // TARİHLER
    // ===========================

    public static DateTime GecmisTarih()
        => Faker.Date.Past(3);

    public static DateTime GelecekTarih()
        => Faker.Date.Future(1);

    // ===========================
    // KOORDİNAT
    // Kayseri merkez civarı
    // ===========================

    public static decimal Latitude()
        => Faker.Random.Decimal(38.60m, 38.90m);

    public static decimal Longitude()
        => Faker.Random.Decimal(35.20m, 35.80m);

    // ===========================
    // PARA
    // ===========================

    public static decimal Para(decimal min, decimal max)
        => Faker.Random.Decimal(min, max);

    // ===========================
    // BOOLEAN
    // ===========================

    public static bool Bool()
        => Faker.Random.Bool();
}