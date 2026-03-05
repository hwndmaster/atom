using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef;

public interface IDbContextProvider
{
    DbContext GetDbContext();
}
