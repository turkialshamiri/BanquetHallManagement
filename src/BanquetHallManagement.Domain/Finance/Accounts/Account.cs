using System;
using BanquetHallManagement.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Finance.Accounts;

public class Account : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public AccountType Type { get; private set; }

    public bool IsActive { get; private set; }

    protected Account()
    {
    }

    public Account(
        Guid id,
        string code,
        string name,
        AccountType type,
        bool isActive = true)
    {
        Id = id;
        SetCode(code);
        SetName(name);
        Type = type;
        IsActive = isActive;
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), maxLength: 20);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: 200);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
