using System;
using System.Collections.Generic;

namespace RacerBooks.Models;

public partial class Item
{
    public string ItemCode { get; set; } = null!;

    public int Stock { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal ItemPrice { get; set; }

    public string ItemDetails { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
