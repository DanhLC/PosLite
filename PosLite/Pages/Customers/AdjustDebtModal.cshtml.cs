using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace PosLite.Pages.Customers;

public class AdjustDebtModalModel : PageModel
{
    private readonly AppDb _db;
    public AdjustDebtModalModel(AppDb db) => _db = db;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    public string CustomerName { get; set; } = "";
    public int CurrentBalance { get; set; }

    [BindProperty] public Input M { get; set; } = new();

    public class Input
    {
        [Required] public string Mode { get; set; } = "delta";
        public string Direction { get; set; } = "increase"; 

        [Range(0, int.MaxValue, ErrorMessage = "Số tiền không hợp lệ")]
        public int Amount { get; set; }

        [StringLength(300)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Load customer and current balance.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnGet()
    {
        var c = await _db.Customers.FirstOrDefaultAsync(x => x.CustomerId == Id);
        if (c == null) return NotFound();

        CustomerName = c.Name;
        CurrentBalance = await _db.CustomerLedgers
            .Where(l => l.CustomerId == Id)
            .SumAsync(l => l.Debit - l.Credit);

        if (string.IsNullOrWhiteSpace(M.Note))
        {
            var vi = CultureInfo.GetCultureInfo("vi-VN");
            M.Note = $"Điều chỉnh công nợ (số dư cũ: {CurrentBalance.ToString("N0", vi)})";
        }

        return Page();
    }

    /// <summary>
    /// Adjust customer debt based on input mode and amount.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostAsync()
    {
        var c = await _db.Customers.FirstOrDefaultAsync(x => x.CustomerId == Id);
        if (c == null) return NotFound();

        CustomerName = c.Name;
        CurrentBalance = await _db.CustomerLedgers
            .Where(l => l.CustomerId == Id)
            .SumAsync(l => l.Debit - l.Credit);

        if (!ModelState.IsValid)
            return Page();

        int delta;

        if (M.Mode == "set")
        {
            delta = M.Amount - CurrentBalance; 
        }
        else
        {
            delta = (M.Direction == "increase" ? 1 : -1) * M.Amount; 
        }

        if (delta == 0)
        {
            return Content(
                "<script>bootstrap.Modal.getInstance(document.getElementById('adjustModal'))?.hide();</script>",
                "text/html");
        }

        _db.CustomerLedgers.Add(new CustomerLedger
        {
            EntryId = Guid.NewGuid(),
            CustomerId = Id,
            Date = DateTime.Now,
            RefType = "ADJ",
            RefId = null,
            Debit = delta > 0 ? delta : 0,
            Credit = delta < 0 ? -delta : 0,
            BalanceAfter = CurrentBalance + delta,
            Note = M.Note
        });

        await _db.SaveChangesAsync();

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "ĐãĐã điều chỉnh công nợ.";

        return Content(@"<script>
            window.appToast?.ok('Đã điều chỉnh công nợ.');
            bootstrap.Modal.getInstance(document.getElementById('adjustModal'))?.hide();
            window.location.reload();
        </script>", "text/html");
    }
}
