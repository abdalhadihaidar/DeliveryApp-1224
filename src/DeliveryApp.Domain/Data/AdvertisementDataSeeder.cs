using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace DeliveryApp.Domain.Data
{
    public class AdvertisementDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Advertisement, Guid> _advertisementRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<AdvertisementDataSeeder> _logger;

        public AdvertisementDataSeeder(
            IRepository<Advertisement, Guid> advertisementRepository,
            IGuidGenerator guidGenerator,
            ILogger<AdvertisementDataSeeder> logger)
        {
            _advertisementRepository = advertisementRepository;
            _guidGenerator = guidGenerator;
            _logger = logger;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // Check if advertisements already exist
            var existingCount = await _advertisementRepository.GetCountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("Advertisements already exist ({Count} found). Skipping advertisement seeding.", existingCount);
                return;
            }

            _logger.LogInformation("Starting advertisement data seeding...");

            var advertisements = new List<Advertisement>
            {
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "توصيل مجاني للطلبات أكثر من 50 ليرة سورية",
                    Description = "احصل على توصيل مجاني لجميع الطلبات التي تزيد عن 50 ليرة سورية",
                    ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ca4b?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants",
                    StartDate = DateTime.Now.AddDays(-5),
                    EndDate = DateTime.Now.AddDays(30),
                    IsActive = true,
                    Priority = 1,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "عرض خاص: خصم 20% على البيتزا",
                    Description = "استمتع بخصم 20% على جميع أنواع البيتزا من مطاعمنا المختارة",
                    ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants?category=pizza",
                    StartDate = DateTime.Now.AddDays(-2),
                    EndDate = DateTime.Now.AddDays(15),
                    IsActive = true,
                    Priority = 2,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "مطعم الشرق الأوسط - أطباق عربية أصيلة",
                    Description = "تذوق ألذ الأطباق العربية الأصيلة من مطعم الشرق الأوسط",
                    ImageUrl = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants/middle-east-restaurant",
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(45),
                    IsActive = true,
                    Priority = 3,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "وجبات صحية من مطعم الصحة واللياقة",
                    Description = "احصل على وجبات صحية ومتوازنة من مطعم الصحة واللياقة",
                    ImageUrl = "https://images.unsplash.com/photo-1490645935967-10de6ba17061?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants/health-fitness-restaurant",
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now.AddDays(20),
                    IsActive = true,
                    Priority = 4,
                    TargetAudience = "Health Conscious",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "سوشي طازج من اليابان",
                    Description = "استمتع بأفضل أنواع السوشي الطازج من مطعم السوشي الياباني",
                    ImageUrl = "https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants/japanese-sushi-restaurant",
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now.AddDays(25),
                    IsActive = true,
                    Priority = 5,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "برجر لحم بقري طازج",
                    Description = "تذوق أفضل برجر لحم بقري طازج من مطعم البرجر الأمريكي",
                    ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants/american-burger-restaurant",
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(18),
                    IsActive = true,
                    Priority = 6,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                }
            };

            await _advertisementRepository.InsertManyAsync(advertisements);
            _logger.LogInformation("Successfully seeded {Count} advertisements.", advertisements.Count);
        }
    }
}

