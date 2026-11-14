using car_service.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace car_service.Data;

/// <summary>Создаёт БД SQLite и наполняет её демонстрационными данными при первом запуске.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp, IConfiguration config)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        await db.Database.EnsureCreatedAsync();

        if (!await db.SiteSettings.AnyAsync())
        {
            db.SiteSettings.Add(new SiteSettings
            {
                CompanyName = "АвтоПрофи",
                Slogan = "Честный ремонт без лишних работ",
                Phone = "+7 (900) 123-45-67",
                CallPhone = "+79001234567",
                Email = "info@autoprofi.example",
                Address = "г. Москва, ул. Автомобильная, д. 15",
                WorkHoursText = "Пн–Сб: 9:00–20:00, Вс — выходной",
                MapEmbedUrl = "https://yandex.ru/map-widget/v1/?ll=37.617635%2C55.755814&z=15&pt=37.617635%2C55.755814%2Cpm2rdm",
                VkUrl = "https://vk.com/",
                TelegramUrl = "https://t.me/",
                AboutShort = "АвтоПрофи — независимый автосервис полного цикла. Работаем с 2014 года, обслуживаем легковые автомобили и кроссоверы любых марок. Мастера с опытом от 7 лет, собственный склад запчастей и прозрачные цены — согласовываем каждую работу до её начала.",
                AboutFull = "Мы начинали как небольшой гаражный бокс на два подъёмника, а сегодня это сервис на шесть постов с собственным складом запчастей и диагностическим оборудованием дилерского уровня.\n\nВсе мастера проходят регулярное обучение и сертификацию у производителей оборудования и запчастей. Мы не навязываем лишних работ: перед ремонтом вы получаете смету, а после — старые детали и подробный заказ-наряд.\n\nНа все работы даём гарантию 12 месяцев, на запчасти — гарантию производителя. Ведём электронную историю обслуживания каждого автомобиля, поэтому при повторном визите мастер уже знает, что и когда менялось.",
                YearsOnMarket = 12,
                CarsServiced = 15000,
                WarrantyMonths = 12,
                LegalInfo = "ООО «АвтоПрофи», ИНН 0000000000, ОГРН 0000000000000. Информация на сайте не является публичной офертой.",
                DevelopersInfo = "Учебный проект. Команда разработки: студенты группы ___ — Фамилия И. О. (backend), Фамилия И. О. (frontend).",
                WorkStart = new TimeOnly(9, 0),
                WorkEnd = new TimeOnly(20, 0),
                SlotMinutes = 60,
                SlotCapacity = 3,
                BookingDaysAhead = 30,
                WorkingDays = "1,2,3,4,5,6"
            });
        }

        if (!await db.Users.AnyAsync())
        {
            // Демонстрационные учётные записи. Пароли можно переопределить в appsettings.json → Seed.
            var seed = config.GetSection("Seed");
            AddUser(db, hasher, "admin@autoprofi.ru", "Администратор", seed["AdminPassword"] ?? "Admin123!", Roles.Admin, "+7 (900) 000-00-01");
            AddUser(db, hasher, "manager@autoprofi.ru", "Мария Менеджерова", seed["ManagerPassword"] ?? "Manager123!", Roles.Manager, "+7 (900) 000-00-02");
            AddUser(db, hasher, "user@autoprofi.ru", "Иван Клиентов", seed["UserPassword"] ?? "User123!", Roles.User, "+7 (900) 000-00-03", "Kia Rio 2019");
            await db.SaveChangesAsync();
        }

        if (!await db.Services.AnyAsync())
        {
            db.Services.AddRange(
                new ServiceItem { Title = "Компьютерная диагностика", Icon = "diag", PriceFrom = 1200, DurationMinutes = 60, SortOrder = 1, IsFeatured = true,
                    ShortDescription = "Считывание ошибок всех блоков, проверка датчиков и параметров двигателя.",
                    Description = "Подключаем дилерский и мультимарочный сканер, считываем ошибки всех электронных блоков, анализируем параметры работы двигателя и АКПП в реальном времени. По итогам выдаём распечатку с расшифровкой и рекомендациями." },
                new ServiceItem { Title = "Замена масла и фильтров", Icon = "oil", PriceFrom = 900, DurationMinutes = 60, SortOrder = 2, IsFeatured = true,
                    ShortDescription = "Моторное масло, масляный, воздушный и салонный фильтры.",
                    Description = "Замена моторного масла с фильтром, а также воздушного и салонного фильтров. Можно приехать со своими материалами или выбрать из наличия на складе. Сбрасываем сервисный интервал." },
                new ServiceItem { Title = "Ремонт тормозной системы", Icon = "brake", PriceFrom = 1500, DurationMinutes = 90, SortOrder = 3, IsFeatured = true,
                    ShortDescription = "Колодки, диски, суппорты, замена тормозной жидкости.",
                    Description = "Замена тормозных колодок и дисков, обслуживание и ремонт суппортов, прокачка и замена тормозной жидкости. Проверяем эффективность торможения после работ." },
                new ServiceItem { Title = "Шиномонтаж", Icon = "tire", PriceFrom = 1600, DurationMinutes = 60, SortOrder = 4, IsFeatured = true,
                    ShortDescription = "Сезонная переобувка, балансировка, ремонт проколов.",
                    Description = "Сезонная замена шин, балансировка на современном стенде, ремонт проколов и порезов, хранение шин. Работаем с дисками до 22 дюймов и низкопрофильной резиной." },
                new ServiceItem { Title = "Ремонт подвески", Icon = "suspension", PriceFrom = 2000, DurationMinutes = 120, SortOrder = 5, IsFeatured = true,
                    ShortDescription = "Диагностика ходовой, замена амортизаторов, рычагов, сайлентблоков.",
                    Description = "Полная диагностика ходовой части на подъёмнике, замена амортизаторов, пружин, рычагов, шаровых опор, сайлентблоков и стоек стабилизатора. После ремонта — регулировка развала-схождения." },
                new ServiceItem { Title = "Ремонт двигателя", Icon = "engine", PriceFrom = 5000, DurationMinutes = 240, SortOrder = 6, IsFeatured = true,
                    ShortDescription = "Замена ГРМ, устранение течей, капитальный ремонт.",
                    Description = "Замена ремня или цепи ГРМ, прокладок и сальников, ремонт ГБЦ, капитальный ремонт двигателя. Даём гарантию на работы 12 месяцев." },
                new ServiceItem { Title = "Обслуживание кондиционера", Icon = "ac", PriceFrom = 2500, DurationMinutes = 60, SortOrder = 7,
                    ShortDescription = "Заправка фреоном, поиск утечек, антибактериальная обработка.",
                    Description = "Проверка системы кондиционирования на герметичность, заправка хладагентом с маслом, антибактериальная обработка испарителя, замена салонного фильтра." },
                new ServiceItem { Title = "Автоэлектрика", Icon = "battery", PriceFrom = 1000, DurationMinutes = 90, SortOrder = 8,
                    ShortDescription = "Аккумулятор, генератор, стартер, проводка, освещение.",
                    Description = "Проверка и замена аккумулятора, ремонт генератора и стартера, поиск утечек тока, ремонт проводки, установка дополнительного оборудования." },
                new ServiceItem { Title = "Кузовной ремонт", Icon = "body", PriceFrom = 3000, DurationMinutes = 240, SortOrder = 9,
                    ShortDescription = "Локальный ремонт, покраска элементов, полировка.",
                    Description = "Устранение вмятин и царапин, локальная и полная покраска элементов в камере, подбор краски по коду, восстановительная полировка кузова." }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Slides.AnyAsync())
        {
            db.Slides.AddRange(
                new Slide { SortOrder = 1, Tag = "Акция ноября", Title = "Бесплатная диагностика ходовой", Subtitle = "При любом ремонте подвески или тормозной системы — диагностика ходовой части в подарок.", ButtonText = "Записаться", ButtonUrl = "/Booking" },
                new Slide { SortOrder = 2, Tag = "Гарантия", Title = "12 месяцев гарантии на все работы", Subtitle = "Согласовываем каждую работу заранее. Старые запчасти — вам в руки.", ButtonText = "О сервисе", ButtonUrl = "/About" },
                new Slide { SortOrder = 3, Tag = "Сезон", Title = "Шиномонтаж без очередей", Subtitle = "Онлайн-запись на удобное время, балансировка и хранение шин.", ButtonText = "Все услуги", ButtonUrl = "/Services" }
            );
        }

        if (!await db.TeamMembers.AnyAsync())
        {
            db.TeamMembers.AddRange(
                new TeamMember { SortOrder = 1, Name = "Алексей Смирнов", Position = "Главный механик", Experience = 18, Description = "Сертифицированный специалист по двигателям и АКПП." },
                new TeamMember { SortOrder = 2, Name = "Дмитрий Кузнецов", Position = "Диагност-электрик", Experience = 11, Description = "Диагностика электронных систем, ремонт проводки." },
                new TeamMember { SortOrder = 3, Name = "Сергей Попов", Position = "Мастер по ходовой", Experience = 9, Description = "Подвеска, тормоза, развал-схождение." },
                new TeamMember { SortOrder = 4, Name = "Ольга Васильева", Position = "Мастер-приёмщик", Experience = 7, Description = "Консультирует, составляет сметы и ведёт историю обслуживания." }
            );
        }

        await db.SaveChangesAsync();

        if (!await db.Reviews.AnyAsync())
        {
            var user = await db.Users.FirstAsync(u => u.Role == Roles.User);
            db.Reviews.AddRange(
                new Review { AuthorName = "Андрей", Rating = 5, Status = ReviewStatus.Approved, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddDays(-20),
                    Text = "Приехал с непонятным стуком в подвеске — нашли за 15 минут, показали на подъёмнике. Ремонт сделали в тот же день, цена совпала со сметой." },
                new Review { AuthorName = "Екатерина", Rating = 5, Status = ReviewStatus.Approved, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddDays(-14),
                    Text = "Удобная онлайн-запись, приняли минута в минуту. Менеджер перезвонил и всё подтвердил. Буду обслуживаться здесь." },
                new Review { AuthorName = "Павел", Rating = 4, Status = ReviewStatus.Approved, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddDays(-7),
                    Text = "Меняли ГРМ. Отдали старые детали, всё объяснили. Минус звезда — зона ожидания маловата, но кофе хороший." },
                new Review { AuthorName = "Наталья", Rating = 5, Status = ReviewStatus.Approved, CreatedAt = DateTime.UtcNow.AddDays(-5),
                    Text = "Заправили кондиционер и сделали обработку, запах в салоне пропал. Спасибо!" },
                new Review { UserId = user.Id, AuthorName = user.FullName, Rating = 5, Status = ReviewStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Text = "Отличный сервис, поменяли масло за 40 минут. (Этот отзыв ожидает модерации — для демонстрации.)" }
            );

            // Выполненная запись демо-пользователя (дает право оставить отзыв) и одна предстоящая.
            var oil = await db.Services.FirstAsync(s => s.Icon == "oil");
            var diag = await db.Services.FirstAsync(s => s.Icon == "diag");
            var today = car_service.Services.AppTime.Today;
            db.Appointments.AddRange(
                new Appointment { UserId = user.Id, ServiceId = oil.Id, ClientName = user.FullName, ClientPhone = user.Phone ?? "", ClientEmail = user.Email, Car = user.Car,
                    Date = today.AddDays(-10), Time = new TimeOnly(11, 0), Status = AppointmentStatus.Completed, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new Appointment { UserId = user.Id, ServiceId = diag.Id, ClientName = user.FullName, ClientPhone = user.Phone ?? "", ClientEmail = user.Email, Car = user.Car,
                    Date = NextWorkday(today.AddDays(2)), Time = new TimeOnly(14, 0), Status = AppointmentStatus.New, Comment = "Горит Check Engine" }
            );
            db.Notifications.Add(new Notification { UserId = user.Id, Text = "Добро пожаловать в личный кабинет АвтоПрофи!" });
            await db.SaveChangesAsync();
        }
    }

    private static DateOnly NextWorkday(DateOnly d) => d.DayOfWeek == DayOfWeek.Sunday ? d.AddDays(1) : d;

    private static void AddUser(AppDbContext db, IPasswordHasher<User> hasher, string email, string name, string password, string role, string phone, string? car = null)
    {
        var u = new User { Email = email, FullName = name, Role = role, Phone = phone, Car = car };
        u.PasswordHash = hasher.HashPassword(u, password);
        db.Users.Add(u);
    }
}
