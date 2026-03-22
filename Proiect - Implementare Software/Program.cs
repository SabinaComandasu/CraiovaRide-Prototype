using Proiect_Implementare_Software.Data;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Services;
using Proiect_Implementare_Software.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Proiect___Implementare_Software.Repositories;
using Proiect___Implementare_Software.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Proiect_Implementare_Software.Models;
using Proiect_Implementare_Software.Utilities;

var builder = WebApplication.CreateBuilder(args);

// 📦 Register services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Identity with roles
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// ✅ Custom services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
builder.Services.AddScoped<IRideService, RideService>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

// ✅ Seed Roles and Admin
async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Optional: Seed default admin
    var adminEmail = "admin@craiovaride.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(newAdmin, "Admin@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}

// 🌱 Product seeder
async Task SeedProductsAsync(AppDbContext db, IWebHostEnvironment env)
{
    const int ExpectedCount = 39;

    var pdfsDir = Path.Combine(env.WebRootPath, "pdfs");
    Directory.CreateDirectory(pdfsDir);
    var logoPath = Path.Combine(env.WebRootPath, "images", "logo.png");

    // Always regenerate all PDFs so layout changes take effect immediately
    bool needsDbSeed = db.Products.Count() != ExpectedCount;
    if (!needsDbSeed)
    {
        // Only regenerate PDFs, skip DB changes
        // (products data is defined below, so we fall through to the loop)
    }
    else
    {
        db.Products.RemoveRange(db.Products.ToList());
        await db.SaveChangesAsync();
    }

    static string[] Lines(string desc, string[] specs, string idealFor, decimal price) =>
        new[] { "", desc, "" }
        .Concat(new[] { "B:Specificatii:" })
        .Concat(specs.Select(s => "  - " + s))
        .Concat(new[] { "  - Pret estimat: " + price.ToString("F2") + " RON", "", "B:Ideal pentru:", idealFor })
        .ToArray();

    var products = new (string Name, string Icon, string Category, decimal Price, string PdfFile, string[] PdfLines)[]
    {
        // ── CATEGORIA 1: Transport persoane ─────────────────────────────────
        ("CraiovaRide Standard", "🚗", "Transport persoane", 15.00m, "standard.pdf",
            Lines("Cursa economica pentru deplasari zilnice in Craiova.",
                new[]{"Pana la 4 pasageri","Aer conditionat inclus","Sofer licentiat si verificat","Urmarire GPS in timp real","Disponibil 24/7"},
                "Naveta zilnica, deplasari scurte, comisioane.", 15.00m)),
        ("CraiovaRide Comfort", "🛋️", "Transport persoane", 22.00m, "comfort.pdf",
            Lines("Confort sporit si experienta placuta de calatorie.",
                new[]{"Pana la 4 pasageri","Spatiu extra pentru picioare","Clima control (incalzire si racire)","Incarcator USB la bord","Sofer profesionist si curtezos"},
                "Calatorii de afaceri, utilizatori care doresc confort sporit.", 22.00m)),
        ("CraiovaRide Premium", "💎", "Transport persoane", 35.00m, "premium.pdf",
            Lines("Transport premium pentru cei care aleg cele mai bune servicii.",
                new[]{"Sedan de lux sau SUV high-end","Pana la 4 pasageri","Apa gratuita la bord","Expediere prioritara (sub 5 minute)","Sofer top-rated, multilingv"},
                "Intalniri VIP, ocazii speciale, clienti corporativi.", 35.00m)),
        ("CraiovaRide XL", "🚐", "Transport persoane", 28.00m, "xl.pdf",
            Lines("Transport spatios pentru grupuri mari sau bagaje voluminoase.",
                new[]{"Pana la 7 pasageri","Minivan sau MPV larg","Portbagaj generos","Pret avantajos pentru grupuri","Aer conditionat pe toata suprafata"},
                "Familii, grupuri de prieteni, calatorii cu bagaje multe.", 28.00m)),
        ("CraiovaRide Pet", "🐾", "Transport persoane", 20.00m, "pet.pdf",
            Lines("Cursa prietenoasa cu animale de companie.",
                new[]{"Animale mici si medii acceptate","Protectie tapiterie inclusa","Sofer iubitor de animale","Husa protectie pentru scaun","Disponibil 24/7"},
                "Calatorii cu pisici, caini sau alte animale de companie.", 20.00m)),
        ("CraiovaRide Women Driver", "👩", "Transport persoane", 18.00m, "women_driver.pdf",
            Lines("Sofer femeie pentru pasagere care prefera aceasta optiune.",
                new[]{"Sofer femeie verificata si licentiata","GPS live partajat","Vehicul curat si bine intretinut","Disponibil 24/7","Evaluari ridicate garantate"},
                "Femei care prefera un sofer femeie pentru confort si siguranta.", 18.00m)),
        ("Ride to Airport", "✈️", "Transport persoane", 55.00m, "ride_to_airport.pdf",
            Lines("Transfer garantat la aeroport, fara stres.",
                new[]{"Punctualitate garantata","Bagaje incluse fara costuri suplimentare","Sofer cu tablita cu numele tau","Vehicul curat si confortabil","Monitorizare trafic in timp real"},
                "Calatori, oameni de afaceri, zboruri importante.", 55.00m)),
        ("Ride from Airport", "🛬", "Transport persoane", 55.00m, "ride_from_airport.pdf",
            Lines("Intampinare la aeroport si transfer la destinatie.",
                new[]{"Asteptare garantata dupa aterizare","Monitorizare zbor in timp real","Sofer cu tablita cu numele tau","Bagaje incluse","Disponibil 24/7"},
                "Sosiri la aeroport, turisti, oameni de afaceri.", 55.00m)),
        ("Ride for Events", "🎉", "Transport persoane", 40.00m, "ride_events.pdf",
            Lines("Transport elegant pentru evenimente speciale.",
                new[]{"Vehicul premium curat si elegant","Sofer in tinuta formala","Deschiderea portierelor inclusa","Flexibilitate la ore de noapte","Rezervare anticipata recomandata"},
                "Nunti, gale, concerte, botezuri, petreceri private.", 40.00m)),
        ("Late Night Ride", "🌙", "Transport persoane", 25.00m, "latenight.pdf",
            Lines("Cursa sigura si verificata in intervalul nocturn.",
                new[]{"Disponibil 22:00 - 06:00","Soferi verificati suplimentar","GPS live partajat cu contacte","Buton SOS in aplicatie","Expediere prioritara noaptea"},
                "Ture de noapte, reveniri dupa evenimente, siguranta nocturna.", 25.00m)),
        ("Family Ride", "👨‍👩‍👧‍👦", "Transport persoane", 30.00m, "family.pdf",
            Lines("Transport confortabil si sigur pentru intreaga familie.",
                new[]{"Pana la 6 pasageri","Scaun auto copil disponibil la cerere","Sofer family-friendly si rabdator","Spatiu generos pentru bagaje","Verificari de siguranta suplimentare"},
                "Familii cu copii, excursii de familie, deplasari in grup.", 30.00m)),
        ("Eco Ride", "🌿", "Transport persoane", 18.00m, "eco.pdf",
            Lines("Cursa prietenoasa cu mediul, cu vehicul electric sau hibrid.",
                new[]{"Vehicul electric sau hibrid certificat","Emisii CO2 zero sau minime","Cabina silentioasa","Pana la 4 pasageri","Sofer eco-certificat"},
                "Calatori eco-constienti, deplasari sustenabile in oras.", 18.00m)),
        ("Electric Ride", "⚡", "Transport persoane", 20.00m, "electric.pdf",
            Lines("Cursa 100% electrica pentru o mobilitate curata.",
                new[]{"Vehicul 100% electric","Autonomie mare (300+ km)","Cabina ultrasilentioasa","Pana la 4 pasageri","Incarcator USB la bord"},
                "Deplasari sustenabile, calatori care sustin energia verde.", 20.00m)),
        ("Shared Ride", "🤝", "Transport persoane", 10.00m, "shared.pdf",
            Lines("Cursa impartita cu alti pasageri - pret minim.",
                new[]{"Pana la 3 pasageri din surse diferite","Traseu optimizat automat","GPS in timp real","Eco-friendly prin impartire","Cel mai mic pret disponibil"},
                "Naveta economica, calatori solo cu buget redus.", 10.00m)),
        ("Business Ride", "💼", "Transport persoane", 45.00m, "business.pdf",
            Lines("Transport executiv pentru profesionisti si companii.",
                new[]{"Sedan sau executive car","Chitanta fiscala furnizata","Wi-Fi 4G la bord","Sofer in tinuta profesionala","Rezervare prioritara garantata"},
                "Clienti corporativi, executivi, intalniri de afaceri.", 45.00m)),

        // ── CATEGORIA 2: Livrare ─────────────────────────────────────────────
        ("Livrare colet mic", "📦", "Livrare", 12.00m, "colet_mic.pdf",
            Lines("Livrare rapida pentru colete mici pana la 5 kg.",
                new[]{"Greutate maxima: 5 kg","Dimensiuni max: 40x30x20 cm","Livrare aceeasi zi in oras","Tracking in timp real","Confirmare prin semnatura"},
                "Documente, cadouri mici, obiecte personale.", 12.00m)),
        ("Livrare colet mare", "📫", "Livrare", 18.00m, "colet_mare.pdf",
            Lines("Livrare pentru colete mari si grele, pana la 25 kg.",
                new[]{"Greutate maxima: 25 kg","Dimensiuni mari acceptate","Ajutor la incarcare/descarcare","Tracking in timp real","Semnatura la livrare"},
                "Electronice, mobilier mic, comenzi online voluminoase.", 18.00m)),
        ("Livrare express", "⚡", "Livrare", 20.00m, "express.pdf",
            Lines("Livrare urgenta cu garantie de 60 de minute.",
                new[]{"Garantie 60 minute in oras","Greutate maxima: 10 kg","Prioritate maxima in coada","Tracking la fiecare 5 minute","Confirmare foto la livrare"},
                "Documente urgente, livrari sensibile la timp.", 20.00m)),
        ("Livrare documente", "📄", "Livrare", 10.00m, "documente.pdf",
            Lines("Livrare securizata de documente si acte oficiale.",
                new[]{"Securitate maxima garantata","Semnatura obligatorie la primire","Confirmare foto a livrarii","Maxim 1 kg","Discretie totala"},
                "Contracte, acte oficiale, documente juridice, corespondenta.", 10.00m)),
        ("Livrare cadouri", "🎁", "Livrare", 15.00m, "cadouri.pdf",
            Lines("Livrare atenta de cadouri ambalate, cu surpriza pastrata.",
                new[]{"Ambalaj protejat in tranzit","Discretie totala garantata","Mesaj personalizat inclus","Confirmare foto la livrare","Disponibil sarbatori si weekend"},
                "Cadouri surpriza, aniversari, zile de nastere, Craciun.", 15.00m)),
        ("Livrare cumparaturi", "🛍️", "Livrare", 14.00m, "cumparaturi.pdf",
            Lines("Ridicare si livrare cumparaturi de la magazine la domiciliu.",
                new[]{"Ridicare din orice magazin din oras","Livrare la domiciliu","Greutate maxima: 15 kg","Tracking in timp real","Verificare produse la ridicare"},
                "Cumparaturi de supermarket, magazine, comenzi click & collect.", 14.00m)),
        ("Livrare food pickup", "🍔", "Livrare", 12.00m, "food_pickup.pdf",
            Lines("Ridicare mancare de la restaurantul favorit si livrare rapida.",
                new[]{"Ridicare din orice restaurant din oras","Pungi termice pentru mentinerea temperaturii","Livrare in 30-45 minute","Greutate maxima: 5 kg","Confirmare preluare"},
                "Comenzi de la restaurante care nu au livrare proprie.", 12.00m)),
        ("Livrare same-day", "🕐", "Livrare", 22.00m, "same_day.pdf",
            Lines("Garantie de livrare in aceeasi zi pentru comenzile plasate devreme.",
                new[]{"Garantie livrare aceeasi zi","Comenzi acceptate pana la ora 18:00","Tracking in timp real","Confirmare prin semnatura","Notificare SMS la livrare"},
                "Pachete urgente, comenzi de ultima ora, livrari critice.", 22.00m)),

        // ── CATEGORIA 3: Inchirieri ──────────────────────────────────────────
        ("Inchiriere trotineta", "🛴", "Inchirieri", 5.00m, "trotineta.pdf",
            Lines("Inchiriere trotineta mecanica pentru deplasari scurte in oras.",
                new[]{"Trotineta verificata si intretinuta","Casca de protectie inclusa","Minim 1 ora","Returnare la orice statie CraiovaRide","Disponibil 06:00 - 22:00"},
                "Deplasari scurte in oras, turisti, agrement.", 5.00m)),
        ("Inchiriere bicicleta", "🚲", "Inchirieri", 8.00m, "bicicleta.pdf",
            Lines("Inchiriere bicicleta de oras pentru mobilitate urbana.",
                new[]{"Bicicleta de oras cu 7 viteze","Casca si lacata incluse","Cos portbagaj inclus","Pana la 8 ore","Returnare la orice statie CraiovaRide"},
                "Ture prin oras, agrement, deplasari de 5-15 km.", 8.00m)),
        ("Inchiriere bicicleta electrica", "⚡🚲", "Inchirieri", 12.00m, "bicicleta_electrica.pdf",
            Lines("Inchiriere e-bike cu motor electric pentru deplasari mai lungi.",
                new[]{"Motor electric 250W","Autonomie baterie: 40-60 km","Casca inclusa","Aplicatie pentru monitorizare baterie","Returnare la statii desemnate"},
                "Deplasari mai lungi, zone cu dealuri, calatori activi.", 12.00m)),
        ("Inchiriere scuter electric", "🛵", "Inchirieri", 15.00m, "scuter_electric.pdf",
            Lines("Inchiriere scuter electric pentru deplasari rapide in oras.",
                new[]{"Viteza maxima: 45 km/h","Autonomie: 60 km pe incarcare","Casca inclusa","Varsta minima: 18 ani (permis AM/A1)","Incarcare completa in 4 ore"},
                "Deplasari rapide prin oras, evitare trafic, mobilitate urbana.", 15.00m)),
        ("Inchiriere masina pe ora", "🚗", "Inchirieri", 35.00m, "masina_ora.pdf",
            Lines("Inchiriere masina cu ora pentru flexibilitate maxima.",
                new[]{"Asigurare CASCO si RCA incluse","Minim 1 ora de inchiriere","Permis de conducere min. 2 ani","Asistenta rutiera 24/7","Carburant pe contul clientului"},
                "Comisioane, intalniri, deplasari scurte fara taximetru.", 35.00m)),
        ("Inchiriere masina pe zi", "📅", "Inchirieri", 200.00m, "masina_zi.pdf",
            Lines("Inchiriere masina pe zi intreaga pentru libertate totala.",
                new[]{"Asigurare CASCO si RCA incluse","Minim 1 zi (24 ore)","Permis de conducere min. 2 ani","Asistenta rutiera 24/7","200 km inclusi, 0.30 RON/km extra"},
                "Excursii, calatorii, deplasari pe distante lungi.", 200.00m)),

        // ── CATEGORIA 4: Servicii speciale ───────────────────────────────────
        ("Transport animale", "🐕", "Servicii speciale", 22.00m, "transport_animale.pdf",
            Lines("Transport specializat pentru animale de companie de toate dimensiunile.",
                new[]{"Spatiu dedicat si securizat pentru animale","Protectie completa tapiterie","Sofer specializat in transport animale","Ventilatie adecvata","Disponibil pentru animale mari si mici"},
                "Vizite la veterinar, relocari, calatorii cu animale mari.", 22.00m)),
        ("Transport bagaje voluminoase", "🧳", "Servicii speciale", 25.00m, "bagaje_voluminoase.pdf",
            Lines("Transport bagaje sau marfuri voluminoase cu vehicul adaptat.",
                new[]{"Vehicul SUV sau van cu spatiu extins","Ajutor la incarcare si descarcare","Asigurare bagaje inclusa","Greutate maxima: 50 kg","Disponibil pentru mutari partiale"},
                "Mutari partiale, echipamente sportive, bagaje supradimensionate.", 25.00m)),
        ("Transport persoane cu nevoi speciale", "♿", "Servicii speciale", 20.00m, "nevoi_speciale.pdf",
            Lines("Transport accesibil si adaptat pentru persoane cu dizabilitati.",
                new[]{"Vehicul adaptat cu rampa de acces","Sofer instruit pentru asistenta persoanelor cu dizabilitati","Spatiu pentru scaun rulant","Urcarea si coborarea asistata","Rabdare si discretie garantate"},
                "Persoane cu mobilitate redusa, utilizatori de scaun rulant.", 20.00m)),
        ("Child Seat Ride", "👶", "Servicii speciale", 20.00m, "child_seat.pdf",
            Lines("Cursa cu scaun auto omologat pentru bebelusi si copii mici.",
                new[]{"Scaun auto certificat CE (0-36 kg)","Potrivit pentru varste 0-12 ani","Sofer instruit in siguranta copiilor","Montare corecta verificata","Disponibil la cerere in avans"},
                "Familii cu bebelusi si copii mici, calatorii cu copii.", 20.00m)),
        ("VIP Ride", "⭐", "Servicii speciale", 80.00m, "vip.pdf",
            Lines("Experienta de transport de lux pentru clientii cei mai exigenti.",
                new[]{"Limuzina sau SUV de lux (Mercedes, BMW)","Concierge personal dedicat","Bauturi si gustari gratuite la bord","Perdea de confidentialitate disponibila","Sistem audio premium"},
                "Clienti VIP, gale de premiere, evenimente de lux, delegatii.", 80.00m)),
        ("Silent Ride", "🔇", "Servicii speciale", 22.00m, "silent.pdf",
            Lines("Cursa in liniste totala - fara conversatie, fara muzica.",
                new[]{"Politica de non-conversatie respectata","Muzica oprita (sau la cerere)","Cabina linistita garantata","Pana la 4 pasageri","Disponibil 24/7"},
                "Calatori care vor sa lucreze, sa se relaxeze sau sa doarma.", 22.00m)),
        ("Ride cu Wi-Fi", "📶", "Servicii speciale", 20.00m, "wifi.pdf",
            Lines("Cursa cu internet Wi-Fi 4G la bord inclus.",
                new[]{"Wi-Fi 4G/LTE de mare viteza inclus","Pana la 4 dispozitive conectate simultan","Fara limite de date in oras","Cabina curata si confortabila","Disponibil 24/7"},
                "Calatori care lucreaza, streaming, videoconferinte in masina.", 20.00m)),
        ("Ride pentru studenti", "🎓", "Servicii speciale", 12.00m, "studenti.pdf",
            Lines("Cursa la pret redus pentru studenti cu legitimatie valabila.",
                new[]{"Reducere 20% fata de tariful standard","Carnet de student necesar la urcare","Pana la 4 pasageri","Disponibil 24/7","Valabil pentru studenti ai institutiilor din Craiova"},
                "Studenti cu legitimatie valabila, deplasari catre facultate.", 12.00m)),
        ("Ride corporate", "🏢", "Servicii speciale", 45.00m, "corporate.pdf",
            Lines("Solutie de transport corporate pentru companii si angajati.",
                new[]{"Contract corporate cu facturare lunara","Factura fiscala pentru fiecare cursa","Sofer profesionist in tinuta formala","Wi-Fi la bord inclus","Raport lunar detaliat al curselor"},
                "Companii, angajati in deplasare, clienti de afaceri.", 45.00m)),
        ("Ride programat in avans", "🗓️", "Servicii speciale", 18.00m, "programat.pdf",
            Lines("Rezerva cursa cu minim 24 de ore inainte pentru garantie totala.",
                new[]{"Rezervare cu 24+ ore in avans","Sofer garantat la ora exacta","Confirmare prin SMS si email","Anulare gratuita cu 2 ore inainte","Prioritate la alocare sofer"},
                "Calatori cu program fix, zboruri, evenimente importante.", 18.00m)),
    };

    foreach (var (name, icon, category, price, pdfFile, pdfLines) in products)
    {
        var pdfPath = "/pdfs/" + pdfFile;
        var fullPdfPath = Path.Combine(pdfsDir, pdfFile);

        var pdfBytes = SimplePdfWriter.CreatePdf(name, pdfLines, logoPath);
        await File.WriteAllBytesAsync(fullPdfPath, pdfBytes);

        if (needsDbSeed)
        {
            db.Products.Add(new Product
            {
                Name = name,
                Icon = icon,
                Category = category,
                Price = price,
                Description = name,
                PdfPath = pdfPath
            });
        }
    }

    if (needsDbSeed)
        await db.SaveChangesAsync();
}

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "User", "Driver", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// 🔁 Seed roles and admin before request pipeline starts
using (var scope = app.Services.CreateScope())
{
    await SeedRolesAndAdminAsync(scope.ServiceProvider);
}

// 🛒 Seed products and generate PDFs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    await SeedProductsAsync(db, env);
}

// 🔧 Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 🌐 Default route based on authentication
app.MapGet("/", async context =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/Home/Index");
    }
    else
    {
        context.Response.Redirect("/Identity/Account/Login");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
