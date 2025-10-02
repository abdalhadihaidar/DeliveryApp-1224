using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using DeliveryApp.Domain.Entities;
using System;
using System.Linq;

namespace DeliveryApp.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class DeliveryAppDbContext :
    AbpDbContext<DeliveryAppDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    
    // Custom entities for delivery app
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<RestaurantCategory> RestaurantCategories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<FavoriteRestaurant> FavoriteRestaurants { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<SpecialOffer> SpecialOffers { get; set; }
    public DbSet<DeliveryStatus> DeliveryStatuses { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<NotificationSettings> NotificationSettings { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<StripeCustomer> StripeCustomers { get; set; }
    public DbSet<ConnectedAccount> ConnectedAccounts { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
    public DbSet<RestaurantPayout> RestaurantPayouts { get; set; }
    public DbSet<Advertisement> Advertisements { get; set; }
    public DbSet<MealCategory> MealCategories { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<AdRequest> AdRequests { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<CODTransaction> CODTransactions { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public DeliveryAppDbContext(DbContextOptions<DeliveryAppDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        // Configure ExtraPropertyDictionary to fix migration issues
        try
        {
            builder.Ignore<Volo.Abp.Data.ExtraPropertyDictionary>();
        }
        catch
        {
            // If ExtraPropertyDictionary is already configured, ignore the error
        }

        // Configure custom entities
        builder.Entity<AppUser>(b =>
        {
            // AppUser inherits from IdentityUser, so it uses the same table (AbpUsers)
            b.HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);
            b.HasMany(u => u.PaymentMethods)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);
            b.HasOne(u => u.DeliveryStatus)
                .WithOne()
                .HasForeignKey<DeliveryStatus>(d => d.Id);
            b.HasOne(u => u.Preferences)
                .WithOne(p => p.User)
                .HasForeignKey<UserPreferences>(p => p.UserId);
            b.HasOne(u => u.CurrentLocation)
                .WithOne()
                .HasForeignKey<Location>(l => l.Id);
            b.Property(u => u.IsEmailConfirmed).HasColumnName("IsEmailConfirmed");
            b.Property(u => u.IsPhoneConfirmed).HasColumnName("IsPhoneConfirmed");
            b.Property(u => u.IsAdminApproved).HasColumnName("IsAdminApproved");
            b.Property(u => u.ApprovedTime).HasColumnName("ApprovedTime");
            b.Property(u => u.ApprovedById).HasColumnName("ApprovedById");
        });

        // Configure discriminator for IdentityUser hierarchy
        builder.Entity<IdentityUser>(b =>
        {
            b.HasDiscriminator<string>("Discriminator")
                .HasValue<IdentityUser>("IdentityUser")
                .HasValue<AppUser>("AppUser");
            
            // Set default discriminator value for existing records
            b.Property<string>("Discriminator")
                .HasDefaultValue("IdentityUser");
        });

        builder.Entity<RestaurantCategory>(b =>
        {
            b.ToTable("RestaurantCategories");
            b.HasIndex(c => c.Name).IsUnique();
            b.HasMany(c => c.Restaurants)
                .WithOne(r => r.Category)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Restaurant>(b =>
        {
            b.ToTable("Restaurants");
            b.HasMany(r => r.Menu)
                .WithOne(m => m.Restaurant)
                .HasForeignKey(m => m.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(r => r.Address)
                .WithOne(a => a.Restaurant)
                .HasForeignKey<Address>(a => a.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(r => r.Category)
                .WithMany(c => c.Restaurants)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(r => r.Tags).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        });

        builder.Entity<MealCategory>(b =>
        {
            b.ToTable("MealCategories");
            b.HasIndex(mc => new { mc.RestaurantId, mc.Name }).IsUnique();
            b.HasMany(mc => mc.MenuItems)
                .WithOne(mi => mi.MealCategory)
                .HasForeignKey(mi => mi.MealCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AdRequest>(b =>
        {
            b.ToTable("AdRequests");
            b.HasOne(ar => ar.Restaurant)
                .WithMany()
                .HasForeignKey(ar => ar.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(ar => ar.ReviewedBy)
                .WithMany()
                .HasForeignKey(ar => ar.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(ar => ar.Advertisement)
                .WithMany()
                .HasForeignKey(ar => ar.AdvertisementId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // existing configuration for MenuItem
        builder.Entity<MenuItem>(b =>
        {
            b.ToTable("MenuItems");
            b.Property(m => m.Options).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        });

        builder.Entity<Order>(b =>
        {
            b.ToTable("Orders");
            b.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);
            b.HasOne(o => o.Restaurant)
                .WithMany()
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(o => o.DeliveryAddress)
                .WithMany()
                .HasForeignKey(o => o.DeliveryAddressId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<OrderItem>(b =>
        {
            b.ToTable("OrderItems");
            b.Property(i => i.Options).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
            b.Property(i => i.SelectedOptions).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        });

        builder.Entity<Address>(b =>
        {
            b.ToTable("Addresses");
        });

        builder.Entity<PaymentMethod>(b =>
        {
            b.ToTable("PaymentMethods");
        });

        builder.Entity<FavoriteRestaurant>(b =>
        {
            b.ToTable("FavoriteRestaurants");
            b.HasKey(f => new { f.UserId, f.RestaurantId });
            b.HasOne(f => f.User)
                .WithMany(u => u.FavoriteRestaurants)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(f => f.Restaurant)
                .WithMany()
                .HasForeignKey(f => f.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        builder.Entity<Review>(b =>
        {
            b.ToTable("Reviews");
            b.HasOne(r => r.Restaurant)
                .WithMany()
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        builder.Entity<SpecialOffer>(b =>
        {
            b.ToTable("SpecialOffers");
            b.HasOne(o => o.Restaurant)
                .WithMany()
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
            b.Property(o => o.ApplicableCategories).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        });
        
        builder.Entity<DeliveryStatus>(b =>
        {
            b.ToTable("DeliveryStatuses");
        });
        
        builder.Entity<UserPreferences>(b =>
        {
            b.ToTable("UserPreferences");
            b.Property(p => p.FavoriteCuisines).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
            b.Property(p => p.DietaryRestrictions).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        });
        
        builder.Entity<NotificationSettings>(b =>
        {
            b.ToTable("NotificationSettings");
        });
        
        builder.Entity<Location>(b =>
        {
            b.ToTable("Locations");
        });
        
        builder.Entity<PaymentTransaction>(b =>
        {
            b.ToTable("PaymentTransactions");
            b.HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        builder.Entity<StripeCustomer>(b =>
        {
            b.ToTable("StripeCustomers");
            b.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        builder.Entity<ConnectedAccount>(b =>
        {
            b.ToTable("ConnectedAccounts");
            b.HasOne(c => c.Restaurant)
                .WithMany()
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        builder.Entity<FinancialTransaction>(b =>
        {
            b.ToTable("FinancialTransactions");
        });
        
        builder.Entity<RestaurantPayout>(b =>
        {
            b.ToTable("RestaurantPayouts");
            b.HasOne(r => r.Restaurant)
                .WithMany()
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        builder.Entity<Advertisement>(b =>
        {
            b.ToTable("Advertisements");
            b.HasOne(a => a.Restaurant)
                .WithMany()
                .HasForeignKey(a => a.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        builder.Entity<ChatSession>(b =>
        {
            b.ToTable("ChatSessions");
            b.HasKey(c => c.Id);
            b.Property(c => c.DeliveryId).IsRequired();
            b.Property(c => c.CustomerId).IsRequired();
            b.Property(c => c.CustomerPhoneNumber).HasMaxLength(20);
            b.Property(c => c.AdminId).IsRequired();
            b.Property(c => c.AdminName).HasMaxLength(100);
            b.Property(c => c.CreatedAt).IsRequired();
            b.Property(c => c.IsActive).IsRequired();
        });
        
        builder.Entity<ChatMessage>(b =>
        {
            b.ToTable("ChatMessages");
            b.HasKey(c => c.Id);
            b.Property(c => c.SessionId).IsRequired();
            b.Property(c => c.SenderId).IsRequired();
            b.Property(c => c.SenderType).HasMaxLength(20).IsRequired();
            b.Property(c => c.Content).HasMaxLength(1000).IsRequired();
            b.Property(c => c.SentAt).IsRequired();
            b.Property(c => c.IsRead).IsRequired();
            b.Property(c => c.MessageType).HasMaxLength(20).IsRequired();
            
            b.HasOne<ChatSession>()
                .WithMany()
                .HasForeignKey(c => c.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<CODTransaction>(b =>
        {
            b.ToTable("CODTransactions");
            b.HasOne(c => c.Order)
                .WithMany()
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(c => c.DeliveryPerson)
                .WithMany()
                .HasForeignKey(c => c.DeliveryPersonId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(c => c.Restaurant)
                .WithMany()
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
