using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

// ====== Domain ======
public class Customer : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class Category : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
}

public class Product : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Unit { get; set; } = "";
    public Guid? CategoryId { get; set; }
    public int Price { get; set; }
}

public class CustomerProductDiscount : BaseEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public double Percent { get; set; } 
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
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

    public double Qty { get; set; }
    public int UnitPrice { get; set; }

    public double DiscountPercent { get; set; }
    public int DiscountAmount { get; set; }
    public int LineTotal { get; set; }
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
}

public class ShopSetting
{
    public string Key { get; set; } = default!; 
    public string? Value { get; set; }
}


public class AppDb : IdentityDbContext<AppUser>
{
    private readonly ICurrentUser _currentUser;

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

        b.Entity<CustomerLedger>().HasKey(x => x.EntryId);
        b.Entity<CustomerLedger>().HasIndex(x => new { x.CustomerId, x.Date });

        b.Entity<ShopSetting>().HasKey(x => x.Key);

        b.Entity<Customer>();
        b.Entity<Category>();
        b.Entity<Product>();
        b.Entity<CustomerProductDiscount>();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var user = _currentUser?.UserName ?? "system";

        foreach (var e in ChangeTracker.Entries<BaseEntity>())
        {
            switch (e.State)
            {
                case EntityState.Added:
                    e.Entity.IsActive = e.Entity.IsActive; 
                    e.Entity.CreatedAt = now;
                    e.Entity.CreatedBy = user;
                    break;

                case EntityState.Modified:
                    e.Property(x => x.CreatedAt).IsModified = false;
                    e.Property(x => x.CreatedBy).IsModified = false;

                    e.Entity.UpdatedAt = now;
                    e.Entity.UpdatedBy = user;
                    break;

                case EntityState.Deleted:
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
