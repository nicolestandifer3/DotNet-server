using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Instrumentation;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.Extensions.Options;

namespace GraphQL.Server
{
    public class BasicGraphQLExecuter<TSchema> : IGraphQLExecuter<TSchema>
        where TSchema : ISchema
    {
        public TSchema Schema { get; }

        private readonly IDocumentExecuter _documentExecuter;
        private readonly GraphQLOptions _options;

        public BasicGraphQLExecuter(
            TSchema schema,
            IDocumentExecuter documentExecuter,
            IOptions<GraphQLOptions> options)
        {
            Schema = schema;

            _documentExecuter = documentExecuter;
            _options = options.Value;
        }

        public virtual async Task<ExecutionResult> ExecuteAsync(GraphQLRequest request, IDictionary<string, object> context, IServiceProvider requestServices, CancellationToken cancellationToken = default)
        {
            var start = DateTime.UtcNow;

            var options = GetOptions(request, context, requestServices, cancellationToken);
            var result = await _documentExecuter.ExecuteAsync(options);

            if (options.EnableMetrics)
            {
                result.EnrichWithApolloTracing(start);
            }

            return result;
        }

        protected virtual ExecutionOptions GetOptions(GraphQLRequest request, IDictionary<string, object> context, IServiceProvider requestServices, CancellationToken cancellationToken)
        {
            var opts = new ExecutionOptions
            {
                Schema = Schema,
                OperationName = request.OperationName,
                Query = request.Query,
                Variables = request.Variables,
                Extensions = request.Extensions,
                UserContext = context,
                CancellationToken = cancellationToken,
                ComplexityConfiguration = _options.ComplexityConfiguration,
                EnableMetrics = _options.EnableMetrics,
                UnhandledExceptionDelegate = _options.UnhandledExceptionDelegate,
                MaxParallelExecutionCount = _options.MaxParallelExecutionCount,
                RequestServices = requestServices,
            };

            return opts;
        }
    }
}
