using BlitzMall_Backend.Data;
using BlitzMall_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BlitzMall_Backend
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db, IConfiguration configuration)
        {
            var roleNames = new[] { ("Buyer", "Buyer"), ("Seller", "Seller"), ("Admin", "Administrator") };
            foreach (var (name, desc) in roleNames)
            {
                if (!await db.Roles.AnyAsync(r => r.Name == name))
                    db.Roles.Add(new Role { Name = name, Description = desc });
            }
            await db.SaveChangesAsync();

            var adminRole     = await db.Roles.FirstAsync(r => r.Name == "Admin");
            var sellerRole    = await db.Roles.FirstAsync(r => r.Name == "Seller");
            var adminEmail    = configuration["Admin:Email"]    ?? "admin@blitzmall.ua";
            var adminPassword = configuration["Admin:Password"] ?? "Admin123!";

            User? adminUser;
            if (!await db.Users.AnyAsync(u => u.Email == adminEmail))
            {
                adminUser = new User
                {
                    Name         = "Admin",
                    Email        = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Status       = "Active",
                    RoleId       = adminRole.Id,
                    CreatedAt    = DateTime.UtcNow
                };
                db.Users.Add(adminUser);
                await db.SaveChangesAsync();
            }
            else
            {
                adminUser = await db.Users.FirstAsync(u => u.Email == adminEmail);
            }

            if (await db.Products.AnyAsync()) return;

            var brands = new[] { "Sony", "JBL", "Samsung", "Xiaomi", "Generic", "Nike", "Tefal" };
            foreach (var b in brands)
                db.Brands.Add(new Brand { Name = b, Description = b });
            await db.SaveChangesAsync();

            var brandDict = await db.Brands.ToDictionaryAsync(b => b.Name!, b => b.Id);

            var cats = new[]
            {
                ("Електроніка",        "Гаджети та техніка"),
                ("Дім і кухня",        "Товари для дому"),
                ("Одяг і взуття",      "Мода та стиль"),
                ("Краса і здоров'я",   "Косметика та догляд"),
                ("Спорт і відпочинок", "Спортивні товари"),
                ("Дитячі товари",      "Для дітей"),
                ("Зоотовари",          "Для тварин"),
            };
            foreach (var (name, desc) in cats)
                db.Categories.Add(new Category { Name = name, Description = desc });
            await db.SaveChangesAsync();

            var catDict = await db.Categories.ToDictionaryAsync(c => c.Name!, c => c.Id);

            var seller = new Seller
            {
                Name        = "BlitzMall Store",
                Description = "Офіційний магазин BlitzMall",
                UserId      = adminUser.Id,
                Email       = adminEmail,
                Phone       = "+380501234567",
                CreatedAt   = DateTime.UtcNow
            };
            db.Sellers.Add(seller);
            await db.SaveChangesAsync();

            var products = new[]
            {
                new { Name="Бездротові навушники Sony WH-1000XM5",  Desc="Преміальні навушники з шумозаглушенням",          Price=1972m,  OldPrice=2560m, Qty=50,  BrandName="Sony",    CatName="Електроніка",    ImgUrl="https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&h=400&fit=crop", Badge="-30%" },
                new { Name="Розумна колонка JBL Charge 5",          Desc="Портативна колонка з 20 годинами роботи",         Price=1250m,  OldPrice=1420m, Qty=30,  BrandName="JBL",     CatName="Електроніка",    ImgUrl="https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=500&h=400&fit=crop", Badge="ХІТ" },
                new { Name="Термокухоль 500 мл",                    Desc="Зберігає температуру до 12 годин",                Price=298m,   OldPrice=350m,  Qty=200, BrandName="Generic", CatName="Дім і кухня",    ImgUrl="https://images.unsplash.com/photo-1514228742587-6b1558fcca3d?w=500&h=400&fit=crop", Badge="-15%" },
                new { Name="Рюкзак міський 20 л",                   Desc="Водовідштовхуючий рюкзак для міста",             Price=890m,   OldPrice=0m,    Qty=80,  BrandName="Nike",    CatName="Спорт і відпочинок", ImgUrl="https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=500&h=400&fit=crop", Badge="НОВЕ" },
                new { Name="Смартфон Samsung Galaxy A55",           Desc="6.6\" AMOLED, 128 ГБ, камера 50 МП",            Price=12990m, OldPrice=14500m,Qty=25,  BrandName="Samsung", CatName="Електроніка",    ImgUrl="https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500&h=400&fit=crop", Badge="-10%" },
                new { Name="Електрочайник Tefal 1.7л",              Desc="Швидкий нагрів, 2400 Вт",                        Price=650m,   OldPrice=800m,  Qty=60,  BrandName="Tefal",   CatName="Дім і кухня",    ImgUrl="https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=500&h=400&fit=crop", Badge="-19%" },
                new { Name="Термос харчовий 1л",                    Desc="Нержавіюча сталь, термос для їжі",               Price=410m,   OldPrice=0m,    Qty=100, BrandName="Generic", CatName="Дім і кухня",    ImgUrl="https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?w=500&h=400&fit=crop", Badge="" },
                new { Name="Зарядна станція Xiaomi 300W",           Desc="Портативна зарядна станція для техніки",         Price=8990m,  OldPrice=11000m,Qty=15,  BrandName="Xiaomi",  CatName="Електроніка",    ImgUrl="https://images.unsplash.com/photo-1609592806596-b8d61907e7d1?w=500&h=400&fit=crop", Badge="-18%" },
                new { Name="Плед плюшевий 150×200",                 Desc="М'який плед для затишку",                       Price=750m,   OldPrice=0m,    Qty=150, BrandName="Generic", CatName="Дім і кухня",    ImgUrl="https://images.unsplash.com/photo-1580301762395-f6a71ee4a25a?w=500&h=400&fit=crop", Badge="" },
                new { Name="LED-лампа розумна Xiaomi",              Desc="WiFi лампа, 16 мільйонів кольорів",             Price=290m,   OldPrice=350m,  Qty=300, BrandName="Xiaomi",  CatName="Дім і кухня",    ImgUrl="https://images.unsplash.com/photo-1550009158-9ebf69173e03?w=500&h=400&fit=crop", Badge="-17%" },
            };

            foreach (var p in products)
            {
                var product = new Product
                {
                    Name        = p.Name,
                    Description = p.Desc,
                    Price       = p.Price,
                    Quantity    = p.Qty,
                    SellerId    = seller.Id,
                    CategoryId  = catDict[p.CatName],
                    BrandId     = brandDict[p.BrandName],
                    IsActive    = true,
                    CreatedDate = DateTime.UtcNow,
                };
                db.Products.Add(product);
                await db.SaveChangesAsync();

                if (!string.IsNullOrEmpty(p.ImgUrl))
                {
                    db.ProdImgs.Add(new ProdImg { ProductId = product.Id, UrlImage = p.ImgUrl });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
