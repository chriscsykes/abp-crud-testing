﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneToManyTest.Data;
using Volo.Abp.DependencyInjection;

namespace OneToManyTest.EntityFrameworkCore;

public class EntityFrameworkCoreOneToManyTestDbSchemaMigrator
    : IOneToManyTestDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreOneToManyTestDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the OneToManyTestDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<OneToManyTestDbContext>()
            .Database
            .MigrateAsync();
    }
}
