using System.Linq.Expressions;
using Genius.Atom.Infrastructure.Entities;

namespace Genius.Atom.Data;

// TODO: This class isn't yet used, but expected to be used in the future feature.
//       It is helpful to abstract from the specific IQueryService type and create an instance of IQueryService<IEntity>
internal sealed class EntityQueryServiceProxy : IQueryService<IEntity>
{
    private readonly Func<Task<IEnumerable<IEntity>>> _getAllAsync;
    private readonly Func<Guid, Task<IEntity?>> _findByIdAsync;

    private EntityQueryServiceProxy(object queryService, Func<object, Task<IEnumerable<IEntity>>> getAllAsync, Func<object, Guid, Task<IEntity?>> findByIdAsync)
    {
        _getAllAsync = () => getAllAsync(queryService);
        _findByIdAsync = (entityId) => findByIdAsync(queryService, entityId);
    }

    internal static EntityQueryServiceProxy CreateForType(Type type, IServiceProvider services)
    {
        var queryServiceType = typeof(IQueryService<>).MakeGenericType(type);
        var queryService = services.GetService(queryServiceType);
        if (queryService is null)
        {
            throw new InvalidOperationException($"Cannot create a {nameof(EntityQueryServiceProxy)} due to missing registration of IQueryService<{type.FullName}>.");
        }

        var getAllAsync = CreateGetAllAsyncFunc(queryServiceType);
        var findByIdAsync = CreateFindByIdAsyncFunc(queryServiceType);

        return new EntityQueryServiceProxy(queryService, getAllAsync, findByIdAsync);
    }

    private static Func<object, Task<IEnumerable<IEntity>>> CreateGetAllAsyncFunc(Type queryServiceType)
    {
        var getAllAsyncMethod = queryServiceType.GetMethod(nameof(IQueryService<object>.GetAllAsync));
        var queryServiceParameter = Expression.Parameter(typeof(object), "queryService");

        // $1 = queryService as IQueryService<type>
        var queryServiceExpression = Expression.Convert(queryServiceParameter, queryServiceType);

        // (..).GetAllAsync()
        var expression = Expression.Call(queryServiceExpression, getAllAsyncMethod ?? throw new InvalidOperationException($"Couldn't find {nameof(IQueryService<object>.GetAllAsync)} method."));

        expression = ExpressionHelpers.WrapTaskExpressionWithResult(expression);
        expression = ExpressionHelpers.WrapWithEnumerableCast(expression, typeof(IEntity));
        expression = ExpressionHelpers.WrapWithTaskFromResult(expression);

        return Expression.Lambda<Func<object, Task<IEnumerable<IEntity>>>>(expression, queryServiceParameter).Compile();
    }

    private static Func<object, Guid, Task<IEntity?>> CreateFindByIdAsyncFunc(Type queryServiceType)
    {
        var findByIdAsyncMethod = queryServiceType.GetMethod(nameof(IQueryService<object>.FindByIdAsync));
        var queryServiceParameter = Expression.Parameter(typeof(object), "queryService");

        // $1 = queryService as IQueryService<type>
        var queryServiceExpression = Expression.Convert(queryServiceParameter, queryServiceType);

        var entityIdParameter = Expression.Parameter(typeof(Guid), "entityId");

        // $1.FindByIdAsync(Guid)
        var expression = Expression.Call(queryServiceExpression,
            findByIdAsyncMethod ?? throw new InvalidOperationException($"Couldn't find {nameof(IQueryService<object>.GetAllAsync)} method."),
            entityIdParameter);

        expression = ExpressionHelpers.WrapTaskExpressionWithResult(expression);
        expression = ExpressionHelpers.WrapWithTaskFromResult(expression, typeof(IEntity));

        return Expression.Lambda<Func<object, Guid, Task<IEntity?>>>(expression, queryServiceParameter, entityIdParameter).Compile();
    }

    public Task<IEntity?> FindByIdAsync(Guid entityId)
    {
        return _findByIdAsync(entityId);
    }

    public Task<IEnumerable<IEntity>> GetAllAsync()
    {
        return _getAllAsync();
    }
}
