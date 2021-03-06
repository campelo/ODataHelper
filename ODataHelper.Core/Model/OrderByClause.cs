namespace ODataQueryHelper.Core.Model
{
    /// <summary>
    /// Represents Order by clause provided in OData Query Expression
    /// </summary>
    public class OrderByClause<T> where T : class
    {
        const string fieldSeparator = @"\,";
        const string orderBySeparator = @"\s+";

        /// <summary>
        /// Collection of order by node 
        /// </summary>
		public List<OrderByNode<T>> OrderByNodes { get; protected set; }

        /// <summary>
        /// Creates new instance of OrderByClause
        /// </summary>
        public OrderByClause()
        {
            OrderByNodes = new List<OrderByNode<T>>();
        }

        /// <summary>
        /// Try and Parse Orderby expression from OData Query
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="expression"/> is not null or empty.</exception>
        /// <exception cref="ODataHelper.Core.Exceptions.PropertyNotFoundException">property name provided in field does not belong to <typeparamref name="T"/>></exception>
        /// <param name="expression">order by expression</param>
		public void TryParseOrderBy(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentNullException(expression);
            }
            List<string> fields = Regex.Split(expression, fieldSeparator)?.ToList();
            if (fields?.Any() == true)
            {
                int seq = 1;
                fields.ForEach(o =>
                {
                    var parts = Regex.Split(o.Trim(), orderBySeparator);
                    if (parts.Length <= 2)
                    {
                        string field = parts[0];
                        string direction = (parts.Length == 2) ? parts[1] : "asc";

                        var newNode = new OrderByNode<T>
                        {
                            Sequence = seq,
                            PropertyName = field,
                            Direction = (string.Compare(direction, "asc", true) == 0) ? OrderByDirectionType.Ascending : OrderByDirectionType.Descending
                        };
                        CreateExpression(newNode);
                        OrderByNodes.Add(newNode);
                        seq++;
                    }
                });
            }
        }

        /// <summary>
        /// Creates Expression for OrderBy/OrderByDescending
        /// </summary>
        /// <param name="node">Order by node to create expression</param>
        private void CreateExpression(OrderByNode<T> node)
        {
            const string expressionName = "o";
            try
            {
                if (!string.IsNullOrWhiteSpace(node?.PropertyName))
                {
                    var param = Expression.Parameter(typeof(T), expressionName);
                    var parts = node.PropertyName.Replace('/', '.').Split('.');
                    Expression parent = param;
                    foreach (var part in parts)
                    {
                        parent = Expression.Property(parent, part);
                    }

                    if (parent.Type.IsValueType)
                    {
                        var converted = Expression.Convert(parent, typeof(object));
                        node.Expression = Expression.Lambda<Func<T, object>>(converted, param);
                    }
                    else
                    {
                        node.Expression = Expression.Lambda<Func<T, object>>(parent, param);
                    }
                    node.PropertyName = parent.ToString();
                    if (node.PropertyName.StartsWith($"{expressionName}."))
                        node.PropertyName = node.PropertyName.Substring($"{expressionName}.".Length);
                }
            }
            catch (ArgumentException ex)
            {
                Error.PropertyNotFound(ex.Message);
            }
        }

    }
}
