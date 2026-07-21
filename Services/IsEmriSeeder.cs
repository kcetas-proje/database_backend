using Bogus;
using KcetasAboneApi.Models;
using KcetasSeeder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Services;

public class IsEmriSeeder
{
    private readonly AppDbContext _context;
    private readonly Faker _faker = new("tr");

    private const int BatchSize = 500;


    public IsEmriSeeder(AppDbContext context)
    {
        _context = context;
    }



    public async Task Generate(int adet)
    {
        Console.WriteLine("İş Emirleri oluşturuluyor...");


        _context.ChangeTracker.AutoDetectChangesEnabled = false;



        var tuketimNoktalari =
            await _context.TuketimNoktasis
            .AsNoTracking()
            .ToListAsync();


        var sayaclar =
            await _context.Sayaclars
            .AsNoTracking()
            .ToListAsync();



        var kullanicilar =
            await _context.Kullanicilars
            .AsNoTracking()
            .ToListAsync();



        if (!tuketimNoktalari.Any())
        {
            Console.WriteLine(
                "Tüketim noktası bulunamadı.");
            return;
        }



        if (!kullanicilar.Any())
        {
            Console.WriteLine(
                "Kullanıcı bulunamadı.");
            return;
        }




        int sonNo =
            await NumaraGenerator.GetLastIsEmriNumber(_context);



        var liste =
            new List<IsEmirleri>();




        string[] tipler =
        {
            "DEGISTIRME",
            "SOKME",
            "KESME",
            "ACMA",
            "ENDEKS_OKUMA",
            "SAYAC_ARIZA",
            "MUHURLEME",
            "KESIF_INCELEME",
            "YENI_BAGLANTI",
            "ENERJI_ACMA"
        };




        for (int i = 0; i < adet; i++)
        {

            sonNo++;



            var tuketim =
                _faker.PickRandom(
                    tuketimNoktalari);



            var sayac =
                sayaclar
                .FirstOrDefault(x =>
                    x.TuketimNoktasiId ==
                    tuketim.TuketimNoktasiId);



            var tip =
                _faker.PickRandom(tipler);



            var created =
                _faker.Date
                .Past(1)
                .ToUniversalTime();



            var durum =
                _faker.PickRandom(
                    "ACIK",
                    "ATANDI",
                    "YOLDA",
                    "SAHADA",
                    "TAMAMLANDI",
                    "IPTAL",
                    "BASARISIZ");



            var isEmri =
                new IsEmirleri
                {

                    IsEmriNo =
                        $"ISEM-{sonNo:D5}",


                    TuketimNoktasiId =
                        tuketim.TuketimNoktasiId,


                    SayacId =
                        sayac?.SayacId,


                    Tip = tip,


                    Oncelik =
                        tip == "ACIL"
                        ? "YUKSEK"
                        : "NORMAL",



                    Durum = durum,


                    AtananKullaniciId =
                        _faker
                        .PickRandom(kullanicilar)
                        .KullaniciId,



                    CreatedAt = created,



                    PlanlananTarih =
                        created
                        .AddDays(
                            _faker.Random.Int(1, 10)),



                    UpdatedAt =
                        durum == "TAMAMLANDI"
                        ? created.AddDays(3)
                        : null,



                    PanoDirekNo =
                        $"PD-{_faker.Random.Number(1000, 9999)}",



                    MuhurNo =
                        $"MHR-{_faker.Random.Number(100000, 999999)}",



                    TutanakNo =
                        $"TTK-{_faker.Random.Number(10000, 99999)}",



                    AboneDurumu =
                        "AKTIF",



                    SayacDurumu =
                        "NORMAL",



                    IncelemeNotu =
                        "Saha kontrolü gerçekleştirildi."

                };





            switch (tip)
            {


                case "ARIZA":


                    isEmri.ArizaTipi =
                        _faker.PickRandom(
                            "SAYAC_ARIZASI",
                            "SIGORTA_ARIZASI",
                            "ENERJI_YOK",
                            "KABLO_ARIZASI");



                    isEmri.Gerekce =
                        "Müşteri arıza bildirimi";



                    isEmri.SahaSonucu =
                        durum == "TAMAMLANDI"
                        ?
                        "Arıza giderildi."
                        :
                        "Ekip yönlendirildi.";


                    break;





                case "KESIF":


                    isEmri.KesifSonucu =
                        _faker.PickRandom(
                        "Bağlantı uygun.",
                        "Ek tesis gerekli.",
                        "Proje kontrol edilmeli.");



                    isEmri.YapiTesisTipi =
                        _faker.PickRandom(
                        "MESKEN",
                        "TICARETHANE");



                    isEmri.HatMesafesi =
                        $"{_faker.Random.Int(20, 400)} m";



                    isEmri.TalepGucu =
                        $"{_faker.Random.Int(5, 200)} kW";



                    isEmri.Gerekce =
                        "Yeni bağlantı keşfi";


                    break;






                case "ACMA":


                    isEmri.AcmaNoktasi =
                        "Ana Pano";



                    isEmri.Gerekce =
                        "Enerji açma işlemi";



                    isEmri.SahaSonucu =
                        "Enerji verme işlemi tamamlandı.";

                    break;







                case "KESME":


                    isEmri.KesmeNoktasi =
                        "Kolon Sigortası";



                    isEmri.KesmeNedeni =
                        _faker.PickRandom(
                        "BORC",
                        "TALEP",
                        "KACAK");



                    isEmri.Gerekce =
                        "Enerji kesme işlemi";



                    isEmri.SahaSonucu =
                        "Kesme işlemi gerçekleştirildi.";

                    break;








                case "SAYAC_DEGISIM":



                    isEmri.SokulenSeriNo =
                        sayac?.SeriNo ??
                        $"OLD-{_faker.Random.Number(10000, 99999)}";



                    isEmri.YeniSeriNo =
                        $"NEW-{_faker.Random.Number(10000, 99999)}";



                    isEmri.EskiMuhurNo =
                        $"ESK-{_faker.Random.Number(10000, 99999)}";



                    isEmri.YeniMuhurNo =
                        $"YEN-{_faker.Random.Number(10000, 99999)}";



                    isEmri.DamgaYili =
                        _faker.Random.Int(2020, 2026);



                    isEmri.SahaSonucu =
                        "Sayaç değişimi tamamlandı.";


                    break;







                case "ENDEKS_OKUMA":



                    isEmri.Gerekce =
                        "Periyodik sayaç okuma";



                    isEmri.SahaSonucu =
                        "Endeks okuma için oluşturuldu.";


                    break;

            }





            liste.Add(isEmri);




            if (liste.Count >= BatchSize)
            {

                _context.IsEmirleris.AddRange(liste);


                await _context.SaveChangesAsync();


                _context.ChangeTracker.Clear();


                liste.Clear();

            }

        }




        if (liste.Any())
        {

            _context.IsEmirleris.AddRange(liste);


            await _context.SaveChangesAsync();


            _context.ChangeTracker.Clear();

        }




        _context.ChangeTracker.AutoDetectChangesEnabled = true;



        Console.WriteLine(
            $"{adet} adet iş emri oluşturuldu.");

    }
}
