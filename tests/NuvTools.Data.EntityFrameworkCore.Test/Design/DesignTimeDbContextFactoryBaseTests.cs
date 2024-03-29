﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NUnit.Framework;
using NuvTools.Data.EntityFrameworkCore.Design;
using System;

namespace NuvTools.Data.EntityFrameworkCore.Test.Design;

public class NuvToolsContext(DbContextOptions options) : DbContext(options)
{
}

public class DesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<NuvToolsContext>
{
    protected override NuvToolsContext CreateNewInstance(DbContextOptionsBuilder<NuvToolsContext> optionsBuilder)
    {
        if (optionsBuilder is null)
            throw new ArgumentNullException(nameof(optionsBuilder));

        InitializeConfiguration(new JsonConfigurationSource { Path = "appsettings.json" });

        Assert.That(Configuration.GetConnectionString("Default") == "It works!");

        return new NuvToolsContext(optionsBuilder.Options);
    }
}

[TestFixture()]
public class DesignTimeDbContextFactoryBaseTests
{
    [Test()]
    public void CreateDbContextTest()
    {
        var factory = new DesignTimeDbContextFactory();

        var context = factory.CreateDbContext(null);

        Assert.That(context is not null);
    }
}