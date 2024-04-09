using System;
using System.Collections.Generic;

namespace RacerBooks.Models;

public partial class Cart
{
    public string ItemCode { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int Quantity { get; set; }

    public virtual User EmailNavigation { get; set; } = null!;

    public virtual Item ItemCodeNavigation { get; set; } = null!;
}
