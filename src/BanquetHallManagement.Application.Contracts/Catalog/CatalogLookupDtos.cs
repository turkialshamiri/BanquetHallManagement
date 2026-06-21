using System;

namespace BanquetHallManagement.Catalog;

public class HallLookupDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
}

public class CustomerLookupDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
}

public class ServiceLookupDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
}
