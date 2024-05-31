using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Genius.Atom.Infrastructure;

public static class ExpressionHelpers
{
    public static string GetPropertyName<TContainer, TProp>(Expression<Func<TContainer, TProp>> propertyLambda)
    {
        Guard.NotNull(propertyLambda);

        if (propertyLambda.Body is not MemberExpression body)
        {
            var unaryBody = (UnaryExpression)propertyLambda.Body;
            body = (MemberExpression)unaryBody.Operand;
        }

        return body.NotNull().Member.Name;
    }

    /// <summary>
    ///   Wraps the input <paramref name="expression"/> of Task<T> type with the following expression:
    ///   (<paramref name="expression"/>).GetAwaiter().GetResult()
    /// </summary>
    /// <param name="expression">The input expression of Task<> type.</param>
    /// <returns>The wrapped expression.</returns>
    public static MethodCallExpression WrapTaskExpressionWithResult(Expression expression)
    {
        Guard.NotNull(expression);
        var taskType = expression.Type;

        if (!typeof(Task).IsAssignableFrom(taskType)
            || !taskType.IsGenericType)
        {
            throw new InvalidOperationException("The input expression must be of Task<T> type.");
        }

        // (..).GetAwaiter()
        var getAwaiterMethod = taskType.GetMethod(nameof(Task.GetAwaiter)).NotNull();
        expression = Expression.Call(expression, getAwaiterMethod);

        var taskAwaiterType = expression.Type;

        // (..).GetResult()
        var getResultMethod = taskAwaiterType.GetMethod(nameof(TaskAwaiter.GetResult)).NotNull();
        return Expression.Call(expression, getResultMethod);
    }

    /// <summary>
    ///   Wraps the input <paramref name="expression"/> with the following expression:
    ///   Task.FromResult(<paramref name="expression"/>)
    ///   or Task.FromResult&lt;<paramref name="targetType"/>&gt;(<paramref name="expression"/>) if <paramref name="targetType"/> is provided.
    /// </summary>
    /// <param name="expression">The input expression.</param>
    /// <param name="targetType">The target type of the task result, to create Task&lt;<paramref name="targetType"/>&gt;.</param>
    /// <returns>The wrapped expression.</returns>
    public static MethodCallExpression WrapWithTaskFromResult(Expression expression, Type? targetType = null)
    {
        Guard.NotNull(expression);

        return Expression.Call(typeof(Task), nameof(Task.FromResult),
            [targetType ?? expression.Type], expression);
    }

    /// <summary>
    ///   Wraps the input <paramref name="expression"/> with the following expression:
    ///   Enumerable.Cast&lt;<paramref name="type"/>&gt;(<paramref name="expression"/>)
    /// </summary>
    /// <param name="expression">The input expression.</param>
    /// <param name="type">The target type to cast.</param>
    /// <returns>The wrapped expression.</returns>
    public static MethodCallExpression WrapWithEnumerableCast(Expression expression, Type type)
    {
        return Expression.Call(typeof(Enumerable), nameof(Enumerable.Cast), new [] { type }, expression);
    }
}
