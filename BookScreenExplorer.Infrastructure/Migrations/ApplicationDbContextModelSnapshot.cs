using BookScreenExplorer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace BookScreenExplorer.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        // Snapshot intentionally omitted in this template.
        // After integrating into your solution, regenerate migrations locally with:
        // dotnet ef migrations add Init --project BookScreenExplorer.Infrastructure --startup-project BookScreenExplorer.Api
    }
}
