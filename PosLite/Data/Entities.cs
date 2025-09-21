using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PosLite.Common;
using System.ComponentModel.DataAnnotations;

// ====== Base & Identity ======
public abstract class BaseEntity
{
    public bool IsActive { get; set; } = true;

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AppUser : IdentityUser { }


public class Customer : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string NameSearch { get; set; } = "";
    public string CodeSearch { get; set; } = "";
}

public class Category : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string NameSearch { get; set; } = "";
}

public class Product : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Unit { get; set; } = "";
    public Guid? CategoryId { get; set; }
    public decimal Price { get; set; }
    public string NameSearch { get; set; } = "";
    public string CodeSearch { get; set; } = "";
}

public class CustomerProductDiscount : BaseEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public double Percent { get; set; }
}

public class SaleInvoice : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = default!;
    public DateTime Date { get; set; } = DateTime.Now;
    public Guid CustomerId { get; set; }

    public int PreviousDebt { get; set; }
    public int TotalAmount { get; set; }
    public int Payment { get; set; }
    public int RemainingDebt { get; set; }

    public List<SaleInvoiceLine> Lines { get; set; } = new();
}

public class SaleInvoiceLine
{
    public Guid LineId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string ProductCode { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string Unit { get; set; } = "";

    public decimal UnitPrice { get; set; }
    public double DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}


public class CustomerLedger
{
    public Guid EntryId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime Date { get; set; }

    public string RefType { get; set; } = default!;
    public string? RefId { get; set; }

    public int Debit { get; set; }
    public int Credit { get; set; }
    public int BalanceAfter { get; set; }
    public string? Note { get; set; }
}

public class ShopSetting
{
    public string Key { get; set; } = default!;
    public string? JsonValue { get; set; }
}

public class ShopSettings
{
    public string ShopName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Hotline { get; set; } = "";
    public string Slogan { get; set; } = "";
    public string Note { get; set; } = "";
    public string LogoUrl { get; set; } = "";
}


/// <summary>
/// Application database context, inheriting from IdentityDbContext to include identity management.
/// </summary>
public class AppDb : IdentityDbContext<AppUser>
{
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Constructor for AppDb, accepting DbContextOptions and ICurrentUser for tracking the current user.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="currentUser"></param>
    public AppDb(DbContextOptions<AppDb> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CustomerProductDiscount> CustomerProductDiscounts => Set<CustomerProductDiscount>();
    public DbSet<SaleInvoice> SaleInvoices => Set<SaleInvoice>();
    public DbSet<SaleInvoiceLine> SaleInvoiceLines => Set<SaleInvoiceLine>();
    public DbSet<CustomerLedger> CustomerLedgers => Set<CustomerLedger>();
    public DbSet<ShopSetting> ShopSettings => Set<ShopSetting>();

    /// <summary>
    /// Configure the entity models and their relationships.
    /// </summary>
    /// <param name="b"></param>
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Customer>().HasKey(x => x.CustomerId);
        b.Entity<Customer>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Customer>().HasIndex(x => x.Phone);

        b.Entity<Category>().HasKey(x => x.CategoryId);
        b.Entity<Category>().HasIndex(x => x.Name).IsUnique();

        b.Entity<Product>().HasKey(x => x.ProductId);
        b.Entity<Product>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Product>().HasIndex(x => new { x.CategoryId });

        b.Entity<CustomerProductDiscount>().HasKey(x => x.Id);
        b.Entity<CustomerProductDiscount>()
            .HasIndex(x => new { x.CustomerId, x.ProductId }).IsUnique();

        b.Entity<SaleInvoice>().HasKey(x => x.InvoiceId);
        b.Entity<SaleInvoice>().HasIndex(x => x.Date);
        b.Entity<SaleInvoice>().HasIndex(x => new { x.CustomerId, x.Date });

        b.Entity<SaleInvoiceLine>().HasKey(x => x.LineId);
        b.Entity<SaleInvoiceLine>().HasIndex(x => x.InvoiceId);
        b.Entity<SaleInvoiceLine>()
            .HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<CustomerLedger>().HasKey(x => x.EntryId);
        b.Entity<CustomerLedger>().HasIndex(x => new { x.CustomerId, x.Date });

        b.Entity<ShopSetting>().HasKey(x => x.Key);

        b.Entity<Customer>();
        b.Entity<Customer>().HasIndex(x => x.NameSearch);
        b.Entity<Customer>().HasIndex(x => x.CodeSearch);

        b.Entity<Category>().HasIndex(x => x.NameSearch);

        b.Entity<Product>().HasKey(x => x.ProductId);
        b.Entity<Product>().HasIndex(x => x.NameSearch);
        b.Entity<Product>().HasIndex(x => x.CodeSearch);
        b.Entity<Product>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Product>().HasIndex(x => x.CategoryId);
        b.Entity<Product>().Property(p => p.Price).HasColumnType("NUMERIC");
        b.Entity<CustomerProductDiscount>();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically set CreatedAt, CreatedBy, UpdatedAt, and UpdatedBy fields.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var user = _currentUser?.UserName ?? "system";

        foreach (var e in ChangeTracker.Entries<BaseEntity>())
        {
            switch (e.State)
            {
                case EntityState.Added:
                    e.Entity.CreatedAt = now;
                    e.Entity.CreatedBy = user;

                    if (e.Entity is Category cAdd)
                        cAdd.NameSearch = TextSearch.Normalize(cAdd.Name);

                    if (e.Entity is Product pAdd)
                    {
                        pAdd.NameSearch = TextSearch.Normalize(pAdd.Name);
                        pAdd.CodeSearch = TextSearch.Normalize(pAdd.Code);
                    }

                    if (e.Entity is Customer cuAdd)
                    {
                        cuAdd.NameSearch = TextSearch.Normalize(cuAdd.Name);
                        cuAdd.CodeSearch = TextSearch.Normalize(cuAdd.Code);
                    }

                    break;

                case EntityState.Modified:
                    e.Property(x => x.CreatedAt).IsModified = false;
                    e.Property(x => x.CreatedBy).IsModified = false;

                    // Không audit nếu chỉ đổi các field kỹ thuật search
                    bool needAudit = true;
                    if (e.Entity is Category)
                    {
                        var names = e.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToHashSet();
                        names.RemoveWhere(n => n is nameof(BaseEntity.CreatedAt) or nameof(BaseEntity.CreatedBy)
                                                 or nameof(BaseEntity.UpdatedAt) or nameof(BaseEntity.UpdatedBy));
                        if (names.Count == 1 && names.Contains(nameof(Category.NameSearch))) needAudit = false;
                    }

                    if (e.Entity is Product)
                    {
                        var names = e.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToHashSet();
                        names.RemoveWhere(n => n is nameof(BaseEntity.CreatedAt) or nameof(BaseEntity.CreatedBy)
                                                 or nameof(BaseEntity.UpdatedAt) or nameof(BaseEntity.UpdatedBy));
                        if (names.All(n => n is nameof(Product.NameSearch) or nameof(Product.CodeSearch)))
                            needAudit = false;
                    }

                    if (e.Entity is Customer)
                    {
                        var names = e.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToHashSet();
                        names.RemoveWhere(n => n is nameof(BaseEntity.CreatedAt) or nameof(BaseEntity.CreatedBy)
                                                 or nameof(BaseEntity.UpdatedAt) or nameof(BaseEntity.UpdatedBy));
                        if (names.All(n => n is nameof(Customer.NameSearch) or nameof(Customer.CodeSearch)))
                            needAudit = false;
                    }

                    if (needAudit)
                    {
                        e.Entity.UpdatedAt = now;
                        e.Entity.UpdatedBy = user;
                    }

                    if (e.Entity is Category cUpd && e.Property(nameof(Category.Name)).IsModified)
                    {
                        cUpd.NameSearch = TextSearch.Normalize(cUpd.Name);
                    }

                    if (e.Entity is Product pUpd)
                    {
                        if (e.Property(nameof(Product.Name)).IsModified)
                            pUpd.NameSearch = TextSearch.Normalize(pUpd.Name);
                        if (e.Property(nameof(Product.Code)).IsModified)
                            pUpd.CodeSearch = TextSearch.Normalize(pUpd.Code);
                    }

                    if (e.Entity is Customer custUpd)
                    {
                        if (e.Property(nameof(Customer.Name)).IsModified)
                            custUpd.NameSearch = TextSearch.Normalize(custUpd.Name);
                        if (e.Property(nameof(Customer.Code)).IsModified)
                            custUpd.CodeSearch = TextSearch.Normalize(custUpd.Code);
                    }
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

}
